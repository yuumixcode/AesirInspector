using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// Unity TextArea 特性的案例 SO。
    /// </summary>
    [AesirExample]
    internal class TextAreaExampleSO : AttributeExampleSO<TextAreaExampleSO>
    {
        [Title("No Parameters")]
        [TextArea]
        public string defaultTextArea =
            "TextArea 绘制高度自适应、可滚动的多行文本域。\nThe text area grows with its content and scrolls when needed.";

        [Title("Parameter: minLines, maxLines")]
        [TextArea(4, 10)]
        public string sizedTextArea =
            "该文本域最少 4 行、最多 10 行。\nThis area shows at least four lines and at most ten.";

        public override void AesirInspectorReset()
        {
            defaultTextArea =
                "TextArea 绘制高度自适应、可滚动的多行文本域。\nThe text area grows with its content and scrolls when needed.";
            sizedTextArea =
                "该文本域最少 4 行、最多 10 行。\nThis area shows at least four lines and at most ten.";
        }
    }
}
