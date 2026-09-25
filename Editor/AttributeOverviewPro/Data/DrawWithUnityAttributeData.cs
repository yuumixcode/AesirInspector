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
            new BilingualData("让 Odin 使用 Unity 的原生绘制系统绘制该成员，用于排查某个属性在 Odin 下绘制异常的问题，且只影响被标注的成员。", "Makes Odin draw the member with Unity's original drawing system, which helps troubleshoot a property that draws incorrectly under Odin, and only affects the member it is applied to."),
            new BilingualData("它本身仍是一个 Odin Drawer（内部调用 Unity 的 PropertyField），并不表示完全禁用 Odin；存在优先级更高的其他特性时，Unity 绘制并不保证生效。", "It is still an Odin drawer that calls into Unity's PropertyField, so it does not mean Odin is fully disabled; when a higher-priority attribute is present, Unity drawing is not guaranteed to take effect."),
            new BilingualData("要求该成员存在对应的 Unity SerializedProperty，否则不会绘制该成员，而是在原位置显示错误信息。", "It requires a corresponding Unity SerializedProperty for the member; otherwise the member is not drawn and an error message is shown in its place."),
            new BilingualData("设置 PreferImGUI = true 可强制走 IMGUI 绘制路径，默认在支持时优先使用 UI Toolkit 的 PropertyField。", "Setting PreferImGUI = true forces the IMGUI drawing path; by default the UI Toolkit PropertyField is preferred when supported.")
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
