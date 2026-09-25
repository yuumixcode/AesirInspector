namespace Runestone.AesirInspector.Editor
{
    [AttributeCategory(AesirAttributeCategory.Meta)]
    public class OnInspectorInitAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new OnInspectorInitAttributeData());
        }
    }
}
