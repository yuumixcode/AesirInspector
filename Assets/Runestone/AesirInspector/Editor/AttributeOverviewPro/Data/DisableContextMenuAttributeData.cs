using System;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// DisableContextMenu 特性的介绍数据。
    /// </summary>
    internal class DisableContextMenuAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("DisableContextMenu", "DisableContextMenu",
                "DisableContextMenu 特性用于禁用属性的右键菜单。",
                "The DisableContextMenu attribute disables the context menu of a property.",
                "https://odininspector.com/attributes/disable-context-menu-attribute");

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("默认禁用成员自身的右键菜单，可用参数控制是否保留集合元素菜单。",
                "Disables the member's own context menu by default; parameters control the collection-element menu."),
            new BilingualData("disableForMember 为 false 时保留成员菜单，仅禁用集合元素菜单。",
                "With disableForMember set to false, the member menu is kept and only collection-element menus are disabled.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(bool).FullName, "disableForMember",
                new BilingualData("是否禁用成员自身的右键菜单，默认 true。",
                    "Whether to disable the member's own context menu. Defaults to true.")),
            new ParameterValue(typeof(bool).FullName, "disableCollectionElements",
                new BilingualData("是否禁用集合元素的右键菜单，默认 false。",
                    "Whether to disable collection-element context menus. Defaults to false."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } =
            Array.Empty<ResolvedStringParameterValue>();

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Disable Context Menu",
                DisableContextMenuExampleSO.Instance)
        };
    }
}
