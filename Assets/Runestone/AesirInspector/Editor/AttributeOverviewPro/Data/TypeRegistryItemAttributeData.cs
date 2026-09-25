using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    internal class TypeRegistryItemAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("TypeRegistryItem", "TypeRegistryItem",
                "TypeRegistryItem 特性用于自定义类型在 Odin 的类型选择器中的样式。",
                "The TypeRegistryItem attribute customizes the appearance of types in Odin's type selector.",
                "https://odininspector.com/attributes/type-registry-item-attribute");

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("为类型在 Odin 类型选择器中自定义显示名称、分组路径、图标与排序，属于编辑器美化。",
                "Customizes how a type appears in Odin's type selector - display name, category path, icon and order - for editor aesthetics."),
            new BilingualData("只能标注在类、结构体、枚举或接口上；CategoryPath 用 \"/\" 建立多级分组（如 \"Demo/Painting Tools\"），Name 会覆盖默认类型名。",
                "Can only be applied to classes, structs, enums or interfaces; CategoryPath builds nested groups with \"/\" (e.g. \"Demo/Painting Tools\") and Name overrides the default type name."),
            new BilingualData("图标颜色分 LightIconColor 与 DarkIconColor，分别作用于浅色与深色编辑器皮肤，Priority 决定同组内的排序。",
                "Icon colors are split into LightIconColor and DarkIconColor for the light and dark editor skins, and Priority controls the order within a group.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(string).FullName, "name",
                new BilingualData("类型名称，用于在类型选择器中显示。", "The type name displayed in the type selector.")),
            new ParameterValue(typeof(string).FullName, "categoryPath",
                new BilingualData("类型在类型选择器中的路径，如 GameObject/UI。",
                    "The category path in the type selector, e.g., GameObject/UI.")),
            new ParameterValue(typeof(SdfIconType).FullName, "Icon",
                new BilingualData("图标类型，默认为 SdfIconType.None。",
                    "The icon type. Defaults to SdfIconType.None.")),
            new ParameterValue(typeof(Color).FullName, "LightIconColor",
                new BilingualData("Light 皮肤下的颜色。", "The icon color in the Light editor skin.")),
            new ParameterValue(typeof(Color).FullName, "DarkIconColor",
                new BilingualData("Dark 皮肤下的颜色。", "The icon color in the Dark editor skin.")),
            new ParameterValue(typeof(int).FullName, "Priority",
                new BilingualData("类型在类型选择器中的优先级，默认为 0。",
                    "The priority in the type selector. Defaults to 0."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = { };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                TypeRegistryItemExampleSO.Instance)
        };
    }
}
