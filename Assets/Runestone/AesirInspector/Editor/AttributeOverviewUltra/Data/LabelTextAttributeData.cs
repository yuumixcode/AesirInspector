using System.Collections.Generic;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// LabelText 特性的介绍数据。
    /// </summary>
    internal class LabelTextAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("LabelText", "LabelText", "LabelText 特性用于更改属性在检查器中显示的标签名称。",
                "The LabelText attribute is used to change the label name of a property displayed in the inspector.",
                OdinInspectorDocumentationLinks.LabelTextUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("用自定义文本替换属性默认的标签名称。",
                "Replaces the default label name of a property with custom text."),
            new BilingualData("text 支持 $ 成员引用与 @ 表达式，可动态生成标签内容。",
                "text supports $ member references and @ expressions, allowing the label to be generated dynamically."),
            new BilingualData("nicifyText 为 true 时会对文本做美化（如 m_myField → My Field），解析得到的文本同样会被美化。",
                "When nicifyText is true the text is nicified (e.g. m_myField -> My Field), and text produced by resolution is nicified as well."),
            new BilingualData("可通过 icon 与 IconColor 在标签旁显示带颜色的 Sdf 图标。",
                "icon and IconColor display a colored SDF icon next to the label.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(string).FullName, "text",
                new BilingualData("要在检查器中显示的标签文本。", "The label text to be displayed in the inspector.")),
            new ParameterValue(typeof(bool).FullName, "nicifyText",
                new BilingualData("是否优化文本显示（例如将 m_myField 转换为 My Field）。默认为 false。",
                    "Whether to nicify the text display (e.g., converting m_myField to My Field). Defaults to false.")),
            new ParameterValue("SdfIconType", "icon",
                new BilingualData("要在标签旁显示的图标。", "The icon to be displayed next to the label.")),
            new ParameterValue(typeof(string).FullName, "IconColor",
                new BilingualData("图标的颜色。支持命名颜色、十六进制和 RGBA 表达式。",
                    "The color of the icon. Supports named colors, hex, and RGBA expressions."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } =
        {
            new ResolvedStringParameterValue("Text", ResolverType.ValueResolver, typeof(string).FullName,
                "None", new List<ParameterValue>())
        };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Usage Examples",
                LabelTextExampleSO.Instance)
        };
    }
}
