namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// TypeRegistryItem 特性介绍面板。
    /// </summary>
    public class TypeRegistryItemAttributePanelSO : AbstractAttributePanelSO
    {
        /// <summary>
        /// 初始化面板数据。
        /// </summary>
        public override void Initialize() => SetData(new TypeRegistryItemAttributeData());
    }
}
