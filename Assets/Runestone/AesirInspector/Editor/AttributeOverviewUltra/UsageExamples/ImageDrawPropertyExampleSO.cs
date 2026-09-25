using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor.Examples;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// Image 特性 DrawProperty 与 DrawPosition 参数的案例 SO。
    /// </summary>
    [AesirExample]
    public class ImageDrawPropertyExampleSO : AttributeExampleSO<ImageDrawPropertyExampleSO>
    {
        [Title("Parameter: DrawProperty")]
        [ToggleLeft]
        public bool drawProperty = true;

        [ShowIf("drawProperty")]
        [InfoBox("[Image(64)] draws the image first, then the Texture2D field.")]
        [Image(64f)]
        public Texture2D drawPropertyOn;

        [HideIf("drawProperty")]
        [InfoBox("[Image(64, DrawProperty = false)] draws only the image.")]
        [Image(64f, DrawProperty = false)]
        public Texture2D drawPropertyOff;

        [Title("Parameter: DrawPosition")]
        [InfoBox("DrawPosition = AfterProperty draws the Texture2D field first, then the image.")]
        [Image(64f, DrawPosition = ImageDrawPosition.AfterProperty)]
        public Texture2D drawAfterProperty;

        [OnInspectorInit]
        void CreateData()
        {
            CleanupData();
            drawPropertyOn = ExampleHelper.GetTexture(960, 180, ExampleTextureTheme.Purple);
            drawPropertyOff = drawPropertyOn;
            drawAfterProperty = ExampleHelper.GetTexture(960, 180, ExampleTextureTheme.Green);
        }

        [OnInspectorDispose]
        void CleanupData()
        {
            drawPropertyOn = null;
            drawPropertyOff = null;
            drawAfterProperty = null;
        }

        public override void AesirInspectorReset()
        {
            drawProperty = true;
            CreateData();
        }
    }
}
