namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// HorizontalGroup 特性的介绍数据。
    /// </summary>
    internal class HorizontalGroupAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("HorizontalGroup", "HorizontalGroup",
                "HorizontalGroup 特性用于将多个属性水平排列在同一行中。",
                "The HorizontalGroup attribute is used to group multiple properties horizontally in a single row.",
                OdinInspectorDocumentationLinks.HorizontalGroupUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("组名相同即归入同一水平组；不指定组名时使用默认组。",
                "Properties with the same group name join the same horizontal group; omitting the name uses the default group."),
            new BilingualData("Width 为 0 时自动分配宽度，比例值（0 到 1）按百分比处理，更大的值按像素处理。",
                "A Width of 0 auto-sizes the column, proportional values (0 to 1) are treated as a percentage, and larger values as pixels."),
            new BilingualData("可与 BoxGroup、VerticalGroup 等其他组特性嵌套，实现更复杂的布局。",
                "Can be nested with other group attributes such as BoxGroup and VerticalGroup for more complex layouts."),
            new BilingualData("水平组中的标签会挤压输入框，通常配合 LabelWidth 或 HideLabel 使用。",
                "Labels in a horizontal group squeeze the input fields, so it is usually combined with LabelWidth or HideLabel.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(float).FullName, "Width",
                new BilingualData("组的宽度。如果小于等于 1，则视为比例；如果大于 1，则视为像素。",
                    "The width of the group. If 1 or less, it's proportional; if greater than 1, it's pixels.")),
            new ParameterValue(typeof(int).FullName, "Gap",
                new BilingualData("组内成员之间的间距（像素）。", "The spacing between members in the group (in pixels).")),
            new ParameterValue(typeof(float).FullName, "MarginLeft",
                new BilingualData("组的左边距。", "The left margin of the group.")),
            new ParameterValue(typeof(float).FullName, "MarginRight",
                new BilingualData("组的右边距。", "The right margin of the group.")),
            new ParameterValue(typeof(string).FullName, "Title",
                new BilingualData("组的标题。", "The title of the group."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = { };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                HorizontalGroupExampleSO.Instance)
        };
    }
}
