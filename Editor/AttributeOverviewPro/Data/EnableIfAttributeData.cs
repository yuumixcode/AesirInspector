using System.Collections.Generic;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// EnableIf 特性的介绍数据。
    /// </summary>
    internal class EnableIfAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("EnableIf", "EnableIf", "EnableIf 特性用于根据条件控制属性是否在检查器中启用（可编辑）。",
                "The EnableIf attribute is used to control whether a property is enabled (editable) in the inspector based on a condition.",
                OdinInspectorDocumentationLinks.EnableIfUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("condition 支持 bool 字段、属性、方法或 @ 表达式；使用 @ 表达式访问成员时应自行做 null 检查。", "condition accepts a bool field, property, method or an @ expression; when an @ expression accesses members, add your own null checks."),
            new BilingualData("条件结果按类型判断：UnityEngine.Object 判空、bool 取自身值、string 判非空；传入 optionalValue 时则改为与该值相等比较（常用于枚举）。", "The condition result is interpreted by type: UnityEngine.Object is tested for null, bool uses its own value and string is tested for non-emptiness; when optionalValue is supplied the result is compared for equality with it instead (commonly with enums)."),
            new BilingualData("条件不满足时属性仍然绘制，只是被禁用（灰显），其值依然会被序列化。", "When the condition is not met the property is still drawn, only disabled (grayed out), and its value is still serialized."),
            new BilingualData("与 DisableIf 相反；当 condition 无法解析时，属性默认为启用状态。", "It is the opposite of DisableIf; when condition cannot be resolved, the property defaults to the enabled state.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(string).FullName, "condition",
                new BilingualData("用于判断的成员名、方法名或表达式。",
                    "The member name, method name, or expression used for judgment.")),
            new ParameterValue(typeof(object).FullName, "optionalValue",
                new BilingualData("可选的比较值。如果提供，则当 condition 成员的值等于此值时，属性才启用。",
                    "An optional comparison value. If provided, the property is only enabled when the value of the condition member equals this value."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } =
        {
            new ResolvedStringParameterValue("Condition", ResolverType.ValueResolver, typeof(bool).FullName,
                "None", new List<ParameterValue>())
        };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Usage Examples",
                EnableIfExampleSO.Instance)
        };
    }
}
