using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// HorizontalGroup 特性的案例 SO：Gap 参数（列间距）。
    /// </summary>
    [AesirExample]
    public class HorizontalGroupGapExampleSO : AttributeExampleSO<HorizontalGroupGapExampleSO>
    {
        [HorizontalGroup("Gap 3", Gap = 3f)]
        public int first;

        [HorizontalGroup("Gap 3")]
        public int second;

        [HorizontalGroup("Gap 3")]
        public int third;

        [HorizontalGroup("Gap 20", Gap = 20)]
        public int fourth;

        [HorizontalGroup("Gap 20")]
        public int fifth;

        public override void AesirInspectorReset()
        {
            first = 0;
            second = 0;
            third = 0;
            fourth = 0;
            fifth = 0;
        }
    }
}
