using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// InfoBox 特性的案例 SO。
    /// </summary>
    [AesirExample]
    public class InfoBoxExampleWithMessageSO : AttributeExampleSO<InfoBoxExampleWithMessageSO>
    {
        [Title("Member Reference ($)")]
        [InfoBox("$messageField")]
        public string messageField = "Dynamic message from field";

        [Title("Member Reference ($)")]
        [InfoBox("$messageField")]
        public int referenceExample;

        [Title("Expression (@)")]
        [InfoBox("@\"Time: \" + DateTime.Now.ToString(\"HH:mm:ss\")")]
        public int expressionExample;

        public override void AesirInspectorReset()
        {
            messageField = "Dynamic message from field";
            referenceExample = 0;
            expressionExample = 0;
        }
    }
}
