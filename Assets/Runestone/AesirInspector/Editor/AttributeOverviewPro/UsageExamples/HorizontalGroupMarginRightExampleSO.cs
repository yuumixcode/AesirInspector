using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// HorizontalGroup 特性的案例 SO：MarginRight 参数（相对右边距 0.4f）。
    /// </summary>
    [AesirExample]
    public class HorizontalGroupMarginRightExampleSO :
        AttributeExampleSO<HorizontalGroupMarginRightExampleSO>
    {
        [HorizontalGroup("MarginRight", MarginRight = 0.4f)]
        public int marginRight;

        [HorizontalGroup("MarginRight")]
        public int noMarginRight;

        public override void AesirInspectorReset()
        {
            marginRight = 0;
            noMarginRight = 0;
        }
    }
}
