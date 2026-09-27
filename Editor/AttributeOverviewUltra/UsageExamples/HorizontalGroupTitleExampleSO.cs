using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// HorizontalGroup 特性的案例 SO：Title 参数（整个横向组的标题）。
    /// </summary>
    [AesirExample]
    public class HorizontalGroupTitleExampleSO : AttributeExampleSO<HorizontalGroupTitleExampleSO>
    {
        [HorizontalGroup("Titled", Title = "Horizontal Group Title")]
        public int first;

        [HorizontalGroup("Titled")]
        public int second;

        [HorizontalGroup("Titled")]
        public int third;

        public override void AesirInspectorReset()
        {
            first = 0;
            second = 0;
            third = 0;
        }
    }
}
