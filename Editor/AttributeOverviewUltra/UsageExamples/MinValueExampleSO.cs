using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// MinValue 特性案例。
    /// </summary>
    [AesirExample]
    internal class MinValueExampleSO : AttributeExampleSO<MinValueExampleSO>
    {
        [Title("No Parameters")]
        [MinValue(0)]
        public int MinimumZero;

        [Title("No Parameters")]
        [MinValue(0)]
        public float FloatMinValue;

        [Title("No Parameters")]
        [MinValue(0)]
        public Vector3 Vector3MinValue;

        [Title("Member Reference ($)")]
        [MinValue("$DynamicMin")]
        public float DynamicMinimum = 10;

        [Title("Member Reference ($)")]
        public float DynamicMin = 10;

        public override void AesirInspectorReset()
        {
            MinimumZero = 0;
            FloatMinValue = 0;
            Vector3MinValue = Vector3.zero;
            DynamicMinimum = 10;
            DynamicMin = 10;
        }
    }
}
