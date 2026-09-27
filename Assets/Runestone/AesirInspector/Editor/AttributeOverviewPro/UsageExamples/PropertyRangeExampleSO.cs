using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// PropertyRange 特性案例。
    /// </summary>
    [AesirExample]
    internal class PropertyRangeExampleSO : AttributeExampleSO<PropertyRangeExampleSO>
    {
        [Title("No Parameters")]
        [PropertyRange(0, 100)]
        public int StaticRange = 50;

        [Title("Member Reference ($)")]
        [PropertyRange("$Min", "$Max")]
        public float DynamicRange = 5;

        [Title("Member Reference ($)")]
        public float Min;

        [Title("Member Reference ($)")]
        public float Max = 10;

        [Title("Combining With Range Attribute")]
        [Range(0f, 10f)]
        public int unityRangeField = 2;

        [Title("No Parameters")]
        [PropertyRange(0.0, 10.0)]
        [ShowInInspector]
        public int RangeProperty { get; set; }

        public override void AesirInspectorReset()
        {
            StaticRange = 50;
            RangeProperty = 0;
            DynamicRange = 5;
            Min = 0;
            Max = 10;
            unityRangeField = 2;
        }
    }
}
