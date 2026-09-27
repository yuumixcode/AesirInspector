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
            new BilingualData("为字段或属性添加开关：开关成员必须是被标注成员所在对象上的 bool 字段或属性，不支持静态成员。",
                "Adds a toggle to a field or property: the toggle member must be a bool field or property on the same object as the annotated member, and static members are not supported."),
            new BilingualData("标注在字段上只控制该字段，标注在类上则该类型的所有实例都带开关。",
                "Applied to a field it controls only that field, while applied to a class every instance of the type gets the toggle."),
            new BilingualData("与 ToggleGroup 的区别：Toggle 只启用/禁用单个值，ToggleGroup 用一个 bool 控制整组字段的展开与折叠。",
                "Unlike ToggleGroup, which uses one bool to expand or collapse a whole group of fields, Toggle enables or disables a single value.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(string).FullName, "toggleMemberName",
                new BilingualData("用作开关的 bool 成员名称。", "The name of the bool member used as the toggle."))
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
