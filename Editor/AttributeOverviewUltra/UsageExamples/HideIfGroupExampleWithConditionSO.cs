using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.Editor
{
    [AesirExample]
    public class HideIfGroupExampleWithConditionSO : AttributeExampleSO<HideIfGroupExampleWithConditionSO>
    {
        [Title("Parameter: Condition (Field)")]
        public bool hideGroup = true;

        [HideIfGroup("Hidden", Condition = "hideGroup")]
        [FoldoutGroup("Hidden/Field")]
        public string fieldNameExample;

        [Title("Parameter: Condition (Property)")]
        [HideIfGroup("Hidden", Condition = "HideGroupProperty")]
        [FoldoutGroup("Hidden/Property")]
        public string propertyNameExample;

        [Title("Parameter: Condition (Method)")]
        [HideIfGroup("Hidden", Condition = "GetHiddenState")]
        [FoldoutGroup("Hidden/Method")]
        public string methodNameExample;

        [Title("Expression (@)")]
        [HideIfGroup("Hidden", Condition = "@hideGroup")]
        [FoldoutGroup("Hidden/Expression")]
        public string attributeExpressionExample;

        public bool HideGroupProperty => hideGroup;

        bool GetHiddenState() => hideGroup;

        public override void AesirInspectorReset()
        {
            hideGroup = true;
            fieldNameExample = string.Empty;
            propertyNameExample = string.Empty;
            methodNameExample = string.Empty;
            attributeExpressionExample = string.Empty;
        }
    }
}
