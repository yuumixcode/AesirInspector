namespace Runestone.AesirInspector.Editor
{
    internal class TypeSelectorSettingsAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("TypeSelectorSettings", "TypeSelectorSettings",
                "TypeSelectorSettings 特性为使用 Odin 绘制的类型选择器提供选项。",
                "The TypeSelectorSettings attribute provides options for Odin's type selector.",
                "https://odininspector.com/attributes/type-selector-settings-attribute");

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("为 Odin 绘制的类型选择器提供选项：是否显示分组、是否优先使用命名空间、是否显示 \"<none>\" 项。",
                "Provides options for Odin's type selector: whether to show categories, prefer namespaces, and show the \"<none>\" item."),
            new BilingualData("只覆盖显式设置的选项，未设置的选项沿用 Odin 的全局设置（GeneralDrawerConfig）。",
                "Only the options you explicitly set are overridden; the rest fall back to Odin's global settings in GeneralDrawerConfig."),
            new BilingualData("FilterTypesFunction 指向一个 bool 方法(Type type)，返回 false 的类型会从选择器中隐藏。",
                "FilterTypesFunction points to a bool method(Type type); types it returns false for are hidden from the selector.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(bool).FullName, "ShowCategories",
                new BilingualData("是否显示类型分组。", "Whether to show type categories.")),
            new ParameterValue(typeof(bool).FullName, "PreferNamespaces",
                new BilingualData("指定是否优先使用命名空间而不是程序集类别名称。",
                    "Whether to prefer namespaces over assembly category names.")),
            new ParameterValue(typeof(bool).FullName, "ShowNoneItem",
                new BilingualData("指定是否显示 '<none>' 项。", "Whether to show the '<none>' item.")),
            new ParameterValue(typeof(string).FullName, "FilterTypesFunction",
                new BilingualData("自定义类型过滤函数，Func<Type, bool>，参数为类型，返回值为 bool，表示是否显示该类型。",
                    "Custom type filter function, Func<Type, bool>, parameter is the type, return value indicates whether to show the type."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = { };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                TypeSelectorSettingsExampleSO.Instance)
        };
    }
}
