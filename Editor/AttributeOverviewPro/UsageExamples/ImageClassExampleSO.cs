using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor.Examples;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// Image 特性标注在类上的案例 SO。
    /// </summary>
    [AesirExample]
    [Image("typeBanner", 72f, IgnorePadding = true)]
    public class ImageClassExampleSO : AttributeExampleSO<ImageClassExampleSO>
    {
        [InfoBox("The banner above this message is drawn by the Image attribute on the class itself.")]
        [DisplayAsString]
        [HideLabel]
        public string classAttribute = "[Image(\"typeBanner\", 72f, IgnorePadding = true)]";

        [HideInInspector]
        public Texture2D typeBanner;

        [OnInspectorInit]
        void CreateData()
        {
            CleanupData();
            typeBanner = ExampleHelper.GetTexture(1200, 160, ExampleTextureTheme.Warm);
        }

        [OnInspectorDispose]
        void CleanupData()
        {
            typeBanner = null;
        }

        public override void AesirInspectorReset()
        {
            CreateData();
        }
    }
}
