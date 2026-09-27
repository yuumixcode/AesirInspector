using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// InfoBox 特性的案例 SO。
    /// </summary>
    [AesirExample]
    public class InfoBoxExampleWithVisibleIfSO : AttributeExampleSO<InfoBoxExampleWithVisibleIfSO>
    {
        [Title("Member Reference : Field")]
        public bool toggleInfoBox;

        [Title("Member Reference : Field")]
        [InfoBox("This box is only visible when toggleInfoBox is true.", "toggleInfoBox")]
        public int referenceExample;

        [Title("Member Reference : Method")]
        [InfoBox("This info box is only shown while in editor mode.", InfoMessageType.Error, "IsInEditMode")]
        public float editorModeInfoBox;

        [Title("Expression (@)")]
        [InfoBox("Visible when current second is even.", "@DateTime.Now.Second % 2 == 0")]
        public int expressionExample;

        static bool IsInEditMode() => !Application.isPlaying;

        public override void AesirInspectorReset()
        {
            toggleInfoBox = false;
            referenceExample = 0;
            editorModeInfoBox = 0;
            expressionExample = 0;
        }
    }
}
