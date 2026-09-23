using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// PropertyTooltip 特性的案例 SO。
    /// </summary>
    [AesirExample]
    public class PropertyTooltipExampleSO : AttributeExampleSO<PropertyTooltipExampleSO>
    {
        [Title("No Parameters")]
        [PropertyTooltip("This is tooltip on an int property.")]
        public int MyInt;

        [Title("Member Reference ($)")]
        [PropertyTooltip("$Tooltip")]
        public string Tooltip = "Dynamic tooltip.";

        [Title("Expression (@)")]
        [PropertyTooltip("@\"Current Time: \" + DateTime.Now.ToString()")]
        public int expressionTooltip;

        [Title("Usage on Methods")]
        [Button]
        [PropertyTooltip("Button Tooltip")]
        private void ButtonWithTooltip()
        {
        }

        public override void AesirInspectorReset()
        {
            MyInt = 0;
            Tooltip = "Dynamic tooltip.";
            expressionTooltip = 0;
        }
    }
}
