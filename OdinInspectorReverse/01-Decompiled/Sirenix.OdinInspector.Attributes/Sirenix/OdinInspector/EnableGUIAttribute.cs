using System;
using System.Diagnostics;

namespace Sirenix.OdinInspector
{
	/// <summary>
	/// <para>An attribute that enables GUI.</para>
	/// </summary>
	/// <example>
	/// <code>
	/// public class InlineEditorExamples : MonoBehaviour
	/// {
	///     [EnableGUI]
	///     public string SomeReadonlyProperty { get { return "My GUI is usually disabled." } }
	/// }
	/// </code>
	/// </example>
	/// <seealso cref="T:Sirenix.OdinInspector.ReadOnlyAttribute" />
	[AttributeUsage(AttributeTargets.All)]
	[Conditional("UNITY_EDITOR")]
	public class EnableGUIAttribute : Attribute
	{
	}
}
