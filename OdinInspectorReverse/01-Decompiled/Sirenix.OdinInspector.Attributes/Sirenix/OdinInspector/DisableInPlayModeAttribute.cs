using System;
using System.Diagnostics;

namespace Sirenix.OdinInspector
{
	/// <summary>
	/// <para>DisableInPlayMode is used on any property, and disables the property when in play mode.</para>
	/// <para>Use this to prevent users from editing a property when in play mode.</para>
	/// </summary>
	/// <example>
	/// <para>The following example shows how DisableInPlayMode is used to disable a property when in play mode.</para>
	/// <code>
	/// public class MyComponent : MonoBehaviour
	/// {
	/// 	[DisableInPlayMode]
	/// 	public int MyInt;
	/// }
	/// </code>
	/// </example>
	/// <seealso cref="T:Sirenix.OdinInspector.HideInPlayModeAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.DisableInEditorModeAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.HideInEditorModeAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.EnableIfAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.DisableIfAttribute" />
	[AttributeUsage(AttributeTargets.All)]
	[DontApplyToListElements]
	[Conditional("UNITY_EDITOR")]
	public class DisableInPlayModeAttribute : Attribute
	{
	}
}
