using System.Collections.Generic;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// MaxValue 特性的介绍数据。
    /// </summary>
    internal class MaxValueAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("Max Value", "最大值", "MaxValue 特性为数字属性设置一个最大值。如果值大于该值，它将被限制在该值。",
                "The MaxValue attribute sets a maximum value for a numeric property. If the value becomes greater than the specified maximum, it will be clamped to it.",
                OdinInspectorDocumentationLinks.MaxValueUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("在检查器中把数值或向量限制到指定上限，向量（Vector2/3/4 等）按分量分别钳制。",
                "Caps numeric or vector values at the given maximum in the inspector; vectors (Vector2/3/4, etc.) are clamped per component."),
            new BilingualData("仅在编辑器中生效：脚本直接赋值不会被钳制。",
                "Only takes effect in the editor: values assigned from script are not clamped."),
            new BilingualData("参数可以是固定数值，也可以引用成员（$）或表达式（@）动态取值；与 MinValue 组合可限定一个完整区间。",
                "The parameter accepts a fixed number, or a $ member reference / @ expression for a dynamic bound; combine with MinValue to constrain a full range.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } = new ParameterValue[1]
        {
            new ParameterValue(typeof(double).FullName, "MaxValue",
                new BilingualData("允许的最大值。支持使用 $ 引用成员。",
                    "The maximum value allowed. Supports $ for member reference."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } =
        {
            new ResolvedStringParameterValue("MaxValue", ResolverType.ValueResolver, "double", "100",
                new List<ParameterValue>())
        };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                MaxValueExampleSO.Instance)
        };
    }
}
