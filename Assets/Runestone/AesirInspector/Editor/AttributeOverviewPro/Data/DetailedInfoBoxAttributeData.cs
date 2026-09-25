using System.Collections.Generic;

namespace Runestone.AesirInspector.Editor
{
    internal class DetailedInfoBoxAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("DetailedInfoBox", "DetailedInfoBox",
                "DetailedInfoBox 特性用于在属性上方绘制一个带有详细信息的可折叠消息框。",
                "The DetailedInfoBox attribute is used to draw a collapsible message box with detailed information above a property.",
                OdinInspectorDocumentationLinks.DetailedInfoBoxUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("在属性上方绘制可折叠的消息框，主消息与详细内容都同时配置中英文两套文本。", "Draws a collapsible message box above the property; both the main message and the detailed content supply Chinese and English texts."),
            new BilingualData("主消息与详细内容都是解析字符串，支持 $ 成员引用与 @ 表达式，可按状态动态显示。", "Both the main message and the detailed content are resolved strings supporting $ member references and @ expressions, so they can reflect runtime state."),
            new BilingualData("支持 Info / Warning / Error / None 四种消息类型；guiAlwaysEnabled 可让消息框在属性被禁用时仍保持启用。", "Supports the Info / Warning / Error / None message types; guiAlwaysEnabled keeps the box enabled even when the property is disabled.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(string).FullName, "chinese",
                new BilingualData("默认（中文）消息框中显示的文本。",
                    "Text displayed in the default (Chinese) message box.")),
            new ParameterValue(typeof(string).FullName, "english",
                new BilingualData("英文模式下消息框中显示的文本。", "Text displayed in the message box in English mode.")),
            new ParameterValue(typeof(string).FullName, "detailsChinese",
                new BilingualData("默认（中文）详细内容文本。", "Default (Chinese) detailed content text.")),
            new ParameterValue(typeof(string).FullName, "detailsEnglish",
                new BilingualData("英文模式下详细内容文本。", "Detailed content text in English mode.")),
            new ParameterValue("InfoMessageType", "infoMessageType",
                new BilingualData("消息框的类型（Info, Warning, Error, None）。",
                    "The type of the message box (Info, Warning, Error, None).")),
            new ParameterValue(typeof(string).FullName, "visibleIf",
                new BilingualData("可选成员名或表达式，用于控制消息框是否显示。",
                    "An optional member name or expression used to control whether the message box is displayed.")),
            new ParameterValue(typeof(bool).FullName, "guiAlwaysEnabled",
                new BilingualData("即使属性被禁用，是否也始终启用消息框。",
                    "Whether the message box is always enabled even if the property is disabled."))
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
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Bilingual Usage",
                DetailInfoBoxBilingualExampleSO.Instance)
        };
    }
}
