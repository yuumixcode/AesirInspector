namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// RequiredListLength 特性介绍面板。
    /// </summary>
    public class RequiredListLengthAttributePanelSO : AbstractAttributePanelSO
    {
        /// <summary>
        /// 初始化面板数据。
        /// </summary>
        public override void Initialize() => SetData(new RequiredListLengthAttributeData());
    }
}
