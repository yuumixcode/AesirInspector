using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor.Examples;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// Image 特性的案例 SO。
    /// </summary>
    [AesirExample]
    public class ImageExampleSO : AttributeExampleSO<ImageExampleSO>
    {
        [Title("No Image Source")]
        [InfoBox(
            "Texture2D value: this preview comes from the Texture2D field that has the Image attribute.")]
        [Image(80f, DrawProperty = false)]
        [HideLabel]
        public Texture2D texture;

        [Title("Sprite Value")]
        [InfoBox("Sprite value: this preview comes from the Sprite field that has the Image attribute.")]
        [Image(64f, DrawProperty = false)]
        [HideLabel]
        public Sprite sprite;

        [Title("Parameter: ImageSource")]
        [InfoBox("This preview comes from memberBanner, not from the decorated Texture2D field.")]
        [Image("memberBanner", 72f)]
        public Texture2D decoratedTexture;

        [HideInInspector]
        public Texture2D memberBanner;

        [OnInspectorInit]
        void CreateData()
        {
            CleanupData();
            texture = ExampleHelper.GetTexture(260, 150, ExampleTextureTheme.Blue);
            var spriteTexture = ExampleHelper.GetTexture(128, 128, ExampleTextureTheme.Purple);
            memberBanner = ExampleHelper.GetTexture(960, 180, ExampleTextureTheme.Warm);
            decoratedTexture = ExampleHelper.GetTexture(128, 128, ExampleTextureTheme.Green);
            sprite = Sprite.Create(spriteTexture, new Rect(0f, 0f, spriteTexture.width, spriteTexture.height),
                new Vector2(0.5f, 0.5f));
        }

        [OnInspectorDispose]
        void CleanupData()
        {
            if (sprite != null)
            {
                DestroyImmediate(sprite);
                sprite = null;
            }

            texture = null;
            memberBanner = null;
            decoratedTexture = null;
        }

        public override void AesirInspectorReset()
        {
            CreateData();
        }
    }
}
