using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// TitleGroup 特性的案例 SO：与 BoxGroup、HorizontalGroup、VerticalGroup、ButtonGroup 组合。
    /// </summary>
    [AesirExample]
    public class TitleGroupCombiningExampleSO : AttributeExampleSO<TitleGroupCombiningExampleSO>
    {
        [BoxGroup("Titles", ShowLabel = false)]
        [TitleGroup("Titles/First Title")]
        public int A;

        [BoxGroup("Titles/Boxed")]
        [TitleGroup("Titles/Boxed/Second Title")]
        public int B;

        [TitleGroup("Titles/Boxed/Second Title")]
        public int C;

        [TitleGroup("Titles/Horizontal Buttons")]
        [ButtonGroup("Titles/Horizontal Buttons/Buttons")]
        [Button]
        void FirstButton() { }

        [ButtonGroup("Titles/Horizontal Buttons/Buttons")]
        [Button]
        void SecondButton() { }

        [TitleGroup("Multiple Stacked Boxes")]
        [HorizontalGroup("Multiple Stacked Boxes/Split")]
        [VerticalGroup("Multiple Stacked Boxes/Split/Left")]
        [BoxGroup("Multiple Stacked Boxes/Split/Left/Box A")]
        public int BoxA;

        [BoxGroup("Multiple Stacked Boxes/Split/Left/Box B")]
        public int BoxB;

        [VerticalGroup("Multiple Stacked Boxes/Split/Right")]
        [BoxGroup("Multiple Stacked Boxes/Split/Right/Box C")]
        public int BoxC;

        [BoxGroup("Multiple Stacked Boxes/Split/Right/Box C")]
        public int BoxD;

        [BoxGroup("Multiple Stacked Boxes/Split/Right/Box C")]
        public int BoxE;

        public override void AesirInspectorReset()
        {
            A = 0;
            B = 0;
            C = 0;
            BoxA = 0;
            BoxB = 0;
            BoxC = 0;
            BoxD = 0;
            BoxE = 0;
        }
    }
}
