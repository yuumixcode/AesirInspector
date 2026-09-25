using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor.Examples;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// Image 特性渲染参数的案例 SO。
    /// </summary>
    [AesirExample]
    public class ImageRenderingExampleSO : AttributeExampleSO<ImageRenderingExampleSO>
    {
        [Title("Parameter: ScaleMode")]
        [ToggleLeft]
        public bool useScaleAndCrop;

        [HideIf("useScaleAndCrop")]
        [InfoBox("ScaleToFit is the default. The whole image stays visible.")]
        [Image(170f, 80f, DrawProperty = false)]
        [HideLabel]
        public Texture2D scaleToFit;

        [ShowIf("useScaleAndCrop")]
        [InfoBox("ScaleAndCrop fills the entire rect and crops what does not fit.")]
        [Image(170f, 80f, ImageScaleMode.ScaleAndCrop, DrawProperty = false)]
        [HideLabel]
        public Texture2D scaleAndCrop;

        [Title("Parameter: FilterMode")]
        [ToggleLeft]
        public bool usePointFiltering = true;

        [HideIf("usePointFiltering")]
        [InfoBox("Bilinear filtering smooths the texture when it is scaled.")]
        [Image(160f, 80f, FilterMode = FilterMode.Bilinear, DrawProperty = false)]
        [HideLabel]
        public Texture2D bilinear;

        [ShowIf("usePointFiltering")]
        [InfoBox("Point filtering keeps each pixel sharp.")]
        [Image(160f, 80f, FilterMode = FilterMode.Point, DrawProperty = false)]
        [HideLabel]
        public Texture2D point;

        [Title("Parameter: AlphaBlend")]
        [ToggleLeft]
        public bool alphaBlend = true;

        [ShowIf("alphaBlend")]
        [InfoBox(
            "AlphaBlend is enabled. Transparent parts of the image blend with the inspector background.")]
        [Image(120f, AlphaBlend = true, DrawProperty = false)]
        [HideLabel]
        public Texture2D alphaBlendOn;

        [HideIf("alphaBlend")]
        [InfoBox("AlphaBlend is disabled. Transparent pixels are treated as opaque.")]
        [Image(120f, AlphaBlend = false, DrawProperty = false)]
        [HideLabel]
        public Texture2D alphaBlendOff;

        [OnInspectorInit]
        void CreateData()
        {
            CleanupData();
            scaleToFit = ExampleHelper.GetTexture(160, 260, ExampleTextureTheme.Blue);
            scaleAndCrop = scaleToFit;
            bilinear = ExampleHelper.GetCheckerTexture(64, 32);
            point = bilinear;
            alphaBlendOn = ExampleHelper.GetTransparentTexture(320, 120);
            alphaBlendOff = alphaBlendOn;
        }

        [OnInspectorDispose]
        void CleanupData()
        {
            scaleToFit = null;
            scaleAndCrop = null;
            bilinear = null;
            point = null;
            alphaBlendOn = null;
            alphaBlendOff = null;
        }

        public override void AesirInspectorReset()
        {
            useScaleAndCrop = false;
            usePointFiltering = true;
            alphaBlend = true;
            CreateData();
        }
    }
}
