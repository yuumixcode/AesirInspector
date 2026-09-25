using System;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// BilingualButton 特性的介绍数据。
    /// </summary>
    internal class BilingualButtonAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("双语按钮", "Bilingual Button",
                "BilingualButton 是 Aesir 提供的双语按钮特性，用于把方法绘制成同时展示中英文名称的按钮，并在点击时执行该方法。",
                "BilingualButton is Aesir's bilingual button attribute; it draws a method as a button showing both Chinese and English names and invokes the method when clicked.");

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("用于方法，把方法绘制成按钮，点击即调用；带参数的方法会在按钮下方绘制参数输入框（displayParameters）。",
                "Applied to methods, drawing them as buttons that invoke the method on click; methods with parameters show parameter fields below the button (displayParameters)."),
            new BilingualData("中英文名称都支持 $ 成员引用与 @ 表达式解析，并按当前语言设置只显示匹配的一条，切换语言时自动更新。",
                "Both names support $ member references and @ expressions, and only the one matching the current language setting is shown, updating automatically when the language changes."),
            new BilingualData(
                "支持 Odin 按钮的样式选项：ButtonSize / ButtonStyle、SDF 图标与图标对齐、stretch、drawResult、expanded、dirtyOnClick 等。",
                "Supports the Odin button styling options: ButtonSize / ButtonStyle, SDF icon and icon alignment, stretch, drawResult, expanded, dirtyOnClick, and more."),
            new BilingualData("必须硬编码使用特性，不能通过 OdinAttributeProcessor 动态添加。",
                "Must be applied literally in code; it cannot be added dynamically via OdinAttributeProcessor.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(string).FullName, "chineseName",
                new BilingualData("中文按钮名称，同时作为 Odin 按钮的标签。",
                    "The Chinese button name, also used as the Odin button label.")),
            new ParameterValue(typeof(string).FullName, "englishName",
                new BilingualData("英文按钮名称，为空时回落到中文名称。",
                    "The English button name; falls back to the Chinese name when omitted.")),
            new ParameterValue("ButtonSizes", "buttonSize",
                new BilingualData("按钮大小（Small, Medium, Large, Gigantic）。默认 Medium。",
                    "The button size (Small, Medium, Large, Gigantic). Defaults to Medium.")),
            new ParameterValue("ButtonStyle", "style",
                new BilingualData("按钮样式（CompactBox, FoldoutButton, Box）。默认 Box。",
                    "The button style (CompactBox, FoldoutButton, Box). Defaults to Box.")),
            new ParameterValue("SdfIconType", "icon",
                new BilingualData("按钮上显示的 SDF 图标。", "The SDF icon drawn on the button.")),
            new ParameterValue("IconAlignment", "buttonIconAlignment",
                new BilingualData("图标对齐方式。默认 LeftOfText。", "The icon alignment. Defaults to LeftOfText.")),
            new ParameterValue(typeof(int).FullName, "buttonHeight",
                new BilingualData("按钮高度像素值，-1 表示使用 ButtonSize 对应高度。",
                    "The button height in pixels; -1 uses the height of the given ButtonSize.")),
            new ParameterValue(typeof(bool).FullName, "stretch",
                new BilingualData("是否拉伸按钮以占满检查器宽度。默认 true。",
                    "Whether to stretch the button to the inspector width. Defaults to true.")),
            new ParameterValue(typeof(bool).FullName, "drawResult",
                new BilingualData("是否绘制方法返回值。默认 true。",
                    "Whether to draw the method's return value. Defaults to true.")),
            new ParameterValue(typeof(bool).FullName, "expanded",
                new BilingualData("是否默认展开参数区域。默认 false。",
                    "Whether the parameter area starts expanded. Defaults to false.")),
            new ParameterValue(typeof(float).FullName, "buttonAlignment",
                new BilingualData("按钮对齐位置（0-1）。默认 0.5。",
                    "The button alignment position (0-1). Defaults to 0.5.")),
            new ParameterValue(typeof(bool).FullName, "displayParameters",
                new BilingualData("是否显示方法参数输入框。默认 true。",
                    "Whether to display method parameter fields. Defaults to true.")),
            new ParameterValue(typeof(bool).FullName, "dirtyOnClick",
                new BilingualData("点击时是否将对象标记为脏。默认 true。",
                    "Whether to mark the object dirty on click. Defaults to true."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } =
            Array.Empty<ResolvedStringParameterValue>();

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Bilingual Button",
                BilingualButtonExampleSO.Instance)
        };
    }
}
