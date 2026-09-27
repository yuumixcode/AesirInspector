using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// ShowIfGroup 特性的案例 SO：组名的 $ 成员引用与 @ 表达式解析。
    /// </summary>
    [AesirExample]
    public class ShowIfGroupExampleWithGroupNameSO : AttributeExampleSO<ShowIfGroupExampleWithGroupNameSO>
    {
        public bool toggle = true;

        public string groupName = "DynamicGroup";

        [ShowIfGroup("$groupName", Condition = "toggle")]
        [BoxGroup("$groupName/Content")]
        public string content;

        [BoxGroup("$groupName/Content")]
        public int value;

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
