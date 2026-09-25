using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// InlineEditor 特性的介绍数据。
    /// </summary>
    internal class InlineEditorAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("InlineEditor", "InlineEditor",
                "InlineEditor 特性用于在当前属性下方直接嵌入另一个对象的编辑器面板。",
                "The InlineEditor attribute is used to embed the editor of another object directly below the property.",
                OdinInspectorDocumentationLinks.InlineEditorUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("作用于类型继承自 UnityEngine.Object 的字段，在属性下方内联嵌入其编辑器面板。",
                "Applies to fields whose type derives from UnityEngine.Object and embeds that object's editor panel below the property."),
            new BilingualData("适合内联编辑 ScriptableObject、Material 等资源，减少在独立窗口中来回切换。",
                "Ideal for inline editing of assets such as ScriptableObjects and Materials, avoiding back-and-forth window switching."),
            new BilingualData("InlineEditorModes 可组合出仅 GUI、带标题、带预览或完整编辑器等显示形态。",
                "InlineEditorModes produces different layouts, such as GUI only, with header, with preview, or the full editor."),
            new BilingualData("配合 HideInInlineEditors / DisableInInlineEditors，可控制成员在内联面板中的可见性与可编辑性。",
                "Combined with HideInInlineEditors / DisableInInlineEditors, it controls the visibility and editability of members inside the inline panel.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(InlineEditorModes).FullName, "inlineEditorMode",
                new BilingualData("编辑器显示模式。默认值为 GUIOnly。",
                    "The mode in which the editor should be drawn. Default is GUIOnly.")),
            new ParameterValue(typeof(InlineEditorObjectFieldModes).FullName, "objectFieldMode",
                new BilingualData("对象字段的绘制模式。默认值为 Boxed。",
                    "The mode in which the object field should be drawn. Default is Boxed."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = { };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                InlineEditorExampleSO.Instance)
        };
    }
}
