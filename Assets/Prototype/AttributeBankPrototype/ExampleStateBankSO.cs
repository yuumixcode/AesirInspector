using System;
using System.Collections.Generic;
using Sirenix.Serialization;
using UnityEditor;
using UnityEngine;

namespace Runestone.BankPrototype
{
    /// <summary>
    /// 单条示例状态记录：类型名 + Odin 序列化数据（bytes + Unity 对象引用列表）。
    /// SerializationData 由 Unity YAML 序列化，其中 ReferencedUnityObjects 的资产引用
    /// 会被 Unity 自动写成 GUID/fileID —— 与 SerializedScriptableObject 内部机制一致。
    /// </summary>
    [Serializable]
    public class ExampleStateEntry
    {
        public string key;

        public string typeName;

        public string checksum;

        public SerializationData state;
    }

    /// <summary>
    /// 原型银行：一个普通 ScriptableObject 资产，内含所有示例的持久化状态记录。
    /// 示例本体永远是 CreateInstance 的内存实例；用户调试后的状态快照存在这里。
    /// </summary>
    [CreateAssetMenu(fileName = "ExampleStateBank", menuName = "Bank Prototype/Example State Bank")]
    public class ExampleStateBankSO : ScriptableObject
    {
        [SerializeField]
        List<ExampleStateEntry> entries = new List<ExampleStateEntry>();

        public int EntryCount => entries.Count;

        public bool HasState(string key)
        {
            foreach (var e in entries)
            {
                if (e.key == key)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 把内存实例的当前状态快照进银行（全字段：Unity 序列化字段 + Odin 序列化字段）。
        /// </summary>
        public void SaveState(string key, UnityEngine.Object instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            var entry = FindEntry(key);
            if (entry == null)
            {
                entry = new ExampleStateEntry { key = key };
                entries.Add(entry);
            }

            entry.typeName = instance.GetType().FullName;
            entry.state = new SerializationData();
            UnitySerializationUtility.SerializeUnityObject(instance, ref entry.state,
                serializeUnityFields: true);
            entry.checksum = ComputeChecksum(entry.state);
            EditorUtility.SetDirty(this);
        }

        /// <summary>
        /// 从银行恢复状态到已存在的实例（不 new、不 GetUninitializedObject，
        /// 由调用方 CreateInstance 保证 UnityEngine.Object 生命周期正确）。
        /// 校验和/类型名不匹配（数据损坏、版本漂移、跨类型）直接返回 false，
        /// 调用方回落到代码默认值 —— 把 Odin reader 的"半态尽力恢复"挡在门外。
        /// </summary>
        public bool TryRestoreState(string key, UnityEngine.Object instance)
        {
            var entry = FindEntry(key);
            if (entry == null || instance == null)
            {
                return false;
            }

            if (entry.typeName != instance.GetType().FullName)
            {
                Debug.LogWarning($"[BankPrototype] '{key}' 类型不匹配（{entry.typeName} → {instance.GetType().FullName}），回落默认值");
                return false;
            }

            if (entry.checksum != ComputeChecksum(entry.state))
            {
                Debug.LogWarning($"[BankPrototype] '{key}' 校验和不匹配（数据损坏），回落默认值");
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
                Debug.LogWarning($"[BankPrototype] 恢复 '{key}' 失败，回落默认值: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// 恢复或创建：有记录则恢复用户调试状态，无记录则返回全新默认实例。
        /// </summary>
        public ScriptableObject RestoreOrCreate(string key, Type exampleType)
        {
            var instance = ScriptableObject.CreateInstance(exampleType);
            instance.name = key;
            TryRestoreState(key, instance);
            return instance;
        }

        public void RemoveState(string key)
        {
            var entry = FindEntry(key);
            if (entry != null)
            {
                entries.Remove(entry);
                EditorUtility.SetDirty(this);
            }
        }

        public void ClearAll()
        {
            entries.Clear();
            EditorUtility.SetDirty(this);
        }

        /// <summary>
        /// 仅供原型验证使用：往指定条目注入垃圾字节，模拟版本漂移/数据损坏。
        /// </summary>
        internal void CorruptEntryForTest(string key)
        {
            var entry = FindEntry(key);
            if (entry == null)
            {
                return;
            }

            entry.state.SerializedBytes = new byte[] { 0xDE, 0xAD, 0xBE, 0xEF };
            entry.state.SerializedBytesString = null;
        }

        ExampleStateEntry FindEntry(string key)
        {
            foreach (var e in entries)
            {
                if (e.key == key)
                {
                    return e;
                }
            }

            return null;
        }

        /// <summary>
        /// 对条目序列化载荷计算 SHA256 校验和。恢复前比对，把"垃圾字节被尽力解析成半态"
        /// 转为可检测失败。
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
                return string.Empty;
            }

            using var sha = System.Security.Cryptography.SHA256.Create();
            return BitConverter.ToString(sha.ComputeHash(payload)).Replace("-", "", StringComparison.Ordinal);
        }
    }
}
