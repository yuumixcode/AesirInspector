using System.Collections.Generic;

namespace Runestone.AesirInspector.Editor
{
    internal class ButtonGroupAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("ButtonGroup", "ButtonGroup", "ButtonGroup 特性用于将多个按钮分组并排显示在同一行中。",
                "The ButtonGroup attribute is used to group multiple buttons side-by-side in the same row.",
                "https://odininspector.com/attributes/button-group-attribute");

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("用于方法，通过相同的组名把多个按钮并排显示在同一行；不指定组名时使用默认组 \"_DefaultGroup\"。",
                "Applied to methods, grouping multiple buttons side by side in one row via the same group name; when no group name is given, the default group \"_DefaultGroup\" is used."),
            new BilingualData("Order 控制组内按钮的排列顺序（数值越小越靠左），ButtonHeight 可自定义按钮高度。",
                "Order controls the arrangement of buttons within the group (smaller values appear further left), and ButtonHeight customizes the button height."),
            new BilingualData("GroupName 支持 $ 成员引用与 @ 表达式解析。",
                "GroupName supports $ member references and @ expressions.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(string).FullName, "groupName",
                new BilingualData("组的名称。不指定时默认使用 \"_DefaultGroup\"。",
                    "The name of the group. Defaults to \"_DefaultGroup\" when not specified.")),
            new ParameterValue(typeof(float).FullName, "order",
                new BilingualData("组内按钮的显示顺序。", "The display order of buttons within the group.")),
            new ParameterValue(typeof(int).FullName, "buttonHeight",
                new BilingualData("按钮的高度（像素）。", "The height of the button (in pixels)."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } =
        {
            new ResolvedStringParameterValue("GroupName", ResolverType.ValueResolver, typeof(string).FullName,
                "None", new List<ParameterValue>
                {
                    new ParameterValue("T", "$value",
                        new BilingualData("应用此特性的成员的值。",
                            "The value of the member that has the attribute applied to it."))
                })
        };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                ButtonGroupExampleSO.Instance),
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("GroupName",
                ButtonGroupExampleWithGroupNameSO.Instance)
        };
    }
}
