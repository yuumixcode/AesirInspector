using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// EnableGUI 特性的案例 SO。
    /// </summary>
    [AesirExample]
    public class EnableGUIExampleSO : AttributeExampleSO<EnableGUIExampleSO>
    {
        [Title("No Parameters")]
        public string normalField = "Normal editable field";

        [Title("Combining With Other Attributes")]
        [ReadOnly]
        public string readOnlyField = "Read-only field (grayed out)";

        [Title("Combining With Other Attributes")]
        [ReadOnly]
        [EnableGUI]
        public string enabledReadOnlyField = "Can receive focus despite being read-only";

        [Title("No Parameters")]
        [ShowInInspector]
        public int GUIDisabledProperty => 10;

        [Title("No Parameters")]
        [ShowInInspector]
        [EnableGUI]
        public int GUIEnabledProperty => 10;

        public override void AesirInspectorReset()
        {
            normalField = "Normal editable field";
            readOnlyField = "Read-only field (grayed out)";
            enabledReadOnlyField = "Can receive focus despite being read-only";
        }
    }
}
