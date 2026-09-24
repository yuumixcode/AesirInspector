using Sirenix.OdinInspector;
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
        [HideLabel]
        [Image(80f, DrawProperty = false)]
        public Texture2D texture;

        [Title("Sprite Value")]
        [HideLabel]
        [Image(64f, DrawProperty = false)]
        public Sprite sprite;

        [Title("Parameter: ImageSource")]
        [Image("banner", 72f)]
        public Texture2D decoratedTexture;

        [HideInInspector]
        public Texture2D banner;

        public override void AesirInspectorReset()
        {
            texture = null;
            sprite = null;
            decoratedTexture = null;
            banner = null;
        }
    }
}
