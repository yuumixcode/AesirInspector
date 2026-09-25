namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// HideInTables 特性介绍面板。
    /// </summary>
    public class HideInTablesAttributePanelSO : AbstractAttributePanelSO
    {
        /// <summary>
        /// 初始化面板数据。
        /// </summary>
        public override void Initialize() => SetData(new HideInTablesAttributeData());
    }
}
