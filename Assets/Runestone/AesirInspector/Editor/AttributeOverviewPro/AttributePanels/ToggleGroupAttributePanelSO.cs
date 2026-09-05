namespace Runestone.AesirInspector.Editor
{
    [AttributeCategory(AesirAttributeCategory.Groups)]
    public class ToggleGroupAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new ToggleGroupAttributeData());
        }
    }
}
