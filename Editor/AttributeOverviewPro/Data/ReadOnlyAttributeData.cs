namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// ReadOnly 特性的介绍数据。
    /// </summary>
    internal class ReadOnlyAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("ReadOnly", "ReadOnly", "ReadOnly 特性使属性在检查器面板中显示为只读状态。",
                "The ReadOnly attribute makes a property appear as read-only in the inspector panel.",
                OdinInspectorDocumentationLinks.ReadOnlyUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("只影响检查器面板中的编辑，脚本逻辑中仍可自由修改该值。",
                "Only affects editing in the inspector panel; the value can still be modified freely in script logic."),
            new BilingualData("可作用于字段、属性以及集合类型（如 List 或 Array）。",
                "Can be applied to fields, properties, and collection types (such as List or Array)."),
            new BilingualData("与 [ShowInInspector] 组合时，可让非序列化的私有字段或属性以只读方式显示，便于实时调试。",
                "Combined with [ShowInInspector], it displays non-serialized private fields or properties as read-only for live debugging.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } = { };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = { };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Usage Examples",
                ReadOnlyExampleSO.Instance)
        };
    }
}
