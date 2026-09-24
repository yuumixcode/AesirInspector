using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.BankPrototype
{
    /// <summary>
    /// 原型示例 C：ButtonExampleSO 的最小复刻（$ 引用按钮名，真实用法）。
    /// </summary>
    public class ProtoButtonExampleSO : ScriptableObject
    {
        public string dynamicButtonName = "Click Me!";

        [Button("$dynamicButtonName")]
        void DynamicButton() => Debug.Log("[ProtoButtonExample] Dynamic button clicked");
    }
}
