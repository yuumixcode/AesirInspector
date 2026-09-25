using System.Collections.Generic;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// Title 特性的介绍数据。
    /// </summary>
    internal class TitleAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("Title", "Title", "Title 特性用于为任意属性添加标题，类似于 Unity 的 Header 特性，但功能更强大。",
                "The Title attribute is used to add a title to any property, similar to Unity's Header attribute but with more powerful features.",
                OdinInspectorDocumentationLinks.TitleUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("用途与 Unity 的 Header 类似，但可作用于字段、属性和方法，并额外支持子标题、对齐方式、粗体与水平分割线。",
                "Serves the same purpose as Unity's Header but works on fields, properties and methods, and adds subtitles, alignment, bold text and a horizontal line."),
            new BilingualData("标题与子标题都支持字符串解析：\"$成员名\" 引用成员，\"@表达式\" 使用表达式。",
                "Both title and subtitle support string resolution: \"$memberName\" references a member, while \"@expression\" evaluates an expression."),
            new BilingualData("默认加粗并显示水平分割线；TitleAlignments.Split 会把标题放在左侧、子标题放在右侧。",
                "Bold and horizontal line are on by default; TitleAlignments.Split places the title on the left and the subtitle on the right.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(string).FullName, "title",
                new BilingualData("主标题文本。", "The main title text.")),
            new ParameterValue(typeof(string).FullName, "subtitle",
                new BilingualData("子标题文本，显示在主标题下方（或右侧，取决于对齐方式）。",
                    "The subtitle text, displayed below the main title (or to the right, depending on alignment).")),
            new ParameterValue("TitleAlignments", "titleAlignment",
                new BilingualData("标题的对齐方式（Left, Centered, Right, Split）。",
                    "The alignment of the title (Left, Centered, Right, Split).")),
            new ParameterValue(typeof(bool).FullName, "horizontalLine",
                new BilingualData("是否在标题下方显示水平分割线。",
                    "Whether to display a horizontal line below the title.")),
            new ParameterValue(typeof(bool).FullName, "bold",
                new BilingualData("标题是否加粗显示。", "Whether the title should be displayed in bold."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } =
        {
            new ResolvedStringParameterValue("Title", ResolverType.ValueResolver, typeof(string).FullName,
                "None", new List<ParameterValue>
                {
                    new ParameterValue("T", "$value",
                        new BilingualData("应用此特性的成员的值。",
                            "The value of the member that has the attribute applied to it."))
                }),
            new ResolvedStringParameterValue("Subtitle", ResolverType.ValueResolver, typeof(string).FullName,
                "None", new List<ParameterValue>
                {
                    new ParameterValue("T", "$value",
                        new BilingualData("应用此特性的成员的值。",
                            "The value of the member that has the attribute applied to it."))
                })
        };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                TitleExampleSO.Instance),
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Title Expression",
                TitleExampleWithTitleSO.Instance),
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Subtitle Expression",
                TitleExampleWithSubtitleSO.Instance)
        };
    }
}
