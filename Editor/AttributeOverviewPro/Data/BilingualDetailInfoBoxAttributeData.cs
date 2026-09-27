using System;
using System.Collections.Generic;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// BilingualDetailInfoBox 特性的介绍数据。
    /// </summary>
    internal class BilingualDetailInfoBoxAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("双语详细信息框", "Bilingual Detail Info Box",
                "BilingualDetailInfoBox 是 Aesir 提供的双语详细信息框特性，在属性上方绘制可展开的消息框，主消息与详细内容都同时配置中英文两套文本。",
                "BilingualDetailInfoBox is Aesir's bilingual detail info box attribute; it draws an expandable message box above the property, with both the message and the details supplying Chinese and English texts.");

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("在属性上方绘制可折叠的消息框，主消息与详细内容都同时配置中英文两套文本，只显示与当前语言设置匹配的一条，切换语言时自动更新。",
                "Draws a collapsible message box above the property; both the message and the details supply Chinese and English texts, but only the one matching the current language setting is shown and it updates automatically when the language changes."),
            new BilingualData("主消息与详细内容都是解析字符串，支持 $ 成员引用与 @ 表达式，可按状态动态显示。",
                "Both the message and the details are resolved strings supporting $ member references and @ expressions, so they can reflect runtime state."),
            new BilingualData("支持 Info / Warning / Error / None 四种消息类型；guiAlwaysEnabled 可让消息框在属性被禁用时仍保持启用。",
                "Supports the Info / Warning / Error / None message types; guiAlwaysEnabled keeps the box enabled even when the property is disabled."),
            new BilingualData("visibleIf 可传成员名或 @ 表达式（为空则始终显示）；AllowMultiple，可在同一成员上叠加多个详细信息框。",
                "visibleIf accepts a member name or @ expression (empty always shows); AllowMultiple lets several detail info boxes stack on the same member.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(string).FullName, "chinese",
                new BilingualData("默认（中文）消息框中显示的文本。",
                    "Text displayed in the default (Chinese) message box.")),
            new ParameterValue(typeof(string).FullName, "english",
                new BilingualData("英文模式下消息框中显示的文本。",
                    "Text displayed in the message box in English mode.")),
            new ParameterValue(typeof(string).FullName, "detailsChinese",
                new BilingualData("默认（中文）详细内容文本。", "Default (Chinese) detailed content text.")),
            new ParameterValue(typeof(string).FullName, "detailsEnglish",
                new BilingualData("英文模式下详细内容文本。", "Detailed content text in English mode.")),
            new ParameterValue("InfoMessageType", "infoMessageType",
                new BilingualData("消息框的类型（Info, Warning, Error, None）。默认 Info。",
                    "The type of the message box (Info, Warning, Error, None). Defaults to Info.")),
            new ParameterValue(typeof(string).FullName, "visibleIf",
                new BilingualData("可选成员名或表达式，用于控制消息框是否显示（为空则始终显示）。",
                    "An optional member name or expression used to control whether the message box is displayed; empty always shows.")),
            new ParameterValue(typeof(bool).FullName, "guiAlwaysEnabled",
                new BilingualData("即使属性被禁用，是否也始终启用消息框。默认 false。",
                    "Whether the message box is always enabled even if the property is disabled. Defaults to false."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } =
        {
            new ResolvedStringParameterValue("Message", ResolverType.ValueResolver, typeof(string).FullName,
                "None", new List<ParameterValue>()),
            new ResolvedStringParameterValue("Details", ResolverType.ValueResolver, typeof(string).FullName,
                "None", new List<ParameterValue>()),
            new ResolvedStringParameterValue("Visible If", ResolverType.ValueResolver, typeof(bool).FullName,
                "None", new List<ParameterValue>())
        };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Bilingual Detail Info Box",
                DetailInfoBoxBilingualExampleSO.Instance)
        };
    }
}
