using System;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// Toggle 特性的介绍数据。
    /// </summary>
    internal class ToggleAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("Toggle", "Toggle",
                "Toggle 特性为字段或属性添加一个开关，用于整体启用/禁用该值；也可标注在类上，为类型的全部实例提供开关。",
                "The Toggle attribute adds an on/off switch for a field or property, or to a type so every instance gets a toggle.",
                "https://odininspector.com/attributes/toggle-attribute");

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("参数为用作开关的 bool 成员名。",
                "The parameter is the name of the bool member used as the switch."),
            new BilingualData("标注在字段上时仅控制该字段；标注在类上时控制该类所有实例的显示。",
                "Applied to a field it controls that field; applied to a class it controls every instance of the type."),
            new BilingualData("与 ToggleGroup 的区别：Toggle 只控制单个值，ToggleGroup 控制一组字段。",
                "Unlike ToggleGroup, Toggle controls a single value rather than a group of fields.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(string).FullName, "toggleMemberName",
                new BilingualData("用作开关的 bool 成员名称。",
                    "The name of the bool member used as the toggle."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } =
            Array.Empty<ResolvedStringParameterValue>();

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Toggle",
                ToggleExampleSO.Instance)
        };
    }
}
