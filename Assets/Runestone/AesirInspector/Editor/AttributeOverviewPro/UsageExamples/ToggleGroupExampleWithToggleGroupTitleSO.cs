using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// ToggleGroup 特性的案例 SO：组标题的 $ 成员引用（字段 / 属性）与 @ 表达式解析。
    /// </summary>
    [AesirExample]
    public class ToggleGroupExampleWithToggleGroupTitleSO : AttributeExampleSO<
        ToggleGroupExampleWithToggleGroupTitleSO>
    {
        public string toggleTitleField = "Dynamic Toggle Title";

        [ToggleGroup(nameof(Toggle1), "$toggleTitleField")]
        public bool Toggle1;

        [ToggleGroup(nameof(Toggle1))]
        public int referenceExample;

        [ToggleGroup(nameof(Toggle3), "$TitleFromProperty")]
        public bool Toggle3;

        [ToggleGroup(nameof(Toggle3))]
        public float Test;

        [ToggleGroup(nameof(Toggle2), "@\"Dynamic_\" + System.DateTime.Now.DayOfWeek")]
        public bool Toggle2;

        [ToggleGroup(nameof(Toggle2))]
        public int expressionExample;

        public string TitleFromProperty => "Test: " + Test;

        public override void AesirInspectorReset()
        {
            toggleTitleField = "Dynamic Toggle Title";
            Toggle1 = false;
            referenceExample = 0;
            Toggle3 = false;
            Test = 0f;
            Toggle2 = false;
            expressionExample = 0;
        }
    }
}
