namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// HideIf 特性介绍面板。
    /// </summary>
    [AttributeCategory(AesirAttributeCategory.Conditionals)]
    public class HideIfAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new HideIfAttributeData());
        }
    }
}
