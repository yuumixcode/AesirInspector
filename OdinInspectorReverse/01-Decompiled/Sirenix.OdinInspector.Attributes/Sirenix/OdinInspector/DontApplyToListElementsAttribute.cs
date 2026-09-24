using System;

namespace Sirenix.OdinInspector
{
	/// <summary>
	/// <para>DontApplyToListElements is used on other attributes, and indicates that those attributes should be applied only to the list, and not to the elements of the list.</para>
	/// <para>Use this on attributes that should only work on a list or array property as a whole, and not on each element of the list.</para>
	/// </summary>
	/// <example>
	/// <para>The following example shows how DontApplyToListElements is used on <see cref="T:Sirenix.OdinInspector.ShowIfAttribute" />.</para>
	/// <code>
	/// [DontApplyToListElements]
	/// [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
	/// public sealed class VisibleIfAttribute : Attribute
	/// {
	///     public string MemberName { get; private set; }
	///
	///     public VisibleIfAttribute(string memberName)
	///     {
	///         this.MemberName = memberName;
	///     }
	/// }
	/// </code>
	/// </example>
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class DontApplyToListElementsAttribute : Attribute
	{
	}
}
