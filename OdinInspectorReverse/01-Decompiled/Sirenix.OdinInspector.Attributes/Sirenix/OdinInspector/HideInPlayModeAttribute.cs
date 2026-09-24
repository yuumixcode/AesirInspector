using System;
using System.Diagnostics;

namespace Sirenix.OdinInspector
{
	/// <summary>
	/// <para>HideInPlayMode is used on any property, and hides the property when not in editor mode.</para>
	/// <para>Use this when you only want a property to only be visible the editor.</para>
	/// </summary>
	/// <example>
	/// <para>The following example shows how HideInPlayMode is used to hide a property when in play mode.</para>
	/// <code>
	/// public class MyComponent : MonoBehaviour
	/// {
	/// 	[HideInPlayMode]
	/// 	public int MyInt;
	/// }
	/// </code>
	/// </example>
	/// <seealso cref="T:Sirenix.OdinInspector.HideInEditorModeAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.DisableInPlayModeAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.EnableIfAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.DisableIfAttribute" />
	[AttributeUsage(AttributeTargets.All)]
	[DontApplyToListElements]
	[Conditional("UNITY_EDITOR")]
	public class HideInPlayModeAttribute : Attribute
	{
	}
}
