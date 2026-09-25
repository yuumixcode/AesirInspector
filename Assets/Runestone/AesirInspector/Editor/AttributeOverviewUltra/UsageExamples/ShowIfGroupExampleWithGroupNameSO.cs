using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.Editor
{
    [AesirExample]
    public class ShowIfGroupExampleWithGroupNameSO : AttributeExampleSO<ShowIfGroupExampleWithGroupNameSO>
    {
        [Title("Member Reference ($)")]
        public bool toggle = true;

        public string groupName = "DynamicGroup";

        [ShowIfGroup("$groupName", Condition = "toggle")]
        [BoxGroup("$groupName/Content")]
        public string content;

        [BoxGroup("$groupName/Content")]
        public int value;

        [Title("Expression (@)")]
        [ShowIfGroup("@\"Group_\" + groupName", Condition = "toggle")]
        public int expressionValue;

        public override void AesirInspectorReset()
        {
            toggle = true;
            groupName = "DynamicGroup";
            content = string.Empty;
            value = 0;
            expressionValue = 0;
        }
    }
}
