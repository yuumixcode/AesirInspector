using System;
using System.Diagnostics;

namespace Sirenix.OdinInspector
{
	/// <summary>
	/// <para>HideInEditorMode is used on any property, and hides the property when not in play mode.</para>
	/// <para>Use this when you only want a property to only be visible play mode.</para>
	/// </summary>
	/// <example>
	/// <para>The following example shows how HideInEditorMode is used to hide a property when in the editor.</para>
	/// <code>
	/// public class MyComponent : MonoBehaviour
	/// {
	/// 	[HideInEditorMode]
	/// 	public int MyInt;
	/// }
	/// </code>
	/// </example>
	/// <seealso cref="T:Sirenix.OdinInspector.HideInPlayModeAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.DisableInPlayModeAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.EnableIfAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.DisableIfAttribute" />
	[DontApplyToListElements]
	[AttributeUsage(AttributeTargets.All)]
	[Conditional("UNITY_EDITOR")]
	public class HideInEditorModeAttribute : Attribute
	{
	}
}
