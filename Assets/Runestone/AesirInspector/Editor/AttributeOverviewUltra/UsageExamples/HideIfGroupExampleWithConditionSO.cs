using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// HideIfGroup 特性的案例 SO：Condition 参数的四种写法（字段 / 属性 / 方法 / @ 表达式）。
    /// </summary>
    [AesirExample]
    public class HideIfGroupExampleWithConditionSO :
        AttributeExampleSO<HideIfGroupExampleWithConditionSO>
    {
        public bool hideGroup = true;

        [HideIfGroup("Hidden", Condition = "hideGroup")]
        [FoldoutGroup("Hidden/Field")]
        public string fieldNameExample;

        [HideIfGroup("Hidden", Condition = "HideGroupProperty")]
        [FoldoutGroup("Hidden/Property")]
        public string propertyNameExample;

        [HideIfGroup("Hidden", Condition = "GetHiddenState")]
        [FoldoutGroup("Hidden/Method")]
        public string methodNameExample;

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
