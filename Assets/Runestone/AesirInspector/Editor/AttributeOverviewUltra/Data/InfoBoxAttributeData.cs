using System.Collections.Generic;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// InfoBox 特性的介绍数据。
    /// </summary>
    internal class InfoBoxAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("InfoBox", "InfoBox", "InfoBox 特性用于在属性上方绘制一个消息框，用于提供提示、警告或错误信息。",
                "The InfoBox attribute is used to draw a message box above a property to provide tips, warnings, or error information.",
                OdinInspectorDocumentationLinks.InfoBoxUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("在属性上方绘制消息框，用于提示、警告或错误信息。",
                "Draws a message box above the property for tips, warnings or errors."),
            new BilingualData("message 与 VisibleIf 都支持 $ 成员引用和 @ 表达式，可实现动态文本与条件显示。",
                "Both message and VisibleIf support $ member references and @ expressions for dynamic text and conditional visibility."),
            new BilingualData("Warning / Error 级别的消息框会被 Odin Scene Validator 追踪，可用于项目校验。",
                "Warning and Error level message boxes are tracked by the Odin Scene Validator and can be used for project validation."),
            new BilingualData("需要可展开的详细说明时，改用 DetailedInfoBox。",
                "Use DetailedInfoBox instead when you need an expandable, more detailed description.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(string).FullName, "message",
                new BilingualData("消息框中显示的文本。", "The text displayed in the message box.")),
            new ParameterValue("InfoMessageType", "infoMessageType",
                new BilingualData("消息框的类型（Info, Warning, Error, None）。",
                    "The type of the message box (Info, Warning, Error, None).")),
            new ParameterValue(typeof(string).FullName, "visibleIfMemberName",
                new BilingualData("可选成员名或表达式，用于控制消息框是否显示。",
                    "An optional member name or expression used to control whether the message box is displayed.")),
            new ParameterValue(typeof(bool).FullName, "GUIAlwaysEnabled",
                new BilingualData("即使属性被禁用，是否也始终启用消息框。",
                    "Whether the message box is always enabled even if the property is disabled.")),
            new ParameterValue("SdfIconType", "icon",
                new BilingualData("要在消息框中显示的自定义图标。", "A custom icon to display in the message box.")),
            new ParameterValue(typeof(string).FullName, "IconColor",
                new BilingualData(
                    "支持多种颜色格式，包括命名颜色、十六进制代码和 RGBA，包括 Odin 特性表达式。以下是可用的命名颜色：black, blue, clear, cyan, gray, green, grey, magenta, orange, purple, red, transparent, transparentBlack, transparentWhite, white, yellow, lightblue, lightcyan, lightgray, lightgreen, lightgrey, lightmagenta, lightorange, lightpurple, lightred, lightyellow, darkblue, darkcyan, darkgray, darkgreen, darkgrey, darkmagenta, darkorange, darkpurple, darkred, darkyellow。",
                    "Supports a variety of color formats, including named colors, hex codes, and RGBA, including Odin attribute expressions. Here are the available named colors: black, blue, clear, cyan, gray, green, grey, magenta, orange, purple, red, transparent, transparentBlack, transparentWhite, white, yellow, lightblue, lightcyan, lightgray, lightgreen, lightgrey, lightmagenta, lightorange, lightpurple, lightred, lightyellow, darkblue, darkcyan, darkgray, darkgreen, darkgrey, darkmagenta, darkorange, darkpurple, darkred, darkyellow."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } =
        {
            new ResolvedStringParameterValue("Message", ResolverType.ValueResolver, typeof(string).FullName,
                "None", new List<ParameterValue>()),
            new ResolvedStringParameterValue("Visible If", ResolverType.ValueResolver, typeof(bool).FullName,
                "None", new List<ParameterValue>())
        };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                InfoBoxExampleSO.Instance),
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Message Expression",
                InfoBoxExampleWithMessageSO.Instance),
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("VisibleIf Expression",
                InfoBoxExampleWithVisibleIfSO.Instance)
        };
    }
}
