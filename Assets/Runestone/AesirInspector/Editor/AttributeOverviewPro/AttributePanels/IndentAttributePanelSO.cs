namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// Indent 特性介绍面板。
    /// </summary>
    [AttributeCategory(AesirAttributeCategory.Misc)]
    public class IndentAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new IndentAttributeData());
        }
    }
}
