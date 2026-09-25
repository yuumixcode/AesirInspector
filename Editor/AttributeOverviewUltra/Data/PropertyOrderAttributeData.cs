namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// PropertyOrder 特性的介绍数据。
    /// </summary>
    internal class PropertyOrderAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("PropertyOrder", "PropertyOrder",
                "PropertyOrder 特性用于自定义检查器中属性和方法的绘制顺序。",
                "The PropertyOrder attribute is used to customize the drawing order of properties and methods in the inspector.",
                OdinInspectorDocumentationLinks.PropertyOrderUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("数值越小越靠前绘制，默认 0；使用负数可确保成员始终显示在最上方。",
                "Lower values are drawn first, with 0 as the default; negative values ensure a member always appears at the top."),
            new BilingualData("适用于字段、属性以及方法，包括 [Button] 和 [OnInspectorGUI] 方法。",
                "Applies to fields, properties and methods, including [Button] and [OnInspectorGUI] methods."),
            new BilingualData("Order 相同时按类别排序：字段 → 属性 → 方法；分组（如 [BoxGroup]）未指定 Order 时沿用其首个成员的 Order。",
                "When orders tie, members are sorted by category: fields, then properties, then methods; a group (such as [BoxGroup]) with no order of its own inherits the order of its first member.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(float).FullName, "order",
                new BilingualData("绘制顺序的数值。默认为 0。", "The numeric value of the drawing order. Defaults to 0."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = { };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Usage Examples",
                PropertyOrderExampleSO.Instance)
        };
    }
}
