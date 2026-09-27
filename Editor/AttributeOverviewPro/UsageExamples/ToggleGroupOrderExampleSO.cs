using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// ToggleGroup 特性的案例 SO：Order 与 CollapseOthersOnExpand 参数。
    /// </summary>
    [AesirExample]
    public class ToggleGroupOrderExampleSO : AttributeExampleSO<ToggleGroupOrderExampleSO>
    {
        [ToggleGroup(nameof(Toggle3), 10)]
        public bool Toggle3;

        [ToggleGroup(nameof(Toggle3))]
        public int field4;

        [ToggleGroup(nameof(Toggle5), CollapseOthersOnExpand = true)]
        public bool Toggle5;

        [ToggleGroup(nameof(Toggle5))]
        public int field6;

        public override void AesirInspectorReset()
        {
            Toggle3 = false;
            field4 = 0;
            Toggle5 = false;
            field6 = 0;
        }
    }
}
