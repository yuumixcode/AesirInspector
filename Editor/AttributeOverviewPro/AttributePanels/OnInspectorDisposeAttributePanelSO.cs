namespace Runestone.AesirInspector.Editor
{
    [AttributeCategory(AesirAttributeCategory.Meta)]
    public class OnInspectorDisposeAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new OnInspectorDisposeAttributeData());
        }
    }
}
