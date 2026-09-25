using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    internal class PreviewFieldAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("PreviewField", "PreviewField",
                "PreviewField 特性绘制一个正方形的 Preview 预览框，代替原有的 ObjectField，默认支持拖拽。",
                "The PreviewField attribute draws a square preview box instead of the default ObjectField, with drag-and-drop support.",
                "https://odininspector.com/attributes/preview-field-attribute");

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("用正方形预览框代替默认的 ObjectField，仅适用于 UnityEngine.Object 类型；可在 Odin 偏好设置中全局启用与定制。",
                "Draws a square preview box instead of the default ObjectField and only applies to UnityEngine.Object types; it can be enabled and customized globally in the Odin preferences."),
            new BilingualData("内置拖拽操作：拖拽 = 移动/交换，Ctrl + 拖拽并放下 = 覆盖，Ctrl + 点击 = 删除实例。",
                "Built-in drag-and-drop: drag = move or swap, Ctrl + drag and drop = replace, Ctrl + click = delete the instance."),
            new BilingualData("height 默认 0，表示沿用 Odin 的全局设置；FilterMode 默认 Bilinear；previewGetter 可让字段显示另一个对象的预览。",
                "height defaults to 0, which falls back to the global Odin setting; FilterMode defaults to Bilinear; previewGetter lets the field preview another object instead.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(string).FullName, "previewGetter",
                new BilingualData("可以渲染一个 Object 的 Preview 预览框，主要是用于渲染 Texture。",
                    "A getter that renders an Object preview, primarily used for rendering Textures.")),
            new ParameterValue(typeof(float).FullName, "height",
                new BilingualData("渲染框的高度。", "The height of the preview box.")),
            new ParameterValue(typeof(ObjectFieldAlignment).FullName, "alignment",
                new BilingualData("对齐样式。", "The alignment style.")),
            new ParameterValue(typeof(FilterMode).FullName, "filterMode",
                new BilingualData("纹理的过滤模式，有 Point、Bilinear、Trilinear。",
                    "The texture filter mode: Point, Bilinear, or Trilinear."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = { };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                PreviewFieldExampleSO.Instance)
        };
    }
}
