using System.Collections;
using System.Collections.Generic;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// ValueDropdown 特性的介绍数据。
    /// </summary>
    internal class ValueDropdownAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("ValueDropdown", "ValueDropdown",
                "ValueDropdown 特性用于在属性上提供一个自定义的下拉选择列表。",
                "The ValueDropdown attribute provides a custom dropdown list for selecting values for a property.",
                OdinInspectorDocumentationLinks.ValueDropdownUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("为属性提供自定义下拉列表：values 指向返回 IList 的成员或表达式（数组、List、ValueDropdownList<T> 均可）。",
                "Provides a custom dropdown for a property: values points to a member or expression returning an IList (arrays, Lists and ValueDropdownList<T> all work)."),
            new BilingualData("选项文本用 \"Group/Item\" 形式即可形成树状分组，配合 ExpandAllMenuItems 默认展开；选项达到 10 个时会启用搜索框（设为 0 则始终启用）。",
                "Labels written as \"Group/Item\" form a tree view that ExpandAllMenuItems can expand by default; search appears once there are 10 items (set the count to 0 to always enable it)."),
            new BilingualData("用 ValueDropdownList<T> 可给每个选项指定显示名称与实际值，适合无法使用 ToString 的类型。",
                "ValueDropdownList<T> lets each option carry a display name and an actual value, which suits types without a usable ToString."),
            new BilingualData("作用于列表时默认也为每个元素提供下拉（DrawDropdownForListElements）；IsUniqueList 保证元素唯一并启用多选，ExcludeExistingValuesInList 可隐藏已选项。",
                "On a list it also gives every element a dropdown by default (DrawDropdownForListElements); IsUniqueList keeps items unique and enables multi-select, and ExcludeExistingValuesInList hides already selected values.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(string).FullName, "values",
                new BilingualData("获取可选值列表的成员名或表达式。",
                    "The member name or expression to get the list of values.")),
            new ParameterValue(typeof(bool).FullName, "AppendNextDrawer",
                new BilingualData("是否在原有的绘制方式后附加下拉按钮，而不是直接替换。",
                    "Whether to append a dropdown button next to the original drawer instead of replacing it.")),
            new ParameterValue(typeof(bool).FullName, "DisableGUIInAppendedDrawer",
                new BilingualData("配合 AppendNextDrawer 使用，是否禁用原有绘制的交互。",
                    "If true, the original drawer will be disabled when using AppendNextDrawer.")),
            new ParameterValue(typeof(bool).FullName, "ExpandAllMenuItems",
                new BilingualData("如果显示为树状图，是否默认展开所有项。",
                    "Whether to expand all menu items by default in tree-view mode.")),
            new ParameterValue(typeof(bool).FullName, "IsUniqueList",
                new BilingualData("当作用于列表时，是否保证列表项唯一。", "Whether to ensure items in the list are unique.")),
            new ParameterValue(typeof(int).FullName, "NumberOfItemsBeforeEnablingSearch",
                new BilingualData("当列表项达到多少个时显示搜索框。",
                    "The number of items required before search is enabled in the dropdown."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } =
        {
            new ResolvedStringParameterValue("Values", ResolverType.ValueResolver,
                typeof(IEnumerable).FullName, "None", new List<ParameterValue>())
        };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                ValueDropdownExampleSO.Instance)
        };
    }
}
