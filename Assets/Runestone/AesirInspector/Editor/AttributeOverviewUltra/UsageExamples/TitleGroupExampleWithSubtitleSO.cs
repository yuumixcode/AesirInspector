using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// TitleGroup 特性的案例 SO。
    /// </summary>
    [AesirExample]
    public class TitleGroupExampleWithSubtitleSO : AttributeExampleSO<TitleGroupExampleWithSubtitleSO>
    {
        [Title("Parameter: subtitle (Literal String)")]
        [TitleGroup("Literal Subtitle", "Optional subtitle")]
        public int literalExample;

        [Title("Member Reference ($)")]
        public string subtitleField = "Subtitle from Field";

        [Title("Member Reference ($)")]
        [TitleGroup("Main Title", "$subtitleField")]
        public int referenceExample;

        [Title("Expression (@)")]
        [TitleGroup("Time Subtitle", "@\"Current Time: \" + System.DateTime.Now.ToString(\"HH:mm:ss\")")]
        public int expressionExample;

        public override void AesirInspectorReset()
        {
            literalExample = 0;
            subtitleField = "Subtitle from Field";
            referenceExample = 0;
            expressionExample = 0;
        }
    }
}
