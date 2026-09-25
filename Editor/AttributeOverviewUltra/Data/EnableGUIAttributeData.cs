namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// EnableGUI 特性的介绍数据。
    /// </summary>
    internal class EnableGUIAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("EnableGUI", "EnableGUI", "强制启用 property，使其正常显示。",
                "Forces a property to be enabled so it displays normally.",
                OdinInspectorDocumentationLinks.EnableGuiUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("强制以启用状态绘制该属性及其子级，常用于让默认灰显的属性（例如 [ShowInInspector] 绘制的只读属性）恢复交互。",
                "Forces the property and its children to be drawn in an enabled state; typically used to restore interaction for properties that are grayed out by default, such as read-only properties drawn with [ShowInInspector]."),
            new BilingualData("只恢复 GUI 的启用状态，并不会让只读或非序列化的成员变成可编辑：与 [ReadOnly] 组合时字段可以获得焦点，但值依然不可修改。",
                "It only restores the enabled GUI state and does not make read-only or non-serialized members editable: combined with [ReadOnly] the field can receive focus, but its value still cannot be changed."),
            new BilingualData("与 DisableIf、DisableIn 等按条件禁用属性的特性不同，它不带任何条件，总是以启用状态绘制。",
                "Unlike DisableIf, DisableIn and similar attributes that disable properties conditionally, it takes no condition and always draws the property enabled.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } = { };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = { };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                EnableGUIExampleSO.Instance)
        };
    }
}
