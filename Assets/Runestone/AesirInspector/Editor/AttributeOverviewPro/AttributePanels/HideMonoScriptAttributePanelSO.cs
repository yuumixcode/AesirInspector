namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// HideMonoScript 特性介绍面板。
    /// </summary>
    public class HideMonoScriptAttributePanelSO : AbstractAttributePanelSO
    {
        /// <summary>
        /// 初始化面板数据。
        /// </summary>
        public override void Initialize() => SetData(new HideMonoScriptAttributeData());
    }
}
