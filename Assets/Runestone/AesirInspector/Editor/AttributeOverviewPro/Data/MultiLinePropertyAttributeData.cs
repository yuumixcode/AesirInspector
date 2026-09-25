namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// MultiLineProperty 特性的介绍数据。
    /// </summary>
    internal class MultiLinePropertyAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("MultiLineProperty", "MultiLineProperty",
                "MultiLineProperty 特性用于创建多行文本输入区域，适用于较长的字符串编辑。",
                "The MultiLineProperty attribute creates a multi-line text input area for editing longer strings.",
                "https://odininspector.com/attributes/multi-line-property-attribute");

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("绘制固定行数的多行文本框，行数由参数指定（默认 3，最小 1）；高度不随内容伸缩，内容超出时出现滚动条。",
                "Draws a multi-line text box with a fixed number of lines set by the parameter (default 3, minimum 1); the height never grows with the content and a scrollbar appears when the text overflows."),
            new BilingualData("与 Unity 的 Multiline/TextArea 不同，可作用于任意成员（字段、属性、方法参数等）；作用于属性时需配合 [ShowInInspector]。",
                "Unlike Unity's Multiline/TextArea, it can be applied to any member (fields, properties, method arguments, etc.); when used on a property, combine it with [ShowInInspector]."),
            new BilingualData("若需要高度随内容自适应，请改用 Unity 的 [TextArea]，它在最小与最大行数之间伸缩。",
                "Use Unity's [TextArea] instead when the height should adapt to the content, as it grows between its minimum and maximum line counts.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } = { };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = { };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                MultiLinePropertyExampleSO.Instance)
        };
    }
}
