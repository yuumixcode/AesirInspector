namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// DisallowModificationsIn 特性介绍面板。
    /// </summary>
    public class DisallowModificationsInAttributePanelSO : AbstractAttributePanelSO
    {
        /// <summary>
        /// 初始化面板数据。
        /// </summary>
        public override void Initialize() => SetData(new DisallowModificationsInAttributeData());
    }
}
