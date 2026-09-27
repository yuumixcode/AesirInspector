using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor.Examples;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// Image 特性布局参数的案例 SO。
    /// </summary>
    [AesirExample]
    public class ImageLayoutExampleSO : AttributeExampleSO<ImageLayoutExampleSO>
    {
        [Title("Natural Size")]
        [InfoBox("No width or height is specified here. The preview uses the texture's natural size.")]
        [Image(DrawProperty = false)]
        [HideLabel]
        public Texture2D naturalSize;

        [Title("Parameter: FitToAvailableWidth")]
        [InfoBox(
            "FitToAvailableWidth scales the image to the normal layout width while preserving aspect ratio.")]
        [Image(FitToAvailableWidth = true, DrawProperty = false)]
        [HideLabel]
        public Texture2D availableWidth;

        [Title("Parameter: Height")]
        [InfoBox("Height is set to 64. The preview uses the Texture2D field it is placed on.")]
        [Image(64f, DrawProperty = false)]
        [HideLabel]
        public Texture2D heightPreview;

        [Title("Parameter: Width, Height")]
        [InfoBox("Width is set to 260 and Height is set to 64.")]
        [Image(260f, 64f, DrawProperty = false)]
        [HideLabel]
        public Texture2D widthAndHeightPreview;

        [Title("Parameter: Alignment")]
        [InfoBox("Alignment = 0 places the preview on the left.")]
        [Image(220f, 56f, Alignment = 0f, DrawProperty = false)]
        [HideLabel]
        public Texture2D alignLeft;

        [InfoBox("Alignment = 0.5 centers the preview.")]
        [Image(220f, 56f, Alignment = 0.5f, DrawProperty = false)]
        [HideLabel]
        public Texture2D alignCenter;

        [InfoBox("Alignment = 1 places the preview on the right.")]
        [Image(220f, 56f, Alignment = 1f, DrawProperty = false)]
        [HideLabel]
        public Texture2D alignRight;

        [Title("Parameter: IgnorePadding")]
        [ToggleLeft]
        public bool useIgnorePadding = true;

        [ShowIf("useIgnorePadding")]
        [InfoBox(
            "IgnorePadding is enabled. ScaleAndCrop fills the preview rect so the edge-to-edge width is visible.")]
        [Image(72f, ImageScaleMode.ScaleAndCrop, IgnorePadding = true, DrawProperty = false)]
        [HideLabel]
        public Texture2D ignorePaddingOn;

        [HideIf("useIgnorePadding")]
        [InfoBox(
            "IgnorePadding is disabled. The same filled banner stays inside the normal inspector padding.")]
        [Image(72f, ImageScaleMode.ScaleAndCrop, DrawProperty = false)]
        [HideLabel]
        public Texture2D ignorePaddingOff;

        [OnInspectorInit]
        void CreateData()
        {
            CleanupData();
            naturalSize = ExampleHelper.GetTexture(260, 80, ExampleTextureTheme.Blue);
            availableWidth = ExampleHelper.GetTexture(1200, 180, ExampleTextureTheme.Green);
            heightPreview = ExampleHelper.GetTexture(1200, 180, ExampleTextureTheme.Blue);
            widthAndHeightPreview = ExampleHelper.GetTexture(520, 128, ExampleTextureTheme.Blue);
            alignLeft = ExampleHelper.GetTexture(520, 128, ExampleTextureTheme.Purple);
            alignCenter = alignLeft;
            alignRight = alignLeft;
            ignorePaddingOn = ExampleHelper.GetTexture(1200, 160, ExampleTextureTheme.Green);
            ignorePaddingOff = ignorePaddingOn;
        }

        [OnInspectorDispose]
        void CleanupData()
        {
            naturalSize = null;
            availableWidth = null;
            heightPreview = null;
            widthAndHeightPreview = null;
            alignLeft = null;
            alignCenter = null;
            alignRight = null;
            ignorePaddingOn = null;
            ignorePaddingOff = null;
        }

        public override void AesirInspectorReset()
        {
            useIgnorePadding = true;
            CreateData();
        }
    }
}
