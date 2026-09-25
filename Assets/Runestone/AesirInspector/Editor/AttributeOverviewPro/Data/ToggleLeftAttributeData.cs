namespace Runestone.AesirInspector.Editor
{
    internal class ToggleLeftAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("ToggleLeft", "ToggleLeft",
                "ToggleLeft 特性用于将 bool 字段的开关绘制在左侧，类似于 Unity 原生的 Toggle 行为。",
                "The ToggleLeft attribute is used to draw the toggle of a bool field on the left side, similar to Unity's native Toggle behavior.",
                OdinInspectorDocumentationLinks.ToggleLeftUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("把 bool 字段或属性的复选框绘制在标签左侧，而不是 Odin 默认的右侧。",
                "Draws the checkbox of a bool field or property before the label instead of Odin's default position after it."),
            new BilingualData("只对 bool 类型生效，并且没有参数。",
                "Only works on bool values and takes no parameters."),
            new BilingualData("常与 EnableIf、DisableIf 等条件特性组合，用开关联动控制其他字段的可编辑状态。",
                "Often combined with conditional attributes such as EnableIf and DisableIf to drive other fields' editability from the toggle.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } = null;
        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = null;

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("No Parameters",
                ToggleLeftExampleSO.Instance)
        };
    }
}
