using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// DelayedProperty 特性的案例 SO。
    /// </summary>
    [AesirExample]
    public class DelayedPropertyExampleSO : AttributeExampleSO<DelayedPropertyExampleSO>
    {
        [Title("No Parameters")]
        [DelayedProperty]
        [OnValueChanged("OnValueChanged")]
        public int delayedInt;

        [Title("No Parameters")]
        [ShowInInspector]
        [OnValueChanged("OnValueChanged")]
        [DelayedProperty]
        public string DelayedProperty { get; set; }

        [Title("Comparison With Immediate Update")]
        [OnValueChanged("OnValueChanged")]
        public int normalInt;

        [Title("Comparison With Unity Delayed")]
        [OnValueChanged("OnValueChanged")]
        [Delayed]
        public int DelayedField;

        void OnValueChanged()
        {
            Debug.Log("Value changed!");
        }

        public override void AesirInspectorReset()
        {
            delayedInt = 0;
            DelayedProperty = null;
            normalInt = 0;
            DelayedField = 0;
        }
    }
}
