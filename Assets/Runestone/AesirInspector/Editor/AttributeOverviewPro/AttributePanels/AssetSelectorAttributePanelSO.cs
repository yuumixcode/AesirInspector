namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// AssetSelector 特性介绍面板。
    /// </summary>
    public class AssetSelectorAttributePanelSO : AbstractAttributePanelSO
    {
        /// <summary>
        /// 初始化面板数据。
        /// </summary>
        public override void Initialize() => SetData(new AssetSelectorAttributeData());
    }
}
