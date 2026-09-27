using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// VerticalGroup 特性的案例 SO：PaddingTop / PaddingBottom 参数。
    /// </summary>
    [AesirExample]
    public class VerticalGroupPaddingExampleSO : AttributeExampleSO<VerticalGroupPaddingExampleSO>
    {
        [VerticalGroup("Padded", PaddingTop = 10, PaddingBottom = 10)]
        public int paddedTopAndBottom;

        [VerticalGroup("Padded")]
        public int defaultPadding;

        public override void AesirInspectorReset()
        {
            paddedTopAndBottom = 0;
            defaultPadding = 0;
        }
    }
}
