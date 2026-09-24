using System;
using System.Diagnostics;

namespace Sirenix.OdinInspector
{
	/// <summary>
	/// <para>LabelWidth is used to change the width of labels for properties.</para>
	/// </summary>
	/// <example>
	/// <para>The following example shows how LabelText is applied to a few property fields.</para>
	/// <code>
	/// public MyComponent : MonoBehaviour
	/// {
	/// 	[LabelWidth("3")]
	/// 	public int MyInt3;
	/// }
	/// </code>
	/// </example>
	/// <seealso cref="T:Sirenix.OdinInspector.TitleAttribute" />
	[DontApplyToListElements]
	[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
	[Conditional("UNITY_EDITOR")]
	public class LabelWidthAttribute : Attribute
	{
		/// <summary>
		/// The new text of the label.
		/// </summary>
		public float Width;

		/// <summary>
		/// Give a property a custom label.
		/// </summary>
		/// <param name="width">The width of the label.</param>
		public LabelWidthAttribute(float width)
		{
			Width = width;
		}
	}
}
