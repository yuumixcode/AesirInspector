namespace Runestone.AesirInspector.Editor
{
    [AttributeCategory(AesirAttributeCategory.Conditionals)]
    internal class HideInAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new HideInAttributeData());
        }
    }
}
