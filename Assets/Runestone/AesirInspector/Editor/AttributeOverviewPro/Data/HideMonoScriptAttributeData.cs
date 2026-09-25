using System;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// HideMonoScript 特性的介绍数据。
    /// </summary>
    internal class HideMonoScriptAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("HideMonoScript", "HideMonoScript",
                "HideMonoScript 特性用于隐藏类顶部的 Script 属性，让 Inspector 更简洁。",
                "The HideMonoScript attribute hides the Script property at the top of a class for a cleaner inspector.",
                "https://odininspector.com/attributes/hide-mono-script-attribute");

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("无参数，标注在类上，隐藏 Inspector 顶部的 Script 引用字段，使界面更简洁。",
                "Takes no parameters and is applied to a class; it hides the Script reference field at the top of the inspector for a cleaner look."),
            new BilingualData("效果等同于全局关闭 Odin 偏好设置中的 \"Show Mono Script In Editor\"，但仅作用于被标注的类型。",
                "Equivalent to globally disabling \"Show Mono Script In Editor\" in Odin preferences, but it only affects the decorated type."),
            new BilingualData("仅影响显示，脚本资源本身仍可在 Project 窗口中访问。",
                "Only the display is affected; the script asset itself remains reachable in the Project window.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } = Array.Empty<ParameterValue>();

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } =
            Array.Empty<ResolvedStringParameterValue>();

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Hide Mono Script",
                HideMonoScriptExampleSO.Instance)
        };
    }
}
