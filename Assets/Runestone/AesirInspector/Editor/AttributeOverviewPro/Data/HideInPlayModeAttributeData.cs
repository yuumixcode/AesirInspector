namespace Runestone.AesirInspector.Editor
{
    internal class HideInPlayModeAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("HideInPlayMode", "HideInPlayMode",
                "HideInPlayMode 特性使属性在 Play 模式下隐藏。",
                "The HideInPlayMode attribute hides a property while in Play mode.",
                OdinInspectorDocumentationLinks.HideInPlayModeUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("无参数，属性仅在 Editor 模式下可见，进入 Play 模式后隐藏。",
                "Takes no parameters; the property is only visible in Editor mode and hidden once Play mode starts."),
            new BilingualData("适合只在编辑期配置、运行时不需要显示的数据。",
                "Suitable for data configured only in the editor that does not need to be shown at runtime."),
            new BilingualData("与 HideInEditorMode 相对，二者分别控制编辑期与运行期的可见性。",
                "Opposite to HideInEditorMode; the two control visibility in editor mode and play mode respectively.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } = { };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = { };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                HideInPlayModeExampleSO.Instance)
        };
    }
}
