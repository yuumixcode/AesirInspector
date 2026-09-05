namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// LabelText 特性介绍面板。
    /// </summary>
    [AttributeCategory(AesirAttributeCategory.Essentials)]
    public class LabelTextAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new LabelTextAttributeData());
        }
    }
}
