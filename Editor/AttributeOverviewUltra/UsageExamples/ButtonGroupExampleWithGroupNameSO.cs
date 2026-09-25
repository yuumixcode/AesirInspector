using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// ButtonGroup 特性的案例 SO。
    /// </summary>
    [AesirExample]
    public class ButtonGroupExampleWithGroupNameSO : AttributeExampleSO<ButtonGroupExampleWithGroupNameSO>
    {
        [Title("Member Reference ($)")]
        public string groupNameField = "Custom Group";

        [Title("No Parameters")]
        [ButtonGroup]
        void DefaultGroupA() { }

        [Title("No Parameters")]
        [ButtonGroup]
        void DefaultGroupB() { }

        [Title("Parameter: Group")]
        [ButtonGroup("My Button Group")]
        void NamedGroupButtonA() { }

        [Title("Parameter: Group")]
        [ButtonGroup("My Button Group")]
        void NamedGroupButtonB() { }

        [Title("Member Reference ($)")]
        [ButtonGroup("$groupNameField")]
        void ReferenceMethod() { }

        [Title("Expression (@)")]
        [ButtonGroup("@\"Group_\" + groupNameField")]
        void ExpressionMethod() { }

        public override void AesirInspectorReset()
        {
            groupNameField = "Custom Group";
        }
    }
}
