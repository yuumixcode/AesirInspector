using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// MinMaxSlider 特性案例。
    /// </summary>
    [AesirExample]
    internal class MinMaxSliderExampleSO : AttributeExampleSO<MinMaxSliderExampleSO>
    {
        [Title("No Parameters")]
        [MinMaxSlider(-10f, 10f)]
        public Vector2 MinMaxValueSlider = new Vector2(-7f, -2f);

        [Title("No Parameters")]
        [MinMaxSlider(0, 100)]
        public Vector2Int Vector2IntRange = new Vector2Int(20, 80);

        [Title("Parameter: ShowFields")]
        [MinMaxSlider(-10f, 10f, true)]
        public Vector2 Vector2Range = new Vector2(-2f, 2f);

        [Title("Member Reference ($)")]
        public float DynamicMin;

        [Title("Member Reference ($)")]
        public float DynamicMax = 10;

        [Title("Member Reference ($)")]
        [MinMaxSlider("$DynamicMin", "$DynamicMax", true)]
        public Vector2 DynamicRange = new Vector2(3f, 7f);

        [Title("Member Reference ($)")]
        public Vector2 MinMaxRange = new Vector2(0f, 50f);

        [Title("Member Reference ($)")]
        [MinMaxSlider("MinMaxRange", true)]
        public Vector2 DynamicMinMaxSlider = new Vector2(25f, 50f);

        [Title("Member Reference ($)")]
        [MinMaxSlider("$DynamicMin", 10f, true)]
        public Vector2 MixedRange = new Vector2(2f, 7f);

        [Title("Expression (@)")]
        [MinMaxSlider("@MinMaxRange.x", "@MinMaxRange.y * 10f", true)]
        public Vector2 Expressive = new Vector2(0f, 450f);

        public override void AesirInspectorReset()
        {
            MinMaxValueSlider = new Vector2(-7f, -2f);
            Vector2IntRange = new Vector2Int(20, 80);
            Vector2Range = new Vector2(-2f, 2f);
            DynamicMin = 0;
            DynamicMax = 10;
            DynamicRange = new Vector2(3f, 7f);
            MinMaxRange = new Vector2(0f, 50f);
            DynamicMinMaxSlider = new Vector2(25f, 50f);
            MixedRange = new Vector2(2f, 7f);
            Expressive = new Vector2(0f, 450f);
        }
    }
}
