using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// VerticalGroup 特性的案例 SO：与 BoxGroup 组合（左右两列各自堆叠多个 BoxGroup）。
    /// </summary>
    [AesirExample]
    public class VerticalGroupCombiningExampleSO : AttributeExampleSO<VerticalGroupCombiningExampleSO>
    {
        [HorizontalGroup("Stacked Split")]
        [VerticalGroup("Stacked Split/Left")]
        [BoxGroup("Stacked Split/Left/Box A")]
        public int BoxA;

        [BoxGroup("Stacked Split/Left/Box B")]
        public int BoxB;

        [VerticalGroup("Stacked Split/Right")]
        [BoxGroup("Stacked Split/Right/Box C")]
        public int BoxC;

        [BoxGroup("Stacked Split/Right/Box C")]
        public int BoxD;

        [BoxGroup("Stacked Split/Right/Box C")]
        public int BoxE;

        public override void AesirInspectorReset()
        {
            BoxA = 0;
            BoxB = 0;
            BoxC = 0;
            BoxD = 0;
            BoxE = 0;
        }
    }
}
