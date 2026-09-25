using System.Collections.Generic;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// Required 特性的介绍数据。
    /// </summary>
    internal class RequiredAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("Required", "Required", "Required 特性用于标记关键字段，如果字段为空，将在检查器中显示错误消息。",
                "The Required attribute is used to mark critical fields. If a field is empty, an error message will be displayed in the inspector.",
                OdinInspectorDocumentationLinks.RequiredUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("标记关键引用或值：当成员为 null、空字符串或已销毁的 UnityEngine.Object 时报告错误（默认类型为 Error）。",
                "Marks critical references or values: it reports an error when the member is null, an empty string, or a destroyed UnityEngine.Object (default type is Error)."),
            new BilingualData("自定义错误消息支持 $ 成员引用与 @ 表达式（例如 \"$DynamicMessage\"）。",
                "Custom error messages support $ member references and @ expressions (for example \"$DynamicMessage\")."),
            new BilingualData("默认消息为 \"<成员名> is required\"；它属于验证器，不会阻止赋值，也不影响序列化或运行时逻辑。",
                "The default message is \"<member name> is required\"; it is a validator, so it does not block assignment and does not affect serialization or runtime logic.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(string).FullName, "errorMessage",
                new BilingualData("字段为空时显示的自定义错误消息。",
                    "Custom error message displayed when the field is empty.")),
            new ParameterValue("InfoMessageType", "infoMessageType",
                new BilingualData("消息的类型，控制左侧显示的图标（None, Info, Warning, Error）。",
                    "The type of the message, controlling the icon displayed on the left (None, Info, Warning, Error)."))
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
                RequiredExampleSO.Instance),
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("ErrorMessage Expression",
                RequiredExampleWithErrorMessageSO.Instance)
        };
    }
}
