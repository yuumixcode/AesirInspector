using System.Collections.Generic;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// ShowIfGroup 特性的介绍数据。
    /// </summary>
    internal class ShowIfGroupAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("ShowIfGroup", "ShowIfGroup",
                "ShowIfGroup 特性用于定义一个组，该组根据条件动态显示或隐藏。组路径可以作为条件。",
                "The ShowIfGroup attribute is used to define a group that is dynamically shown or hidden based on a condition. The group path can serve as the condition.",
                OdinInspectorDocumentationLinks.ShowIfGroupUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("按条件显示 / 隐藏整个组，可与其他组特性（如 BoxGroup、TabGroup）组合使用。",
                "Shows or hides an entire group by condition, and can be combined with other group attributes such as BoxGroup and TabGroup."),
            new BilingualData("未指定 Condition 时，组路径本身即作为条件成员名。",
                "When no Condition is specified, the group path itself is used as the condition member name."),
            new BilingualData("Value 参数可按值匹配显示（如 Value = InfoMessageType.Info）。",
                "The Value parameter shows the group when the condition matches a specific value (e.g. Value = InfoMessageType.Info)."),
            new BilingualData("若只是控制单个组的显隐，优先使用所有组特性都支持的 VisibleIf 参数，无需使用 ShowIfGroup。",
                "To control the visibility of a single group, prefer the VisibleIf parameter supported by all group attributes instead of ShowIfGroup.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(string).FullName, "GroupName",
                new BilingualData("组的路径。如果没有指定 Condition，则路径也用作条件判断。",
                    "The path of the group. If no Condition is specified, the path also serves as the condition.")),
            new ParameterValue(typeof(string).FullName, "Condition",
                new BilingualData("控制组显示的条件成员名或表达式。",
                    "The condition member name or expression controlling group visibility.")),
            new ParameterValue(typeof(object).FullName, "Value",
                new BilingualData("可选值，当 condition 的值匹配此值时组显示。",
                    "Optional value; the group is shown when condition matches this value."))
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
                ShowIfGroupExampleSO.Instance),
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("GroupName Resolved",
                ShowIfGroupExampleWithGroupNameSO.Instance),
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Condition Resolved",
                ShowIfGroupExampleWithConditionSO.Instance)
        };
    }
}
