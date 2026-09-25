using System;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// Image 特性的介绍数据。
    /// </summary>
    internal class ImageAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("Image", "Image",
                "Image 特性在 Inspector 中直接绘制图片，可绘制所标注的值本身，也可通过 ImageSource 指向另一个成员或资源路径。",
                "The Image attribute draws an image directly in the inspector; it can draw the decorated value or another member/asset path via ImageSource.",
                "https://odininspector.com/attributes/image-attribute");

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("在 Inspector 中直接绘制图片，适合横幅、分区标题等内联预览，而非可编辑的对象字段。",
                "Draws an image directly in the inspector, ideal for banners, section headers and other inline previews rather than editable object fields."),
            new BilingualData("不指定 ImageSource 时绘制所标注的 Texture2D / Sprite 值。",
                "Without ImageSource, the decorated Texture2D / Sprite value is drawn."),
            new BilingualData("ImageSource 支持成员名、@ 表达式或资源路径，也可标注在类上绘制固定图片。",
                "ImageSource accepts a member name, an @ expression or an asset path, and the attribute can also be placed on a class to draw a fixed image."),
            new BilingualData("DrawProperty 默认为 true，会同时绘制原属性字段；设为 false 时只显示图片。",
                "DrawProperty defaults to true and draws the original field as well; set it to false to show only the image.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(string).FullName, "imageSource",
                new BilingualData("图片来源：成员名、@ 表达式或资源路径，为空时绘制所标注的值。",
                    "The image source: a member name, @ expression or asset path; empty draws the decorated value.")),
            new ParameterValue(typeof(float).FullName, "width",
                new BilingualData("图片宽度（像素），仅指定高度时按比例缩放。",
                    "The image width in pixels; when only height is given the image scales proportionally.")),
            new ParameterValue(typeof(float).FullName, "height",
                new BilingualData("图片高度（像素）。", "The image height in pixels.")),
            new ParameterValue("ImageScaleMode", "scaleMode",
                new BilingualData("缩放模式（StretchToFit, ScaleToFit 等）。",
                    "The scale mode (StretchToFit, ScaleToFit, ...).")),
            new ParameterValue(typeof(bool).FullName, "DrawProperty",
                new BilingualData("是否同时绘制原属性字段，默认 true。",
                    "Whether to draw the original field as well. Defaults to true."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } =
            Array.Empty<ResolvedStringParameterValue>();

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Image",
                ImageExampleSO.Instance)
        };
    }
}
