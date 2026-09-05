namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// ValidateInput 特性介绍面板。
    /// </summary>
    [AttributeCategory(AesirAttributeCategory.Validation)]
    public class ValidateInputAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new ValidateInputAttributeData());
        }
    }
}
