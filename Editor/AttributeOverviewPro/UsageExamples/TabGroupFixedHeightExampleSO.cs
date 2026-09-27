using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// TabGroup 特性的案例 SO：UseFixedHeight 参数（组高度自适应内容 vs 固定为最高的标签页）。
    /// </summary>
    [AesirExample]
    public class TabGroupFixedHeightExampleSO : AttributeExampleSO<TabGroupFixedHeightExampleSO>
    {
        [TabGroup("Adaptive", "Short")]
        public int shortTab;

        [TabGroup("Adaptive", "Tall")]
        [DisplayAsString]
        public string tallTab = "\n\n\n\nTall Content";

        [TabGroup("Fixed", "Short", UseFixedHeight = true)]
        public int fixedShortTab;

        [TabGroup("Fixed", "Tall", UseFixedHeight = true)]
        [DisplayAsString]
        public string fixedTallTab = "\n\n\n\nTall Content";

        public override void AesirInspectorReset()
        {
            shortTab = 0;
            tallTab = "\n\n\n\nTall Content";
            fixedShortTab = 0;
            fixedTallTab = "\n\n\n\nTall Content";
        }
    }
}
