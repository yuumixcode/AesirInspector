namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// ShowInInlineEditors 特性介绍面板。
    /// </summary>
    public class ShowInInlineEditorsAttributePanelSO : AbstractAttributePanelSO
    {
        /// <summary>
        /// 初始化面板数据。
        /// </summary>
        public override void Initialize() => SetData(new ShowInInlineEditorsAttributeData());
    }
}
