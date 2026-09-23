using System;
using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// HorizontalGroup 特性的案例 SO。
    /// </summary>
    [AesirExample]
    public class HorizontalGroupExampleSO : AttributeExampleSO<HorizontalGroupExampleSO>
    {
        [Serializable]
        [HideLabel]
        public struct SomeFieldType
        {
            [ListDrawerSettings(ShowIndexLabels = true)]
            [LabelText("@$property.Parent.NiceName")]
            public float[] x;
        }

        [Title("No Parameters")]
        [HorizontalGroup]
        public SomeFieldType Left1;

        [HorizontalGroup]
        public SomeFieldType Right1;

        [Title("Parameter: Width")]
        [HorizontalGroup("row1", Width = 0.25f)]
        public SomeFieldType Left3;

        [HorizontalGroup("row1", Width = 150f)]
        public SomeFieldType Center3;

        [HorizontalGroup("row1")]
        public SomeFieldType Right3;

        [HorizontalGroup("Split", 0.5f)]
        [BoxGroup("Split/Left")]
        public int left;

        [BoxGroup("Split/Right")]
        public int right;

        [HorizontalGroup("Fixed", Width = 100)]
        public int fixedWidth;

        [HorizontalGroup("Fixed")]
        public int flexibleWidth;

        [Title("Parameter: MarginRight")]
        [HorizontalGroup("row2", MarginRight = 0.4f)]
        public SomeFieldType Left2;

        [HorizontalGroup("row2")]
        public SomeFieldType Right2;

        [Title("Parameter: Gap")]
        [HorizontalGroup("row3", Gap = 3f)]
        public SomeFieldType Left4;

        [HorizontalGroup("row3")]
        public SomeFieldType Center4;

        [HorizontalGroup("row3")]
        public SomeFieldType Right4;

        [HorizontalGroup("Gap", Gap = 20)]
        public int gap1;

        [HorizontalGroup("Gap")]
        public int gap2;

        [Title("Parameter: Title")]
        [HorizontalGroup("row4", Title = "Horizontal Group Title")]
        public SomeFieldType Left5;

        [HorizontalGroup("row4")]
        public SomeFieldType Center5;

        [HorizontalGroup("row4")]
        public SomeFieldType Right5;

        public override void AesirInspectorReset()
        {
            Left1 = default;
            Right1 = default;
            Left3 = default;
            Center3 = default;
            Right3 = default;
            left = 0;
            right = 0;
            fixedWidth = 0;
            flexibleWidth = 0;
            Left2 = default;
            Right2 = default;
            Left4 = default;
            Center4 = default;
            Right4 = default;
            gap1 = 0;
            gap2 = 0;
            Left5 = default;
            Center5 = default;
            Right5 = default;
        }
    }
}
