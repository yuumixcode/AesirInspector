using System.Collections.Generic;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// ValidateInput 特性的介绍数据。
    /// </summary>
    internal class ValidateInputAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("ValidateInput", "ValidateInput",
                "ValidateInput 特性用于在检查器中对属性值进行自定义验证，并在验证失败时显示消息。",
                "The ValidateInput attribute is used to perform custom validation on property values in the inspector and display a message when validation fails.",
                OdinInspectorDocumentationLinks.ValidateInputUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("在检查器中验证属性值：condition 可以是方法名、\"$成员\" 引用或 \"@表达式\"，返回 true 表示验证通过。",
                "Validates a property value in the inspector: condition can be a method name, a \"$member\" reference or an \"@expression\", and returning true means the value is valid."),
            new BilingualData("验证方法可接收属性值等参数，并能通过 ref string message、ref InfoMessageType messageType 动态改写错误消息与类型。",
                "The validation method can take parameters such as the value, and can rewrite the message and its type through ref string message and ref InfoMessageType messageType."),
            new BilingualData("只在编辑器中生效，脚本直接修改的值不会被验证；IncludeChildren 默认开启，子字段变化也会触发验证。",
                "It only works in the editor, so values changed by script are not validated; IncludeChildren is on by default, so changes to child fields also trigger validation.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(string).FullName, "condition",
                new BilingualData("验证逻辑的方法名或表达式。返回 true 表示验证通过。",
                    "The method name or expression for validation logic. Returning true indicates validation passed.")),
            new ParameterValue(typeof(string).FullName, "defaultMessage",
                new BilingualData("验证失败时显示的默认消息。支持字符串解析。",
                    "The default message displayed when validation fails. Supports string resolution.")),
            new ParameterValue("InfoMessageType", "messageType",
                new BilingualData("消息的类型（Info, Warning, Error, None）。默认为 Error。",
                    "The type of the message (Info, Warning, Error, None). Defaults to Error.")),
            new ParameterValue(typeof(bool).FullName, "IncludeChildren",
                new BilingualData("子字段修改时是否也触发验证。默认为 true。",
                    "Whether to trigger validation when sub-fields are modified. Defaults to true.")),
            new ParameterValue(typeof(bool).FullName, "ContinuousValidationCheck",
                new BilingualData("是否每帧都进行验证，而不仅是在值改变时。默认为 false。",
                    "Whether to perform validation every frame, not just when the value changes. Defaults to false."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } =
        {
            new ResolvedStringParameterValue("Condition", ResolverType.ValueResolver, typeof(bool).FullName,
                "None", new List<ParameterValue>
                {
                    new ParameterValue("ref string", "message",
                        new BilingualData("可通过 ref 修改的错误消息。", "Error message that can be modified via ref.")),
                    new ParameterValue("ref InfoMessageType", "messageType",
                        new BilingualData("可通过 ref 修改的消息类型。", "Message type that can be modified via ref.")),
                    new ParameterValue("T", "$value",
                        new BilingualData("当前属性的值。", "The current value of the property.")),
                    new ParameterValue("InspectorProperty", "$property",
                        new BilingualData("当前的 InspectorProperty 对象。",
                            "The current InspectorProperty object."))
                }),
            new ResolvedStringParameterValue("Default Message", ResolverType.ValueResolver,
                typeof(string).FullName, "None", new List<ParameterValue>())
        };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Usage Examples",
                ValidateInputExampleSO.Instance)
        };
    }
}
