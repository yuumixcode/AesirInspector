using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// OnValueChanged 特性的案例 SO。
    /// </summary>
    [AesirExample]
    public class OnValueChangedExampleSO : AttributeExampleSO<OnValueChangedExampleSO>
    {
        [Title("No Parameters")]
        [OnValueChanged("OnValueChange")]
        public int value;

        [Title("Parameter: IncludeChildren")]
        [OnValueChanged("CreateMaterial")]
        public Shader shader;

        [Title("Parameter: IncludeChildren")]
        [ReadOnly]
        [InlineEditor(InlineEditorModes.LargePreview)]
        public Material material;

        void OnValueChange()
        {
            Debug.Log("Value changed to: " + value);
        }

        void CreateMaterial()
        {
            if (material != null)
            {
                DestroyImmediate(material);
            }

            if (shader != null)
            {
                material = new Material(shader);
            }
        }

        public override void AesirInspectorReset()
        {
            value = 0;
            shader = null;
            if (material != null)
            {
                DestroyImmediate(material);
                material = null;
            }
        }
    }
}
