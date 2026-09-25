using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.Editor
{
    internal class HideInAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("HideIn", "HideIn", "HideIn 特性用于在指定编辑器模式下隐藏属性。",
                "The HideIn attribute is used to hide a property in a specified editor mode.",
                OdinInspectorDocumentationLinks.HideInUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("通过 PrefabKind 指定隐藏条件，多个类型可用 | 组合，任一匹配即隐藏。",
                "The PrefabKind parameter defines the hide condition; multiple kinds can be combined with |, and the property is hidden when any of them matches."),
            new BilingualData("仅在 Prefab 相关场景生效；场景中的非 Prefab 对象需显式包含 PrefabKind.NonPrefabInstance 才会隐藏。",
                "Only takes effect in prefab-related contexts; non-prefab scene objects are hidden only when PrefabKind.NonPrefabInstance is explicitly included."),
            new BilingualData("与 DisableIn 相对：HideIn 直接隐藏属性，DisableIn 仅禁用交互。",
                "In contrast to DisableIn: HideIn hides the property entirely, while DisableIn only disables interaction.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(PrefabKind).FullName, "kind",
                new BilingualData(
                    "Prefab 当前的类型，可以同时为多种类型，用 | 分隔，如: PrefabKind.InstanceInScene | PrefabKind.InstanceInPrefab",
                    "The current type of Prefab, multiple types can be combined using |, e.g.: PrefabKind.InstanceInScene | PrefabKind.InstanceInPrefab")),
            new ParameterValue(">>> PrefabKind", "PrefabKind.None",
                new BilingualData("此时无意义，枚举占位符。", "No meaning; enum placeholder.")),
            new ParameterValue(">>> PrefabKind", "PrefabKind.InstanceInScene",
                new BilingualData("表示当前脚本挂载的物体是 Prefab，并且是场景中的实例时生效。",
                    "Applies when the object is a Prefab instance in a scene.")),
            new ParameterValue(">>> PrefabKind", "PrefabKind.InstanceInPrefab",
                new BilingualData("表示当前脚本挂载的物体是 Prefab，并且是嵌套在其他预制体中的物体时生效。",
                    "Applies when the object is a Prefab nested inside another prefab.")),
            new ParameterValue(">>> PrefabKind", "PrefabKind.Regular",
                new BilingualData("表示当前脚本挂载的物体是 Regular Prefab 时生效。",
                    "Applies when the object is a regular Prefab.")),
            new ParameterValue(">>> PrefabKind", "PrefabKind.Variant",
                new BilingualData("表示当前脚本挂载的物体是 Prefab Variant (变体) 时生效。",
                    "Applies when the object is a Prefab Variant.")),
            new ParameterValue(">>> PrefabKind", "PrefabKind.NonPrefabInstance",
                new BilingualData("表示当前脚本挂载的物体是场景中的非 Prefab 实例时生效。",
                    "Applies when the object is a non-Prefab instance in the scene.")),
            new ParameterValue(">>> PrefabKind", "PrefabKind.PrefabInstance",
                new BilingualData("PrefabInstance = InstanceInPrefab | InstanceInScene",
                    "PrefabInstance = InstanceInPrefab | InstanceInScene")),
            new ParameterValue(">>> PrefabKind", "PrefabKind.PrefabAsset",
                new BilingualData("PrefabAsset = Variant | Regular", "PrefabAsset = Variant | Regular")),
            new ParameterValue(">>> PrefabKind", "PrefabKind.PrefabInstanceAndNonPrefabInstance",
                new BilingualData(
                    "PrefabInstanceAndNonPrefabInstance = InstanceInPrefab | InstanceInScene | NonPrefabInstance",
                    "PrefabInstanceAndNonPrefabInstance = InstanceInPrefab | InstanceInScene | NonPrefabInstance")),
            new ParameterValue(">>> PrefabKind", "PrefabKind.All",
                new BilingualData("All = PrefabInstanceAndNonPrefabInstance | PrefabAsset",
                    "All = PrefabInstanceAndNonPrefabInstance | PrefabAsset"))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = { };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                HideInExampleSO.Instance)
        };
    }
}
