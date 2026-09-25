using System.Collections.Generic;

namespace Runestone.AesirInspector.Editor
{
    internal class TableMatrixAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("TableMatrix", "TableMatrix", "TableMatrix 特性将二维数组绘制成一个表格。",
                "The TableMatrix attribute draws a two-dimensional array as a table.",
                "https://odininspector.com/attributes/table-matrix-attribute");

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData(
                "二维数组需要 Odin 序列化才能保存（案例继承 SerializedScriptableObject）；若沿用 Unity 序列化，可用 [ShowInInspector] 让 Odin 绘制。",
                "Two-dimensional arrays need Odin serialization to be saved (the examples derive from SerializedScriptableObject); with Unity serialization you can still expose them via [ShowInInspector]."),
            new BilingualData("表格默认的绘制方向与代码中 \"[行, 列]\" 的书写顺序相反，可用 Transpose 反转。",
                "By default the table is drawn transposed relative to the code's \"[row, column]\" order; set Transpose to invert it."),
            new BilingualData("拖拽行或列标签可移动整行、整列，右键表格可打开上下文菜单。",
                "Drag row or column labels to move entire rows or columns, and right-click the table to open a context menu."),
            new BilingualData("自定义 DrawElementMethod 的绘制代码使用 UnityEditor API，需要包在 #if UNITY_EDITOR 中。",
                "Custom DrawElementMethod drawing code uses UnityEditor APIs, so it must be wrapped in #if UNITY_EDITOR.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(bool).FullName, "Transpose",
                new BilingualData("是否转置，默认为 false。", "Whether to transpose the table. Defaults to false.")),
            new ParameterValue(typeof(string).FullName, "Labels",
                new BilingualData("自定义绘制表头的方法，返回一个元组。",
                    "Custom method for drawing table headers, returning a tuple.")),
            new ParameterValue(typeof(bool).FullName, "IsReadOnly",
                new BilingualData("是否只读，默认为 false。", "Whether the table is read-only. Defaults to false.")),
            new ParameterValue(typeof(bool).FullName, "ResizableColumns",
                new BilingualData("是否可以修改列宽，默认为 true。", "Whether columns are resizable. Defaults to true.")),
            new ParameterValue(typeof(string).FullName, "HorizontalTitle",
                new BilingualData("横向标题。", "The horizontal title.")),
            new ParameterValue(typeof(string).FullName, "VerticalTitle",
                new BilingualData("纵向标题。", "The vertical title.")),
            new ParameterValue(typeof(int).FullName, "RowHeight",
                new BilingualData("行高。", "The row height.")),
            new ParameterValue(typeof(bool).FullName, "SquareCells",
                new BilingualData("是否使单元格保持正方形，默认为 false。",
                    "Whether to keep cells square. Defaults to false.")),
            new ParameterValue(typeof(bool).FullName, "HideColumnIndices",
                new BilingualData("隐藏绘制图表的列标。", "Hides column indices in the table.")),
            new ParameterValue(typeof(bool).FullName, "HideRowIndices",
                new BilingualData("隐藏绘制图表的行标。", "Hides row indices in the table.")),
            new ParameterValue(typeof(bool).FullName, "RespectIndentLevel",
                new BilingualData("绘制的表是否应遵循当前 GUI 缩进级别。",
                    "Whether the table should respect the current GUI indent level.")),
            new ParameterValue(typeof(string).FullName, "DrawElementMethod",
                new BilingualData("自定义绘制二维数组中的元素样式。", "Custom method for drawing elements in the 2D array."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } =
        {
            new ResolvedStringParameterValue("DrawElementMethod", ResolverType.ValueResolver,
                typeof(string).FullName, "None", new List<ParameterValue>()),
            new ResolvedStringParameterValue("HorizontalTitle", ResolverType.ValueResolver,
                typeof(string).FullName, "None", new List<ParameterValue>()),
            new ResolvedStringParameterValue("VerticalTitle", ResolverType.ValueResolver,
                typeof(string).FullName, "None", new List<ParameterValue>())
        };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeOdinSerializedExample("Basic Usage",
                TableMatrixExampleSO.Instance),
            new AttributeExamplePreviewItem().InitializeOdinSerializedExample("DrawElementMethod Resolved",
                TableMatrixExampleWithDrawElementMethodSO.Instance),
            new AttributeExamplePreviewItem().InitializeOdinSerializedExample("HorizontalTitle Resolved",
                TableMatrixExampleWithHorizontalTitleSO.Instance)
        };
    }
}
