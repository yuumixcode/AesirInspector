using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// Title 特性的案例 SO。
    /// </summary>
    [AesirExample]
    public class TitleExampleWithSubtitleSO : AttributeExampleSO<TitleExampleWithSubtitleSO>
    {
        [Title("Parameter: Subtitle")]
        [Title("Static title", "Static subtitle")]
        public int E;

        [Title("Member Reference ($)")]
        public string dynamicSubtitle = "Subtitle from Field";

        [Title("Member Reference ($)")]
        [Title("Main Title", "$dynamicSubtitle")]
        public int referenceExample;

        [Title("Expression (@)")]
        [Title("Main Title", "@\"Current Time: \" + System.DateTime.Now.ToString(\"HH:mm:ss\")")]
        public int expressionExample;

        [Title("Expression (@)")]
        [Title("Static title", "@DateTime.Now.ToString(\"HH:mm:ss\")")]
        public int subtitleExpression;

        public override void AesirInspectorReset()
        {
            E = 0;
            dynamicSubtitle = "Subtitle from Field";
            referenceExample = 0;
            expressionExample = 0;
            subtitleExpression = 0;
        }
    }
}
