using System;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// HideReferenceObjectPicker 特性的介绍数据。
    /// </summary>
    internal class HideReferenceObjectPickerAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("HideReferenceObjectPicker", "HideReferenceObjectPicker",
                "HideReferenceObjectPicker 特性用于隐藏非 Unity 序列化引用类型属性上方的多态对象选择器。",
                "The HideReferenceObjectPicker attribute hides the polymorphic object picker shown above the properties of non-Unity-serialized reference types.",
                "https://odininspector.com/attributes/hide-reference-object-picker-attribute");

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("无参数，作用于成员本身。",
                "Takes no parameters and applies to the member itself."),
            new BilingualData("隐藏后该属性只能保持当前类型，无法在 Inspector 中切换派生类型。",
                "Once hidden, the property keeps its current type and can no longer switch derived types in the inspector.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } = Array.Empty<ParameterValue>();

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } =
            Array.Empty<ResolvedStringParameterValue>();

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Hide Reference Object Picker",
                HideReferenceObjectPickerExampleSO.Instance)
        };
    }
}
