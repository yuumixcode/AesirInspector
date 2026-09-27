using System.Collections.Generic;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// Button 特性的介绍数据。
    /// </summary>
    internal class ButtonAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("Button", "Button", "Button 特性用于将方法直接绘制成一个检查器中的按钮。点击按钮将调用该方法。",
                "The Button attribute is used to directly draw a method as a button in the inspector. Clicking the button will call the method.",
                OdinInspectorDocumentationLinks.ButtonUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("把方法绘制成按钮，点击即调用；带参数的方法会在按钮下方显示参数输入框，Expanded 可让其默认展开。",
                "Draws a method as a button that invokes it on click; methods with parameters show parameter fields below the button, which Expanded can expand by default."),
            new BilingualData("按钮名称支持 $ 成员引用与 @ 表达式解析。",
                "The button name supports $ member references and @ expressions."),
            new BilingualData("可自定义按钮外观：ButtonSize / ButtonHeight、ButtonStyle、SDF 图标与图标对齐、Stretch 拉伸等。",
                "Customizes the button appearance: ButtonSize / ButtonHeight, ButtonStyle, SDF icon and icon alignment, Stretch, and more."),
            new BilingualData("ButtonAlignment 仅在 Stretch 为 false 时生效；DirtyOnClick 为 false 会同时禁用按钮操作产生的撤销。",
                "ButtonAlignment only takes effect when Stretch is false; setting DirtyOnClick to false also disables undo for changes caused by the button.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(string).FullName, "name",
                new BilingualData("按钮显示的名称字符串。", "The name string displayed on the button.")),
            new ParameterValue("ButtonSizes", "buttonSize",
                new BilingualData("按钮的大小（Small, Medium, Large, Gigantic）。也可以直接填入 int 指定高度。",
                    "The size of the button (Small, Medium, Large, Gigantic). You can also directly input an int to specify the height.")),
            new ParameterValue("ButtonStyle", "parameterBtnStyle",
                new BilingualData("按钮的样式样式类型。", "The style type of the button.")),
            new ParameterValue("SdfIconType", "icon",
                new BilingualData("在按钮上显示的图标。", "The icon displayed on the button.")),
            new ParameterValue("IconAlignment", "iconAlignment",
                new BilingualData("图标的对齐方式。", "The alignment of the icon.")),
            new ParameterValue(typeof(bool).FullName, "Stretch",
                new BilingualData("是否拉伸按钮宽度以占满检查器宽度。默认为 true。",
                    "Whether to stretch the button width to fill the inspector width. Defaults to true."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } =
        {
            new ResolvedStringParameterValue("Name", ResolverType.ValueResolver, typeof(string).FullName,
                "None", new List<ParameterValue>())
        };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Usage Examples",
                ButtonExampleSO.Instance)
        };
    }
}
