namespace Runestone.AesirInspector.Editor
{
    internal class ShowPropertyResolverAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("ShowPropertyResolver", "ShowPropertyResolver",
                "ShowPropertyResolver 特性用于在检查器中显示属性的解析器信息，便于调试。",
                "The ShowPropertyResolver attribute is used to display the property's resolver information in the inspector for debugging purposes.");

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("在属性上方显示负责把该成员加入属性树的 Resolver 名称（没有 Resolver 时显示 None）。",
                "Displays the name of the Resolver responsible for bringing the member into the property tree above the property (showing None when there is no resolver)."),
            new BilingualData("适合排查本不应出现在 Inspector 中的成员为何被显示这类问题。",
                "Useful for investigating why a member that should not normally appear in the inspector is being shown."),
            new BilingualData("与 ShowDrawerChain 互补：一个查看 Resolver，一个查看绘制器链。",
                "Complements ShowDrawerChain: one inspects the resolver, the other the drawer chain.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } = { };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = null;

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                ShowPropertyResolverExampleSO.Instance)
        };
    }
}
