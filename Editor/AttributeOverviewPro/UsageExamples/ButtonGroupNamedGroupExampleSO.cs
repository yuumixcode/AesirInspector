using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// ButtonGroup 特性的案例 SO：具名组（Parameter: Group）。
    /// </summary>
    [AesirExample]
    public class ButtonGroupNamedGroupExampleSO : AttributeExampleSO<ButtonGroupNamedGroupExampleSO>
    {
        [ButtonGroup("My Button Group")]
        [Button(ButtonSizes.Large)]
        void BigButtonInGroup() { }

        [ButtonGroup("My Button Group")]
        [GUIColor(0f, 1f, 0f)]
        void GreenButtonInGroup() { }

        public override void AesirInspectorReset() { }
    }
}
