using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// ButtonGroup 特性的案例 SO：组名的 $ 成员引用与 @ 表达式解析。
    /// </summary>
    [AesirExample]
    public class ButtonGroupResolvedStringExampleSO : AttributeExampleSO<ButtonGroupResolvedStringExampleSO>
    {
        public string groupNameField = "Custom Group";

        [ButtonGroup("$groupNameField")]
        void ReferenceMethod() { }

        [ButtonGroup("@\"Group_\" + groupNameField")]
        void ExpressionMethod() { }

        public override void AesirInspectorReset()
        {
            groupNameField = "Custom Group";
        }
    }
}
