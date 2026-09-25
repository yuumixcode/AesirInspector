namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// PropertySpace 特性的介绍数据。
    /// </summary>
    internal class PropertySpaceAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("PropertySpace", "PropertySpace",
                "PropertySpace 特性用于在检查器中属性的前后添加间距（像素为单位）。",
                "The PropertySpace attribute is used to add spacing (in pixels) before and after a property in the inspector.",
                OdinInspectorDocumentationLinks.PropertySpaceUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("与 Unity 的 [Space] 作用相同，但可应用于任意成员（字段、属性、方法），而不只是字段。",
                "Works like Unity's [Space], but can be applied to any member (fields, properties, methods), not just fields."),
            new BilingualData("默认在成员上方添加 8 像素间距；用两个参数可分别设置上方（SpaceBefore）和下方（SpaceAfter）的间距。",
                "Adds 8 pixels of spacing above the member by default; two arguments set the spacing above (SpaceBefore) and below (SpaceAfter) separately."),
            new BilingualData("支持负值，可用于收紧成员之间已有的间距。",
                "Negative values are supported and can tighten existing spacing between members.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(float).FullName, "SpaceBefore",
                new BilingualData("属性上方的间距像素值。", "The pixel value for spacing before the property.")),
            new ParameterValue(typeof(float).FullName, "SpaceAfter",
                new BilingualData("属性下方的间距像素值。", "The pixel value for spacing after the property."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = { };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Usage Examples",
                PropertySpaceExampleSO.Instance)
        };
    }
}
