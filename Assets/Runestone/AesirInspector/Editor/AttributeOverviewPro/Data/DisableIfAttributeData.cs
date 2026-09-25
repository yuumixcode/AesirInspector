using System.Collections.Generic;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// DisableIf 特性的介绍数据。
    /// </summary>
    internal class DisableIfAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("DisableIf", "DisableIf", "DisableIf 特性用于根据条件控制属性是否在检查器中禁用。",
                "The DisableIf attribute is used to control whether a property is disabled in the inspector based on a condition.",
                OdinInspectorDocumentationLinks.DisableIfUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("条件为真时属性在检视器中变灰且不可编辑；condition 可为 bool 成员、属性、方法或 @ 表达式。", "When the condition is true the property is greyed out and cannot be edited; condition may be a bool member, a property, a method, or an @ expression."),
            new BilingualData("传入 optionalValue 时，仅当 condition 成员的值等于该值才禁用（常用于枚举成员等于某个枚举值）。", "When optionalValue is supplied, the property is disabled only if the value of the condition member equals it (commonly used with an enum member equal to a specific enum value)."),
            new BilingualData("该特性不会作用于列表元素（DontApplyToListElements），只影响成员自身；AllowMultiple 允许叠加多个条件。", "The attribute does not apply to list elements (DontApplyToListElements) and only affects the member itself; AllowMultiple lets several conditions be stacked.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(string).FullName, "condition",
                new BilingualData("用于判断的成员名、方法名或表达式。",
                    "The member name, method name, or expression used for judgment.")),
            new ParameterValue(typeof(object).FullName, "optionalValue",
                new BilingualData("可选的比较值。如果提供，则当 condition 成员的值等于此值时，属性被禁用。",
                    "An optional comparison value. If provided, the property is disabled when the value of the condition member equals this value."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } =
        {
            new ResolvedStringParameterValue("Condition", ResolverType.ValueResolver, typeof(bool).FullName,
                "None", new List<ParameterValue>())
        };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Usage Examples",
                DisableIfExampleSO.Instance)
        };
    }
}
