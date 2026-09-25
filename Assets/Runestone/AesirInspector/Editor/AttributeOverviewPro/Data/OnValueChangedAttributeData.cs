using System.Collections.Generic;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// OnValueChanged 特性的介绍数据。
    /// </summary>
    internal class OnValueChangedAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("OnValueChanged", "OnValueChanged",
                "OnValueChanged 特性用于在属性值在 Inspector 面板中被修改时触发一个方法。",
                "The OnValueChanged attribute is used to trigger a method whenever a property value is changed in the Inspector.",
                OdinInspectorDocumentationLinks.OnValueChangedUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("仅在检查器中修改值时触发，脚本修改不会触发。",
                "Only triggers when the value is changed in the inspector; changes made by script do not trigger it."),
            new BilingualData("方法可以无参，也可以有一个与属性类型一致的参数来接收新值。",
                "The method can take no parameters, or a single parameter matching the property's type to receive the new value."),
            new BilingualData("includeChildren 默认为 false：引用类型的子属性变化不会触发；值类型（如 Vector3）的子值变化始终会触发。",
                "includeChildren defaults to false, so changes to a reference type's child values do not trigger it; for value types (such as Vector3) child value changes always trigger it.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(string).FullName, "methodName",
                new BilingualData("修改时触发的方法名。", "The name of the method to trigger on change.")),
            new ParameterValue(typeof(bool).FullName, "includeChildren",
                new BilingualData("如果为 true，则子属性的修改也会触发此方法。",
                    "If true, changes to child properties will also trigger this method."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } =
        {
            new ResolvedStringParameterValue("Method", ResolverType.ActionResolver, "void", "None",
                new List<ParameterValue>())
        };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                OnValueChangedExampleSO.Instance)
        };
    }
}
