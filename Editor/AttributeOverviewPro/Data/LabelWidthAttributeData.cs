namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// LabelWidth 特性的介绍数据。
    /// </summary>
    internal class LabelWidthAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("LabelWidth", "LabelWidth", "LabelWidth 特性用于自定义属性标签的宽度。",
                "The LabelWidth attribute is used to customize the width of property labels.",
                OdinInspectorDocumentationLinks.LabelWidthUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("用于自定义属性标签的宽度，常用于对齐多个属性的输入框。",
                "Customizes the width of a property label, commonly used to align the input fields of multiple properties."),
            new BilingualData("正数表示标签的绝对像素宽度；负数表示在当前宽度基础上减少的量（相对宽度）。",
                "A positive value is an absolute pixel width; a negative value is an offset subtracted from the current width (relative width)."),
            new BilingualData("会影响当前属性及其子属性的标签宽度。",
                "Affects the label width of the current property and its child properties.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(float).FullName, "width",
                new BilingualData("标签宽度：正数为绝对像素值，负数为在当前标签宽度基础上的相对增减（例如 -50 表示减少 50 像素）。",
                    "The label width: a positive value is an absolute pixel width, while a negative value is a relative change from the current label width (for example -50 reduces it by 50 pixels)."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = { };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                LabelWidthExampleSO.Instance)
        };
    }
}
