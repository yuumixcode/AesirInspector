using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// VerticalGroup 特性的案例 SO。
    /// </summary>
    [AesirExample]
    public class VerticalGroupExampleSO : AttributeExampleSO<VerticalGroupExampleSO>
    {
        [Title("No Parameters")]
        [HorizontalGroup("Split")]
        [VerticalGroup("Split/Left")]
        public InfoMessageType First;

        [VerticalGroup("Split/Left")]
        public InfoMessageType Second;

        [HideLabel]
        [VerticalGroup("Split/Right")]
        public int A;

        [HideLabel]
        [VerticalGroup("Split/Right")]
        public int B;

        [Title("Parameter: PaddingTop, PaddingBottom")]
        [VerticalGroup("Padded", PaddingTop = 10, PaddingBottom = 10)]
        public int padded1;

        [VerticalGroup("Padded")]
        public int padded2;

        [Title("Combining With BoxGroup")]
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
            First = default;
            Second = default;
            A = 0;
            B = 0;
            padded1 = 0;
            padded2 = 0;
            BoxA = 0;
            BoxB = 0;
            BoxC = 0;
            BoxD = 0;
            BoxE = 0;
        }
    }
}
