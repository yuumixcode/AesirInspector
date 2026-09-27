using System.Collections.Generic;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// HideIf 特性的介绍数据。
    /// </summary>
    internal class HideIfAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("HideIf", "HideIf", "HideIf 特性用于根据条件动态隐藏属性。",
                "The HideIf attribute is used to dynamically hide properties based on a condition.",
                OdinInspectorDocumentationLinks.HideIfUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("condition 支持 bool 字段、属性、方法或 @ 表达式；使用 @ 表达式访问成员时应自行做 null 检查。",
                "condition accepts a bool field, property, method or an @ expression; when an @ expression accesses members, add your own null checks."),
            new BilingualData(
                "条件结果按类型判断：UnityEngine.Object 判空、bool 取自身值、string 判非空；传入 optionalValue 时则改为与该值相等比较（常用于枚举）。",
                "The condition result is interpreted by type: UnityEngine.Object is tested for null, bool uses its own value and string is tested for non-emptiness; when optionalValue is supplied the result is compared for equality with it instead (commonly with enums)."),
            new BilingualData("只控制显示，不影响序列化与业务逻辑；隐藏状态切换默认带滑动动画，可用 animate 参数关闭。",
                "It only controls visibility and does not affect serialization or game logic; the visibility transition is animated by default and can be turned off with the animate parameter."),
            new BilingualData("与 ShowIf 相反；需要按条件隐藏整组属性时，请使用 HideIfGroup。",
                "It is the opposite of ShowIf; to hide a whole group of properties conditionally, use HideIfGroup.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(string).FullName, "condition",
                new BilingualData("控制隐藏的成员名或表达式。",
                    "The member name or expression that controls visibility.")),
            new ParameterValue(typeof(object).FullName, "optionalValue",
                new BilingualData("可选值。如果提供此参数，只有当 condition 的值等于此值时，属性才会隐藏。",
                    "Optional value. If provided, the property will only be hidden if the condition's value equals this value."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } =
        {
            new ResolvedStringParameterValue("Condition", ResolverType.ValueResolver, typeof(bool).FullName,
                "None", new List<ParameterValue>())
        };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                HideIfExampleSO.Instance)
        };
    }
}
