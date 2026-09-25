using System;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// BilingualTitle 特性的介绍数据。
    /// </summary>
    internal class BilingualTitleAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("双语标题", "Bilingual Title",
                "BilingualTitle 是 Aesir 提供的双语标题特性，在 Inspector 中同时展示中文与英文标题，随语言设置自动高亮当前语言。",
                "BilingualTitle is Aesir's bilingual title attribute, showing Chinese and English titles at the same time and highlighting the active language automatically.");

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("同时配置中英文标题，只显示与当前语言设置匹配的一条，切换语言时自动更新。", "Both Chinese and English titles are supplied, but only the one matching the current language setting is shown and it updates automatically when the language changes."),
            new BilingualData("标题与副标题（chineseSubTitle / englishSubTitle）都支持 $ 成员引用与 @ 表达式解析。", "Both the title and the subtitles (chineseSubTitle / englishSubTitle) support $ member references and @ expressions."),
            new BilingualData("可控制 TitleAlignments 对齐、horizontalLine 分割线与 bold 加粗；beforeSpace 在根属性上不会添加前置空格。", "Controls TitleAlignments, the horizontalLine separator and bold text; beforeSpace does not add leading space on the root property.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(string).FullName, "chineseTitle",
                new BilingualData("中文标题文本。", "The Chinese title text.")),
            new ParameterValue(typeof(string).FullName, "englishTitle",
                new BilingualData("英文标题文本，为空时回落到中文标题。",
                    "The English title text; falls back to the Chinese title when omitted.")),
            new ParameterValue(typeof(string).FullName, "chineseSubTitle",
                new BilingualData("中文副标题文本。", "The Chinese subtitle text.")),
            new ParameterValue(typeof(string).FullName, "englishSubTitle",
                new BilingualData("英文副标题文本，为空时回落到中文副标题。",
                    "The English subtitle text; falls back to the Chinese subtitle when omitted.")),
            new ParameterValue("TitleAlignments", "titleAlignment",
                new BilingualData("标题对齐方式（Left, Centered, Right, Split）。",
                    "Title alignment (Left, Centered, Right, Split).")),
            new ParameterValue(typeof(bool).FullName, "horizontalLine",
                new BilingualData("是否显示标题下方的水平分割线。默认 true。",
                    "Whether to draw the horizontal line below the title. Defaults to true.")),
            new ParameterValue(typeof(bool).FullName, "bold",
                new BilingualData("标题是否加粗显示。默认 true。",
                    "Whether the title is drawn in bold. Defaults to true.")),
            new ParameterValue(typeof(bool).FullName, "beforeSpace",
                new BilingualData("是否在标题前添加空格。默认 true。",
                    "Whether to add space before the title. Defaults to true."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } =
            Array.Empty<ResolvedStringParameterValue>();

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Bilingual Title",
                BilingualTitleExampleSO.Instance)
        };
    }
}
