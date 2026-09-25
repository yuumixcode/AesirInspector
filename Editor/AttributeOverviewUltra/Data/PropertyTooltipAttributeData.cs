using System.Collections.Generic;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// PropertyTooltip 特性的介绍数据。
    /// </summary>
    internal class PropertyTooltipAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("PropertyTooltip", "PropertyTooltip",
                "PropertyTooltip 特性用于为属性添加提示信息，当鼠标悬停在属性标签上时显示。",
                "The PropertyTooltip attribute adds a tooltip to a property, shown when the mouse hovers over the property label.",
                OdinInspectorDocumentationLinks.PropertyTooltipUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("鼠标悬停在属性标签上时显示提示文本。",
                "Shows a tooltip when the mouse hovers over the property's label."),
            new BilingualData("与 Unity 的 [Tooltip] 不同，除字段外还可作用于属性和方法（如 [Button]）。",
                "Unlike Unity's [Tooltip], it can also be applied to properties and methods (such as [Button]), not only fields."),
            new BilingualData("文本支持成员引用（$）与 Odin 表达式（@），可动态生成提示内容。",
                "The text supports member references ($) and Odin expressions (@), so the tooltip content can be generated dynamically.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(string).FullName, "tooltip",
                new BilingualData("提示信息文本或表达式。", "The tooltip text or expression."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } =
        {
            new ResolvedStringParameterValue("Tooltip", ResolverType.ValueResolver, typeof(string).FullName,
                "None", new List<ParameterValue>())
        };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                PropertyTooltipExampleSO.Instance)
        };
    }
}
