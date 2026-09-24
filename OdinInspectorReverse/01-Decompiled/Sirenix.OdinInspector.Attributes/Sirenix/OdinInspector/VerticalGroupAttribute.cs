using System;
using System.Diagnostics;

namespace Sirenix.OdinInspector
{
	/// <summary>
	/// <para>VerticalGroup is used to gather properties together in a vertical group in the inspector.</para>
	/// <para>This doesn't do much in and of itself, but in combination with other groups, such as <see cref="T:Sirenix.OdinInspector.HorizontalGroupAttribute" /> it can be very useful.</para>
	/// </summary>
	/// <example>
	/// <para>The following example demonstrates how VerticalGroup can be used in conjunction with <see cref="T:Sirenix.OdinInspector.HorizontalGroupAttribute" /></para>
	/// <code>
	/// public class MyComponent : MonoBehaviour
	/// {
	/// 	[HorizontalGroup("Split")]
	/// 	[VerticalGroup("Split/Left")]
	/// 	public Vector3 Vector;
	///
	/// 	[VerticalGroup("Split/Left")]
	/// 	public GameObject First;
	///
	/// 	[VerticalGroup("Split/Left")]
	/// 	public GameObject Second;
	///
	/// 	[VerticalGroup("Split/Right", PaddingTop = 18f)]
	/// 	public int A;
	///
	/// 	[VerticalGroup("Split/Right")]
	/// 	public int B;
	/// }
	/// </code>
	/// </example>
	/// <seealso cref="T:Sirenix.OdinInspector.HorizontalGroupAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.BoxGroupAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.TabGroupAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.ToggleGroupAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.ButtonGroupAttribute" />
	[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
	[Conditional("UNITY_EDITOR")]
	public class VerticalGroupAttribute : PropertyGroupAttribute
	{
		/// <summary>
		/// Space in pixels at the top of the group.
		/// </summary>
		public float PaddingTop;

		/// <summary>
		/// Space in pixels at the bottom of the group.
		/// </summary>
		public float PaddingBottom;

		/// <summary>
		/// Groups properties vertically.
		/// </summary>
		/// <param name="groupId">The group ID.</param>
		/// <param name="order">The group order.</param>
		public VerticalGroupAttribute(string groupId, float order = 0f)
			: base(groupId, order)
		{
		}

		/// <summary>
		/// <para>Groups properties vertically.</para>
		/// <para>GroupId: _DefaultVerticalGroup</para>
		/// </summary>
		/// <param name="order">The group order.</param>
		public VerticalGroupAttribute(float order = 0f)
			: this("_DefaultVerticalGroup", order)
		{
		}

		/// <summary>
		/// Combines properties that have been group vertically.
		/// </summary>
		/// <param name="other">The group attribute to combine with.</param>
		protected override void CombineValuesWith(PropertyGroupAttribute other)
		{
			if (other is VerticalGroupAttribute a)
			{
				if (a.PaddingTop != 0f)
				{
					PaddingTop = a.PaddingTop;
				}
				if (a.PaddingBottom != 0f)
				{
					PaddingBottom = a.PaddingBottom;
				}
			}
		}
	}
}
