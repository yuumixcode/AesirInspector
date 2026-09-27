using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// DetailInfoBox 特性的案例 SO。
    /// </summary>
    [AesirExample]
    public class DetailInfoBoxBilingualExampleSO : AttributeExampleSO<DetailInfoBoxBilingualExampleSO>
    {
        [Title("Parameter: chinese, english, detailsChinese, detailsEnglish")]
        [BilingualDetailInfoBox("这是中文消息", "This is English message", "这是中文详细内容",
            "This is English detailed content")]
        public int bilingualExample;

        [Title("Member Reference ($)")]
        [BilingualDetailInfoBox("动态详细内容", "Dynamic Details", "$dynamicDetailsChinese",
            "$dynamicDetailsEnglish")]
        public int dynamicExample;

        [Title("Member Reference ($)")]
        public string dynamicDetailsChinese = "来自字段的中文详细内容";

        [Title("Member Reference ($)")]
        public string dynamicDetailsEnglish = "Detailed content from field";

        [Title("Parameter: infoMessageType")]
        [BilingualDetailInfoBox("点击 DetailedInfoBox...", "Click the DetailedInfoBox...",
            "……以展开更多信息！\n这可以减少编辑器中不必要的杂乱内容，同时在需要时保留所有相关信息。",
            "... to reveal more information!\nThis allows you to reduce unnecessary clutter in your editors, and still have all the relevant information available when required.")]
        public int infoMessageTypeExample;

        [Title("Parameter: infoMessageType")]
        [BilingualDetailInfoBox("这是警告消息", "This is a warning message", "警告消息的中文详细内容。",
            "Detailed content of the warning message.", InfoMessageType.Warning)]
        public int warningMessageTypeExample;

        [Title("Parameter: infoMessageType")]
        [BilingualDetailInfoBox("这是错误消息", "This is an error message", "错误消息的中文详细内容。",
            "Detailed content of the error message.", InfoMessageType.Error)]
        public int errorMessageTypeExample;

        [Title("Parameter: infoMessageType")]
        [BilingualDetailInfoBox("这是无图标消息", "This is a message without an icon", "无图标消息的中文详细内容。",
            "Detailed content of the message without an icon.", InfoMessageType.None)]
        public int noneMessageTypeExample;

        [Title("Parameter: visibleIf")]
        public bool showInfoBox = true;

        [Title("Parameter: visibleIf")]
        [BilingualDetailInfoBox("仅在 showInfoBox 为 true 时显示", "Only shown while showInfoBox is true",
            "切换 showInfoBox 以显示或隐藏此消息框。", "Toggle showInfoBox to show or hide this message box.",
            InfoMessageType.Info, "showInfoBox")]
        public int visibleIfExample;

        public override void AesirInspectorReset()
        {
            bilingualExample = 0;
            dynamicExample = 0;
            dynamicDetailsChinese = "来自字段的中文详细内容";
            dynamicDetailsEnglish = "Detailed content from field";
            infoMessageTypeExample = 0;
            warningMessageTypeExample = 0;
            errorMessageTypeExample = 0;
            noneMessageTypeExample = 0;
            showInfoBox = true;
            visibleIfExample = 0;
        }
    }
}
