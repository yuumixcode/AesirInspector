using System;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// CustomContextMenu 特性的介绍数据。
    /// </summary>
    internal class CustomContextMenuAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("CustomContextMenu", "CustomContextMenu",
                "CustomContextMenu 特性用于为属性的右键菜单添加自定义菜单项，点击后执行指定的方法或表达式。",
                "The CustomContextMenu attribute adds a custom item to a property's context menu that runs the specified method or expression.",
                "https://odininspector.com/attributes/custom-context-menu-attribute");

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("右键点击字段标签即可看到自定义菜单项。",
                "Right-click the field label to see the custom menu item."),
            new BilingualData("action 支持方法名（成员引用）与 @ 表达式。",
                "The action supports a method name (member reference) and @ expressions."),
            new BilingualData("可在同一属性上叠加多个 CustomContextMenu。",
                "Several CustomContextMenu items can be stacked on the same property.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(string).FullName, "menuItem",
                new BilingualData("右键菜单中显示的菜单项名称。",
                    "The menu item name shown in the context menu.")),
            new ParameterValue(typeof(string).FullName, "action",
                new BilingualData("点击菜单项时执行的方法名或 @ 表达式。",
                    "The method name or @ expression executed when the item is clicked."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } =
            Array.Empty<ResolvedStringParameterValue>();

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                CustomContextMenuExampleSO.Instance),
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Action",
                CustomContextMenuExampleWithActionSO.Instance)
        };
    }
}
