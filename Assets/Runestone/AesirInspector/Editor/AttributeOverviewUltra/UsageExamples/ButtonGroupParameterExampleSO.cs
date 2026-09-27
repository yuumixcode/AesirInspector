using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// ButtonGroup 特性的案例 SO：Order 与 ButtonHeight 参数。
    /// </summary>
    [AesirExample]
    public class ButtonGroupParameterExampleSO : AttributeExampleSO<ButtonGroupParameterExampleSO>
    {
        [ButtonGroup("Ordered", Order = 20)]
        void OrderTwenty() { }

        [ButtonGroup("Ordered", Order = 10)]
        void OrderTen() { }

        [ButtonGroup("Tall", ButtonHeight = 40)]
        void ButtonHeightForty() { }

        [ButtonGroup("Tall")]
        void DefaultButtonHeight() { }

        public override void AesirInspectorReset() { }
    }
}
