using System;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// ShowInInlineEditors 特性的介绍数据。
    /// </summary>
    internal class ShowInInlineEditorsAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("ShowInInlineEditors", "ShowInInlineEditors",
                "ShowInInlineEditors 特性用于让属性只在 InlineEditor 内部绘制时显示。",
                "The ShowInInlineEditors attribute shows a property only while it is drawn inside an InlineEditor.",
                "https://odininspector.com/attributes/show-in-inline-editors-attribute");

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("无参数；仅当属性绘制在 InlineEditor 内部时显示，普通 Inspector 中隐藏。",
                "Takes no parameters; the property is shown only while drawn inside an InlineEditor and is hidden in regular inspectors."),
            new BilingualData("与 HideInInlineEditors 相反；若希望内联编辑时禁用而非隐藏，应使用 DisableInInlineEditors。",
                "The opposite of HideInInlineEditors; to disable rather than hide the property inside inline editors, use DisableInInlineEditors."),
            new BilingualData("适合暴露只在内联编辑时有用的辅助信息。",
                "Useful for exposing helper information that only matters while editing inline.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } = Array.Empty<ParameterValue>();

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } =
            Array.Empty<ResolvedStringParameterValue>();

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Show In Inline Editors",
                ShowInInlineEditorExampleSO.Instance)
        };
    }
}
