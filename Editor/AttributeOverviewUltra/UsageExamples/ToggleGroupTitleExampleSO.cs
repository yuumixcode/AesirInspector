using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// ToggleGroup 特性的案例 SO：ToggleGroupTitle 参数（自定义组标题）。
    /// </summary>
    [AesirExample]
    public class ToggleGroupTitleExampleSO : AttributeExampleSO<ToggleGroupTitleExampleSO>
    {
        [ToggleGroup(nameof(Toggle2), "Custom Title")]
        public bool Toggle2;

        [ToggleGroup(nameof(Toggle2))]
        public int field3;

        [ToggleGroup(nameof(Toggle4), "Toggle 4")]
        public bool Toggle4;

        [ToggleGroup(nameof(Toggle4))]
        public int field5;

        public override void AesirInspectorReset()
        {
            Toggle2 = false;
            field3 = 0;
            Toggle4 = false;
            field5 = 0;
        }
    }
}
