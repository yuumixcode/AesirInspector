using System;
using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// HorizontalGroup 特性的案例 SO：默认横向组，以及与其他组（BoxGroup）嵌套。
    /// </summary>
    [AesirExample]
    public class HorizontalGroupExampleSO : AttributeExampleSO<HorizontalGroupExampleSO>
    {
        [HorizontalGroup]
        public SomeFieldType Left1;

        [HorizontalGroup]
        public SomeFieldType Right1;

        [HorizontalGroup("Split", 0.5f)]
        [BoxGroup("Split/Left")]
        public int left;

        [BoxGroup("Split/Right")]
        public int right;

        public override void AesirInspectorReset()
        {
            Left1 = default;
            Right1 = default;
            left = 0;
            right = 0;
        }

        [Serializable]
        [HideLabel]
        public struct SomeFieldType
        {
            [ListDrawerSettings(ShowIndexLabels = true)]
            [LabelText("@$property.Parent.NiceName")]
            public float[] x;
        }
    }
}
