namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// BilingualDetailInfoBox 特性介绍面板（Aesir 自定义双语特性，归入 Aesir Customs 分类）。
    /// </summary>
    public class BilingualDetailInfoBoxAttributePanelSO : AbstractAttributePanelSO
    {
        /// <summary>
        /// 初始化面板数据。
        /// </summary>
        public override void Initialize() => SetData(new BilingualDetailInfoBoxAttributeData());
    }
}
