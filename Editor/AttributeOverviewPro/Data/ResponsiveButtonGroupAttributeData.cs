using System;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// ResponsiveButtonGroup 特性的介绍数据。
    /// </summary>
    internal class ResponsiveButtonGroupAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("ResponsiveButtonGroup", "ResponsiveButtonGroup",
                "ResponsiveButtonGroup 特性把多个按钮组合成一组，并根据可用空间自动换行、调整按钮宽度。",
                "The ResponsiveButtonGroup attribute groups buttons together and wraps or resizes them based on the available layout space.",
                "https://odininspector.com/attributes/responsive-button-group-attribute");

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("与 ButtonGroup 的固定横排不同，按钮组会随窗口宽度自动换行。",
                "Unlike ButtonGroup's fixed horizontal row, this group wraps as the window width changes."),
            new BilingualData("UniformLayout 让组内按钮宽度统一；DefaultButtonSize 设定默认按钮尺寸。",
                "UniformLayout makes all buttons the same width; DefaultButtonSize sets the default button size."),
            new BilingualData("组名可包含 / 以嵌套到其他分组（如 FoldoutGroup）之下。",
                "Group names may contain / to nest under other groups (such as FoldoutGroup).")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(string).FullName, "group",
                new BilingualData("按钮组的名称，默认 _DefaultResponsiveButtonGroup。",
                    "The button group name; defaults to _DefaultResponsiveButtonGroup.")),
            new ParameterValue(typeof(bool).FullName, "UniformLayout",
                new BilingualData("是否让组内按钮宽度统一（以最长名称为准）。",
                    "Whether all buttons share the same width (based on the longest name).")),
            new ParameterValue("ButtonSizes", "DefaultButtonSize",
                new BilingualData("组内按钮的默认尺寸。",
                    "The default button size used inside the group."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } =
            Array.Empty<ResolvedStringParameterValue>();

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Responsive Button Group",
                ResponsiveButtonGroupExampleSO.Instance)
        };
    }
}
