namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// TypeInfoBox 特性介绍面板。
    /// </summary>
    public class TypeInfoBoxAttributePanelSO : AbstractAttributePanelSO
    {
        /// <summary>
        /// 初始化面板数据。
        /// </summary>
        public override void Initialize() => SetData(new TypeInfoBoxAttributeData());
    }
}
