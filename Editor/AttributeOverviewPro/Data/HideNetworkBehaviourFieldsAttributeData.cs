using System;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// HideNetworkBehaviourFields 特性的介绍数据。
    /// </summary>
    internal class HideNetworkBehaviourFieldsAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("HideNetworkBehaviourFields", "HideNetworkBehaviourFields",
                "HideNetworkBehaviourFields 特性用于隐藏 NetworkBehaviour 特有的 Network Channel 与 Network Send Interval 属性，让 Inspector 更简洁。",
                "The HideNetworkBehaviourFields attribute hides the Network Channel and Network Send Interval properties specific to NetworkBehaviour for a cleaner inspector.",
                "https://odininspector.com/attributes/hide-network-behaviour-fields-attribute");

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("无参数，标注在类上。",
                "Takes no parameters and is applied to a class."),
            new BilingualData("仅对派生自 NetworkBehaviour 的类生效；未启用 UNET 模块的项目中该特性不产生效果。",
                "Only affects classes derived from NetworkBehaviour; it has no effect in projects without the UNET module."),
            new BilingualData("对非 NetworkBehaviour 类型无任何影响。",
                "It has no effect at all on types that are not NetworkBehaviour.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } = Array.Empty<ParameterValue>();

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } =
            Array.Empty<ResolvedStringParameterValue>();

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
            Array.Empty<AttributeExamplePreviewItem>();
    }
}
