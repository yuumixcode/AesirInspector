using System.Collections.Generic;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// ShowIf 特性的介绍数据。
    /// </summary>
    internal class ShowIfAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("ShowIf", "ShowIf", "ShowIf 特性用于根据条件动态显示或隐藏属性。",
                "The ShowIf attribute is used to dynamically show or hide properties based on a condition.",
                OdinInspectorDocumentationLinks.ShowIfUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("根据 bool 成员、方法或 @ 表达式控制显示；提供可选值时按值匹配显示（如 [ShowIf(\"enumField\", SomeEnum.Value)]）。",
                "Controls visibility from a bool member, method, or @ expression; when an optional value is supplied, the property is shown only if the condition equals it (e.g. [ShowIf(\"enumField\", SomeEnum.Value)])."),
            new BilingualData("只影响可见性：隐藏的字段仍会被序列化，也不影响运行时逻辑；与 HideIf 行为相反。",
                "Affects visibility only: hidden fields are still serialized and runtime logic is unaffected; it is the opposite of HideIf."),
            new BilingualData("Animate 默认为 true，显示 / 隐藏切换时会播放滑动动画。",
                "Animate defaults to true, playing a slide animation when the property is shown or hidden.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(string).FullName, "condition",
                new BilingualData("控制显示的成员名或表达式。",
                    "The member name or expression that controls visibility.")),
            new ParameterValue(typeof(object).FullName, "optionalValue",
                new BilingualData("可选值。如果提供此参数，只有当 condition 的值等于此值时，属性才会显示。",
                    "Optional value. If provided, the property will only be shown if the condition's value equals this value."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } =
        {
            new ResolvedStringParameterValue("Condition", ResolverType.ValueResolver, typeof(bool).FullName,
                "None", new List<ParameterValue>())
        };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                ShowIfExampleSO.Instance)
        };
    }
}
