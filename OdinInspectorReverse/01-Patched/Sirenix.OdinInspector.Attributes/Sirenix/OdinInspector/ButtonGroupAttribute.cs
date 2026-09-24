using System;
using System.Diagnostics;

namespace Sirenix.OdinInspector
{
	/// <summary>
	/// <para>ButtonGroup is used on any instance function, and adds buttons to the inspector organized into horizontal groups.</para>
	/// <para>Use this to organize multiple button in a tidy horizontal group.</para>
	/// </summary>
	/// <example>
	/// <para>The following example shows how ButtonGroup is used to organize two buttons into one group.</para>
	/// <code>
	///             	public class MyComponent : MonoBehaviour
	///             	{
	///             		[ButtonGroup("MyGroup")]
	///             		private void A()
	///             		{
	///             			// ..
	///             		}
	///
	///             		[ButtonGroup("MyGroup")]
	///             		private void B()
	///             		{
	///             			// ..
	///             		}
	///             	}
	/// </code>
	/// </example>
	/// <example>
	/// <para>The following example shows how ButtonGroup can be used to create multiple groups of buttons.</para>
	/// <code>
	///             	public class MyComponent : MonoBehaviour
	///             	{
	///             		[ButtonGroup("First")]
	///             		private void A()
	///             		{ }
	///
	///             		[ButtonGroup("First")]
	///             		private void B()
	///             		{ }
	///
	///             		[ButtonGroup("")]
	///             		private void One()
	///             		{ }
	///
	///             		[ButtonGroup("")]
	///             		private void Two()
	///             		{ }
	///
	///             		[ButtonGroup("")]
	///             		private void Three()
	///             		{ }
	///             	}
	/// </code>
	/// </example>
	/// <seealso cref="T:Sirenix.OdinInspector.ButtonAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.InlineButtonAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.BoxGroupAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.FoldoutGroupAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.HorizontalGroupAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.TabGroupAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.ToggleGroupAttribute" />
	[IncludeMyAttributes]
	[ShowInInspector]
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
	[Conditional("UNITY_EDITOR")]
	public class ButtonGroupAttribute : PropertyGroupAttribute
	{
		/// <summary>
		/// Gets the height of the button. If it's zero or below then use default.
		/// </summary>
		[ButtonHeightSelector]
		public int ButtonHeight;

		private IconAlignment buttonIconAlignment;

		private int buttonAlignment;

		private bool stretch;

		/// <summary>
		/// The alignment of the icon that is displayed inside the button.
		/// </summary>
		[ShowInInspector]
		[OdinDesignerBinding(new string[] { "buttonIconAlignment", "HasDefinedButtonIconAlignment" })]
		public IconAlignment IconAlignment
		{
			get
			{
				return buttonIconAlignment;
			}
			set
			{
				buttonIconAlignment = value;
				HasDefinedButtonIconAlignment = true;
			}
		}

		/// <summary>
		/// The alignment of the button represented by a range from 0 to 1 where 0 is the left edge of the available space and 1 is the right edge.
		/// </summary>
		[ShowInInspector]
		[OdinDesignerBinding(new string[] { "buttonAlignment", "HasDefinedButtonAlignment" })]
		public int ButtonAlignment
		{
			get
			{
				return buttonAlignment;
			}
			set
			{
				buttonAlignment = value;
				HasDefinedButtonAlignment = true;
			}
		}

		/// <summary>
		/// Whether the button should stretch to fill all of the available space. Default value is true.
		/// </summary>
		[ShowInInspector]
		[OdinDesignerBinding(new string[] { "stretch", "HasDefinedStretch" })]
		public bool Stretch
		{
			get
			{
				return stretch;
			}
			set
			{
				stretch = value;
				HasDefinedStretch = true;
			}
		}

		public bool HasDefinedButtonIconAlignment { get; private set; }

		public bool HasDefinedButtonAlignment { get; private set; }

		public bool HasDefinedStretch { get; private set; }

		/// <summary>
		/// Organizes the button into the specified button group.
		/// </summary>
		/// <param name="group">The group to organize the button into.</param>
		/// <param name="order">The order of the group in the inspector..</param>
		public ButtonGroupAttribute(string group = "_DefaultGroup", float order = 0f)
			: base(group, order)
		{
		}
	}
}
