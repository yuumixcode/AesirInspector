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
            new BilingualData("只能标记方法（按钮），会按可用宽度自动换行并调整按钮宽度，与 ButtonGroup 的固定横排不同。",
                "Can only be applied to methods (buttons); it wraps and resizes buttons according to the available width, unlike ButtonGroup's fixed row."),
            new BilingualData(
                "UniformLayout = true 时同排按钮宽度统一（取最长名称）；DefaultButtonSize 设置组内默认按钮尺寸，默认为 Medium。",
                "With UniformLayout = true all buttons in a row share the same width (based on the longest name); DefaultButtonSize sets the group's default button size, which is Medium by default."),
            new BilingualData("组名可用 / 嵌套到其他分组之下（如 \"SomeGroup/SomeBtnGroup\"），从而嵌入 FoldoutGroup、TabGroup 等。",
                "The group name may use / to nest under other groups (such as \"SomeGroup/SomeBtnGroup\"), embedding it in FoldoutGroup, TabGroup, and similar.")
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
                new BilingualData("组内按钮的默认尺寸。", "The default button size used inside the group."))
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
