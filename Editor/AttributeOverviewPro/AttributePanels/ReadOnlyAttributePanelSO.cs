namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// ReadOnly 特性介绍面板。
    /// </summary>
    [AttributeCategory(AesirAttributeCategory.Essentials)]
    public class ReadOnlyAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new ReadOnlyAttributeData());
        }
    }
}
