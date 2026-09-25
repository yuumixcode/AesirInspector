namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// ShowDrawerChain 特性介绍面板（Debug 分类）。
    /// </summary>
    public class ShowDrawerChainAttributePanelSO : AbstractAttributePanelSO
    {
        /// <summary>
        /// 初始化面板数据。
        /// </summary>
        public override void Initialize() => SetData(new ShowDrawerChainAttributeData());
    }
}
