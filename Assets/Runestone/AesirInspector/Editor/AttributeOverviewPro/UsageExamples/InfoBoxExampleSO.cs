using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// InfoBox 特性的案例 SO。
    /// </summary>
    [AesirExample]
    public class InfoBoxExampleSO : AttributeExampleSO<InfoBoxExampleSO>
    {
        [FoldoutGroup("No Parameters")]
        [InfoBox("This is a default info box.")]
        public int defaultInfoBox;

        [FoldoutGroup("Parameter: MessageType (Info)")]
        [InfoBox("Default info box.", InfoMessageType.Info)]
        public int infoTypeInfoBox;

        [FoldoutGroup("Parameter: MessageType (Warning)")]
        [InfoBox("This is a warning info box.", InfoMessageType.Warning)]
        public int warningInfoBox;

        [FoldoutGroup("Parameter: MessageType (Error)")]
        [InfoBox("This is an error info box.", InfoMessageType.Error)]
        public int errorInfoBox;

        [FoldoutGroup("Parameter: MessageType (None)")]
        [InfoBox("This info box has no icon.", InfoMessageType.None)]
        public int noIconInfoBox;

        [FoldoutGroup("Parameter: GUIAlwaysEnabled")]
        [ReadOnly]
        [InfoBox("This info box is always enabled, even if the property is read-only.",
            GUIAlwaysEnabled = true)]
        public int guiAlwaysEnabled;

        [FoldoutGroup("Parameter: Icon, IconColor")]
        [InfoBox("Custom icon and color.", Icon = SdfIconType.InfoCircle, IconColor = "cyan")]
        public int customIcon;

        [FoldoutGroup("Parameter: VisibleIf")]
        public bool ToggleInfoBoxes = true;

        [FoldoutGroup("Parameter: VisibleIf")]
        [InfoBox("This info box is hideable by the toggle above.", "ToggleInfoBoxes")]
        public float hideableInfoBox1;

        [FoldoutGroup("Parameter: VisibleIf")]
        [InfoBox("This info box is hideable by the toggle above.", "ToggleInfoBoxes")]
        public float hideableInfoBox2;

        public override void AesirInspectorReset()
        {
            defaultInfoBox = 0;
            infoTypeInfoBox = 0;
            warningInfoBox = 0;
            errorInfoBox = 0;
            noIconInfoBox = 0;
            guiAlwaysEnabled = 0;
            customIcon = 0;
            ToggleInfoBoxes = true;
            hideableInfoBox1 = 0;
            hideableInfoBox2 = 0;
        }
    }
}
