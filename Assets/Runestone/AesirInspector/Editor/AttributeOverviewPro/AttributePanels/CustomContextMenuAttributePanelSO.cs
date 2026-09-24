namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// CustomContextMenu 特性介绍面板。
    /// </summary>
    public class CustomContextMenuAttributePanelSO : AbstractAttributePanelSO
    {
        /// <summary>
        /// 初始化面板数据。
        /// </summary>
        public override void Initialize() => SetData(new CustomContextMenuAttributeData());
    }
}
