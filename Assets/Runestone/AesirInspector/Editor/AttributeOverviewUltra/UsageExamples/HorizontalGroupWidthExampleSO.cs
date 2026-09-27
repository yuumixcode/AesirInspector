using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// HorizontalGroup 特性的案例 SO：Width 参数（相对宽度 0.25f、绝对宽度 150 与 100、自动填充）。
    /// </summary>
    [AesirExample]
    public class HorizontalGroupWidthExampleSO : AttributeExampleSO<HorizontalGroupWidthExampleSO>
    {
        [HorizontalGroup("Relative", Width = 0.25f)]
        public int relativeWidth;

        [HorizontalGroup("Relative")]
        public int autoFill;

        [HorizontalGroup("Absolute", Width = 150f)]
        public int width150;

        [HorizontalGroup("Absolute", Width = 100)]
        public int width100;

        public override void AesirInspectorReset()
        {
            relativeWidth = 0;
            autoFill = 0;
            width150 = 0;
            width100 = 0;
        }
    }
}
