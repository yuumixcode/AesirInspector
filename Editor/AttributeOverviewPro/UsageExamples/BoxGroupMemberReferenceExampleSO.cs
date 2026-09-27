using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// BoxGroup 特性的案例 SO：组名的 $ 成员引用与默认组。
    /// </summary>
    [AesirExample]
    public class BoxGroupMemberReferenceExampleSO : AttributeExampleSO<BoxGroupMemberReferenceExampleSO>
    {
        [BoxGroup("$G")]
        public string E = "Dynamic box title 2";

        [BoxGroup("$G")]
        public string F;

        [BoxGroup]
        public string G = "Dynamic Box Title";

        [BoxGroup]
        public string H;

        public override void AesirInspectorReset()
        {
            E = "Dynamic box title 2";
            F = string.Empty;
            G = "Dynamic Box Title";
            H = string.Empty;
        }
    }
}
