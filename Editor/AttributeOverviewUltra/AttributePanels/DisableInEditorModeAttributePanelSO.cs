namespace Runestone.AesirInspector.Editor
{
    [AttributeCategory(AesirAttributeCategory.Conditionals)]
    internal class DisableInEditorModeAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new DisableInEditorModeAttributeData());
        }
    }
}
