using System;
using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// BoxGroup 特性的案例 SO。
    /// </summary>
    [AesirExample]
    public class BoxGroupExampleSO : AttributeExampleSO<BoxGroupExampleSO>
    {
        [Serializable]
        public struct SomeStruct
        {
            public int One;

            public int Two;

            public int Three;
        }

        [Title("No Parameters")]
        [BoxGroup("Some Title")]
        public string A;

        [BoxGroup("Some Title")]
        public string B;

        [Title("Parameter: ShowLabel, CenterLabel")]
        [BoxGroup("NoTitle", false)]
        public string I;

        [BoxGroup("NoTitle")]
        public string J;

        [BoxGroup("Centered Title", centerLabel: true)]
        public string C;

        [BoxGroup("Centered Title")]
        public string D;

        [Title("Parameter: LabelText")]
        [BoxGroup("Custom Title", LabelText = "This is a Box Group")]
        public int labelTextBox;

        [Title("Member Reference ($)")]
        [BoxGroup("$G")]
        public string E = "Dynamic box title 2";

        [BoxGroup("$G")]
        public string F;

        [BoxGroup]
        public string G = "Dynamic Box Title";

        [BoxGroup]
        public string H;

        [Title("Combining With Other Attributes")]
        [Button(ButtonSizes.Large)]
        [HorizontalGroup("Buttons in Boxes")]
        [BoxGroup("Buttons in Boxes/One")]
        void Button1() { }

        [Button(ButtonSizes.Large)]
        [HorizontalGroup("Buttons in Boxes")]
        [BoxGroup("Buttons in Boxes/Two")]
        void Button2() { }

        [HorizontalGroup("Buttons in Boxes", Width = 60f)]
        [BoxGroup("Buttons in Boxes/Double")]
        [Button]
        void Accept() { }

        [Button]
        [BoxGroup("Buttons in Boxes/Double")]
        void Cancel() { }

        [BoxGroup("A Struct In A Box")]
        [HideLabel]
        public SomeStruct BoxedStruct;

        public SomeStruct DefaultStruct;

        public override void AesirInspectorReset()
        {
            A = string.Empty;
            B = string.Empty;
            C = string.Empty;
            D = string.Empty;
            E = "Dynamic box title 2";
            F = string.Empty;
            G = "Dynamic Box Title";
            H = string.Empty;
            I = string.Empty;
            J = string.Empty;
            labelTextBox = 0;
            BoxedStruct = default;
            DefaultStruct = default;
        }
    }
}
