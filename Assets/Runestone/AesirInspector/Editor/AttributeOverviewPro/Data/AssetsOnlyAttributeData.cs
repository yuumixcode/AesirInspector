namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// AssetsOnly 特性的介绍数据，包含标题和案例预览项。
    /// </summary>
    internal class AssetsOnlyAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("AssetsOnly", "AssetsOnly",
                "AssetsOnly 用于 UnityEngine.Object 类型，并将 Property 限制为项目 Asset，而不是场景对象。\n" +
                "当您想要确保对象来自项目而不是场景时，请使用此项。",
                "AssetsOnly is used on object properties, and restricts the property to project assets, and not scene objects.\n" +
                "Use this when you want to ensure an object is from the project, and not from the scene.",
                OdinInspectorDocumentationLinks.AssetsOnlyUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("把 UnityEngine.Object 字段限制为项目资源（预制体、材质、网格等），场景中的对象无法被赋值。", "Restricts a UnityEngine.Object field to project assets (prefabs, materials, meshes, and so on); objects from the scene cannot be assigned."),
            new BilingualData("该特性没有参数，可用于单个对象字段，也可用于对象列表或数组。", "The attribute has no parameters and works on a single object field as well as on lists or arrays of objects."),
            new BilingualData("与 SceneObjectsOnly 相反：当需要确保对象来自场景而非项目资源时，使用 SceneObjectsOnly。", "It is the opposite of SceneObjectsOnly: use SceneObjectsOnly when you must ensure the object comes from the scene rather than the project.")
        };
        public override ParameterValue[] AttributeParameters { get; set; } = null;
        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = null;

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("No Parameters",
                AssetsOnlyExampleSO.Instance)
        };
    }
}
