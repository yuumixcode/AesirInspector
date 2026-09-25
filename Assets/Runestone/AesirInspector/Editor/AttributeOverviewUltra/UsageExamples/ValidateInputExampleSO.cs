using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// ValidateInput 特性的案例 SO。
    /// </summary>
    [AesirExample]
    internal class ValidateInputExampleSO : AttributeExampleSO<ValidateInputExampleSO>
    {
        [FoldoutGroup("Expression (@)")]
        [ValidateInput("@!string.IsNullOrEmpty($value)", "The string cannot be empty!")]
        public string notEmptyString = "Hello";

        [FoldoutGroup("Parameter: Condition, DefaultMessage")]
        [ValidateInput("ValidateGreaterThanZero", "Value must be greater than zero")]
        public int greaterThanZero = 10;

        [FoldoutGroup("Parameter: Condition, DefaultMessage, MessageType")]
        [ValidateInput("MustBeNull", "This field should be null.")]
        public ScriptableObject mustBeNullExample;

        [FoldoutGroup("Parameter: DefaultMessage ($)")]
        [ValidateInput("AlwaysFalse", "$message", InfoMessageType.Warning)]
        public string message = "Dynamic ValidateInput message";

        [FoldoutGroup("Parameter: Condition (ref string errorMessage)")]
        [ValidateInput("HasMeshRendererDynamicMessage", "Prefab must have a MeshRenderer component")]
        public GameObject dynamicMessage;

        [FoldoutGroup("Parameter: Condition (ref string errorMessage, ref InfoMessageType? messageType)")]
        [ValidateInput("HasMeshRendererDynamicMessageAndType", "Prefab must have a MeshRenderer component")]
        public GameObject dynamicMessageAndType;

        [FoldoutGroup("Parameter: Condition (ref string errorMessage, ref InfoMessageType? messageType)")]
        [ValidateInput("ValidateWithDynamicMessage")]
        public int dynamicMessageValue = 5;

        [FoldoutGroup("Parameter: Condition (ref string errorMessage, ref InfoMessageType? messageType)")]
        [InfoBox("Change GameObject value to update message type", InfoMessageType.None)]
        public InfoMessageType currentMessageType;

        bool ValidateGreaterThanZero(int value) => value > 0;

        bool ValidateWithDynamicMessage(int value, ref string errorMessage, ref InfoMessageType? messageType)
        {
            if (value < 0)
            {
                errorMessage = "Value is negative!";
                messageType = InfoMessageType.Error;
                return false;
            }

            if (value < 10)
            {
                errorMessage = "Value is small (below 10).";
                messageType = InfoMessageType.Warning;
                return false;
            }

            return true;
        }

        bool MustBeNull(ScriptableObject scripty) => scripty == null;

        bool AlwaysFalse(string value) => false;

        bool HasMeshRendererDynamicMessage(GameObject gameObject, ref string errorMessage)
        {
            if (gameObject == null)
            {
                return true;
            }

            if (gameObject.GetComponentInChildren<MeshRenderer>() == null)
            {
                errorMessage = "\"" + gameObject.name + "\" must have a MeshRenderer component";
                return false;
            }

            return true;
        }

        bool HasMeshRendererDynamicMessageAndType(GameObject gameObject,
            ref string errorMessage,
            ref InfoMessageType? messageType)
        {
            if (gameObject == null)
            {
                return true;
            }

            if (gameObject.GetComponentInChildren<MeshRenderer>() == null)
            {
                errorMessage = "\"" + gameObject.name + "\" should have a MeshRenderer component";
                messageType = currentMessageType;
                return false;
            }

            return true;
        }

        public override void AesirInspectorReset()
        {
            notEmptyString = "Hello";
            greaterThanZero = 10;
            mustBeNullExample = null;
            message = "Dynamic ValidateInput message";
            dynamicMessage = null;
            dynamicMessageAndType = null;
            dynamicMessageValue = 5;
            currentMessageType = InfoMessageType.None;
        }
    }
}
