namespace Runestone.AesirInspector.Editor
{
    [AttributeCategory(AesirAttributeCategory.Conditionals)]
    public class HideIfGroupAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new HideIfGroupAttributeData());
        }
    }
}
