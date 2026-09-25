using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// Searchable 特性的介绍数据。
    /// </summary>
    internal class SearchableAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("Searchable", "Searchable", "Searchable 特性为列表、数组或类添加一个搜索框，方便快速筛选内容。",
                "The Searchable attribute adds a search field to a list, array, or class, allowing for quick content filtering.",
                OdinInspectorDocumentationLinks.SearchableUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("为列表、数组或类添加搜索框；默认开启模糊搜索与递归搜索，可匹配子对象字段的内容。",
                "Adds a search box to a list, array, or class; fuzzy search and recursive search are on by default, matching the contents of child object fields."),
            new BilingualData("直接作用于字典时搜索框不生效；但来自上级的递归搜索仍能匹配字典内容。",
                "The search box does not take effect when applied directly to a dictionary, though a recursive search from above can still match its contents."),
            new BilingualData(
                "可作用于成员，也可标注在类或根检查类型（Component / ScriptableObject / OdinEditorWindow）上，使其所有实例都可搜索。",
                "Can be applied to a member, or to a class or root inspected type (Component / ScriptableObject / OdinEditorWindow) to make every instance searchable."),
            new BilingualData("FilterOptions 控制匹配范围（属性名、友好名、值类型、值的字符串、ISearchFilterable 接口）。",
                "FilterOptions controls what is matched (property name, nice name, value type, value string, and the ISearchFilterable interface).")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(bool).FullName, "FuzzySearch",
                new BilingualData("是否启用模糊搜索。默认值为 true。",
                    "Whether to enable fuzzy searching. Default is true.")),
            new ParameterValue(typeof(bool).FullName, "Recursive",
                new BilingualData("是否递归搜索子属性。默认值为 true。",
                    "Whether to search child properties recursively. Default is true.")),
            new ParameterValue(typeof(SearchFilterOptions).FullName, "FilterOptions",
                new BilingualData("搜索过滤选项。", "Options for how searching should filter properties."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = { };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                SearchableExampleSO.Instance)
        };
    }
}
