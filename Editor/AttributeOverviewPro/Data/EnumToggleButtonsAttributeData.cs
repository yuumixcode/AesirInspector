namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// EnumToggleButtons 特性的介绍数据。
    /// </summary>
    internal class EnumToggleButtonsAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("EnumToggleButtons", "EnumToggleButtons",
                "EnumToggleButtons 特性将枚举绘制为一排按钮，提供更直观的交互体验。",
                "The EnumToggleButtons attribute draws an enum as a set of horizontal toggle buttons, providing a more intuitive interaction.",
                OdinInspectorDocumentationLinks.EnumToggleButtonsUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("把枚举绘制成一排按钮而不是下拉框；带有 [Flags] 的位掩码枚举支持多选，再次点击已选中的项会清除该位。",
                "Draws an enum as a row of buttons instead of a dropdown; bitmask enums marked with [Flags] support multi-selection, and clicking a selected member again clears its bit."),
            new BilingualData("位掩码枚举下，右键或按住 Ctrl 点击会直接把值设为该项，而不是切换它的位。",
                "For bitmask enums, right-clicking or Ctrl-clicking sets the value directly to that member instead of toggling its bit."),
            new BilingualData("按钮过宽时会自动折行成多排；配合 [HideLabel] 可让按钮占满整行宽度。",
                "When the buttons do not fit they automatically wrap into multiple rows; combined with [HideLabel] they fill the entire row width."),
            new BilingualData("可通过枚举成员上的 [LabelText] 为每个枚举项设置自定义文本或图标（SdfIconType）。",
                "Use [LabelText] on the enum members to give each one custom text or an icon (SdfIconType).")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            // EnumToggleButtons 没有公开参数
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = { };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                EnumToggleButtonsExampleSO.Instance)
        };
    }
}
