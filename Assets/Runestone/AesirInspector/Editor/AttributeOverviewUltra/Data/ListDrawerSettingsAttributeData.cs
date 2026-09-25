namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// ListDrawerSettings 特性的介绍数据。
    /// </summary>
    internal class ListDrawerSettingsAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("ListDrawerSettings", "ListDrawerSettings",
                "ListDrawerSettings 特性用于自定义列表或数组在 Inspector 中的绘制方式。",
                "The ListDrawerSettings attribute is used to customize how lists or arrays are drawn in the Inspector.",
                OdinInspectorDocumentationLinks.ListDrawerSettingsUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("IsReadOnly 只移除增删与拖拽等编辑能力，不像 [ReadOnly] 那样禁用每个元素的 GUI。",
                "IsReadOnly only removes editing capabilities such as add, remove and drag; unlike [ReadOnly], it does not disable the GUI of each element."),
            new BilingualData("分页（NumberOfItemsPerPage）、拖拽（DraggableItems）等默认行为取自 Odin 偏好设置，可在此显式覆盖。",
                "Defaults such as paging (NumberOfItemsPerPage) and dragging (DraggableItems) come from Odin preferences and can be explicitly overridden here."),
            new BilingualData("ListElementLabelName 指定元素内某个成员作为该元素的标签，便于识别条目。",
                "ListElementLabelName selects a member inside each element as its label, making entries easier to identify."),
            new BilingualData(
                "可通过 CustomAddFunction、OnTitleBarGUI、OnBeginListElementGUI / OnEndListElementGUI 等回调注入自定义增删与绘制逻辑。",
                "Callbacks such as CustomAddFunction, OnTitleBarGUI and OnBeginListElementGUI / OnEndListElementGUI let you inject custom add/remove and drawing logic.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(bool).FullName, "IsReadOnly",
                new BilingualData("是否为只读模式。", "Whether the list is read-only.")),
            new ParameterValue(typeof(bool).FullName, "ShowFoldout",
                new BilingualData("是否显示折叠箭头。", "Whether to show the foldout arrow.")),
            new ParameterValue(typeof(bool).FullName, "ShowIndexLabels",
                new BilingualData("是否显示元素序号。", "Whether to show index labels for elements.")),
            new ParameterValue(typeof(string).FullName, "ListElementLabelName",
                new BilingualData("作为元素标签的成员名。", "The name of the member to use as the element label.")),
            new ParameterValue(typeof(int).FullName, "NumberOfItemsPerPage",
                new BilingualData("每页显示的元素数量。", "The number of items to show per page.")),
            new ParameterValue(typeof(bool).FullName, "DraggableItems",
                new BilingualData("是否允许拖拽排序。", "Whether items can be reordered by dragging.")),
            new ParameterValue(typeof(string).FullName, "ElementColor",
                new BilingualData("元素的背景颜色。", "The background color of the elements."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = { };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                ListDrawerSettingsExampleSO.Instance)
        };
    }
}
