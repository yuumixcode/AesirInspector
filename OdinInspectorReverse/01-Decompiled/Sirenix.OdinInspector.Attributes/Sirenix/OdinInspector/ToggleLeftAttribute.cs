using System;
using System.Diagnostics;

namespace Sirenix.OdinInspector
{
	/// <summary>
	/// <para>Draws the checkbox before the label instead of after.</para>
	/// </summary>
	/// <remarks>ToggleLeftAttribute can be used an all fields and properties of type boolean</remarks>
	/// <example>
	/// <code>
	/// public class MyComponent : MonoBehaviour
	/// {
	/// 	[ToggleLeft]
	/// 	public bool MyBoolean;
	/// }
	/// </code>
	/// </example>
	[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
	[Conditional("UNITY_EDITOR")]
	public sealed class ToggleLeftAttribute : Attribute
	{
	}
}
