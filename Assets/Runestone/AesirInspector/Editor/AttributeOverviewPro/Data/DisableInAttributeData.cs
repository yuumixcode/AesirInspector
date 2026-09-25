using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.Editor
{
    internal class DisableInAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("DisableIn", "DisableIn", "DisableIn 特性用于在指定编辑器模式下禁用属性。",
                "The DisableIn attribute is used to disable a property in a specified editor mode.",
                OdinInspectorDocumentationLinks.DisableInUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("只有当属性所在对象的 PrefabKind 与参数位掩码有交集时才会被禁用，无交集时保持可编辑。", "The property is disabled only when the PrefabKind of its object intersects the parameter bitmask; with no intersection it stays editable."),
            new BilingualData("PrefabKind 由目标对象解析，因此只对 GameObject 或 Component 上的成员有意义；ScriptableObject 等对象始终视为 PrefabKind.None。", "The PrefabKind is resolved from the target object, so it only makes sense for members on a GameObject or Component; other objects such as ScriptableObjects always resolve to PrefabKind.None."),
            new BilingualData("被禁用后属性仍然绘制，只是失去焦点、无法修改；HideIn 则是直接隐藏属性。", "A disabled property is still drawn, but cannot be focused or modified; HideIn hides it outright instead."),
            new BilingualData("标注在集合上时作用于整个集合，不会逐个作用于列表元素。", "Applied to a collection it affects the collection as a whole rather than each list element.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(PrefabKind).FullName, "kind",
                new BilingualData(
                    "Prefab 当前的类型，可以同时为多种类型，用 | 分隔，如: PrefabKind.InstanceInScene | PrefabKind.InstanceInPrefab",
                    "The current type of Prefab, multiple types can be combined using |, e.g.: PrefabKind.InstanceInScene | PrefabKind.InstanceInPrefab")),
            new ParameterValue(">>> PrefabKind", "PrefabKind.None",
                new BilingualData("无意义，枚举占位符。", "No meaning; enum placeholder.")),
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
                DisableInExampleSO.Instance)
        };
    }
}
