namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// BoxGroup 特性的介绍数据。
    /// </summary>
    internal class BoxGroupAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("BoxGroup", "BoxGroup", "BoxGroup 特性用于将多个属性包裹在一个带有边框和可选标题的盒子中。",
                "The BoxGroup attribute is used to group multiple properties inside a box with a border and an optional title.",
                OdinInspectorDocumentationLinks.BoxGroupUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("通过组名把属性归入同一个盒子；组名包含路径（如 \"Parent/Child\"）时会创建嵌套组。", "Groups properties into a box by group name; path-based names (e.g. \"Parent/Child\") create nested groups."),
            new BilingualData("不指定组名时使用默认组 \"_DefaultBoxGroup\" 且不显示标题；组名支持 $ 成员引用与 @ 表达式。", "When no group name is given, the default group \"_DefaultBoxGroup\" is used and no title is shown; group names support $ member references and @ expressions."),
            new BilingualData("showLabel 控制是否显示标题（带组名的构造函数默认 true），centerLabel 使标题居中，LabelText 可自定义标题文本。", "showLabel controls whether the title is drawn (true by default in the constructor taking a group name), centerLabel centers it, and LabelText overrides the title text.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(bool).FullName, "showLabel",
                new BilingualData("是否显示组标题。默认值为 true。", "Whether to show the group label. Default is true.")),
            new ParameterValue(typeof(bool).FullName, "centerLabel",
                new BilingualData("标题是否居中显示。", "Whether to center the label.")),
            new ParameterValue(typeof(string).FullName, "LabelText",
                new BilingualData("自定义显示的标题文本。", "Custom label text to display."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = { };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                BoxGroupExampleSO.Instance)
        };
    }
}
