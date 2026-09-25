namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// HideDuplicateReferenceBox 特性介绍面板。
    /// </summary>
    public class HideDuplicateReferenceBoxAttributePanelSO : AbstractAttributePanelSO
    {
        /// <summary>
        /// 初始化面板数据。
        /// </summary>
        public override void Initialize() => SetData(new HideDuplicateReferenceBoxAttributeData());
    }
}
