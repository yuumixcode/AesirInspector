namespace Runestone.AesirInspector.Editor
{
    internal class DelayedPropertyAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("DelayedProperty", "DelayedProperty",
                "DelayedProperty 特性延迟属性值的更新，直到用户按下回车键或输入框失去焦点。",
                "The DelayedProperty attribute delays the update of a property value until the user presses enter or the input field loses focus.",
                OdinInspectorDocumentationLinks.DelayedUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("延迟属性值更新，直到用户按下回车或输入框失去焦点；适合配合 OnValueChanged 避免打字过程中频繁触发昂贵操作。", "Delays applying the value until the user presses enter or the field loses focus; useful together with OnValueChanged to avoid triggering expensive work while typing."),
            new BilingualData("与 Unity 自带的 [Delayed] 功能类似，但 DelayedProperty 还能作用于属性（Property），而不限于字段。", "Similar to Unity's built-in [Delayed], but DelayedProperty can also be applied to properties, not only to fields.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } = { };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = { };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                DelayedPropertyExampleSO.Instance)
        };
    }
}
