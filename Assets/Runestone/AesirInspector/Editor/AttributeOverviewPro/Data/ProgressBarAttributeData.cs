using System.Collections.Generic;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// ProgressBar 特性的介绍数据。
    /// </summary>
    internal class ProgressBarAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("Progress Bar", "进度条",
                "ProgressBar 特性在 Inspector 中绘制一个进度条。它可以用于显示数字属性的当前进度。",
                "The ProgressBar attribute draws a progress bar in the inspector. It can be used to visualize the progress of a numeric property.",
                OdinInspectorDocumentationLinks.ProgressBarUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("为数值属性绘制水平进度条，可直接点击或拖拽改变数值（获得焦点后也可用左右方向键微调）。",
                "Draws a horizontal progress bar for numeric properties that can be clicked or dragged to change the value (arrow keys adjust it once focused)."),
            new BilingualData("默认显示数值标签；Segmented 为 true 时默认不显示，且一旦指定 CustomValueStringGetter，数值标签会被自定义文本取代。",
                "The numeric value label is shown by default; Segmented = true hides it by default, and once CustomValueStringGetter is set the numeric label is replaced by the custom text."),
            new BilingualData("Min/Max 可用 $ 引用成员或 @ 表达式动态取值；放在只有 getter 的属性上时只能显示，无法编辑。",
                "Min/Max can be resolved dynamically with a $ member reference or @ expression; placed on a getter-only property, the bar is display-only and cannot be edited.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } = new ParameterValue[4]
        {
            new ParameterValue(typeof(double).FullName, "Min",
                new BilingualData("进度条的最小值。支持使用 $ 引用成员。",
                    "The minimum value of the progress bar. Supports $ for member reference.")),
            new ParameterValue(typeof(double).FullName, "Max",
                new BilingualData("进度条的最大值。支持使用 $ 引用成员。",
                    "The maximum value of the progress bar. Supports $ for member reference.")),
            new ParameterValue(typeof(float).FullName, "R, G, B",
                new BilingualData("进度条的颜色（0-1 范围）。", "The color of the progress bar (0-1 range).")),
            new ParameterValue(typeof(string).FullName, "CustomValueString",
                new BilingualData("显示在进度条上的自定义文本。支持使用 $ 引用成员。",
                    "Custom text to display on the progress bar. Supports $ for member reference."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } =
        {
            new ResolvedStringParameterValue("Min", ResolverType.ValueResolver, "double", "0",
                new List<ParameterValue>()),
            new ResolvedStringParameterValue("Max", ResolverType.ValueResolver, "double", "100",
                new List<ParameterValue>()),
            new ResolvedStringParameterValue("CustomValueString", ResolverType.ValueResolver, "string",
                "None", new List<ParameterValue>())
        };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                ProgressBarExampleSO.Instance)
        };
    }
}
