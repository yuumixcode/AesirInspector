using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.BankPrototype
{
    /// <summary>
    /// 原型示例 A：Unity 原生序列化的示例 SO（模拟 AttributeExampleSO 路线）。
    /// 覆盖风险点：普通字段 + AnimationCurve/Gradient + Material 资产引用 + List + Button。
    /// </summary>
    public class ProtoUnityExampleSO : ScriptableObject
    {
        public string dynamicButtonName = "Click Me!";

        public int tweakValue = 42;

        public List<string> notes = new List<string> { "default note" };

        public AnimationCurve curve = AnimationCurve.Linear(0, 0, 1, 1);

        [SerializeField]
        Gradient colorGradient = new Gradient();

        [SerializeField]
        Material referencedMaterial;

        public Gradient ColorGradient => colorGradient;

        public Material ReferencedMaterial => referencedMaterial;

        [Button("$dynamicButtonName")]
        void ClickMe() => Debug.Log($"[ProtoUnityExample] Clicked! tweakValue={tweakValue}");

        [Button("Add Note")]
        void AddNote() => notes.Add($"note {notes.Count}");
    }
}
