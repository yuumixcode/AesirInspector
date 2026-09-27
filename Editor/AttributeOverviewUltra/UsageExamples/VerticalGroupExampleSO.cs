using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// VerticalGroup 特性的案例 SO：与 HorizontalGroup 组合成左右两列。
    /// </summary>
    [AesirExample]
    public class VerticalGroupExampleSO : AttributeExampleSO<VerticalGroupExampleSO>
    {
        [HorizontalGroup("Split")]
        [VerticalGroup("Split/Left")]
        public InfoMessageType First;

        [VerticalGroup("Split/Left")]
        public InfoMessageType Second;

        [HideLabel]
        [VerticalGroup("Split/Right")]
        public int A;

        [HideLabel]
        [VerticalGroup("Split/Right")]
        public int B;

        public override void AesirInspectorReset()
        {
            First = default;
            Second = default;
            A = 0;
            B = 0;
        }
    }
}
