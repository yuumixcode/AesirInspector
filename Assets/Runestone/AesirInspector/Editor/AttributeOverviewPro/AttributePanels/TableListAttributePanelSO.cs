namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// TableList 特性介绍面板。
    /// </summary>
    [AttributeCategory(AesirAttributeCategory.Collections)]
    public class TableListAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new TableListAttributeData());
        }
    }
}
