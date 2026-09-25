namespace Runestone.AesirInspector.Editor
{
    internal class DisableInEditorModeAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("DisableInEditorMode", "DisableInEditorMode",
                "DisableInEditorMode 特性使属性在 Editor 模式下禁用。",
                "The DisableInEditorMode attribute disables a property while in Editor mode.",
                OdinInspectorDocumentationLinks.DisableInEditorModeUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("属性在编辑模式（非 Play 模式）下被禁用，进入 Play 模式后恢复可编辑，适合只在运行时调整的调试数据。",
                "The property is disabled in editor mode (when not playing) and becomes editable again in Play mode, which suits debug data you only tweak at runtime."),
            new BilingualData("特性带 [Conditional(\"UNITY_EDITOR\")]，只在编辑器中生效，不会进入构建。",
                "The attribute is marked with [Conditional(\"UNITY_EDITOR\")], so it only takes effect in the editor and is stripped from builds."),
            new BilingualData("与 DisableInPlayMode 正好相反；若要隐藏而不是禁用属性，请使用 HideInEditorMode。",
                "It is the mirror image of DisableInPlayMode; to hide rather than disable a property, use HideInEditorMode.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } = { };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = { };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                DisableInEditorModeExampleSO.Instance)
        };
    }
}
