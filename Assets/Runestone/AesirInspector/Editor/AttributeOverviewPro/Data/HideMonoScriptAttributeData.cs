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
            new BilingualData("无参数，标注在类上。",
                "Takes no parameters and is applied to a class."),
            new BilingualData("隐藏后仍可通过右键菜单等方式访问脚本资源。",
                "The script asset remains reachable through context menus after hiding.")
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
