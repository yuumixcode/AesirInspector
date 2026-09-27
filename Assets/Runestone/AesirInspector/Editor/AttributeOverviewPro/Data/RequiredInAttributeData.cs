using System.Collections.Generic;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// RequiredIn 特性的介绍数据。
    /// </summary>
    internal class RequiredInAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("RequiredIn", "RequiredIn",
                "RequiredIn 是 Required 特性的变体，专门用于预制体（Prefab）对象。它允许你指定属性在特定的预制体类型中不能为空。",
                "RequiredIn is a variant of the Required attribute specifically for Prefab objects. It allows you to specify that a property must not be null in certain prefab kinds.",
                OdinInspectorDocumentationLinks.RequiredInUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("Required 的预制体版本：仅当当前对象匹配指定的 PrefabKind 时才执行非空检查，其他状态下不会报错。",
                "The prefab-oriented variant of Required: the null check only runs when the current object matches the specified PrefabKind, and reports nothing otherwise."),
            new BilingualData(
                "PrefabKind 是位标志，可用 | 组合多种状态（如 PrefabKind.InstanceInScene | PrefabKind.Regular）。",
                "PrefabKind is a bit flag, so multiple states can be combined with | (for example PrefabKind.InstanceInScene | PrefabKind.Regular)."),
            new BilingualData("ErrorMessage 支持 $ 成员引用与 @ 表达式；默认消息为 \"<成员名> is required\"。",
                "ErrorMessage supports $ member references and @ expressions; the default message is \"<member name> is required\".")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue("PrefabKind", "Kind",
                new BilingualData("指定该属性必须存在的预制体类型。",
                    "Specifies the prefab kinds where this property must be present.")),
            new ParameterValue(typeof(string).FullName, "ErrorMessage",
                new BilingualData("当验证失败时显示的自定义错误消息。",
                    "Custom error message to display when validation fails."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } =
        {
            new ResolvedStringParameterValue("Error Message", ResolverType.ValueResolver,
                typeof(string).FullName, "None", new List<ParameterValue>
                {
                    new ParameterValue("T", "$value",
                        new BilingualData("应用此特性的成员的值（通常为空）。",
                            "The value of the member that has the attribute applied to it (usually empty)."))
                })
        };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                RequiredInExampleSO.Instance)
        };
    }
}
