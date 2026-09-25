namespace Runestone.AesirInspector.Editor
{
    internal class DisableInPlayModeAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("DisableInPlayMode", "DisableInPlayMode",
                "DisableInPlayMode 特性使属性在 Play 模式下禁用。",
                "The DisableInPlayMode attribute disables a property while in Play mode.",
                OdinInspectorDocumentationLinks.DisableInPlayModeUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("属性在 Play 模式下被禁用，防止运行时误改，退出 Play 模式后恢复可编辑。",
                "The property is disabled in Play mode to prevent accidental runtime edits, and becomes editable again once you leave Play mode."),
            new BilingualData("特性带 [Conditional(\"UNITY_EDITOR\")]，只在编辑器中生效，不会进入构建。",
                "The attribute is marked with [Conditional(\"UNITY_EDITOR\")], so it only takes effect in the editor and is stripped from builds."),
            new BilingualData("与 DisableInEditorMode 正好相反；若要隐藏而不是禁用属性，请使用 HideInPlayMode。",
                "It is the mirror image of DisableInEditorMode; to hide rather than disable a property, use HideInPlayMode.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } = { };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = { };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                DisableInPlayModeExampleSO.Instance)
        };
    }
}
