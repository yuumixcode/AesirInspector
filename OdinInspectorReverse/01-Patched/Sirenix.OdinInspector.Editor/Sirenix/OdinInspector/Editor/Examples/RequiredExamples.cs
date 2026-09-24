using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(RequiredAttribute), "Required displays an error when objects are missing.")]
	internal class RequiredExamples
	{
		[Required]
		public GameObject MyGameObject;

		[Required("Custom error message.")]
		public Rigidbody MyRigidbody;

		[InfoBox("Use $ to indicate a member string as message.", InfoMessageType.Info, null)]
		[Required("$DynamicMessage")]
		public GameObject GameObject;

		public string DynamicMessage = "Dynamic error message";
	}
}
