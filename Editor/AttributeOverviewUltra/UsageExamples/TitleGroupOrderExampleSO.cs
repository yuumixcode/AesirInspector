using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// TitleGroup 特性的案例 SO：order 参数（数值小的组排在上面）。
    /// </summary>
    [AesirExample]
    public class TitleGroupOrderExampleSO : AttributeExampleSO<TitleGroupOrderExampleSO>
    {
        [TitleGroup("Order 5", order: 5)]
        public int order5;

        [TitleGroup("Order 2", order: 2)]
        public int order2;

        public override void AesirInspectorReset()
        {
            order5 = 0;
            order2 = 0;
        }
    }
}
