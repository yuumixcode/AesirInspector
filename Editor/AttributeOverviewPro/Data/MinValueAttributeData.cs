using System.Collections.Generic;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// MinValue 特性的介绍数据。
    /// </summary>
    internal class MinValueAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("Min Value", "最小值", "MinValue 特性为数字属性设置一个最小值。如果值小于该值，它将被限制在该值。",
                "The MinValue attribute sets a minimum value for a numeric property. If the value becomes less than the specified minimum, it will be clamped to it.",
                OdinInspectorDocumentationLinks.MinValueUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("在检查器中把数值或向量限制到指定下限，向量（Vector2/3/4 等）按分量分别钳制。",
                "Caps numeric or vector values at the given minimum in the inspector; vectors (Vector2/3/4, etc.) are clamped per component."),
            new BilingualData("仅在编辑器中生效：脚本直接赋值不会被钳制。",
                "Only takes effect in the editor: values assigned from script are not clamped."),
            new BilingualData("参数可以是固定数值，也可以引用成员（$）或表达式（@）动态取值；与 MaxValue 组合可限定一个完整区间。",
                "The parameter accepts a fixed number, or a $ member reference / @ expression for a dynamic bound; combine with MaxValue to constrain a full range.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } = new ParameterValue[1]
        {
            new ParameterValue(typeof(double).FullName, "MinValue",
                new BilingualData("允许的最小值。支持使用 $ 引用成员。",
                    "The minimum value allowed. Supports $ for member reference."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } =
        {
            new ResolvedStringParameterValue("MinValue", ResolverType.ValueResolver, "double", "0",
                new List<ParameterValue>())
        };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                MinValueExampleSO.Instance)
        };
    }
}
