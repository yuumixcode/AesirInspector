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
            new BilingualData("在匹配的 PrefabKind 下禁用并置灰成员以阻止修改，同时启用校验：若在引入该特性之前已有改动，会以校验错误的形式提示。",
                "In the matching PrefabKind it disables and grays out the member to prevent modifications and also enables validation: changes made before the attribute was introduced are reported as validation errors."),
            new BilingualData(
                "只对 Prefab 相关对象生效，包括 Prefab 资源、变体、场景中的实例、嵌套在其他 Prefab 中的实例以及非 Prefab 实例；PrefabKind 由目标对象解析得出。",
                "It only applies to prefab-related objects, including prefab assets, variants, instances in scenes, instances nested inside other prefabs and non-prefab instances; the PrefabKind is resolved from the target object."),
            new BilingualData("与 DisableIn 的区别：DisableIn 只禁用绘制，DisallowModificationsIn 还会校验并提示已经存在的改动。",
                "Difference from DisableIn: DisableIn only disables drawing, while DisallowModificationsIn also validates and reports changes that already exist.")
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
