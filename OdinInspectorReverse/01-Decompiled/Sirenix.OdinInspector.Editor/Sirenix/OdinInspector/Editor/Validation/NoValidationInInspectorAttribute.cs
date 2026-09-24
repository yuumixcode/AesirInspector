using System;

namespace Sirenix.OdinInspector.Editor.Validation
{
	/// <summary>
	/// Put this attribute on a validator class to prevent the ValidatorDrawer from running that validator in the inspector.
	/// Typically you would use this for a validation-related attribute that has its own, complex custom drawer that should
	/// handle the validation and error/warning drawing while the inspector is being drawn, but still needs a validator to 
	/// run for the project validation scans.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class NoValidationInInspectorAttribute : Attribute
	{
	}
}
