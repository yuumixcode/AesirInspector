namespace Runestone.AesirInspector.Editor
{
    internal class EnumPagingAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("EnumPaging", "EnumPaging", "EnumPaging 特性作用于枚举类型，绘制一个可循环的枚举按钮。",
                "The EnumPaging attribute draws a looping button for enum fields.",
                "https://odininspector.com/attributes/enum-paging-attribute");

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("只作用于枚举类型：在枚举下拉框两侧绘制左右箭头按钮，按枚举定义顺序循环切换，末项之后回到首项。",
                "It only works on enums: it draws left and right arrow buttons beside the enum dropdown that cycle through the values in declaration order and wrap from the last value back to the first."),
            new BilingualData("箭头按钮与下拉框共用同一个值，可配合 OnValueChanged 在切换时执行逻辑（例如改变 Unity 编辑器当前选择的工具）。",
                "The arrow buttons and the dropdown share the same value, so it can be combined with OnValueChanged to run logic on every change, for example changing the currently selected Unity Editor tool."),
            new BilingualData(
                "与 EnumToggleButtons 的区别：EnumPaging 保留下拉框并增加翻页按钮，EnumToggleButtons 则把枚举整体绘制成一排按钮。",
                "Difference from EnumToggleButtons: EnumPaging keeps the dropdown and adds paging buttons, whereas EnumToggleButtons draws the whole enum as a row of buttons.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } = { };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = { };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                EnumPagingExampleSO.Instance)
        };
    }
}
