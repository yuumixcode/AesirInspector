using System;
using System.Diagnostics;

namespace Sirenix.OdinInspector
{
	/// <summary>
	/// Tells the validation system that this member should not be validated. It will not show validation messages in the inspector, and it will not be scanned by the project validator.
	/// </summary>
	[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
	[Conditional("UNITY_EDITOR")]
	public class DontValidateAttribute : Attribute
	{
	}
}
