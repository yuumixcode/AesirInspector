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
            new BilingualData("当同一个引用值被重复绘制时，Odin 会把后续引用包进引用框，该特性用于隐藏这个引用框。",
                "When the same reference value is drawn more than once, Odin wraps the later references in a reference box; this attribute hides that box."),
            new BilingualData("第一个引用本来就不会显示引用框，只有后续的重复引用才会被包裹。",
                "The first reference is never wrapped in a reference box; only the subsequent duplicate references are."),
            new BilingualData("若值递归引用自身，则无论是否标注该特性都会绘制引用框，以避免无限深度的绘制循环。",
                "If the value references itself recursively, the reference box is drawn regardless of this attribute, to avoid an infinitely deep draw loop.")
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
