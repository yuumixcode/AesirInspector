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
            new BilingualData("无参数，隐藏非 Unity 序列化引用类型成员上方的多态对象选择器，隐藏后无法在 Inspector 中切换派生类型。",
                "Takes no parameters and hides the polymorphic object picker above non-Unity-serialized reference members; once hidden, derived types can no longer be switched in the inspector."),
            new BilingualData("仍可右键将实例置空以重新赋值；若不希望被修改，可配合 DisableContextMenu 使用。",
                "You can still right-click to set the instance to null and assign a new value; combine with DisableContextMenu if you want to prevent that."),
            new BilingualData("主要面向 Odin 序列化的引用类型（如 SerializedMonoBehaviour 中的字段），Unity 原生序列化的引用类型不会出现该选择器。",
                "Mainly relevant to Odin-serialized reference types (e.g. fields in SerializedMonoBehaviour); Unity-serialized reference types never show this picker.")
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
