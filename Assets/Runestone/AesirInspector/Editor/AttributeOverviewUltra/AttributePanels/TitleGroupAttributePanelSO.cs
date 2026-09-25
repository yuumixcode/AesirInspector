namespace Runestone.AesirInspector.Editor
{
    [AttributeCategory(AesirAttributeCategory.Groups)]
    public class TitleGroupAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new TitleGroupAttributeData());
        }
    }
}
