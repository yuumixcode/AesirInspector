using System;
using System.Diagnostics;

namespace Sirenix.OdinInspector
{
	/// <summary>
	/// Hides a property if it is drawn within an <see cref="T:Sirenix.OdinInspector.InlineEditorAttribute" />.
	/// </summary>
	[DontApplyToListElements]
	[AttributeUsage(AttributeTargets.All)]
	[Conditional("UNITY_EDITOR")]
	public class HideInInlineEditorsAttribute : Attribute
	{
	}
}
