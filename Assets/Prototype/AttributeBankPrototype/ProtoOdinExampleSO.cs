using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.BankPrototype
{
    /// <summary>
    /// 原型示例 B：Odin 序列化的示例 SO（模拟 OdinAttributeExampleSO 路线）。
    /// 覆盖风险点：字典等 Odin-only 类型 + Material 引用藏在 Odin 序列化字段内 + ShowInInspector。
    /// </summary>
    public class ProtoOdinExampleSO : SerializedScriptableObject
    {
        public Dictionary<string, int> statDictionary = new Dictionary<string, int>
        {
            { "attack", 10 },
            { "defense", 5 }
        };

        public Dictionary<string, Material> materialLibrary = new Dictionary<string, Material>();

        [ShowInInspector]
        public string ReadOnlySummary => $"dict count = {statDictionary?.Count ?? -1}";

        [Button("Add Stat")]
        void AddStat() => statDictionary[$"stat_{statDictionary.Count}"] = Random.Range(1, 100);
    }
}
