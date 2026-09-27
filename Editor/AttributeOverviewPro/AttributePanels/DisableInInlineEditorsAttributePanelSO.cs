namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// DisableInInlineEditors 特性介绍面板。
    /// </summary>
    public class DisableInInlineEditorsAttributePanelSO : AbstractAttributePanelSO
    {
        /// <summary>
        /// 初始化面板数据。
        /// </summary>
        public override void Initialize() => SetData(new DisableInInlineEditorsAttributeData());
    }
}
