using System;
using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// BoxGroup 特性的案例 SO：基础参数（标题、显示标签、居中标题、LabelText、结构体装箱）。
    /// </summary>
    [AesirExample]
    public class BoxGroupExampleSO : AttributeExampleSO<BoxGroupExampleSO>
    {
        [BoxGroup("Some Title")]
        public string A;

        [BoxGroup("Some Title")]
        public string B;

        [BoxGroup("NoTitle", false)]
        public string I;

        [BoxGroup("NoTitle")]
        public string J;

        [BoxGroup("Centered Title", centerLabel: true)]
        public string C;

        [BoxGroup("Centered Title")]
        public string D;

        [BoxGroup("Custom Title", LabelText = "This is a Box Group")]
        public int labelTextBox;

        [BoxGroup("A Struct In A Box")]
        [HideLabel]
        public SomeStruct BoxedStruct;

        public SomeStruct DefaultStruct;

        public override void AesirInspectorReset()
        {
            A = string.Empty;
            B = string.Empty;
            I = string.Empty;
            J = string.Empty;
            C = string.Empty;
            D = string.Empty;
            labelTextBox = 0;
            BoxedStruct = default;
            DefaultStruct = default;
        }

        [Serializable]
        public struct SomeStruct
        {
            public int One;

            public int Two;

            public int Three;
        }
    }
}
