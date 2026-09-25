using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// TabGroup 特性的介绍数据。
    /// </summary>
    internal class TabGroupAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("TabGroup", "TabGroup", "TabGroup 特性用于将多个属性组织在不同的页签（Tabs）中。",
                "The TabGroup attribute is used to organize multiple properties into different tabs.",
                OdinInspectorDocumentationLinks.TabGroupUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("用 \"组名, 页签名\" 组织页签；同一组名的成员进入同一页签栏，单参数重载使用默认组 _DefaultTabGroup。",
                "Organizes tabs with \"group name, tab name\"; members sharing a group name go into the same tab bar, and the single-argument overload uses the default group _DefaultTabGroup."),
            new BilingualData("组路径支持 / 嵌套，可在页签内再划分页签（如 \"ParentGroup/First Tab/InnerGroup\"）。",
                "The group path supports / nesting, allowing tabs within tabs (for example \"ParentGroup/First Tab/InnerGroup\")."),
            new BilingualData("UseFixedHeight 让组内所有页签保持相同高度，默认值为 false。",
                "UseFixedHeight keeps every tab in the group at the same height, and defaults to false."),
            new BilingualData("TabLayouting 控制页签排列方式：MultiRow 为多行排列，Shrink 为收缩排列。",
                "TabLayouting controls how tabs are laid out: MultiRow wraps to multiple rows, and Shrink compresses them.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(string).FullName, "tab",
                new BilingualData("页签名称。", "The name of the tab.")),
            new ParameterValue(typeof(bool).FullName, "useFixedHeight",
                new BilingualData("是否为所有页签使用固定高度。默认值为 false。",
                    "Whether to use a fixed height for all tabs in the group. Default is false.")),
            new ParameterValue(typeof(SdfIconType).FullName, "icon",
                new BilingualData("页签显示的图标。", "The icon to display on the tab.")),
            new ParameterValue(typeof(string).FullName, "TextColor",
                new BilingualData("页签文本颜色。", "The text color of the tab."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = { };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                TabGroupExampleSO.Instance)
        };
    }
}
