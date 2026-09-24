using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// BilingualText 特性的案例 SO。
    /// </summary>
    [AesirExample]
    internal class BilingualTextExampleSO : AttributeExampleSO<BilingualTextExampleSO>
    {
        [BilingualTitle("只读双语文本", "Read-only Bilingual Text")]
        [BilingualText("这段文本以只读方式展示。", "This text is displayed as read-only.")]
        public string readOnlyText = "Hello Aesir";

        [BilingualText("带图标的文本", "Text With Icon", icon: SdfIconType.StarFill)]
        public int textWithIcon = 42;

        [BilingualText("不美化英文文本", "doNotNicifyEnglishText", nicifyEnglishText: false)]
        public bool notNicifiedField;

        [BilingualText("带颜色的图标文本", "Colored Icon Text",
            icon: SdfIconType.ExclamationTriangleFill, iconColor: "#FFAA00")]
        public float coloredIconField = 3.14f;

        public override void AesirInspectorReset()
        {
            readOnlyText = "Hello Aesir";
            textWithIcon = 42;
            notNicifiedField = false;
            coloredIconField = 3.14f;
        }
    }
}
