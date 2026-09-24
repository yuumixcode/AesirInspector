using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// Unity Range 特性的案例 SO。
    /// </summary>
    [AesirExample]
    internal class RangeExampleSO : AttributeExampleSO<RangeExampleSO>
    {
        [Title("Parameter: min, max (float)")]
        [Range(0f, 10f)]
        public float floatRange = 5f;

        [Title("Parameter: min, max (int)")]
        [Range(0, 100)]
        public int intRange = 50;

        [Title("Parameter: min, max (normalized)")]
        [Range(0f, 1f)]
        public float normalizedRange = 0.5f;

        public override void AesirInspectorReset()
        {
            floatRange = 5f;
            intRange = 50;
            normalizedRange = 0.5f;
        }
    }
}
