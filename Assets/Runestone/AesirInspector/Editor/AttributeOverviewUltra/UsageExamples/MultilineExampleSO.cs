using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// Unity Multiline 特性的案例 SO。
    /// </summary>
    [AesirExample]
    internal class MultilineExampleSO : AttributeExampleSO<MultilineExampleSO>
    {
        [Title("No Parameters")]
        [Multiline]
        public string defaultMultiline =
            "Multiline 让字符串以多行文本框绘制。\nThe string is drawn as a multi-line text box.";

        [Title("Parameter: lines")]
        [Multiline(6)]
        public string sixLines = "该字段预留 6 行高度。\nThis field reserves six lines of height.";

        public override void AesirInspectorReset()
        {
            defaultMultiline = "Multiline 让字符串以多行文本框绘制。\nThe string is drawn as a multi-line text box.";
            sixLines = "该字段预留 6 行高度。\nThis field reserves six lines of height.";
        }
    }
}
