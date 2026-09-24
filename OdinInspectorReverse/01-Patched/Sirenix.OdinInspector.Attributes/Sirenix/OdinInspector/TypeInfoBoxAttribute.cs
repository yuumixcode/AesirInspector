using System;
using System.Diagnostics;

namespace Sirenix.OdinInspector
{
	/// <summary>
	/// <para>The TypeInfoBox attribute adds an info box to the very top of a type in the inspector.</para>
	/// <para>Use this to add an info box to the top of a class in the inspector, without having to use neither the PropertyOrder nor the OnInspectorGUI attribute.</para>
	/// </summary>
	/// <example>
	/// <para>The following example demonstrates the use of the TypeInfoBox attribute.</para>
	/// <code>
	/// [TypeInfoBox("This is my component and it is mine.")]
	/// public class MyComponent : MonoBehaviour
	/// {
	///     // Class implementation.
	/// }
	/// </code>
	/// </example>
	/// <seealso cref="T:Sirenix.OdinInspector.InfoBoxAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.DetailedInfoBoxAttribute" />
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, AllowMultiple = true, Inherited = true)]
	[Conditional("UNITY_EDITOR")]
	public class TypeInfoBoxAttribute : Attribute
	{
		/// <summary>
		/// The message to display in the info box.
		/// </summary>
		public string Message;

		/// <summary>
		/// Draws an info box at the top of a type in the inspector.
		/// </summary>
		/// <param name="message">The message to display in the info box.</param>
		public TypeInfoBoxAttribute(string message)
		{
			Message = message;
		}
	}
}
