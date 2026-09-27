using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// ToggleGroup 特性的案例 SO：组标题的 $ 成员引用。
    /// </summary>
    [AesirExample]
    public class ToggleGroupMemberReferenceExampleSO : AttributeExampleSO<ToggleGroupMemberReferenceExampleSO>
    {
        [ToggleGroup("EnableGroupOne", "$GroupOneTitle")]
        public bool EnableGroupOne = true;

        [ToggleGroup("EnableGroupOne")]
        public string GroupOneTitle = "One";

        [ToggleGroup("EnableGroupOne")]
        public float GroupOneA;

        [ToggleGroup("EnableGroupOne")]
        public float GroupOneB;

        public override void AesirInspectorReset()
        {
            EnableGroupOne = true;
            GroupOneTitle = "One";
            GroupOneA = 0f;
            GroupOneB = 0f;
        }
    }
}
