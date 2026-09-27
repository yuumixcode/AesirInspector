using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// MultiLineProperty 特性的案例 SO。
    /// </summary>
    [AesirExample]
    public class MultiLinePropertyExampleSO : AttributeExampleSO<MultiLinePropertyExampleSO>
    {
        [FoldoutGroup("No Parameters")]
        [MultiLineProperty]
        public string defaultMultiLine = "Line 1\nLine 2\nLine 3";

        [FoldoutGroup("Parameter: Lines")]
        [MultiLineProperty(10)]
        public string tallMultiLine = "This text area spans 10 lines";

        [FoldoutGroup("Comparison With Unity TextArea")]
        [TextArea(4, 10)]
        public string UnityTextAreaField = "";

        [FoldoutGroup("Comparison With Unity Multiline")]
        [Multiline(10)]
        public string UnityMultilineField = "";

        [FoldoutGroup("Combining With HideLabel")]
        [HideLabel]
        [MultiLineProperty(5)]
        public string hiddenLabelMultiLine = "Without label and 5 lines tall";

        [FoldoutGroup("Combining With HideLabel")]
        [HideLabel]
        [MultiLineProperty(10)]
        [Title("Wide Multiline Text Field", null, TitleAlignments.Left, true, false)]
        public string WideMultilineTextField = "";

        [FoldoutGroup("Usage with Properties")]
        [MultiLineProperty(10)]
        [ShowInInspector]
        [InfoBox("Odin supports properties, but Unity's own Multiline attribute only works on fields.")]
        public string OdinMultilineProperty { get; set; }

        public override void AesirInspectorReset()
        {
            defaultMultiLine = "Line 1\nLine 2\nLine 3";
            tallMultiLine = "This text area spans 10 lines";
            hiddenLabelMultiLine = "Without label and 5 lines tall";
            UnityTextAreaField = "";
            UnityMultilineField = "";
            WideMultilineTextField = "";
            OdinMultilineProperty = null;
        }
    }
}
