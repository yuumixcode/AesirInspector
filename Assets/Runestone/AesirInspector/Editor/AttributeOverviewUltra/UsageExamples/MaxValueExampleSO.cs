using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// MaxValue 特性案例。
    /// </summary>
    [AesirExample]
    internal class MaxValueExampleSO : AttributeExampleSO<MaxValueExampleSO>
    {
        [Title("No Parameters")]
        [MaxValue(100)]
        public int MaximumHundred = 100;

        [Title("No Parameters")]
        [MaxValue(0)]
        public float FloatMaxValue;

        [Title("No Parameters")]
        [MaxValue(0)]
        public Vector3 Vector3MaxValue;

        [Title("Member Reference ($)")]
        [MaxValue("$DynamicMax")]
        public float DynamicMaximum = 50;

        [Title("Member Reference ($)")]
        public float DynamicMax = 50;

        public override void AesirInspectorReset()
        {
            MaximumHundred = 100;
            FloatMaxValue = 0;
            Vector3MaxValue = Vector3.zero;
            DynamicMaximum = 50;
            DynamicMax = 50;
        }
    }
}
