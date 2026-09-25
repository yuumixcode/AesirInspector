using System.Collections.Generic;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// HideIfGroup 特性的介绍数据。
    /// </summary>
    internal class HideIfGroupAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("HideIfGroup", "HideIfGroup",
                "HideIfGroup 特性用于定义一个组，该组根据条件动态隐藏或显示。组路径可以作为条件。",
                "The HideIfGroup attribute is used to define a group that is dynamically hidden or shown based on a condition. The group path can serve as the condition.",
                OdinInspectorDocumentationLinks.HideIfGroupUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData(
                "组路径默认也作为条件成员名使用（如 [HideIfGroup(\"Toggle\")] 表示 Toggle 为 true 时隐藏该组），可用 Condition 参数覆盖。",
                "The group path is also used as the condition member name by default (so [HideIfGroup(\"Toggle\")] hides the group while Toggle is true); the Condition parameter overrides it."),
            new BilingualData("Condition 支持成员名、属性、方法或 @ 表达式；配合 Value 参数可以按枚举等值匹配隐藏。",
                "Condition accepts a member name, property, method or an @ expression; with the Value parameter the group is hidden when the condition equals that value, which is handy for enums."),
            new BilingualData("它是组特性，可以和其他组特性（如 BoxGroup）组合，也可以串联多个 HideIfGroup 表达更复杂的条件。",
                "It is a group attribute, so it can be combined with other group attributes (such as BoxGroup) and multiple HideIfGroup attributes can be chained for more complex conditions."),
            new BilingualData("可见性变化默认带淡入淡出动画（animate 参数可关闭）；若只需控制单个组的可见性，官方更推荐使用其他组特性自带的 VisibleIf 参数。",
                "Visibility changes are animated with a fade by default (turn it off with the animate parameter); to control the visibility of a single group, Odin recommends the VisibleIf parameter that all group attributes already have.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(string).FullName, "GroupName",
                new BilingualData("组的路径。如果没有指定 Condition，则路径也用作条件判断。",
                    "The path of the group. If no Condition is specified, the path also serves as the condition.")),
            new ParameterValue(typeof(string).FullName, "Condition",
                new BilingualData("控制组隐藏的条件成员名或表达式。",
                    "The condition member name or expression controlling group visibility.")),
            new ParameterValue(typeof(object).FullName, "Value",
                new BilingualData("可选值，当 condition 的值匹配此值时组隐藏。",
                    "Optional value; the group is hidden when condition matches this value."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } =
        {
            new ResolvedStringParameterValue("GroupName", ResolverType.ValueResolver, typeof(string).FullName,
                "None", new List<ParameterValue>()),
            new ResolvedStringParameterValue("Condition", ResolverType.ValueResolver, typeof(bool).FullName,
                "None", new List<ParameterValue>())
        };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                HideIfGroupExampleSO.Instance),
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("GroupName Resolved",
                HideIfGroupExampleWithGroupNameSO.Instance),
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Condition Resolved",
                HideIfGroupExampleWithConditionSO.Instance)
        };
    }
}
