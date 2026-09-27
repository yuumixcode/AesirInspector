using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// ResponsiveButtonGroup 特性的案例 SO：UniformLayout 与 DefaultButtonSize 参数。
    /// </summary>
    [AesirExample]
    public class ResponsiveButtonGroupParameterExampleSO :
        AttributeExampleSO<ResponsiveButtonGroupParameterExampleSO>
    {
        [ResponsiveButtonGroup("UniformGroup", UniformLayout = true)]
        public void Uniform1() { }

        [ResponsiveButtonGroup("UniformGroup")]
        public void Uniform2() { }

        [ResponsiveButtonGroup("UniformGroup")]
        public void LongesNameWins() { }

        [ResponsiveButtonGroup("UniformGroup")]
        public void Uniform4() { }

        [ResponsiveButtonGroup("DefaultButtonSize", DefaultButtonSize = ButtonSizes.Small)]
        public void Small1() { }

        [ResponsiveButtonGroup("DefaultButtonSize")]
        public void Small2() { }

        [Button(ButtonSizes.Large)]
        [ResponsiveButtonGroup("DefaultButtonSize")]
        public void LargeOverridesGroupSize() { }

        public override void AesirInspectorReset() { }
    }
}
