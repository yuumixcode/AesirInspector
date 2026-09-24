using System;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// DisallowModificationsIn 特性的介绍数据。
    /// </summary>
    internal class DisallowModificationsInAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("DisallowModificationsIn", "DisallowModificationsIn",
                "DisallowModificationsIn 特性按 Prefab 类型禁用或置灰成员，阻止修改并在已有改动时给出校验错误提示。",
                "The DisallowModificationsIn attribute disables or grays out members based on prefab kind, preventing modifications and reporting validation errors for existing changes.",
                "https://odininspector.com/attributes/disallow-modifications-in-attribute");

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("参数为 PrefabKind，可组合使用（如 PrefabInstanceAndNonPrefabInstance）。",
                "The parameter is a PrefabKind and can be combined (for example PrefabInstanceAndNonPrefabInstance)."),
            new BilingualData("适用于 Prefab 变体、场景中的 Prefab 实例以及嵌套在其他 Prefab 中的实例。",
                "Applies to prefab variants, prefab instances in scenes and instances nested inside other prefabs."),
            new BilingualData("若在引入该特性之前已有修改，会以校验错误的形式提示。",
                "If a modification already exists before the attribute was introduced, it is reported as a validation error.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue("PrefabKind", "kind",
                new BilingualData("要禁用修改的 Prefab 类型（可组合）。",
                    "The prefab kind(s) in which modifications are disallowed (combinable)."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } =
            Array.Empty<ResolvedStringParameterValue>();

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Disallow Modifications In",
                DisallowModificationsInExampleSO.Instance)
        };
    }
}
