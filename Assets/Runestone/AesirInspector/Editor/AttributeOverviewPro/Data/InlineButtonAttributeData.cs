using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// InlineButton 特性的介绍数据。
    /// </summary>
    internal class InlineButtonAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("InlineButton", "InlineButton", "InlineButton 特性用于在属性值的右侧绘制一个按钮。",
                "The InlineButton attribute draws a button to the right of the property value.",
                OdinInspectorDocumentationLinks.InlineButtonUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("在属性值右侧绘制按钮，点击时执行 action 指定的方法或表达式。",
                "Draws a button to the right of the property value; clicking it invokes the method or expression specified by action."),
            new BilingualData("action 支持成员引用（$）与表达式（@）；Label 省略时默认使用方法名拆分后的文本。",
                "action supports member references ($) and expressions (@); when Label is omitted, the split PascalCase form of the method name is used."),
            new BilingualData("可通过 ShowIf 控制按钮的显示条件，用 Icon、ButtonColor、TextColor 自定义外观。",
                "ShowIf controls when the button appears, while Icon, ButtonColor and TextColor customize its appearance."),
            new BilingualData("允许在同一属性上标注多个 InlineButton，按钮会依次排列。",
                "Multiple InlineButton attributes can be applied to one property, and the buttons are laid out in order.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(string).FullName, "action",
                new BilingualData("点击时触发的方法名或表达式。", "The method name or expression to trigger on click.")),
            new ParameterValue(typeof(string).FullName, "label",
                new BilingualData("按钮显示的文本。默认使用方法名。",
                    "The label to display on the button. Defaults to the method name.")),
            new ParameterValue(typeof(SdfIconType).FullName, "icon",
                new BilingualData("按钮显示的图标。", "The icon to display on the button.")),
            new ParameterValue(typeof(string).FullName, "ShowIf",
                new BilingualData("控制按钮显示的条件表达式。",
                    "The condition expression that controls the visibility of the button.")),
            new ParameterValue(typeof(string).FullName, "ButtonColor",
                new BilingualData("按钮的背景颜色。", "The background color of the button.")),
            new ParameterValue(typeof(string).FullName, "TextColor",
                new BilingualData("按钮的文字颜色。", "The text color of the button."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } =
        {
            new ResolvedStringParameterValue("Action", ResolverType.ActionResolver, "void", "None",
                new List<ParameterValue>()),
            new ResolvedStringParameterValue("ShowIf", ResolverType.ValueResolver, typeof(bool).FullName,
                "None", new List<ParameterValue>())
        };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                InlineButtonExampleSO.Instance)
        };
    }
}
