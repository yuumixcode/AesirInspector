namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// DisableContextMenu 特性介绍面板。
    /// </summary>
    public class DisableContextMenuAttributePanelSO : AbstractAttributePanelSO
    {
        /// <summary>
        /// 初始化面板数据。
        /// </summary>
        public override void Initialize() => SetData(new DisableContextMenuAttributeData());
    }
}
