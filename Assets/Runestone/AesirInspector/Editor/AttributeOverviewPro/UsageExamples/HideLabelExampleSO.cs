using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// HideLabel 特性的案例 SO。
    /// </summary>
    [AesirExample]
    public class HideLabelExampleSO : AttributeExampleSO<HideLabelExampleSO>
    {
        [FoldoutGroup("Standard Property")]
        public int normalProperty;

        [FoldoutGroup("No Parameters")]
        [HideLabel]
        public int hiddenLabelProperty;

        [FoldoutGroup("Combining With HorizontalGroup")]
        [HorizontalGroup("Combining With HorizontalGroup/Group")]
        [HideLabel]
        public int a;

        [FoldoutGroup("Combining With HorizontalGroup")]
        [HorizontalGroup("Combining With HorizontalGroup/Group")]
        [HideLabel]
        public int b;

        [FoldoutGroup("Combining With ColorPalette")]
        [Title("Wide Colors", null, TitleAlignments.Left, true, true)]
        [HideLabel]
        [ColorPalette("Fall")]
        public Color WideColor1 = new Color(0.85f, 0.44f, 0.22f);

        [FoldoutGroup("Combining With ColorPalette")]
        [HideLabel]
        [ColorPalette("Fall")]
        public Color WideColor2 = new Color(0.72f, 0.16f, 0.16f);

        [FoldoutGroup("Combining With Title")]
        [HideLabel]
        [Title("Wide Vector", null, TitleAlignments.Left, true, true)]
        public Vector3 WideVector1 = new Vector3(1f, 2f, 3f);

        [FoldoutGroup("Combining With Title")]
        [HideLabel]
        public Vector4 WideVector2 = new Vector4(1f, 2f, 3f, 4f);

        [FoldoutGroup("Combining With Title")]
        [HideLabel]
        [Title("Wide String", null, TitleAlignments.Left, true, true)]
        public string WideString = "This string field has no label and spans the full width";

        [FoldoutGroup("Combining With MultiLineProperty")]
        [Title("Wide Multiline Text Field", null, TitleAlignments.Left, true, true)]
        [HideLabel]
        [MultiLineProperty(3)]
        public string WideMultilineTextField = "";

        public override void AesirInspectorReset()
        {
            normalProperty = 0;
            hiddenLabelProperty = 0;
            a = 0;
            b = 0;
            WideColor1 = new Color(0.85f, 0.44f, 0.22f);
            WideColor2 = new Color(0.72f, 0.16f, 0.16f);
            WideVector1 = new Vector3(1f, 2f, 3f);
            WideVector2 = new Vector4(1f, 2f, 3f, 4f);
            WideString = "This string field has no label and spans the full width";
            WideMultilineTextField = "";
        }
    }
}
