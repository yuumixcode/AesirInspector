using System;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// Unity Multiline 特性的介绍数据。
    /// </summary>
    internal class MultilineAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("Multiline", "Multiline",
                "Multiline 是 Unity 内置特性，让字符串字段以多行文本框绘制。Odin 完整沿用该特性，并可通过参数指定显示行数。",
                "Multiline is a Unity built-in attribute that draws a string field as a multi-line text box. Odin fully supports it and lets you specify the visible line count.",
                OdinInspectorDocumentationLinks.MultilineUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("Unity 内置特性，Odin 沿用其多行文本框绘制行为。",
                "A Unity built-in attribute whose multi-line text box drawing is kept by Odin."),
            new BilingualData("只能作用于字段（Unity 限制），若需作用于属性请使用 Odin 的 MultiLineProperty。",
                "Can only be applied to fields (a Unity limitation); use Odin's MultiLineProperty for properties."),
            new BilingualData("始终占用固定行数，不随内容伸缩，文本超出时显示滚动条。",
                "Always occupies a fixed number of lines and never expands or contracts with the content; a scrollbar appears when the text does not fit.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(int).FullName, "lines",
                new BilingualData("文本框显示的行数，默认 3。",
                    "The number of lines shown in the text box; defaults to 3."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } =
            Array.Empty<ResolvedStringParameterValue>();

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Multiline",
                MultilineExampleSO.Instance)
        };
    }
}
