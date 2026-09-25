using System;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// BilingualInfoBox 特性的介绍数据。
    /// </summary>
    internal class BilingualInfoBoxAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("双语信息框", "Bilingual Info Box",
                "BilingualInfoBox 是 Aesir 提供的双语信息框特性，在属性上方绘制同时展示中英文的消息框，支持消息类型、图标与显示条件。",
                "BilingualInfoBox is Aesir's bilingual info box attribute; it draws a message box above the property showing Chinese and English text, with message type, icon and visibility condition support.");

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("在属性上方绘制消息框，同时配置中英文两套文本，只显示与当前语言设置匹配的一条，切换语言时自动更新。",
                "Draws a message box above the property; both Chinese and English texts are supplied, but only the one matching the current language setting is shown and it updates automatically when the language changes."),
            new BilingualData("指定 SDF 图标时会优先绘制该图标，取代消息类型自带的图标。",
                "When an SDF icon is specified, that icon is drawn instead of the icon of the message type."),
            new BilingualData("visibleIf 可传成员名或 @ 表达式（为空则始终显示），据此按条件控制消息框是否显示。",
                "visibleIf accepts a member name or @ expression (empty always shows), conditionally controlling whether the box is displayed."),
            new BilingualData("AllowMultiple，可在同一成员上叠加多个信息框。",
                "AllowMultiple: several info boxes can be stacked on the same member.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(string).FullName, "chinese",
                new BilingualData("中文消息文本。", "The Chinese message text.")),
            new ParameterValue(typeof(string).FullName, "english",
                new BilingualData("英文消息文本，为空时回落到中文文本。",
                    "The English message text; falls back to the Chinese text when omitted.")),
            new ParameterValue("InfoMessageType", "infoMessageType",
                new BilingualData("消息类型（Info, Warning, Error, None）。默认 Info。",
                    "The message type (Info, Warning, Error, None). Defaults to Info.")),
            new ParameterValue("SdfIconType", "icon",
                new BilingualData("消息框上的 SDF 图标。", "The SDF icon shown on the message box.")),
            new ParameterValue(typeof(string).FullName, "visibleIf",
                new BilingualData("显示条件：成员名或 @ 表达式，为空则始终显示。",
                    "Visibility condition: a member name or @ expression; empty always shows.")),
            new ParameterValue(typeof(string).FullName, "iconColor",
                new BilingualData("图标颜色（Odin 颜色字符串或十六进制）。",
                    "The icon color (an Odin color string or hex value).")),
            new ParameterValue(typeof(bool).FullName, "guiAlwaysEnabled",
                new BilingualData("消息框 GUI 是否始终启用（不随属性禁用而变灰）。默认 false。",
                    "Whether the box GUI is always enabled instead of graying out with the property. Defaults to false."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } =
            Array.Empty<ResolvedStringParameterValue>();

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Bilingual Info Box",
                BilingualInfoBoxExampleSO.Instance)
        };
    }
}
