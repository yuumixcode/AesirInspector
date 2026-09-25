using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// SuppressInvalidAttributeError 特性的案例 SO。
    /// </summary>
    [AesirExample]
    public class
        SuppressInvalidAttributeErrorExampleSO : AttributeExampleSO<SuppressInvalidAttributeErrorExampleSO>
    {
        [Title("Without Suppression")]
        [Range(0f, 10f)]
        public string InvalidAttributeError =
            "This field will have an error box for the Range attribute on a string field.";

        [Title("With Suppression")]
        [SuppressInvalidAttributeError]
        [Range(0f, 10f)]
        public string SuppressedError =
            "The error has been suppressed on this field, and thus no error box will appear.";

        public override void AesirInspectorReset()
        {
            InvalidAttributeError =
                "This field will have an error box for the Range attribute on a string field.";
            SuppressedError =
                "The error has been suppressed on this field, and thus no error box will appear.";
        }
    }
}
