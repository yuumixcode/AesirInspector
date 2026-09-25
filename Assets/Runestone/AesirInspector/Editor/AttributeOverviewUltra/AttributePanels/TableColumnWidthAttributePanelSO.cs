namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// TableColumnWidth 特性介绍面板。
    /// </summary>
    public class TableColumnWidthAttributePanelSO : AbstractAttributePanelSO
    {
        /// <summary>
        /// 初始化面板数据。
        /// </summary>
        public override void Initialize() => SetData(new TableColumnWidthAttributeData());
    }
}
