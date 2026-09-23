using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// Title 特性的案例 SO。
    /// </summary>
    [AesirExample]
    public class TitleExampleSO : AttributeExampleSO<TitleExampleSO>
    {
        [FoldoutGroup("No Parameters")]
        [Title("Static title")]
        public int defaultTitle;

        [FoldoutGroup("No Parameters")]
        public int D;

        [FoldoutGroup("Parameter: Subtitle")]
        [Title("Title with subtitle", "This is a subtitle")]
        public int subtitleTitle;

        [FoldoutGroup("Parameter: TitleAlignment (Centered)")]
        [Title("Centered title", TitleAlignment = TitleAlignments.Centered)]
        public int centeredTitle;

        [FoldoutGroup("Parameter: TitleAlignment (Right)")]
        [Title("Right title", TitleAlignment = TitleAlignments.Right)]
        public int rightTitle;

        [FoldoutGroup("Parameter: TitleAlignment (Split)")]
        [Title("Split title", "Subtitle to the right", TitleAlignment = TitleAlignments.Split)]
        public int splitTitle;

        [FoldoutGroup("Parameter: HorizontalLine (False)")]
        [Title("Title without horizontal line", HorizontalLine = false)]
        public int noLineTitle;

        [FoldoutGroup("Parameter: Bold (False)")]
        [Title("Not bold title", Bold = false)]
        public int notBoldTitle;

        [FoldoutGroup("Usage on Properties and Methods")]
        [ShowInInspector]
        [Title("Title on a Property")]
        public int S { get; set; }

        [FoldoutGroup("Usage on Properties and Methods")]
        [Button]
        [Title("Title on a Method")]
        public void DoNothing()
        {
        }

        public override void AesirInspectorReset()
        {
            defaultTitle = 0;
            D = 0;
            subtitleTitle = 0;
            centeredTitle = 0;
            rightTitle = 0;
            splitTitle = 0;
            noLineTitle = 0;
            notBoldTitle = 0;
            S = 0;
        }
    }
}
