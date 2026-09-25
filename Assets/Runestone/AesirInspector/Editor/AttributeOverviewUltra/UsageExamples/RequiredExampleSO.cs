using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// Required 特性的案例 SO。
    /// </summary>
    [AesirExample]
    public class RequiredExampleSO : AttributeExampleSO<RequiredExampleSO>
    {
        [Title("No Parameters")]
        [Required]
        public GameObject defaultRequired;

        [Title("Parameter: ErrorMessage")]
        [Required("Custom error message.")]
        public Rigidbody myRigidbody;

        [Title("Parameter: ErrorMessage, MessageType")]
        [Required("This is an info message.", InfoMessageType.Info)]
        public Rigidbody infoRequired;

        [Title("Parameter: ErrorMessage, MessageType")]
        [Required("This is a warning message.", InfoMessageType.Warning)]
        public ScriptableObject warningRequired;

        [Title("Parameter: ErrorMessage, MessageType")]
        [Required("This is an error message.", InfoMessageType.Error)]
        public GameObject errorRequired;

        public override void AesirInspectorReset()
        {
            defaultRequired = null;
            myRigidbody = null;
            infoRequired = null;
            warningRequired = null;
            errorRequired = null;
        }
    }
}
