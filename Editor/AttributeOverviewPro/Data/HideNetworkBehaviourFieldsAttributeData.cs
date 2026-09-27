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
            new BilingualData(
                "无参数，标注在类上，隐藏 NetworkBehaviour 特有的 \"Network Channel\" 与 \"Network Send Interval\" 字段。",
                "Takes no parameters and is applied to a class; it hides the \"Network Channel\" and \"Network Send Interval\" fields specific to NetworkBehaviour."),
            new BilingualData("仅对派生自 UnityEngine.Networking.NetworkBehaviour 的类生效，对其他类型无任何影响。",
                "Only affects classes derived from UnityEngine.Networking.NetworkBehaviour; it has no effect on any other type."),
            new BilingualData("项目未启用旧版 UNET 网络模块（UnityEngine.Networking）时，该特性不产生效果。",
                "It has no effect in projects where the legacy UNET networking module (UnityEngine.Networking) is not available.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } = Array.Empty<ParameterValue>();

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } =
            Array.Empty<ResolvedStringParameterValue>();

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
            Array.Empty<AttributeExamplePreviewItem>();
    }
}
