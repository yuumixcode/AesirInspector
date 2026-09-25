namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// ShowPropertyResolver 特性介绍面板（Debug 分类）。
    /// </summary>
    public class ShowPropertyResolverAttributePanelSO : AbstractAttributePanelSO
    {
        /// <summary>
        /// 初始化面板数据。
        /// </summary>
        public override void Initialize() => SetData(new ShowPropertyResolverAttributeData());
    }
}
