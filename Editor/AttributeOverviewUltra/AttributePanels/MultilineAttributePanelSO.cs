namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// Unity Multiline 特性介绍面板（Unity 分类）。
    /// </summary>
    public class MultilineAttributePanelSO : AbstractAttributePanelSO
    {
        /// <summary>
        /// 初始化面板数据。
        /// </summary>
        public override void Initialize() => SetData(new MultilineAttributeData());
    }
}
