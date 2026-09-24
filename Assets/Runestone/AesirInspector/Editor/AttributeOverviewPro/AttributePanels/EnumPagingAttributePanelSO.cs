namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// EnumPaging 特性介绍面板。
    /// </summary>
    public class EnumPagingAttributePanelSO : AbstractAttributePanelSO
    {
        /// <summary>
        /// 初始化面板数据。
        /// </summary>
        public override void Initialize() => SetData(new EnumPagingAttributeData());
    }
}
