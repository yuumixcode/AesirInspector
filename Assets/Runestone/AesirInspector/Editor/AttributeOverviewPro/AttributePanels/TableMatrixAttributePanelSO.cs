namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// TableMatrix 特性介绍面板。
    /// </summary>
    public class TableMatrixAttributePanelSO : AbstractAttributePanelSO
    {
        /// <summary>
        /// 初始化面板数据。
        /// </summary>
        public override void Initialize() => SetData(new TableMatrixAttributeData());
    }
}
