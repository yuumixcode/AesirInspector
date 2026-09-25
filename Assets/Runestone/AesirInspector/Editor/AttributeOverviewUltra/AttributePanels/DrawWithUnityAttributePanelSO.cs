namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// DrawWithUnity 特性介绍面板。
    /// </summary>
    public class DrawWithUnityAttributePanelSO : AbstractAttributePanelSO
    {
        /// <summary>
        /// 初始化面板数据。
        /// </summary>
        public override void Initialize() => SetData(new DrawWithUnityAttributeData());
    }
}
