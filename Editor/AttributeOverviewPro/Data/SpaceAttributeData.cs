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
            new BilingualData("Unity 内置特性，Odin 沿用其间距绘制行为。",
                "A Unity built-in attribute whose spacing behavior is kept by Odin."),
            new BilingualData("只能作用于字段（Unity 限制），若需作用于属性请使用 Odin 的 PropertySpace。",
                "Can only be applied to fields (a Unity limitation); use Odin's PropertySpace for properties."),
            new BilingualData("PropertySpace 还支持分别设置前后间距（SpaceBefore / SpaceAfter）。",
                "PropertySpace additionally supports separate before/after spacing (SpaceBefore / SpaceAfter).")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(float).FullName, "height",
                new BilingualData("间距高度（像素），默认 8。",
                    "The spacing height in pixels; defaults to 8."))
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
