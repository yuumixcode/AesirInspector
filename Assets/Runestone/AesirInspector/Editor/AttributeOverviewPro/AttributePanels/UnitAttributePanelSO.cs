namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// Unit 特性介绍面板。
    /// </summary>
    public class UnitAttributePanelSO : AbstractAttributePanelSO
    {
        /// <summary>
        /// 初始化面板数据。
        /// </summary>
        public override void Initialize() => SetData(new UnitAttributeData());
    }
}
