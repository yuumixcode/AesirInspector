using System.Collections.Generic;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// PropertyRange 特性的介绍数据。
    /// </summary>
    internal class PropertyRangeAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("Property Range", "属性范围",
                "PropertyRange 特性与 Unity 的 Range 特性类似，但它支持使用 $ 符号来引用成员作为动态范围。",
                "The PropertyRange attribute is similar to Unity's Range attribute, but it supports using the $ symbol to reference members for a dynamic range.",
                OdinInspectorDocumentationLinks.PropertyRangeUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("与 Unity 的 [Range] 类似，但可同时作用于字段和属性（属性需配合 [ShowInInspector]）。",
                "Similar to Unity's [Range], but it can be applied to both fields and properties (properties need [ShowInInspector])."),
            new BilingualData("Min/Max 可用 $ 引用成员或 @ 表达式动态取值，范围会随成员值变化实时更新。",
                "Min/Max can be resolved dynamically with a $ member reference or @ expression, and the range updates live as those members change."),
            new BilingualData("上下界的书写顺序不影响结果，内部会自动取较小值作为下限、较大值作为上限。",
                "The order of the two bounds does not matter: the smaller value is used as the lower bound and the larger as the upper bound.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } = new ParameterValue[2]
        {
            new ParameterValue(typeof(double).FullName, "Min",
                new BilingualData("滑动条的最小值。支持使用 $ 引用成员。",
                    "The minimum value of the slider. Supports $ for member reference.")),
            new ParameterValue(typeof(double).FullName, "Max",
                new BilingualData("滑动条的最大值。支持使用 $ 引用成员。",
                    "The maximum value of the slider. Supports $ for member reference."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } =
        {
            new ResolvedStringParameterValue("Min", ResolverType.ValueResolver, "double", "0",
                new List<ParameterValue>()),
            new ResolvedStringParameterValue("Max", ResolverType.ValueResolver, "double", "100",
                new List<ParameterValue>())
        };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                PropertyRangeExampleSO.Instance)
        };
    }
}
