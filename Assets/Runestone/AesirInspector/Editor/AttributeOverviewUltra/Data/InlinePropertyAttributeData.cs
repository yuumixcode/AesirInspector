namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// InlineProperty 特性的介绍数据。
    /// </summary>
    internal class InlinePropertyAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("InlineProperty", "InlineProperty",
                "InlineProperty 特性用于将类或结构体的内容直接显示在其父级属性的同一行（或紧凑地显示），而不是显示为折叠组。",
                "The InlineProperty attribute is used to display the content of a class or struct inline with its parent property, instead of as a foldout group.",
                OdinInspectorDocumentationLinks.InlinePropertyUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("将类型的内容直接排布在父属性标签旁，而不是绘制成可折叠组。",
                "Places the contents of a type next to its parent label instead of rendering them as a foldout group."),
            new BilingualData("既可标注在类型定义上（如 [Serializable] 结构体），也可标注在具体成员上。",
                "Can be applied to a type definition (such as a [Serializable] struct) or to an individual member."),
            new BilingualData("配合 HorizontalGroup 与 HideLabel，可做出类似 Vector2 的单行紧凑布局。",
                "Combined with HorizontalGroup and HideLabel, it produces compact single-line layouts similar to Vector2."),
            new BilingualData("LabelWidth 用于统一设置所有内联子属性的标签宽度。",
                "LabelWidth sets the label width for all inlined child properties.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(float).FullName, "LabelWidth",
                new BilingualData("内联属性的标签宽度。", "The label width of the inline properties."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = { };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                InlinePropertyExampleSO.Instance)
        };
    }
}
