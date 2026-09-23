using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// ToggleLeft 特性的案例 SO。
    /// </summary>
    [AesirExample]
    public class ToggleLeftExampleSO : AttributeExampleSO<ToggleLeftExampleSO>
    {
        [Title("No Parameters")]
        [InfoBox("Draws the toggle button before the label for a bool property.", InfoMessageType.Info)]
        [ToggleLeft]
        public bool leftToggled;

        [Title("Combining With EnableIf")]
        [EnableIf("leftToggled")]
        public int A;

        [Title("Combining With EnableIf")]
        [EnableIf("leftToggled")]
        public bool B;

        [Title("Combining With EnableIf")]
        [EnableIf("leftToggled")]
        public bool C;

        public override void AesirInspectorReset()
        {
            leftToggled = false;
            A = 0;
            B = false;
            C = false;
        }
    }
}
