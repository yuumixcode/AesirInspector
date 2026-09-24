using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(SuppressInvalidAttributeErrorAttribute))]
	internal class SuppressInvalidAttributeErrorExample
	{
		[Range(0f, 10f)]
		public string InvalidAttributeError = "This field will have an error box for the Range attribute on a string field.";

		[SuppressInvalidAttributeError]
		[Range(0f, 10f)]
		public string SuppressedError = "The error has been suppressed on this field, and thus no error box will appear.";
	}
}
