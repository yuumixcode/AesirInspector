using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// ProgressBar 特性案例。
    /// </summary>
    [AesirExample]
    internal class ProgressBarExampleSO : AttributeExampleSO<ProgressBarExampleSO>
    {
        [FoldoutGroup("No Parameters")]
        [ProgressBar(0, 100)]
        public int BasicProgressBar = 50;

        [FoldoutGroup("Parameter: R, G, B, Height")]
        [ProgressBar(0.0, 100.0)]
        public int ColoredBar = 50;

        [FoldoutGroup("Parameter: R, G, B, Height")]
        [ProgressBar(-100.0, 100.0, 1f, 1f, 1f, Height = 30)]
        [HideLabel]
        public short BigColoredProgressBar = 50;

        [FoldoutGroup("Parameter: R, G, B, Height")]
        [ProgressBar(-100, 100, Height = 30, ColorGetter = "CustomColor")]
        public int LargeProgressBar;

        [FoldoutGroup("Parameter: Segmented")]
        [ProgressBar(0.0, 10.0, 0f, 1f, 0f, Segmented = true)]
        public int SegmentedColoredBar = 5;

        [FoldoutGroup("Parameter: ColorGetter")]
        [ProgressBar(0, 1, ColorGetter = "GetColor")]
        public float ColoredProgressBar = 0.5f;

        [FoldoutGroup("Parameter: ColorGetter")]
        [ProgressBar(0.0, 100.0, ColorGetter = "GetHealthBarColor")]
        public float DynamicHealthBarColor = 50f;

        [FoldoutGroup("Member Reference ($)")]
        [ProgressBar(0, "$MaxHealth", ColorGetter = "GetHealthColor")]
        public float HealthBar = 80;

        [FoldoutGroup("Member Reference ($)")]
        public float MaxHealth = 100;

        [FoldoutGroup("Member Reference ($)")]
        [ProgressBar("Min", "Max")]
        public float DynamicProgressBar = 50f;

        [FoldoutGroup("Member Reference ($)")]
        public float Min;

        [FoldoutGroup("Member Reference ($)")]
        public float Max = 100f;

        [FoldoutGroup("Parameter: BackgroundColorGetter, DrawValueLabel")]
        [Range(0f, 300f)]
        [HideLabel]
        public float StackedHealth = 150f;

        [FoldoutGroup("Parameter: R, G, B, Height")]
        Color CustomColor = new Color(0.2f, 0.6f, 1f);

        [FoldoutGroup("Parameter: BackgroundColorGetter, DrawValueLabel")]
        [ProgressBar(0.0, 100.0, ColorGetter = "GetStackedHealthColor",
            BackgroundColorGetter = "GetStackHealthBackgroundColor", DrawValueLabel = false)]
        [ShowInInspector]
        [HideLabel]
        float StackedHealthProgressBar => StackedHealth % 100.01f;

        Color GetColor() => Color.Lerp(Color.red, Color.green, ColoredProgressBar);

        Color GetHealthColor() => Color.Lerp(Color.red, Color.green, HealthBar / MaxHealth);

        Color GetHealthBarColor(float value) =>
            Color.Lerp(Color.red, Color.green, Mathf.Pow(value / 100f, 2f));

        Color GetStackedHealthColor()
        {
            if (StackedHealth > 200f)
            {
                return Color.white;
            }

            if (StackedHealth > 100f)
            {
                return Color.green;
            }

            return Color.red;
        }

        Color GetStackHealthBackgroundColor()
        {
            if (StackedHealth > 200f)
            {
                return Color.green;
            }

            if (StackedHealth > 100f)
            {
                return Color.red;
            }

            return new Color(0.16f, 0.16f, 0.16f, 1f);
        }

        public override void AesirInspectorReset()
        {
            BasicProgressBar = 50;
            ColoredBar = 50;
            BigColoredProgressBar = 50;
            LargeProgressBar = 0;
            CustomColor = new Color(0.2f, 0.6f, 1f);
            SegmentedColoredBar = 5;
            ColoredProgressBar = 0.5f;
            DynamicHealthBarColor = 50f;
            HealthBar = 80;
            MaxHealth = 100;
            DynamicProgressBar = 50f;
            Min = 0;
            Max = 100f;
            StackedHealth = 150f;
        }
    }
}
