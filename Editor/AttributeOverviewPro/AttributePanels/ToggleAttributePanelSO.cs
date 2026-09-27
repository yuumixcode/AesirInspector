namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// Toggle 特性介绍面板。
    /// </summary>
    public class ToggleAttributePanelSO : AbstractAttributePanelSO
    {
        /// <summary>
        /// 初始化面板数据。
        /// </summary>
        public override void Initialize() => SetData(new ToggleAttributeData());
    }
}
