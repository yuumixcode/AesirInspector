using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// TitleGroup 特性的案例 SO：组名的 $ 成员引用与 @ 表达式解析。
    /// </summary>
    [AesirExample]
    public class TitleGroupExampleWithGroupNameSO : AttributeExampleSO<TitleGroupExampleWithGroupNameSO>
    {
        public string groupNameField = "Dynamic Group";

        [TitleGroup("$groupNameField")]
        public int referenceExample;

        [TitleGroup("$groupNameField", "Optional subtitle")]
        public string secondReferenceExample;

        [TitleGroup("@GetExpressionGroupName()")]
        public int expressionExample;

        [TitleGroup("$groupNameField/Buttons")]
        [Button]
        void GroupNameButton() { }

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
