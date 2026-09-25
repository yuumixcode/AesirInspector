using System.Collections.Generic;

namespace Runestone.AesirInspector.Editor
{
    internal class ToggleGroupAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("ToggleGroup", "ToggleGroup",
                "ToggleGroup 特性用于为 Toggle 类型属性创建一个可折叠的组，切换开关时将展开或折叠组内容。",
                "The ToggleGroup attribute is used to create a collapsible group for toggle-type properties, expanding or collapsing content when the toggle is switched.",
                "https://odininspector.com/attributes/toggle-group-attribute");

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("组名即开关成员名，同一组的所有成员必须指定相同的开关成员，且该成员需为同一对象上的 bool 字段或属性（不支持静态成员）。",
                "The group name is the toggle member name: every member of a group must specify the same toggle, and it must be a bool field or property on the same object (static members are not supported)."),
            new BilingualData("组标题默认使用开关成员名，可通过 groupTitle 或 \"$成员名\" 动态设置。",
                "The group title defaults to the toggle member name and can be set with groupTitle or a \"$memberName\" string."),
            new BilingualData("CollapseOthersOnExpand 默认为 true：展开当前组时会自动折叠其他已展开的 Toggle 组。",
                "CollapseOthersOnExpand defaults to true, so opening one group automatically collapses the other expanded toggle groups."),
            new BilingualData("与 Toggle 的区别：Toggle 只启用/禁用单个值，ToggleGroup 用一个 bool 控制整组字段的展开与折叠。",
                "Unlike Toggle, which enables or disables a single value, ToggleGroup uses one bool to expand or collapse a whole group of fields.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(string).FullName, "toggleMemberName",
                new BilingualData("开关成员名：任意 bool 字段、属性或方法，用于展开/折叠该组，同时作为组名。",
                    "The toggle member name: any bool field, property or method that expands or collapses the group, also used as the group name.")),
            new ParameterValue(typeof(string).FullName, "toggleGroupTitle",
                new BilingualData("组的标题文本（默认与字段名相同）。",
                    "The title text of the group (defaults to the field name).")),
            new ParameterValue(typeof(float).FullName, "order",
                new BilingualData("组在 Inspector 中的显示顺序。",
                    "The display order of the group in the Inspector.")),
            new ParameterValue(typeof(bool).FullName, "collapseOthersOnExpand",
                new BilingualData("展开时是否自动折叠其他组。", "Whether to collapse other groups when expanding."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } =
        {
            new ResolvedStringParameterValue("ToggleGroupTitle", ResolverType.ValueResolver,
                typeof(string).FullName, "None", new List<ParameterValue>
                {
                    new ParameterValue("T", "$value",
                        new BilingualData("应用此特性的成员的值。",
                            "The value of the member that has the attribute applied to it."))
                })
        };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                ToggleGroupExampleSO.Instance),
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("ToggleGroupTitle",
                ToggleGroupExampleWithToggleGroupTitleSO.Instance)
        };
    }
}
