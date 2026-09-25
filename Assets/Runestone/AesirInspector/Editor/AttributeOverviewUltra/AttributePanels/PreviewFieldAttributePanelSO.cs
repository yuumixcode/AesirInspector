namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// PreviewField 特性介绍面板。
    /// </summary>
    public class PreviewFieldAttributePanelSO : AbstractAttributePanelSO
    {
        /// <summary>
        /// 初始化面板数据。
        /// </summary>
        public override void Initialize() => SetData(new PreviewFieldAttributeData());
    }
}
