namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// TableList 特性的介绍数据。
    /// </summary>
    internal class TableListAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("TableList", "TableList", "TableList 特性将列表或数组绘制为一个表格，每个元素的字段对应表格的列。",
                "The TableList attribute draws a list or array as a table, where each field of the element corresponds to a column.",
                OdinInspectorDocumentationLinks.TableListUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("将列表或数组的每个元素绘制为表格的一行，元素成员成为列，适合展示含多个成员的结构体或类列表。",
                "Draws each element of a list or array as a table row with its members as columns, ideal for lists of structs or classes with several members."),
            new BilingualData("元素成员可用 [VerticalGroup] 合并进同一列、用 [TableColumnWidth] 调整列宽，带 [HideInTables] 的成员不会成为列。",
                "Element members can be merged into one column with [VerticalGroup] and sized with [TableColumnWidth], while members marked [HideInTables] are excluded from the table."),
            new BilingualData("IsReadOnly 只移除增删、拖拽等列表操作，单元格仍可编辑；这与 [ReadOnly] 会禁用整个属性的 GUI 不同。",
                "IsReadOnly only removes list operations such as add, remove and drag, leaving cells editable, unlike [ReadOnly] which disables all GUI for the property."),
            new BilingualData("元素成员上的其他特性（如 [Button]、[PreviewField]、[TextArea]）在单元格内仍会正常绘制。",
                "Other attributes on element members, such as [Button], [PreviewField] and [TextArea], are still drawn normally inside the table cells.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(int).FullName, "NumberOfItemsPerPage",
                new BilingualData("每页显示的元素数量。", "The number of items to show per page.")),
            new ParameterValue(typeof(bool).FullName, "ShowPaging",
                new BilingualData("是否启用分页。", "Whether to enable paging.")),
            new ParameterValue(typeof(bool).FullName, "DrawScrollView",
                new BilingualData("是否使用滚动视图绘制表格。", "Whether to draw the table inside a scroll view.")),
            new ParameterValue(typeof(int).FullName, "MaxScrollViewHeight",
                new BilingualData("滚动视图的最大高度。", "The maximum height of the scroll view.")),
            new ParameterValue(typeof(bool).FullName, "HideToolbar",
                new BilingualData("是否隐藏表格顶部的工具栏（包含添加按钮和搜索框）。",
                    "Whether to hide the toolbar at the top of the table.")),
            new ParameterValue(typeof(bool).FullName, "AlwaysExpanded",
                new BilingualData("表格是否始终处于展开状态。", "Whether the table should always be expanded."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = { };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                TableListExampleSO.Instance)
        };
    }
}
