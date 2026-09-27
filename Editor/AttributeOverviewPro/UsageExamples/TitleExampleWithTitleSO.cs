using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// Title 特性的案例 SO。
    /// </summary>
    [AesirExample]
    public class TitleExampleWithTitleSO : AttributeExampleSO<TitleExampleWithTitleSO>
    {
        [Title("Member Reference ($)")]
        public string dynamicTitle = "Title from Field";

        [Title("Member Reference ($)")]
        [Title("$dynamicTitle")]
        public int referenceExample;

        [Title("Member Reference ($)")]
        public string MyTitle = "My Dynamic Title";

        [Title("Member Reference ($)")]
        public string MySubtitle = "My Dynamic Subtitle";

        [Title("Member Reference ($)")]
        [Title("$Combined", null, TitleAlignments.Centered)]
        public int Q;

        [Title("Expression (@)")]
        [Title("@DateTime.Now.ToString(\"dd:MM:yyyy\")", "@DateTime.Now.ToString(\"HH:mm:ss\")")]
        public int Expression;

        [Title("Expression (@)")]
        [Title("@\"Current Date: \" + System.DateTime.Now.ToString(\"dd:MM:yyyy\")")]
        public int expressionExample;

        [Title("Member Reference ($)")]
        [ShowInInspector]
        public string Combined => MyTitle + " - " + MySubtitle;

        public override void AesirInspectorReset()
        {
            dynamicTitle = "Title from Field";
            referenceExample = 0;
            MyTitle = "My Dynamic Title";
            MySubtitle = "My Dynamic Subtitle";
            Q = 0;
            Expression = 0;
            expressionExample = 0;
        }
    }
}
