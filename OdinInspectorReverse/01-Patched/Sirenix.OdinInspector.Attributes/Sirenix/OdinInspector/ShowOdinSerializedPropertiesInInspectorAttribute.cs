using System;
using System.Diagnostics;

namespace Sirenix.OdinInspector
{
	/// <summary>
	/// Marks a type as being specially serialized. Odin uses this attribute to check whether it should include non-Unity-serialized members in the inspector.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	[Conditional("UNITY_EDITOR")]
	public class ShowOdinSerializedPropertiesInInspectorAttribute : Attribute
	{
	}
}
