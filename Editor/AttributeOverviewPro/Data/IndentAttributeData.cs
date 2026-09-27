namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// Indent 特性的介绍数据。
    /// </summary>
    internal class IndentAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("Indent", "缩进", "Indent 特性允许在 Inspector 中对属性进行缩进。你可以指定缩进的层级。",
                "The Indent attribute allows for indenting properties in the inspector. You can specify the level of indentation.",
                OdinInspectorDocumentationLinks.IndentUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("在 Inspector 中为属性增加缩进，用于体现属性之间的从属关系。",
                "Adds indentation to a property in the inspector to express its logical subordination to other properties."),
            new BilingualData("IndentLevel 默认为 1；负值表示相对当前层级减少缩进。",
                "IndentLevel defaults to 1; a negative value reduces indentation relative to the current level."),
            new BilingualData("允许在同一属性上叠加多个 Indent，缩进量会依次累加。",
                "Multiple Indent attributes can be stacked on one property, and their indentation levels accumulate.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } = new ParameterValue[1]
        {
            new ParameterValue(typeof(int).FullName, "IndentLevel",
                new BilingualData("缩进的层级。默认值为 1。", "The level of indentation. Default value is 1."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = null;

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                IndentExampleSO.Instance)
        };
    }
}
