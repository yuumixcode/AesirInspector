using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// TitleGroup 特性的案例 SO：subtitle 参数（字面量 / $ 成员引用 / @ 表达式）与 TitleAlignments。
    /// </summary>
    [AesirExample]
    public class TitleGroupExampleWithSubtitleSO : AttributeExampleSO<TitleGroupExampleWithSubtitleSO>
    {
        [TitleGroup("Literal Subtitle", "Optional subtitle")]
        public int literalExample;

        public string subtitleField = "Subtitle from Field";

        [TitleGroup("$ Field Subtitle", "$subtitleField")]
        public int referenceExample;

        [TitleGroup("Centered Title", "Optional subtitle", TitleAlignments.Centered)]
        public int centered;

        [TitleGroup("Split Title", "Optional subtitle", TitleAlignments.Split)]
        public int split;

        public override void AesirInspectorReset()
        {
            literalExample = 0;
            subtitleField = "Subtitle from Field";
            referenceExample = 0;
            centered = 0;
            split = 0;
        }
    }
}
