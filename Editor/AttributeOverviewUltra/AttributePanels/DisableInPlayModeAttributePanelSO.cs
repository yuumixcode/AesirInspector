namespace Runestone.AesirInspector.Editor
{
    [AttributeCategory(AesirAttributeCategory.Conditionals)]
    internal class DisableInPlayModeAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new DisableInPlayModeAttributeData());
        }
    }
}
