namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// VerticalGroup 特性的介绍数据。
    /// </summary>
    internal class VerticalGroupAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("VerticalGroup", "VerticalGroup", "VerticalGroup 特性用于将多个属性垂直排列在一个组中。",
                "The VerticalGroup attribute is used to group multiple properties vertically.",
                OdinInspectorDocumentationLinks.VerticalGroupUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("将多个属性在检查器中垂直堆叠为一组；单独使用作用有限，主要用于把 HorizontalGroup 的水平区域拆成多列。",
                "Stacks multiple properties vertically in the inspector; on its own it does little, and it is mainly used to split a HorizontalGroup into columns."),
            new BilingualData("组名支持路径嵌套（如 \"Split/Left\"），可据此在同一 HorizontalGroup 下建立多列。",
                "The group name supports path nesting (e.g. \"Split/Left\"), which is how several columns are created under one HorizontalGroup."),
            new BilingualData("可用 PaddingTop / PaddingBottom 调整组的上下边距；在 TableList 中也可用它把多个成员合并到同一列。",
                "PaddingTop and PaddingBottom adjust the group's top and bottom spacing, and inside a TableList the group can merge several members into a single column.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(float).FullName, "PaddingTop",
                new BilingualData("顶边距。", "The top padding of the group.")),
            new ParameterValue(typeof(float).FullName, "PaddingBottom",
                new BilingualData("底边距。", "The bottom padding of the group."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = { };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                VerticalGroupExampleSO.Instance)
        };
    }
}
