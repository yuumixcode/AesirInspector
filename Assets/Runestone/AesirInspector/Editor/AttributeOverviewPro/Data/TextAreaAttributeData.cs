using System;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// Unity TextArea 特性的介绍数据。
    /// </summary>
    internal class TextAreaAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("Text Area", "Text Area",
                "TextArea 是 Unity 内置特性，把字符串字段绘制为高度自适应、可滚动的多行文本域。Odin 完整沿用该特性。",
                "TextArea is a Unity built-in attribute that draws a string field as a height-flexible, scrollable multi-line text area. Odin fully supports it.",
                OdinInspectorDocumentationLinks.TextAreaUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("Unity 内置特性，Odin 沿用其绘制行为。",
                "A Unity built-in attribute whose drawing behavior is kept by Odin."),
            new BilingualData("文本域高度随内容自适应，超出最大行数时出现滚动条。",
                "The area grows with the content and scrolls once the maximum line count is exceeded."),
            new BilingualData("只能作用于字段（Unity 限制）。",
                "Can only be applied to fields (a Unity limitation).")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(int).FullName, "minLines",
                new BilingualData("文本域的最小行数，默认 3。",
                    "The minimum number of lines; defaults to 3.")),
            new ParameterValue(typeof(int).FullName, "maxLines",
                new BilingualData("文本域的最大行数，超出后出现滚动条，默认 3。",
                    "The maximum number of lines before a scrollbar appears; defaults to 3."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } =
            Array.Empty<ResolvedStringParameterValue>();

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Text Area",
                TextAreaExampleSO.Instance)
        };
    }
}
