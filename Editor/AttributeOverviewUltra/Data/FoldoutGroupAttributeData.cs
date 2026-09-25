namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// FoldoutGroup 特性的介绍数据。
    /// </summary>
    internal class FoldoutGroupAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("FoldoutGroup", "FoldoutGroup", "FoldoutGroup 特性用于将多个属性组织在一个可折叠的组中。",
                "The FoldoutGroup attribute is used to group multiple properties inside a collapsible foldout.",
                OdinInspectorDocumentationLinks.FoldoutGroupUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("通过组名把多个属性归入同一个可折叠组以保持 Inspector 整洁，组名支持路径嵌套（如 'Parent/Child'）。",
                "Groups multiple properties into the same collapsible foldout to keep the Inspector tidy; group names support nested paths (e.g. 'Parent/Child')."),
            new BilingualData("expanded 未指定时组默认折叠；同一组的多个特性中只要有一处指定了 expanded，该状态就会应用到整个组。",
                "When expanded is not specified the group is collapsed by default; if any of the attributes sharing the group specifies expanded, that state is applied to the whole group."),
            new BilingualData("组名支持 $ 成员引用，可用字段动态生成标题（如 \"$GroupTitle\"）。",
                "Group names support $ member references, so a field can generate the title dynamically (e.g. \"$GroupTitle\")."),
            new BilingualData("可通过 order 参数调整组之间的排序，组内还可以嵌套其他组特性（如 BoxGroup）或放置 Button。",
                "The order parameter controls how groups are sorted, and a foldout can contain other group attributes (such as BoxGroup) or buttons.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(bool).FullName, "expanded",
                new BilingualData("组是否在初始状态下展开。", "Whether the group should be expanded by default.")),
            new ParameterValue(typeof(bool).FullName, "HasDefinedExpanded",
                new BilingualData("内部使用，标识是否显式设置了展开状态。",
                    "Internal use, indicates if the expanded state has been explicitly defined."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = { };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                FoldoutGroupExampleSO.Instance)
        };
    }
}
