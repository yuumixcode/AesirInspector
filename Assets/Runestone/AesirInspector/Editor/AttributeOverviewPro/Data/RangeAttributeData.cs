using System;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// Unity Range 特性的介绍数据。
    /// </summary>
    internal class RangeAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("Range", "Range",
                "Range 是 Unity 内置特性，为数值字段绘制一个限定取值范围的滑块。Odin 完整沿用该特性。",
                "Range is a Unity built-in attribute that draws a slider constraining a numeric field to a range. Odin fully supports it.",
                OdinInspectorDocumentationLinks.RangeUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("Unity 内置特性，Odin 沿用其滑块绘制行为。",
                "A Unity built-in attribute whose slider drawing is kept by Odin."),
            new BilingualData("同时限定取值范围：超出范围的值会被钳制。",
                "It also constrains the value: out-of-range values are clamped."),
            new BilingualData("只能作用于字段（Unity 限制），若需作用于属性请使用 Odin 的 PropertyRange。",
                "Can only be applied to fields (a Unity limitation); use Odin's PropertyRange for properties.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(float).FullName, "min",
                new BilingualData("允许的最小值。", "The minimum allowed value.")),
            new ParameterValue(typeof(float).FullName, "max",
                new BilingualData("允许的最大值。", "The maximum allowed value."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } =
            Array.Empty<ResolvedStringParameterValue>();

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Range",
                RangeExampleSO.Instance)
        };
    }
}
