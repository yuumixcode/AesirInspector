using System;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// DisableInInlineEditors 特性的介绍数据。
    /// </summary>
    internal class DisableInInlineEditorsAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("DisableInInlineEditors", "DisableInInlineEditors",
                "DisableInInlineEditors 特性用于在 InlineEditor 内部绘制时禁用该属性。",
                "The DisableInInlineEditors attribute disables a property while it is drawn inside an InlineEditor.",
                "https://odininspector.com/attributes/disable-in-inline-editors-attribute");

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("无参数，作用于成员本身。",
                "Takes no parameters and applies to the member itself."),
            new BilingualData("仅在 InlineEditor 内部生效，在普通 Inspector 中不受影响。",
                "Only takes effect inside an InlineEditor; regular inspectors are unaffected."),
            new BilingualData("常与 InlineEditor 配合，避免在内联编辑器中重复修改嵌套对象的字段。",
                "Commonly paired with InlineEditor to avoid editing nested object fields inside the inline editor.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } = Array.Empty<ParameterValue>();

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } =
            Array.Empty<ResolvedStringParameterValue>();

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Disable In Inline Editors",
                DisableInInlineEditorExampleSO.Instance)
        };
    }
}
