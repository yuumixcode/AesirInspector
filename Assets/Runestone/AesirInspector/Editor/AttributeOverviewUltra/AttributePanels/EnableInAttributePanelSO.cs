namespace Runestone.AesirInspector.Editor
{
    [AttributeCategory(AesirAttributeCategory.Conditionals)]
    internal class EnableInAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new EnableInAttributeData());
        }
    }
}
