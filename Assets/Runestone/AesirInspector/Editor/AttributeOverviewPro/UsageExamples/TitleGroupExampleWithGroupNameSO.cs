using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// TitleGroup 特性的案例 SO。
    /// </summary>
    [AesirExample]
    public class TitleGroupExampleWithGroupNameSO : AttributeExampleSO<TitleGroupExampleWithGroupNameSO>
    {
        [Title("Member Reference ($)")]
        public string groupNameField = "Dynamic Group";

        [Title("Member Reference ($)")]
        [TitleGroup("$groupNameField")]
        public int referenceExample;

        [Title("Member Reference ($)")]
        [TitleGroup("$groupNameField", "Optional subtitle")]
        public string secondReferenceExample;

        [Title("Member Reference ($)")]
        [TitleGroup("$groupNameField/Buttons")]
        [Button]
        private void GroupNameButton()
        {
        }

        [Title("Expression (@)")]
        [TitleGroup("@GetExpressionGroupName()")]
        public int expressionExample;

        string GetExpressionGroupName() => "Dynamic_" + groupNameField;

        public override void AesirInspectorReset()
        {
            groupNameField = "Dynamic Group";
            referenceExample = 0;
            secondReferenceExample = null;
            expressionExample = 0;
        }
    }
}
