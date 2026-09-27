using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// ResponsiveButtonGroup 特性的案例 SO：基础用法（按钮自动换行铺满一行）。
    /// </summary>
    [AesirExample]
    public class ResponsiveButtonGroupExampleSO : AttributeExampleSO<ResponsiveButtonGroupExampleSO>
    {
        [ResponsiveButtonGroup]
        public void Foo() { }

        [ResponsiveButtonGroup]
        public void Bar() { }

        [ResponsiveButtonGroup]
        public void Baz() { }

        public override void AesirInspectorReset() { }
    }
}
