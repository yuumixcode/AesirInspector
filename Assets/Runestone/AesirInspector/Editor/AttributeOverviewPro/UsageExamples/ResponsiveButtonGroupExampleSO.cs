using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// ResponsiveButtonGroup 特性的案例 SO。
    /// </summary>
    [AesirExample]
    public class ResponsiveButtonGroupExampleSO : AttributeExampleSO<ResponsiveButtonGroupExampleSO>
    {
        [Title("No Parameters")]
        [ResponsiveButtonGroup]
        public void Foo()
        {
        }

        [ResponsiveButtonGroup]
        public void Bar()
        {
        }

        [ResponsiveButtonGroup]
        public void Baz()
        {
        }

        [Title("Parameter: UniformLayout")]
        [ResponsiveButtonGroup("UniformGroup", UniformLayout = true)]
        public void Foo1()
        {
        }

        [ResponsiveButtonGroup("UniformGroup")]
        public void Foo2()
        {
        }

        [ResponsiveButtonGroup("UniformGroup")]
        public void LongesNameWins()
        {
        }

        [ResponsiveButtonGroup("UniformGroup")]
        public void Foo4()
        {
        }

        [Title("Parameter: DefaultButtonSize")]
        [ResponsiveButtonGroup("DefaultButtonSize", DefaultButtonSize = ButtonSizes.Small)]
        public void Bar1()
        {
        }

        [ResponsiveButtonGroup("DefaultButtonSize")]
        public void Bar2()
        {
        }

        [Button(ButtonSizes.Large)]
        [ResponsiveButtonGroup("DefaultButtonSize")]
        public void Bar3()
        {
        }

        [Title("Combining With Groups")]
        [FoldoutGroup("SomeOtherGroup", 0f)]
        [ResponsiveButtonGroup("SomeOtherGroup/SomeBtnGroup")]
        public void Baz1()
        {
        }

        [ResponsiveButtonGroup("SomeOtherGroup/SomeBtnGroup")]
        public void Baz2()
        {
        }

        public override void AesirInspectorReset()
        {
        }
    }
}
