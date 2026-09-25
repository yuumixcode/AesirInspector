using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Sirenix.Serialization;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// 状态存储条目类型。
    /// </summary>
    internal enum UltraStateStoreEntryKind
    {
        /// <summary>面板当前选中的示例类型名（轻量 string 记录，无序列化数据）。</summary>
        PanelSelection = 0,

        /// <summary>示例的用户调试数据快照（Odin 序列化数据）。</summary>
        ExampleState = 1
    }

    /// <summary>
    /// 状态存储条目。
    /// </summary>
    [Serializable]
    internal class UltraStateStoreEntry
    {
        public string key;

        public string typeName;

        public string checksum;

        public int kind;

        /// <summary>PanelSelection 条目的值（示例类型全名）；ExampleState 条目忽略此字段。</summary>
        public string selectionValue;

        public SerializationData state;
    }

    /// <summary>
    /// Attribute Overview Ultra 的状态存储：持久化 Attribute Overview Ultra 的用户状态与数据。
    /// 面板与示例在 Ultra 中均为 CreateInstance 的内存实例，用户数据以快照形式持久化于此：
    /// 面板选中记录（UI 状态，轻量 string）+ 示例数据快照（Odin 序列化数据，SHA256 校验和 + 类型名双校验）。
    /// 单文件资产，条目类别编码在 key 前缀（PanelSelection/、ExampleState/）中。
    /// </summary>
    public class UltraStateStoreSO : ScriptableObject
    {
        static UltraStateStoreSO _cachedStore;

        [SerializeField]
        List<UltraStateStoreEntry> entries = new List<UltraStateStoreEntry>();

        public int EntryCount => entries.Count;

        /// <summary>
        /// 获取或创建全局状态存储实例（静态缓存，Domain Reload 后自动重载）。
        /// 示例单例路由（GetMemoryExample）与 Ultra 窗口共用同一状态存储。
        /// </summary>
        public static UltraStateStoreSO LoadOrCreate()
        {
            if (_cachedStore)
            {
                return _cachedStore;
            }

            _cachedStore = AssetDatabase.LoadAssetAtPath<UltraStateStoreSO>(
                AesirInspectorPaths.AttributeOverviewUltraStateStorePath);
            if (_cachedStore != null)
            {
                return _cachedStore;
            }

            var folder = Path.GetDirectoryName(AesirInspectorPaths.AttributeOverviewUltraStateStorePath);
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            _cachedStore = CreateInstance<UltraStateStoreSO>();
            AssetDatabase.CreateAsset(_cachedStore, AesirInspectorPaths.AttributeOverviewUltraStateStorePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return _cachedStore;
        }

        /// <summary>
        /// 获取指定示例类型的内存单例：状态存储有快照则恢复用户数据，否则返回全新默认实例。
        /// 这是示例 SO 单例（AttributeExampleSO / OdinAttributeExampleSO 的 Instance）的唯一后端，
        /// 全程零 AssetDatabase 写操作。
        /// </summary>
        public static T GetMemoryExample<T>() where T : ScriptableObject
        {
            var bank = LoadOrCreate();
            return (T)bank.RestoreOrCreateExample(typeof(T).Name, typeof(T));
        }

        #region --- 面板选中记录 ---

        const string PanelSelectionPrefix = "PanelSelection/";

        /// <summary>
        /// 记录面板当前选中的示例类型名。
        /// </summary>
        public void SavePanelSelection(string panelKey, string exampleTypeName)
        {
            var entry = FindOrCreateEntry(PanelSelectionPrefix + panelKey);
            entry.kind = (int)UltraStateStoreEntryKind.PanelSelection;
            entry.typeName = exampleTypeName;
            entry.selectionValue = exampleTypeName;
            entry.state = default;
            entry.checksum = null;
            EditorUtility.SetDirty(this);
        }

        /// <summary>
        /// 尝试获取面板上次选中的示例类型名。
        /// </summary>
        public bool TryGetPanelSelection(string panelKey, out string exampleTypeName)
        {
            exampleTypeName = null;
            var entry = FindEntry(PanelSelectionPrefix + panelKey);
            if (entry == null || entry.kind != (int)UltraStateStoreEntryKind.PanelSelection)
            {
                return false;
            }

            exampleTypeName = entry.selectionValue;
            return !string.IsNullOrEmpty(exampleTypeName);
        }

        #endregion

        #region --- 示例状态快照 ---

        const string ExampleStatePrefix = "ExampleState/";

        /// <summary>
        /// 把内存示例实例的当前数据（全部字段，含 Odin 序列化成员）快照进状态存储。
        /// </summary>
        public void SaveExampleState(string exampleKey, Object instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            var entry = FindOrCreateEntry(ExampleStatePrefix + exampleKey);
            entry.kind = (int)UltraStateStoreEntryKind.ExampleState;
            entry.typeName = instance.GetType().FullName;
            entry.selectionValue = null;
            entry.state = new SerializationData();
            UnitySerializationUtility.SerializeUnityObject(instance, ref entry.state, true);
            entry.checksum = ComputeChecksum(entry.state);
            EditorUtility.SetDirty(this);
        }

        /// <summary>
        /// 恢复状态到已存在的实例（调用方负责 CreateInstance 保证生命周期正确）。
        /// 校验和/类型名不匹配（数据损坏、版本漂移、跨类型）返回 false，调用方回落默认值。
        /// </summary>
        public bool TryRestoreExampleState(string exampleKey, Object instance)
        {
            var entry = FindEntry(ExampleStatePrefix + exampleKey);
            if (entry == null || instance == null || entry.kind != (int)UltraStateStoreEntryKind.ExampleState)
            {
                return false;
            }

            if (entry.typeName != instance.GetType().FullName)
            {
                return false;
            }

            if (entry.checksum != ComputeChecksum(entry.state))
            {
                return false;
            }

            try
            {
                var data = entry.state;
                UnitySerializationUtility.DeserializeUnityObject(instance, ref data);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[AttributeOverviewUltra] 恢复 '{exampleKey}' 失败，回落默认值: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// 恢复或创建：有快照则恢复用户调试状态，否则返回全新默认实例。
        /// </summary>
        public ScriptableObject RestoreOrCreateExample(string exampleKey, Type exampleType)
        {
            var instance = CreateInstance(exampleType);
            instance.name = exampleKey;
            TryRestoreExampleState(exampleKey, instance);
            return instance;
        }

        public void RemoveExampleState(string exampleKey)
        {
            RemoveEntry(ExampleStatePrefix + exampleKey);
        }

        /// <summary>
        /// 移除指定面板的选中记录（面板重置用）。
        /// </summary>
        public void RemovePanelSelection(string panelKey)
        {
            RemoveEntry(PanelSelectionPrefix + panelKey);
        }

        #endregion

        #region --- Internal ---

        UltraStateStoreEntry FindEntry(string fullKey)
        {
            foreach (var e in entries)
            {
                if (e.key == fullKey)
                {
                    return e;
                }
            }

            return null;
        }

        UltraStateStoreEntry FindOrCreateEntry(string fullKey)
        {
            var entry = FindEntry(fullKey);
            if (entry == null)
            {
                entry = new UltraStateStoreEntry { key = fullKey };
                entries.Add(entry);
            }

            return entry;
        }

        void RemoveEntry(string fullKey)
        {
            var entry = FindEntry(fullKey);
            if (entry != null)
            {
                entries.Remove(entry);
                EditorUtility.SetDirty(this);
            }
        }

        /// <summary>
        /// 对快照载荷计算 SHA256 校验和。恢复前比对，把"垃圾数据被尽力解析成半态"转为可检测失败。
        /// 覆盖 Binary（SerializedBytes/SerializedBytesString）与 Nodes（SerializationNodes）两种载荷形态。
        /// </summary>
        static string ComputeChecksum(SerializationData data)
        {
            var payload = data.SerializedBytes;
            if ((payload == null || payload.Length == 0) && !string.IsNullOrEmpty(data.SerializedBytesString))
            {
                payload = Convert.FromBase64String(data.SerializedBytesString);
            }

            if (payload == null || payload.Length == 0)
            {
                if (data.SerializationNodes == null || data.SerializationNodes.Count == 0)
                {
                    return string.Empty;
                }

                // Nodes 格式：对节点内容做规范化字符串哈希
                var nodeBuffer = new StringBuilder();
                foreach (var node in data.SerializationNodes)
                {
                    nodeBuffer.Append(node.Name).Append('\x1F').Append((int)node.Entry).Append('\x1F')
                        .Append(node.Data).Append('\x1E');
                }

                payload = Encoding.UTF8.GetBytes(nodeBuffer.ToString());
            }

            using var sha = SHA256.Create();
            return BitConverter.ToString(sha.ComputeHash(payload)).Replace("-", "", StringComparison.Ordinal);
        }

        #endregion
    }
}
