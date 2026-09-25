using System.Collections.Generic;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// MinMaxSlider 特性的介绍数据。
    /// </summary>
    internal class MinMaxSliderAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("Min Max Slider", "最小最大滑动条",
                "MinMaxSlider 特性为 Vector2, Vector2Int 以及相关的数字对提供了一个滑动条，用于选择一个范围。",
                "The MinMaxSlider attribute provides a slider for Vector2, Vector2Int and other numeric pairs to select a range.",
                OdinInspectorDocumentationLinks.MinMaxSliderUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("用双滑块选择一个范围，值保存在 Vector2 中（x 为最小值、y 为最大值），Vector2Int 同样支持。",
                "Uses a two-knob slider to pick a range, stored in a Vector2 (x is the min, y is the max); Vector2Int is supported as well."),
            new BilingualData("ShowFields 默认为 false；设为 true 时在滑块旁显示数值输入框，可直接键入精确值。",
                "ShowFields defaults to false; set it to true to show numeric fields next to the slider for entering exact values."),
            new BilingualData("上下界可用 $ 引用成员或 @ 表达式动态取值，也可用一个返回 Vector2 的成员同时提供上下界（此时覆盖 MinValue/MaxValue）。",
                "The bounds can be resolved dynamically with a $ member reference or @ expression, or supplied together by a single member returning a Vector2, which then overrides MinValue/MaxValue.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } = new ParameterValue[3]
        {
            new ParameterValue(typeof(float).FullName, "MinValue",
                new BilingualData("滑动条的最小值。支持使用 $ 引用成员。",
                    "The minimum value of the slider. Supports $ for member reference.")),
            new ParameterValue(typeof(float).FullName, "MaxValue",
                new BilingualData("滑动条的最大值。支持使用 $ 引用成员。",
                    "The maximum value of the slider. Supports $ for member reference.")),
            new ParameterValue(typeof(bool).FullName, "ShowFields",
                new BilingualData("是否显示数值输入框。", "Whether to show numeric input fields."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } =
        {
            new ResolvedStringParameterValue("MinValue", ResolverType.ValueResolver, "float", "0",
                new List<ParameterValue>()),
            new ResolvedStringParameterValue("MaxValue", ResolverType.ValueResolver, "float", "1",
                new List<ParameterValue>())
        };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                MinMaxSliderExampleSO.Instance)
        };
    }
}
