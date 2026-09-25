using System;
using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.Editor
{
    internal class TypeDrawerSettingsAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("TypeDrawerSettings", "TypeDrawerSettings",
                "TypeDrawerSettings 特性用于设置 Type 类型的绘制样式。",
                "The TypeDrawerSettings attribute configures the drawing style for Type fields.",
                "https://odininspector.com/attributes/type-drawer-settings-attribute");

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("用于配置 Type 字段的类型选择器：BaseType 限定可选的基类范围，Filter 决定包含哪些类型类别。",
                "Configures the type selector of a Type field: BaseType limits the selectable base type, and Filter decides which kinds of types are included."),
            new BilingualData(
                "Filter 是 TypeInclusionFilter 的按位组合，可同时包含多种类别（如 IncludeConcreteTypes | IncludeInterfaces）。",
                "Filter is a bitwise combination of TypeInclusionFilter values, so several categories can be included at once (e.g. IncludeConcreteTypes | IncludeInterfaces)."),
            new BilingualData("Type 无法被 Unity 直接序列化，需要 Odin 序列化（案例继承 SerializedScriptableObject）才能保存所选类型。",
                "Type cannot be serialized by Unity directly; Odin serialization (the example derives from SerializedScriptableObject) is required to save the chosen type.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(Type).FullName, "BaseType",
                new BilingualData("基类类型，用于限制可选择的类型范围。",
                    "The base type to filter which types are selectable.")),
            new ParameterValue(typeof(TypeInclusionFilter).FullName, "Filter",
                new BilingualData("过滤器，默认为 TypeInclusionFilter.IncludeAll。",
                    "The type inclusion filter. Defaults to TypeInclusionFilter.IncludeAll."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = { };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                TypeDrawerSettingsExampleSO.Instance)
        };
    }
}
