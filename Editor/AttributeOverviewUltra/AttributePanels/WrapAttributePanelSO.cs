namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// Wrap 特性介绍面板。
    /// </summary>
    public class WrapAttributePanelSO : AbstractAttributePanelSO
    {
        /// <summary>
        /// 初始化面板数据。
        /// </summary>
        public override void Initialize() => SetData(new WrapAttributeData());
    }
}
