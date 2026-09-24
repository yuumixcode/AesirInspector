using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// BilingualTitle 特性的案例 SO。
    /// </summary>
    [AesirExample]
    internal class BilingualTitleExampleSO : AttributeExampleSO<BilingualTitleExampleSO>
    {
        [BilingualTitle("基础标题", "Basic Title")]
        public int basicField = 10;

        [BilingualTitle("带副标题", "With Subtitle", "副标题显示在标题下方", "The subtitle below the title")]
        public string fieldWithSubtitle = "Hello";

        [BilingualTitle("居中对齐且无分割线", "Centered Without Line",
            titleAlignment: TitleAlignments.Centered, horizontalLine: false)]
        public float centeredField = 1.5f;

        [BilingualTitle("取消加粗", "Not Bold", bold: false, beforeSpace: false)]
        public bool notBoldField;

        public override void AesirInspectorReset()
        {
            basicField = 10;
            fieldWithSubtitle = "Hello";
            centeredField = 1.5f;
            notBoldField = false;
        }
    }
}
