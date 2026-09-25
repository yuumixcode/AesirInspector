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
            new BilingualData("在属性上方绘制可折叠的消息框，主消息与详细内容都是解析字符串，支持 $ 成员引用与 @ 表达式，可按状态动态显示。",
                "Draws a collapsible message box above the property; both the main message and the detailed content are resolved strings supporting $ member references and @ expressions, so they can reflect runtime state."),
            new BilingualData("支持 Info / Warning / Error / None 四种消息类型；设置了 visibleIf 时，Warning / Error 级别的消息框会被 Odin Scene Validator 追踪，可用于项目校验。",
                "Supports the Info / Warning / Error / None message types; when visibleIf is set, Warning and Error level boxes are tracked by the Odin Scene Validator and can be used for project validation."),
            new BilingualData("visibleIf 可传成员名或 @ 表达式（为空则始终显示）；需要更轻量的提示时改用 InfoBox。",
                "visibleIf accepts a member name or @ expression (empty always shows); use InfoBox instead when a lighter hint is enough.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(string).FullName, "message",
                new BilingualData("消息框中显示的主消息文本。",
                    "The main message text displayed in the message box.")),
            new ParameterValue(typeof(string).FullName, "details",
                new BilingualData("展开后显示的详细内容文本。",
                    "The detailed content text shown when the message box is expanded.")),
            new ParameterValue("InfoMessageType", "infoMessageType",
                new BilingualData("消息框的类型（Info, Warning, Error, None）。默认 Info。",
                    "The type of the message box (Info, Warning, Error, None). Defaults to Info.")),
            new ParameterValue(typeof(string).FullName, "visibleIf",
                new BilingualData("可选成员名或表达式，用于控制消息框是否显示（为空则始终显示）。",
                    "An optional member name or expression used to control whether the message box is displayed; empty always shows."))
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
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Message",
                DetailedInfoBoxExampleWithMessageSO.Instance),
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Details",
                DetailedInfoBoxExampleWithDetailsSO.Instance),
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Visible If",
                DetailedInfoBoxExampleWithVisibleIfSO.Instance)
        };
    }
}
