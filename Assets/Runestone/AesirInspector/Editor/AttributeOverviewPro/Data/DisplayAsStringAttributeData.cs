using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// DisplayAsString 特性的介绍数据。
    /// </summary>
    internal class DisplayAsStringAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("DisplayAsString", "DisplayAsString",
                "DisplayAsString 特性将属性值绘制为简单的文本标签，而不是可编辑的输入框。",
                "The DisplayAsString attribute draws the property value as a simple text label instead of an editable input field.",
                OdinInspectorDocumentationLinks.DisplayAsStringUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("使用属性值的 ToString() 绘制为纯文本标签且不可编辑，适用于字符串、数字、Color 等任意类型；配合 [HideLabel] 可作为提示信息展示。", "Draws the property value as a plain, non-editable text label using its ToString(), and works for any type such as strings, numbers or Colors; combined with [HideLabel] it can display a message."),
            new BilingualData("overflow 默认为 true：文本过长时溢出并在控件边界被裁剪；设为 false 时会自动换行展开为多行。", "overflow defaults to true, so long text overflows and is clipped at the control bounds; set it to false to let the text wrap onto multiple lines."),
            new BilingualData("富文本默认关闭，需要显式开启 enableRichText；字号与对齐分别由 fontSize 和 alignment 控制。", "Rich text is off by default and must be enabled explicitly with enableRichText; font size and alignment are controlled by fontSize and alignment."),
            new BilingualData("作用于集合时不会替换为文本，而是交给后续 Drawer 正常绘制集合。", "On collections it does not replace the drawing with text; the collection is drawn normally by the following drawer.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(bool).FullName, "enableRichText",
                new BilingualData("是否启用富文本渲染。", "Whether to enable rich text rendering.")),
            new ParameterValue(typeof(int).FullName, "fontSize",
                new BilingualData("字体大小。", "The font size of the text.")),
            new ParameterValue(typeof(TextAlignment).FullName, "alignment",
                new BilingualData("文本对齐方式。", "The alignment of the text.")),
            new ParameterValue(typeof(bool).FullName, "overflow",
                new BilingualData("文本过长时是否允许溢出显示。如果为 false，则会进行裁剪。",
                    "Whether the text should overflow if it's too long. If false, it will be clipped."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = { };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                DisplayAsStringExampleSO.Instance)
        };
    }
}
