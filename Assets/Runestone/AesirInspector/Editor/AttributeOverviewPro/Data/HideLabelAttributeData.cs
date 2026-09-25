namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// HideLabel 特性的介绍数据。
    /// </summary>
    internal class HideLabelAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("HideLabel", "HideLabel",
                "HideLabel 特性用于隐藏属性在 Inspector 中默认显示的标签（Label）。",
                "The HideLabel attribute is used to hide the default label of a property in the Inspector.",
                OdinInspectorDocumentationLinks.HideLabelUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("隐藏标签的同时移除标签占用的水平空间，使输入框占满整行。",
                "Hides the label and removes the horizontal space it occupied, letting the input field span the full row."),
            new BilingualData("常用于 HorizontalGroup 中，让控件占满所在列或整行的宽度。",
                "Commonly used in HorizontalGroups so controls fill the full width of their column or row."),
            new BilingualData("适合自身已具备清晰视觉含义的属性，如颜色块、预览图等。",
                "Ideal for properties that already convey clear visual meaning, such as color blocks or previews.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } = { };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = { };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                HideLabelExampleSO.Instance)
        };
    }
}
