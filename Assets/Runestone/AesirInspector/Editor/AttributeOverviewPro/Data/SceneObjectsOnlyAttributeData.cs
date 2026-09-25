namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// SceneObjectsOnly 特性的介绍数据。
    /// </summary>
    internal class SceneObjectsOnlyAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("SceneObjectsOnly", "SceneObjectsOnly",
                "SceneObjectsOnly 特性用于限制对象引用仅能选择场景中的对象，而不能选择项目资源（Prefab 等）。",
                "The SceneObjectsOnly attribute restricts object references to only allow scene objects, preventing the selection of project assets like prefabs.",
                OdinInspectorDocumentationLinks.SceneObjectsOnlyUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("用于对象引用字段，限制只能选择场景中的对象，不能选择项目资源（Prefab 等）。",
                "Used on object reference fields to restrict selection to scene objects only, disallowing project assets such as prefabs."),
            new BilingualData("拖入项目资源时会显示验证错误。",
                "A validation error is shown if a project asset is dragged into the field."),
            new BilingualData("与 AssetsOnly 特性正好相反。",
                "The exact opposite of the AssetsOnly attribute.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } = { };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = { };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                SceneObjectsOnlyExampleSO.Instance)
        };
    }
}
