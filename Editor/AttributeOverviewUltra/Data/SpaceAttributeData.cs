using System;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// Unity Space 特性的介绍数据。
    /// </summary>
    internal class SpaceAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("Space", "Space",
                "Space 是 Unity 内置特性，在字段之间插入垂直间距以分隔内容。Odin 完整沿用该特性，并支持自定义间距高度。",
                "Space is a Unity built-in attribute that inserts vertical spacing between fields to separate content. Odin fully supports it with a customizable height.",
                OdinInspectorDocumentationLinks.SpaceUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("Unity 内置特性，只能标记字段；Odin 完全沿用其绘制行为，在被标记字段上方插入间距。",
                "A Unity built-in attribute that can only mark fields; Odin keeps its drawing behavior and inserts the spacing above the marked field."),
            new BilingualData("需要作用于属性或方法时，改用 Odin 的 PropertySpace。",
                "Use Odin's PropertySpace when the spacing must be applied to properties or methods."),
            new BilingualData("PropertySpace 可分别设置前后间距（SpaceBefore / SpaceAfter），并支持负值来收紧间距。",
                "PropertySpace can set the spacing before and after separately (SpaceBefore / SpaceAfter), and supports negative values to tighten the spacing.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(float).FullName, "height",
                new BilingualData("间距高度（像素），默认 8。", "The spacing height in pixels; defaults to 8."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } =
            Array.Empty<ResolvedStringParameterValue>();

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Space",
                SpaceExampleSO.Instance)
        };
    }
}
