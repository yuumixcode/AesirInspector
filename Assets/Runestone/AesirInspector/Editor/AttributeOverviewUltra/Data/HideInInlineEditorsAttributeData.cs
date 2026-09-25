namespace Runestone.AesirInspector.Editor
{
    internal class HideInInlineEditorsAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("HideInInlineEditors", "HideInInlineEditors",
                "HideInInlineEditors 特性使属性在 InlineEditor 中隐藏。",
                "The HideInInlineEditors attribute hides a property when displayed within an InlineEditor.",
                OdinInspectorDocumentationLinks.HideInPlayModeUrl); // 暂时借用，链接不带s

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("无参数，属性在 InlineEditor 内联绘制的编辑器中隐藏，在常规 Inspector 中照常显示。",
                "Takes no parameters; the property is hidden inside editors drawn by InlineEditor but still shown in a regular inspector."),
            new BilingualData("仅在配合 InlineEditor 使用、成员被内联展开时才会生效。",
                "Only takes effect when used together with InlineEditor and the member is drawn inline."),
            new BilingualData("与 DisableInInlineEditors（禁用）相对，与 ShowInInlineEditors（仅内联时显示）互补。",
                "Opposite to DisableInInlineEditors (which disables) and complementary to ShowInInlineEditors (which shows only when inline).")
        };

        public override ParameterValue[] AttributeParameters { get; set; } = { };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = { };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                HideInInlineEditorsExampleSO.Instance)
        };
    }
}
