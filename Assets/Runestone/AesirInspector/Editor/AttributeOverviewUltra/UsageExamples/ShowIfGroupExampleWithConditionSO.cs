using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.Editor
{
    [AesirExample]
    public class ShowIfGroupExampleWithConditionSO : AttributeExampleSO<ShowIfGroupExampleWithConditionSO>
    {
        [Title("Parameter: Condition (Field)")]
        public bool showGroup = true;

        [ShowIfGroup("Show", Condition = "showGroup")]
        [FoldoutGroup("Show/Field")]
        public string fieldNameExample;

        [Title("Parameter: Condition (Property)")]
        [ShowIfGroup("Show", Condition = "ShowGroupProperty")]
        [FoldoutGroup("Show/Property")]
        public string propertyNameExample;

        [Title("Parameter: Condition (Method)")]
        [ShowIfGroup("Show", Condition = "GetShowState")]
        [FoldoutGroup("Show/Method")]
        public string methodNameExample;

        [Title("Expression (@)")]
        [ShowIfGroup("Show", Condition = "@showGroup")]
        [FoldoutGroup("Show/Expression")]
        public string attributeExpressionExample;

        public bool ShowGroupProperty => showGroup;

        bool GetShowState() => showGroup;

        public override void AesirInspectorReset()
        {
            showGroup = true;
            fieldNameExample = string.Empty;
            propertyNameExample = string.Empty;
            methodNameExample = string.Empty;
            attributeExpressionExample = string.Empty;
        }
    }
}
