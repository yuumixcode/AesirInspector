using System;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// BilingualText 特性的介绍数据。
    /// </summary>
    internal class BilingualTextAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("双语文本", "Bilingual Text",
                "BilingualText 是 Aesir 提供的双语文本特性，把属性以只读文本形式绘制，并同时展示中英文内容，可附带图标。",
                "BilingualText is Aesir's bilingual text attribute; it draws the property as read-only text showing both Chinese and English content, optionally with an icon.");

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("同时配置中英文两套文本，只显示与当前语言设置匹配的一条，切换语言时自动更新。", "Both Chinese and English texts are supplied, but only the one matching the current language setting is shown and it updates automatically when the language changes."),
            new BilingualData("文本支持 $ 成员引用与 @ 表达式解析，可按运行时状态动态显示。", "The text supports $ member references and @ expressions, so it can be resolved dynamically from runtime state."),
            new BilingualData("支持 SDF 图标与图标颜色（iconColor），仅在指定图标时绘制。", "Supports an SDF icon and icon color (iconColor); the icon is drawn only when one is specified.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(string).FullName, "chinese",
                new BilingualData("中文文本内容。", "The Chinese text content.")),
            new ParameterValue(typeof(string).FullName, "english",
                new BilingualData("英文文本内容，为空时回落到中文文本。",
                    "The English text content; falls back to the Chinese text when omitted.")),
            new ParameterValue(typeof(bool).FullName, "nicifyEnglishText",
                new BilingualData("英文文本是否美化显示（拆分驼峰、添加空格）。默认 true。",
                    "Whether to nicify the English text (split camel case, add spaces). Defaults to true.")),
            new ParameterValue("SdfIconType", "icon",
                new BilingualData("文本前的 SDF 图标。", "The SDF icon drawn before the text.")),
            new ParameterValue(typeof(string).FullName, "iconColor",
                new BilingualData("图标颜色（Odin 颜色字符串或十六进制）。",
                    "The icon color (an Odin color string or hex value)."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } =
            Array.Empty<ResolvedStringParameterValue>();

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Bilingual Text",
                BilingualTextExampleSO.Instance)
        };
    }
}
