namespace Runestone.AesirInspector.Editor
{
    internal class TableColumnWidthAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("TableColumnWidth", "TableColumnWidth",
                "TableColumnWidth 特性用于设置 TableList 特性标记的 List 的元素宽度。",
                "The TableColumnWidth attribute sets the width of elements in a TableList-marked list.",
                "https://odininspector.com/attributes/table-column-width-attribute");

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("仅对 [TableList] 列表生效：把列表元素封装为类，在元素内部的字段上标记本特性。",
                "Only takes effect on [TableList] lists: wrap the list elements in a class and mark that class's inner fields with this attribute."),
            new BilingualData("width 为像素宽度；未标记宽度的列会自动分配剩余空间。",
                "width is measured in pixels; columns without a marked width automatically share the remaining space."),
            new BilingualData("resizable 默认为 true，可拖拽调整列宽；设为 false 则固定宽度。",
                "resizable defaults to true so the column can be dragged to resize; setting it to false fixes the width.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(int).FullName, "width",
                new BilingualData("宽度，单位为像素。", "The width in pixels.")),
            new ParameterValue(typeof(bool).FullName, "resizable",
                new BilingualData("是否允许调整宽度，默认为 true。", "Whether the width is resizable. Defaults to true."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = { };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Table Column Width",
                TableColumnWidthExampleSO.Instance)
        };
    }
}
