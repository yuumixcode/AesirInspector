namespace Runestone.AesirInspector.Editor
{
    [AttributeCategory(AesirAttributeCategory.Essentials)]
    public class DelayedPropertyAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new DelayedPropertyAttributeData());
        }
    }
}
