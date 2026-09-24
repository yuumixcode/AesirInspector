using System;
using System.Collections.Generic;
using Sirenix.Serialization;
using UnityEditor;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// 状态银行条目类型。
    /// </summary>
    enum UltraBankEntryKind
    {
        /// <summary>面板当前选中的示例类型名（轻量 string 记录，无序列化数据）。</summary>
        PanelSelection = 0,

        /// <summary>示例的用户调试状态快照（Odin 序列化数据）。</summary>
        ExampleState = 1
    }

    /// <summary>
    /// 状态银行条目。
    /// </summary>
    [Serializable]
    class UltraStateBankEntry
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
    /// Attribute Overview Ultra 的状态银行。
    /// 面板与示例在 Ultra 中均为 CreateInstance 的内存实例，用户调试状态以快照形式持久化于此：
    /// 面板选中记录（轻量）+ 示例状态快照（Odin 序列化数据，SHA256 校验和 + 类型名双校验）。
    /// 单文件资产，分类信息编码在 key 前缀中。
    /// </summary>
    public class UltraStateBankSO : ScriptableObject
    {
        [SerializeField]
        List<UltraStateBankEntry> entries = new List<UltraStateBankEntry>();

        public int EntryCount => entries.Count;

        #region --- 面板选中记录 ---

        const string PanelSelectionPrefix = "PanelSelection/";

        /// <summary>
        /// 记录面板当前选中的示例类型名。
        /// </summary>
        public void SavePanelSelection(string panelKey, string exampleTypeName)
        {
            var entry = FindOrCreateEntry(PanelSelectionPrefix + panelKey);
            entry.kind = (int)UltraBankEntryKind.PanelSelection;
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
            if (entry == null || entry.kind != (int)UltraBankEntryKind.PanelSelection)
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
        /// 把内存示例实例的当前状态（全部字段，含 Odin 序列化成员）快照进银行。
        /// </summary>
        public void SaveExampleState(string exampleKey, UnityEngine.Object instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            var entry = FindOrCreateEntry(ExampleStatePrefix + exampleKey);
            entry.kind = (int)UltraBankEntryKind.ExampleState;
            entry.typeName = instance.GetType().FullName;
            entry.selectionValue = null;
            entry.state = new SerializationData();
            UnitySerializationUtility.SerializeUnityObject(instance, ref entry.state,
                serializeUnityFields: true);
            entry.checksum = ComputeChecksum(entry.state);
            EditorUtility.SetDirty(this);
        }

        /// <summary>
        /// 恢复状态到已存在的实例（调用方负责 CreateInstance 保证生命周期正确）。
        /// 校验和/类型名不匹配（数据损坏、版本漂移、跨类型）返回 false，调用方回落默认值。
        /// </summary>
        public bool TryRestoreExampleState(string exampleKey, UnityEngine.Object instance)
        {
            var entry = FindEntry(ExampleStatePrefix + exampleKey);
            if (entry == null || instance == null ||
                entry.kind != (int)UltraBankEntryKind.ExampleState)
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
            var instance = ScriptableObject.CreateInstance(exampleType);
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

        UltraStateBankEntry FindEntry(string fullKey)
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

        UltraStateBankEntry FindOrCreateEntry(string fullKey)
        {
            var entry = FindEntry(fullKey);
            if (entry == null)
            {
                entry = new UltraStateBankEntry { key = fullKey };
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
                var nodeBuffer = new System.Text.StringBuilder();
                foreach (var node in data.SerializationNodes)
                {
                    nodeBuffer.Append(node.Name).Append('\x1F')
                        .Append((int)node.Entry).Append('\x1F')
                        .Append(node.Data).Append('\x1E');
                }

                payload = System.Text.Encoding.UTF8.GetBytes(nodeBuffer.ToString());
            }

            using var sha = System.Security.Cryptography.SHA256.Create();
            return BitConverter.ToString(sha.ComputeHash(payload)).Replace("-", "", StringComparison.Ordinal);
        }

        #endregion
    }
}
