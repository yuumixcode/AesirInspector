namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// MinValue 特性介绍面板。
    /// </summary>
    [AttributeCategory(AesirAttributeCategory.Numbers)]
    public class MinValueAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new MinValueAttributeData());
        }
    }
}
