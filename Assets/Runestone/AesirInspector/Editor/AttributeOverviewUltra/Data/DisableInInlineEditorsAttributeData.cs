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
            new BilingualData("仅在 InlineEditor 内部绘制时禁用该属性，普通 Inspector 中不受影响。",
                "Disables the property only while it is drawn inside an InlineEditor; regular inspectors are unaffected."),
            new BilingualData("常与 InlineEditor 配合，避免在内联编辑器中重复修改已经在主 Inspector 中编辑的字段。",
                "Commonly paired with InlineEditor to avoid re-editing, inside the inline editor, fields that are already edited in the main inspector."),
            new BilingualData("与 HideInInlineEditors 相对：一个禁用属性，一个直接隐藏属性。",
                "It is the counterpart of HideInInlineEditors: one disables the property, the other hides it.")
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
