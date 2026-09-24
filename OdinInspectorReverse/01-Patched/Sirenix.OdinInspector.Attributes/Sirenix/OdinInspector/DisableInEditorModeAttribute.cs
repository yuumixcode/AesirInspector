using System;
using System.Diagnostics;

namespace Sirenix.OdinInspector
{
	/// <summary>
	/// <para>DisableInEditorMode is used on any property, and disables the property when not in play mode.</para>
	/// <para>Use this when you only want a property to be editable when in play mode.</para>
	/// </summary>
	/// <example>
	/// <para>The following example shows how DisableInEditorMode is used to disable a property when in the editor.</para>
	/// <code>
	/// public class MyComponent : MonoBehaviour
	/// {
	/// 	[DisableInEditorMode]
	/// 	public int MyInt;
	/// }
	/// </code>
	/// </example>
	/// <seealso cref="T:Sirenix.OdinInspector.DisableInPlayModeAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.EnableIfAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.DisableIfAttribute" />
	[DontApplyToListElements]
	[AttributeUsage(AttributeTargets.All)]
	[Conditional("UNITY_EDITOR")]
	public class DisableInEditorModeAttribute : Attribute
	{
	}
}
