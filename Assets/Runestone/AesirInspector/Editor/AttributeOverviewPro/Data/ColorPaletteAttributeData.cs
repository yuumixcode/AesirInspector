using System.Collections.Generic;

namespace Runestone.AesirInspector.Editor
{
    internal class ColorPaletteAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("ColorPalette", "ColorPalette", "ColorPalette 特性为 Color 属性提供调色板样式的绘制。",
                "The ColorPalette attribute provides a palette-style drawer for Color properties.",
                "https://odininspector.com/attributes/color-palette-attribute");

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("为 Color 字段（含 Color 数组）提供调色板选择；不指定 PaletteName 时可从所有可用调色板中选择。", "Provides palette selection for Color fields (including Color arrays); when no PaletteName is given, all available palettes can be chosen from."),
            new BilingualData("PaletteName 支持 $ 成员引用与 @ 表达式，可按运行时状态动态切换调色板。", "PaletteName supports $ member references and @ expressions, so the palette can be switched dynamically from runtime state."),
            new BilingualData("颜色值与调色板并不绑定：颜色仍可手动编辑，且修改调色板后已有颜色不会自动更新。", "The color is not bound to the palette: it can still be edited manually, and existing colors do not update when the palette is edited."),
            new BilingualData("调色板在 Tools > Odin > Inspector > Preferences > Drawers > Color Palettes 中配置。", "Palettes are configured under Tools > Odin > Inspector > Preferences > Drawers > Color Palettes.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(bool).FullName, "ShowAlpha",
                new BilingualData("是否显示 Alpha 通道，默认为 true。",
                    "Whether to show the alpha channel, defaults to true.")),
            new ParameterValue(typeof(string).FullName, "PaletteName",
                new BilingualData("调色板的名称。", "The name of the palette."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } =
        {
            new ResolvedStringParameterValue("PaletteName", ResolverType.ValueResolver,
                typeof(string).FullName, "None", new List<ParameterValue>())
        };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                ColorPaletteExampleSO.Instance),
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("PaletteName Resolved",
                ColorPaletteExampleWithPaletteNameSO.Instance)
        };
    }
}
