using System.Collections.Generic;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// SuffixLabel 特性的介绍数据。
    /// </summary>
    internal class SuffixLabelAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("SuffixLabel", "SuffixLabel", "SuffixLabel 特性用于在属性输入框的末尾添加一个后缀标签。",
                "The SuffixLabel attribute adds a label to the end of a property's input field.",
                OdinInspectorDocumentationLinks.SuffixLabelUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("在输入框末尾添加说明性标签，常用于标注单位或补充上下文信息。",
                "Adds an explanatory label at the end of the input field, commonly used for units or extra context."),
            new BilingualData("支持静态字符串、$ 成员引用与 @ 表达式（如 \"$Suffix\" 或 \"@this.SuffixText\"）。",
                "Supports static strings, $ member references, and @ expressions (for example \"$Suffix\" or \"@this.SuffixText\")."),
            new BilingualData("Overlay = true 时标签绘制在输入框内部并右对齐（更紧凑）；默认为 false，标签绘制在输入框之后。",
                "With Overlay = true the label is drawn inside the input field and right-aligned (more compact); it defaults to false, drawing the label after the field."),
            new BilingualData("还可通过 Icon / IconColor 在标签旁显示 SDF 图标及其颜色。",
                "It can also show an SDF icon and its color next to the label via Icon / IconColor.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(string).FullName, "label",
                new BilingualData("后缀标签显示的文本或表达式。", "The suffix label text or expression.")),
            new ParameterValue(typeof(bool).FullName, "overlay",
                new BilingualData("是否将标签覆盖在属性输入框上（内部显示）。",
                    "Whether the label should be overlaid on top of the property's input field."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } =
        {
            new ResolvedStringParameterValue("Label", ResolverType.ValueResolver, typeof(string).FullName,
                "None", new List<ParameterValue>())
        };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                SuffixLabelExampleSO.Instance)
        };
    }
}
