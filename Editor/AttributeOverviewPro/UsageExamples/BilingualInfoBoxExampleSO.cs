using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// BilingualInfoBox 特性的案例 SO。
    /// </summary>
    [AesirExample]
    internal class BilingualInfoBoxExampleSO : AttributeExampleSO<BilingualInfoBoxExampleSO>
    {
        [BilingualTitle("消息类型", "Message Types")]
        [BilingualInfoBox("这是一条普通信息。", "This is a regular info message.")]
        public int infoField;

        [BilingualInfoBox("这是一条警告信息。", "This is a warning message.", InfoMessageType.Warning)]
        public int warningField;

        [BilingualInfoBox("这是一条错误信息。", "This is an error message.", InfoMessageType.Error)]
        public int errorField;

        [BilingualInfoBox("带图标的信息框。", "Info box with an icon.", icon: SdfIconType.InfoCircleFill)]
        public int iconField;

        [BilingualTitle("叠加与条件显示", "Stacking And Visibility")]
        [BilingualInfoBox("第一条叠加消息。", "The first stacked message.")]
        [BilingualInfoBox("第二条叠加消息。", "The second stacked message.", InfoMessageType.Warning)]
        public int stackedField;

        public bool showConditionalBox = true;

        [BilingualInfoBox("仅当 showConditionalBox 为真时显示。", "Shown only while showConditionalBox is true.",
            visibleIf: "@showConditionalBox")]
        public int conditionalField;

        public override void AesirInspectorReset()
        {
            infoField = 0;
            warningField = 0;
            errorField = 0;
            iconField = 0;
            stackedField = 0;
            showConditionalBox = true;
            conditionalField = 0;
        }
    }
}
