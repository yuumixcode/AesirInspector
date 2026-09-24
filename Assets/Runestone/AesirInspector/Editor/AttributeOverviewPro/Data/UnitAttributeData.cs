using System;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// Unit 特性的介绍数据。
    /// </summary>
    internal class UnitAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("Unit", "Unit",
                "Unit 特性为数值字段添加单位，并支持在基准单位与显示单位之间换算，让数值更符合使用习惯。",
                "The Unit attribute adds a unit to a numeric field and converts between a base unit and a display unit for more readable values.",
                "https://odininspector.com/attributes/unit-attribute");

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("单参数时只标注单位；双参数时把基准单位换算为显示单位。",
                "A single parameter only labels the unit; two parameters convert the base unit into the display unit."),
            new BilingualData("DisplayAsString 以只读文本显示，ForceDisplayUnit 强制使用指定显示单位。",
                "DisplayAsString shows the value as read-only text; ForceDisplayUnit always uses the given display unit."),
            new BilingualData("单位字符串（如 \"kg\"）也可代替 Units 枚举传入。",
                "A unit string (such as \"kg\") can be passed instead of the Units enum.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue("Units", "unit",
                new BilingualData("单一单位（Units 枚举或单位字符串）。",
                    "A single unit (a Units enum value or unit string).")),
            new ParameterValue("Units", "base",
                new BilingualData("基准单位，字段值以该单位存储。",
                    "The base unit in which the field value is stored.")),
            new ParameterValue("Units", "display",
                new BilingualData("显示单位，绘制时按该单位换算。",
                    "The display unit used when drawing the value.")),
            new ParameterValue(typeof(bool).FullName, "DisplayAsString",
                new BilingualData("是否以只读文本显示（不可编辑）。",
                    "Whether to display the value as read-only text.")),
            new ParameterValue(typeof(bool).FullName, "ForceDisplayUnit",
                new BilingualData("是否强制使用显示单位（不显示单位切换按钮）。",
                    "Whether to always use the display unit instead of showing a unit switcher."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } =
            Array.Empty<ResolvedStringParameterValue>();

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Unit",
                UnitExampleSO.Instance)
        };
    }
}
