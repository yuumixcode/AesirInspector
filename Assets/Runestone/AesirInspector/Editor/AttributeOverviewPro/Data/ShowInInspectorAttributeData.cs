namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// ShowInInspector 特性的介绍数据。
    /// </summary>
    internal class ShowInInspectorAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("Show In Inspector", "在检查器中显示",
                "ShowInInspector 特性用于在 Inspector 中显示非序列化的成员，如私有字段、属性或方法返回值。",
                "The ShowInInspector attribute is used to display non-serialized members in the inspector, such as private fields, properties, or method return values.",
                OdinInspectorDocumentationLinks.ShowInInspectorUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("在 Inspector 中显示本来不会显示的成员，如私有字段、C# 属性与静态成员。",
                "Displays members that would otherwise not appear in the inspector, such as private fields, C# properties, and static members."),
            new BilingualData("只负责显示，不会序列化：仅用 [ShowInInspector] 的成员改动不会被保存，需配合 [SerializeField] 或 [OdinSerialize]。",
                "Display only, not serialization: changes to a member marked only with [ShowInInspector] are not saved and require [SerializeField] or [OdinSerialize]."),
            new BilingualData("常与 [ReadOnly] 组合，用于实时查看调试值（如 [ShowInInspector, ReadOnly] private int currentHealth;）。",
                "Often combined with [ReadOnly] to inspect live debug values (e.g. [ShowInInspector, ReadOnly] private int currentHealth;).")
        };

        public override ParameterValue[] AttributeParameters { get; set; } = null;

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = null;

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                ShowInInspectorExampleSO.Instance)
        };
    }
}
