namespace Runestone.AesirInspector.Editor
{
    internal class HideInEditorModeAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("HideInEditorMode", "HideInEditorMode",
                "HideInEditorMode 特性使属性在 Editor 模式下隐藏。",
                "The HideInEditorMode attribute hides a property while in Editor mode.",
                OdinInspectorDocumentationLinks.HideInEditorModeUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("无参数，属性仅在 Play 模式下可见，Editor 模式下隐藏。",
                "Takes no parameters; the property is only visible in Play mode and hidden in Editor mode."),
            new BilingualData("适合只在运行时产生意义、编辑期无需编辑的调试数据。",
                "Suitable for runtime-only debug data that does not need editing in the editor."),
            new BilingualData("与 HideInPlayMode 相对，二者分别控制运行期与编辑期的可见性。",
                "Opposite to HideInPlayMode; the two control visibility in play mode and editor mode respectively.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } = { };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = { };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                HideInEditorModeExampleSO.Instance)
        };
    }
}
