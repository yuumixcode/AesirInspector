using System;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// DrawWithUnity 特性的介绍数据。
    /// </summary>
    internal class DrawWithUnityAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("DrawWithUnity", "DrawWithUnity",
                "DrawWithUnity 特性让 Odin 使用 Unity 的原生绘制系统绘制该成员，可用于排查某个属性在 Odin 下绘制异常的问题。",
                "The DrawWithUnity attribute makes Odin draw the member with Unity's original drawing system, useful when troubleshooting an Odin-drawn property.",
                "https://odininspector.com/attributes/draw-with-unity-attribute");

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("无参数，作用于成员本身。",
                "Takes no parameters and applies to the member itself."),
            new BilingualData("仅影响绘制方式，不表示对属性完全禁用 Odin；其他更高优先级的特性仍会生效。",
                "It only affects drawing; it does not fully disable Odin, and higher-priority attributes still apply."),
            new BilingualData("可选择性关闭某个成员的 Odin 绘制，而不影响整个类型。",
                "Lets you selectively opt a single member out of Odin drawing without affecting the whole type.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } = Array.Empty<ParameterValue>();

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } =
            Array.Empty<ResolvedStringParameterValue>();

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Draw With Unity",
                DrawWithUnityExampleSO.Instance)
        };
    }
}
