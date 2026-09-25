using System.Collections.Generic;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// GUIColor 特性的介绍数据，包含标题、参数说明、解析字符串参数和案例预览项。
    /// </summary>
    internal class GUIColorAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("GUIColor", "GUIColor", "GUIColor 特性用于改变 GUI 元素的颜色。它可以用于突出特殊的、重要的字段。",
                "GUIColor is used to change the color of GUI elements. It can be used to highlight special or important fields.",
                OdinInspectorDocumentationLinks.GuiColorUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("可以用 r、g、b、a 直接指定颜色（三参数构造重载中 a 默认为 1），也可以用 getColor 以字符串解析颜色。", "The color can be specified directly with r, g, b and a (the three-argument constructor overload defaults a to 1), or resolved from a string with getColor."),
            new BilingualData("getColor 支持 $ 成员引用与 @ 表达式，例如 \"@useRed ? UnityEngine.Color.red : UnityEngine.Color.green\" 可根据条件动态改变颜色。", "getColor supports $ member references and @ expressions, so for example \"@useRed ? UnityEngine.Color.red : UnityEngine.Color.green\" changes the color dynamically based on a condition."),
            new BilingualData("颜色作用于整个属性的绘制（包含标签与按钮），因此常用来突出重要字段或按钮。", "The color applies to the drawing of the whole property, including its label and buttons, which makes it handy for highlighting important fields or buttons.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } = new ParameterValue[5]
        {
            new ParameterValue(typeof(float).FullName, "r",
                new BilingualData("红色通道 (0-1)。", "Red channel (0-1).")),
            new ParameterValue(typeof(float).FullName, "g",
                new BilingualData("绿色通道 (0-1)。", "Green channel (0-1).")),
            new ParameterValue(typeof(float).FullName, "b",
                new BilingualData("蓝色通道 (0-1)。", "Blue channel (0-1).")),
            new ParameterValue(typeof(float).FullName, "a",
                new BilingualData("Alpha 通道 (0-1)。", "Alpha channel (0-1).")),
            new ParameterValue(typeof(string).FullName, "getColor",
                new BilingualData(
                    "支持多种颜色格式，包括命名颜色（例如 \"red\"、\"orange\"、\"green\"、\"blue\"）、十六进制代码（例如 \"#FF0000\" 和 \"#FF0000FF\"）以及 RGBA（例如 \"RGBA(1,1,1,1)\"）或 RGB（例如 \"RGB(1,1,1)\"），包括 Odin 特性表达式（例如 \"@this.MyColor\"）。以下是可用的命名颜色：black, blue, clear, cyan, gray, green, grey, magenta, orange, purple, red, transparent, transparentBlack, transparentWhite, white, yellow, lightblue, lightcyan, lightgray, lightgreen, lightgrey, lightmagenta, lightorange, lightpurple, lightred, lightyellow, darkblue, darkcyan, darkgray, darkgreen, darkgrey, darkmagenta, darkorange, darkpurple, darkred, darkyellow。",
                    "Supports a variety of color formats, including named colors (e.g. \"red\", \"orange\", \"green\", \"blue\"), hex codes (e.g. \"#FF0000\" and \"#FF0000FF\"), and RGBA (e.g. \"RGBA(1,1,1,1)\") or RGB (e.g. \"RGB(1,1,1)\"), including Odin attribute expressions (e.g \"@this.MyColor\"). Here are the available named colors: black, blue, clear, cyan, gray, green, grey, magenta, orange, purple, red, transparent, transparentBlack, transparentWhite, white, yellow, lightblue, lightcyan, lightgray, lightgreen, lightgrey, lightmagenta, lightorange, lightpurple, lightred, lightyellow, darkblue, darkcyan, darkgray, darkgreen, darkgrey, darkmagenta, darkorange, darkpurple, darkred, darkyellow."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } =
        {
            new ResolvedStringParameterValue("Get Color", ResolverType.ValueResolver, typeof(Color).FullName,
                "None", new List<ParameterValue>
                {
                    new ParameterValue("T", "$value",
                        new BilingualData("应用此特性的成员的值。",
                            "The value of the member that has the attribute applied to it."))
                })
        };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Parameters",
                GUIColorExampleSO.Instance),
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("GetColor",
                GUIColorExampleWithColorSO.Instance)
        };
    }
}
