namespace Runestone.AesirInspector.Editor
{
    internal class ChildGameObjectOnlyAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("ChildGameObjectOnly", "ChildGameObjectOnly",
                "ChildGameObjectOnly 特性作用于继承 Component 或者 GameObject 的字段上，在面板上绘制一个小按钮，用于选择当前物体的子物体。",
                "The ChildGameObjectOnly attribute draws a button to select a child GameObject for fields inheriting Component or GameObject.",
                "https://odininspector.com/attributes/child-game-object-only-attribute");

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("作用于 Component 或 GameObject 字段，在对象字段旁增加按钮，以下拉方式从当前物体的子物体中选择可赋值对象。",
                "Applied to Component or GameObject fields, it adds a button next to the object field that offers a dropdown of assignable objects found among the current object's children."),
            new BilingualData("IncludeSelf 默认 true（包含当前物体自身），IncludeInactive 默认 false（不包含未激活的子物体）。",
                "IncludeSelf defaults to true (the current object itself is included) and IncludeInactive defaults to false (inactive children are excluded)."),
            new BilingualData("只在场景 GameObject 层级中有意义，ScriptableObject 上没有子物体可供搜索。",
                "It is only meaningful within a scene GameObject hierarchy; a ScriptableObject has no children to search.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(bool).FullName, "IncludeSelf",
                new BilingualData("是否包含当前物体，默认为 true。",
                    "Whether to include the current object. Defaults to true.")),
            new ParameterValue(typeof(bool).FullName, "IncludeInactive",
                new BilingualData("是否包含非激活的物体，默认为 false。",
                    "Whether to include inactive objects. Defaults to false."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = { };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                ChildGameObjectOnlyExampleSO.Instance)
        };
    }
}
