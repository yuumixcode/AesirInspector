using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// TitleGroup 特性的案例 SO。
    /// </summary>
    [AesirExample]
    public class TitleGroupExampleSO : AttributeExampleSO<TitleGroupExampleSO>
    {
        [FoldoutGroup("No Parameters")]
        [TitleGroup("No Parameters/Ints")]
        public int SomeInt1;

        [FoldoutGroup("No Parameters")]
        [TitleGroup("No Parameters/Ints", "Optional subtitle", TitleAlignments.Split)]
        public int SomeInt2;

        [FoldoutGroup("Member Reference ($)")]
        [TitleGroup("Member Reference ($)/$SomeString1", "Optional subtitle")]
        public string SomeString1 = "Dynamic Title";

        [FoldoutGroup("Member Reference ($)")]
        [TitleGroup("Member Reference ($)/$SomeString1", "Optional subtitle")]
        public string SomeString2;

        [FoldoutGroup("Parameter: Subtitle")]
        [TitleGroup("Parameter: Subtitle/Main Title", "This is a subtitle")]
        public int withSubtitle;

        [FoldoutGroup("Parameter: Alignment")]
        [TitleGroup("Parameter: Alignment/Centered Title", "Subtitle", TitleAlignments.Centered)]
        public int centered;

        [FoldoutGroup("Parameter: Alignment")]
        [TitleGroup("Parameter: Alignment/Split Title", "Subtitle", TitleAlignments.Split)]
        public int split;

        [FoldoutGroup("Parameter: HorizontalLine")]
        [TitleGroup("Parameter: HorizontalLine/No Horizontal Line", horizontalLine: false)]
        public int noHorizontalLine;

        [FoldoutGroup("Parameter: BoldTitle")]
        [TitleGroup("Parameter: BoldTitle/Not Bold", boldTitle: false)]
        public int notBold;

        [FoldoutGroup("Parameter: Indent")]
        [TitleGroup("Parameter: Indent/Indented Title", indent: true)]
        public int indented;

        [FoldoutGroup("Parameter: Indent")]
        [TitleGroup("Parameter: Indent/Deeply Indented", indent: true)]
        public int nestedIndented;

        [FoldoutGroup("Parameter: Order")]
        [TitleGroup("Parameter: Order/Order 5", order: 5)]
        public int order5;

        [FoldoutGroup("Parameter: Order")]
        [TitleGroup("Parameter: Order/Order 2", order: 2)]
        public int order2;

        [FoldoutGroup("Properties And Methods")]
        [TitleGroup("Properties And Methods/Vectors", "Optional subtitle", TitleAlignments.Centered)]
        public Vector2 SomeVector1;

        [FoldoutGroup("Combining With Other Attributes")]
        [BoxGroup("Combining With Other Attributes/Titles", ShowLabel = false)]
        [TitleGroup("Combining With Other Attributes/Titles/First Title")]
        public int A;

        [FoldoutGroup("Combining With Other Attributes")]
        [BoxGroup("Combining With Other Attributes/Titles/Boxed")]
        [TitleGroup("Combining With Other Attributes/Titles/Boxed/Second Title")]
        public int B;

        [FoldoutGroup("Combining With Other Attributes")]
        [TitleGroup("Combining With Other Attributes/Titles/Boxed/Second Title")]
        public int C;

        [FoldoutGroup("Combining With Other Attributes")]
        [TitleGroup("Combining With Other Attributes/Multiple Stacked Boxes")]
        [HorizontalGroup("Combining With Other Attributes/Multiple Stacked Boxes/Split")]
        [VerticalGroup("Combining With Other Attributes/Multiple Stacked Boxes/Split/Left")]
        [BoxGroup("Combining With Other Attributes/Multiple Stacked Boxes/Split/Left/Box A")]
        public int BoxA;

        [FoldoutGroup("Combining With Other Attributes")]
        [BoxGroup("Combining With Other Attributes/Multiple Stacked Boxes/Split/Left/Box B")]
        public int BoxB;

        [FoldoutGroup("Combining With Other Attributes")]
        [VerticalGroup("Combining With Other Attributes/Multiple Stacked Boxes/Split/Right")]
        [BoxGroup("Combining With Other Attributes/Multiple Stacked Boxes/Split/Right/Box C")]
        public int BoxC;

        [FoldoutGroup("Combining With Other Attributes")]
        [VerticalGroup("Combining With Other Attributes/Multiple Stacked Boxes/Split/Right")]
        [BoxGroup("Combining With Other Attributes/Multiple Stacked Boxes/Split/Right/Box C")]
        public int BoxD;

        [FoldoutGroup("Combining With Other Attributes")]
        [VerticalGroup("Combining With Other Attributes/Multiple Stacked Boxes/Split/Right")]
        [BoxGroup("Combining With Other Attributes/Multiple Stacked Boxes/Split/Right/Box C")]
        public int BoxE;

        [FoldoutGroup("Properties And Methods")]
        [TitleGroup("Properties And Methods/Vectors")]
        [ShowInInspector]
        public Vector2 SomeVector2 { get; set; }

        [FoldoutGroup("No Parameters")]
        [TitleGroup("No Parameters/Ints/Buttons")]
        [Button]
        void IntButton() { }

        [FoldoutGroup("Member Reference ($)")]
        [TitleGroup("Member Reference ($)/$SomeString1/Buttons")]
        [Button]
        void StringButton() { }

        [FoldoutGroup("Properties And Methods")]
        [TitleGroup("Properties And Methods/Vectors")]
        [Button]
        void VectorButton() { }

        [FoldoutGroup("Combining With Other Attributes")]
        [TitleGroup("Combining With Other Attributes/Titles/Horizontal Buttons")]
        [ButtonGroup("Combining With Other Attributes/Titles/Horizontal Buttons/Buttons")]
        [Button]
        void FirstButton() { }

        [FoldoutGroup("Combining With Other Attributes")]
        [ButtonGroup("Combining With Other Attributes/Titles/Horizontal Buttons/Buttons")]
        [Button]
        void SecondButton() { }

        public override void AesirInspectorReset()
        {
            SomeInt1 = 0;
            SomeInt2 = 0;
            SomeString1 = "Dynamic Title";
            SomeString2 = null;
            withSubtitle = 0;
            centered = 0;
            split = 0;
            noHorizontalLine = 0;
            notBold = 0;
            indented = 0;
            nestedIndented = 0;
            order5 = 0;
            order2 = 0;
            SomeVector1 = default;
            SomeVector2 = default;
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
