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
            new BilingualData("右键点击字段标签即可看到自定义菜单项，点击后执行指定的方法或表达式。", "Right-click the field label to see the custom menu item; clicking it runs the specified method or expression."),
            new BilingualData("MenuItem 与 Action 都是解析字符串，支持 $ 成员引用与 @ 表达式。", "Both MenuItem and Action are resolved strings supporting $ member references and @ expressions."),
            new BilingualData("不支持静态方法；同一属性可叠加多个 CustomContextMenu。", "Static methods are not supported; several CustomContextMenu items can be stacked on the same property.")
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
