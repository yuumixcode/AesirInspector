using System;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// HideDuplicateReferenceBox 特性的介绍数据。
    /// </summary>
    internal class HideDuplicateReferenceBoxAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("HideDuplicateReferenceBox", "HideDuplicateReferenceBox",
                "HideDuplicateReferenceBox 特性用于隐藏重复引用提示框：当属性因重复引用被绘制成引用时，不再显示引用框。",
                "The HideDuplicateReferenceBox attribute hides the reference box that would otherwise be drawn when a property is rendered as a reference due to duplicate reference values.",
                "https://odininspector.com/attributes/hide-duplicate-reference-box-attribute");

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("无参数，作用于成员本身。",
                "Takes no parameters and applies to the member itself."),
            new BilingualData("若值递归引用自身，则无论是否标注该特性都会绘制引用框。",
                "If the value references itself recursively, the reference box is drawn regardless of this attribute.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } = Array.Empty<ParameterValue>();

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } =
            Array.Empty<ResolvedStringParameterValue>();

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Hide Duplicate Reference Box",
                HideDuplicateReferenceBoxExampleSO.Instance)
        };
    }
}
