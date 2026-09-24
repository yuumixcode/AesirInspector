using Sirenix.OdinInspector.Editor.Examples.Internal;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[ExampleAsComponentData(Namespaces = new string[] { "Sirenix.OdinInspector.Editor.Examples" })]
	[AttributeExample(typeof(ValidateInputAttribute), "ValidateInput is used to display error boxes in case of invalid values.\nIn this case the GameObject must have a MeshRenderer component.")]
	internal class ValidateInputExamples
	{
		[Title("Default message", "You can just provide a default message that is always used", TitleAlignments.Left, true, true)]
		[ValidateInput("MustBeNull", "This field should be null.", InfoMessageType.Error)]
		[HideLabel]
		public MyScriptyScriptableObject DefaultMessage;

		[ValidateInput("HasMeshRendererDynamicMessage", "Prefab must have a MeshRenderer component", InfoMessageType.Error)]
		[HideLabel]
		[Space(12f)]
		[Title("Dynamic message", "Or the validation method can dynamically provide a custom message", TitleAlignments.Left, true, true)]
		public GameObject DynamicMessage;

		[Title("Dynamic message type", "The validation method can also control the type of the message", TitleAlignments.Left, true, true)]
		[Space(12f)]
		[ValidateInput("HasMeshRendererDynamicMessageAndType", "Prefab must have a MeshRenderer component", InfoMessageType.Error)]
		[HideLabel]
		public GameObject DynamicMessageAndType;

		[InfoBox("Change GameObject value to update message type", InfoMessageType.None, null)]
		[HideLabel]
		[Space(8f)]
		public InfoMessageType MessageType;

		[HideLabel]
		[Space(12f)]
		[ValidateInput("AlwaysFalse", "$Message", InfoMessageType.Warning)]
		[Title("Dynamic default message", "Use $ to indicate a member string as default message", TitleAlignments.Left, true, true)]
		public string Message = "Dynamic ValidateInput message";

		private bool AlwaysFalse(string value)
		{
			return false;
		}

		private bool MustBeNull(MyScriptyScriptableObject scripty)
		{
			return scripty == null;
		}

		private bool HasMeshRendererDefaultMessage(GameObject gameObject)
		{
			if (gameObject == null)
			{
				return true;
			}
			return gameObject.GetComponentInChildren<MeshRenderer>() != null;
		}

		private bool HasMeshRendererDynamicMessage(GameObject gameObject, ref string errorMessage)
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

		private bool HasMeshRendererDynamicMessageAndType(GameObject gameObject, ref string errorMessage, ref InfoMessageType? messageType)
		{
			if (gameObject == null)
			{
				return true;
			}
			if (gameObject.GetComponentInChildren<MeshRenderer>() == null)
			{
				errorMessage = "\"" + gameObject.name + "\" should have a MeshRenderer component";
				messageType = MessageType;
				return false;
			}
			return true;
		}
	}
}
