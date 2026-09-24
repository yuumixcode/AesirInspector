namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// Unity Range 特性介绍面板（Unity 分类）。
    /// </summary>
    public class RangeAttributePanelSO : AbstractAttributePanelSO
    {
        /// <summary>
        /// 初始化面板数据。
        /// </summary>
        public override void Initialize() => SetData(new RangeAttributeData());
    }
}
