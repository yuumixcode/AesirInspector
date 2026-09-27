using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// ShowIfGroup 特性的案例 SO：Condition 参数的四种写法（字段 / 属性 / 方法 / @ 表达式）。
    /// </summary>
    [AesirExample]
    public class ShowIfGroupExampleWithConditionSO :
        AttributeExampleSO<ShowIfGroupExampleWithConditionSO>
    {
        public bool showGroup = true;

        [ShowIfGroup("Show", Condition = "showGroup")]
        [FoldoutGroup("Show/Field")]
        public string fieldNameExample;

        [ShowIfGroup("Show", Condition = "ShowGroupProperty")]
        [FoldoutGroup("Show/Property")]
        public string propertyNameExample;

        [ShowIfGroup("Show", Condition = "GetShowState")]
        [FoldoutGroup("Show/Method")]
        public string methodNameExample;

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
