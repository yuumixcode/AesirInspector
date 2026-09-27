using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// ResponsiveButtonGroup 特性的案例 SO：与 FoldoutGroup 嵌套组合。
    /// </summary>
    [AesirExample]
    public class ResponsiveButtonGroupCombiningExampleSO :
        AttributeExampleSO<ResponsiveButtonGroupCombiningExampleSO>
    {
        [FoldoutGroup("SomeOtherGroup")]
        [ResponsiveButtonGroup("SomeOtherGroup/SomeBtnGroup")]
        public void Baz1() { }

        [ResponsiveButtonGroup("SomeOtherGroup/SomeBtnGroup")]
        public void Baz2() { }

        public override void AesirInspectorReset() { }
    }
}
