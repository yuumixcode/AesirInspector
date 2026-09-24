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
            new BilingualData("无参数，作用于成员本身。",
                "Takes no parameters and applies to the member itself."),
            new BilingualData("与 DisableInInlineEditors 相反：普通 Inspector 中隐藏，仅内联编辑器中可见。",
                "The opposite of DisableInInlineEditors: hidden in regular inspectors, visible only inside inline editors."),
            new BilingualData("适合把内联编辑时才需要的辅助信息暴露出来。",
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
