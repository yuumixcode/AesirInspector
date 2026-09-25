namespace Runestone.AesirInspector.Editor
{
    internal class WrapAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("Wrap", "Wrap", "Wrap 特性为数字类型的属性设置数值循环范围，当数值超出范围时会自动从另一端开始。",
                "The Wrap attribute sets a looping range for numeric properties, automatically wrapping values from one end to the other when they exceed the range.",
                "https://odininspector.com/attributes/wrap-attribute");

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("为数值字段设置循环区间：调整数值越过一端时会从另一端绕回，适合角度、弧度等环形数值。",
                "Sets a looping range for a numeric field: pushing the value past one end wraps it to the other, which suits cyclic values like angles and radians."),
            new BilingualData("支持 int、long、short、float、double、decimal 以及 Vector2/3/4，但不支持无符号整数。",
                "Supports int, long, short, float, double, decimal and Vector2/3/4, but not unsigned integer types."),
            new BilingualData("传入的 min/max 会自动按大小排序（较小者为 Min），因此参数顺序写反也不影响结果。",
                "The min/max arguments are automatically ordered by size (the smaller becomes Min), so swapping them does not change the result.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(float).FullName, "min",
                new BilingualData("范围的最小值。", "The minimum value of the range.")),
            new ParameterValue(typeof(float).FullName, "max",
                new BilingualData("范围的最大值。", "The maximum value of the range."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = { };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                WrapExampleSO.Instance)
        };
    }
}
