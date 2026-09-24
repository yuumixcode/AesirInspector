using System;
using System.Diagnostics;

namespace Sirenix.OdinInspector
{
	/// <summary>
	/// Delays applying changes to properties while they still being edited in the inspector.
	/// Similar to Unity's built-in Delayed attribute, but this attribute can also be applied to properties.
	/// </summary>
	[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
	[Conditional("UNITY_EDITOR")]
	public class DelayedPropertyAttribute : Attribute
	{
	}
}
