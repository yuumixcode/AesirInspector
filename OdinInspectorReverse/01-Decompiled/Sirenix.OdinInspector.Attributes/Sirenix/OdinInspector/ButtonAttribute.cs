using System;
using System.Diagnostics;

namespace Sirenix.OdinInspector
{
	/// <summary>
	/// <para>Buttons are used on functions, and allows for clickable buttons in the inspector.</para>
	/// </summary>
	/// <example>
	/// <para>The following example shows a component that has an initialize method, that can be called from the inspector.</para>
	/// <code>
	/// public class MyComponent : MonoBehaviour
	/// {
	/// 	[Button]
	/// 	private void Init()
	/// 	{
	/// 		// ...
	/// 	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// <para>The following example show how a Button could be used to test a function.</para>
	/// <code>
	/// public class MyBot : MonoBehaviour
	/// {
	/// 	[Button]
	/// 	private void Jump()
	/// 	{
	/// 		// ...
	/// 	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// <para>The following example show how a Button can named differently than the function it's been attached to.</para>
	/// <code>
	/// public class MyComponent : MonoBehaviour
	/// {
	/// 	[Button("Function")]
	/// 	private void MyFunction()
	/// 	{
	/// 		// ...
	/// 	}
	/// }
	/// </code>
	/// </example>
	/// <seealso cref="T:Sirenix.OdinInspector.InlineButtonAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.ButtonGroupAttribute" />
	[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = false)]
	[Conditional("UNITY_EDITOR")]
	public class ButtonAttribute : ShowInInspectorAttribute
	{
		/// <summary>
		/// Use this to override the label on the button.
		/// </summary>
		[PropertyOrder(-10f)]
		public string Name;

		/// <summary>
		/// The style in which to draw the button.
		/// </summary>
		[PropertyOrder(-9f)]
		public ButtonStyle Style;

		/// <summary>
		/// If the button contains parameters, you can disable the foldout it creates by setting this to true.
		/// </summary>
		public bool Expanded;

		/// <summary>
		/// <para>Whether to display the button method's parameters (if any) as values in the inspector. True by default.</para>
		/// <para>If this is set to false, the button method will instead be invoked through an ActionResolver or ValueResolver (based on whether it returns a value), giving access to contextual named parameter values like "InspectorProperty property" that can be passed to the button method.</para>
		/// </summary>
		public bool DisplayParameters = true;

		/// <summary>
		/// Whether the containing object or scene (if there is one) should be marked dirty when the button is clicked. True by default. Note that if this is false, undo for any changes caused by the button click is also disabled, as registering undo events also causes dirtying.
		/// </summary>
		public bool DirtyOnClick = true;

		/// <summary>
		/// The icon to be displayed inside the button.
		/// </summary>
		[PropertyOrder(-8f)]
		public SdfIconType Icon;

		private int buttonHeight;

		private bool drawResult;

		private bool drawResultIsSet;

		private bool stretch;

		private IconAlignment buttonIconAlignment;

		private float buttonAlignment;

		/// <summary>
		/// Gets the height of the button. If it's zero or below then use default.
		/// </summary>
		[PropertyOrder(-6f)]
		[ShowInInspector]
		[ButtonHeightSelector]
		[OdinDesignerBinding(new string[] { "buttonHeight", "HasDefinedButtonHeight" })]
		public int ButtonHeight
		{
			get
			{
				return buttonHeight;
			}
			set
			{
				buttonHeight = value;
				HasDefinedButtonHeight = true;
			}
		}

		/// <summary>
		/// The alignment of the icon that is displayed inside the button.
		/// </summary>
		[PropertyOrder(-7f)]
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
		/// ButtonAlignment only has an effect when Stretch is set to false.
		/// </summary>
		[PropertyOrder(-5f)]
		[ShowInInspector]
		[OdinDesignerBinding(new string[] { "buttonAlignment", "HasDefinedButtonAlignment" })]
		public float ButtonAlignment
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
		[PropertyOrder(-4f)]
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

		/// <summary>
		/// If the button has a return type, set this to false to not draw the result. Default value is true.
		/// </summary>
		[ShowInInspector]
		[OdinDesignerBinding(new string[] { "drawResult", "drawResultIsSet" })]
		public bool DrawResult
		{
			get
			{
				return drawResult;
			}
			set
			{
				drawResult = value;
				drawResultIsSet = true;
			}
		}

		public bool DrawResultIsSet => drawResultIsSet;

		public bool HasDefinedButtonHeight { get; private set; }

		public bool HasDefinedIcon => Icon != SdfIconType.None;

		public bool HasDefinedButtonIconAlignment { get; private set; }

		public bool HasDefinedButtonAlignment { get; private set; }

		public bool HasDefinedStretch { get; private set; }

		/// <summary>
		/// Creates a button in the inspector named after the method.
		/// </summary>
		public ButtonAttribute()
		{
			Name = null;
		}

		/// <summary>
		/// Creates a button in the inspector named after the method.
		/// </summary>
		/// <param name="size">The size of the button.</param>
		public ButtonAttribute(ButtonSizes size)
		{
			Name = null;
			ButtonHeight = (int)size;
		}

		/// <summary>
		/// Creates a button in the inspector named after the method.
		/// </summary>
		/// <param name="buttonSize">The size of the button.</param>
		public ButtonAttribute(int buttonSize)
		{
			ButtonHeight = buttonSize;
			Name = null;
		}

		/// <summary>
		/// Creates a button in the inspector with a custom name.
		/// </summary>
		/// <param name="name">Custom name for the button.</param>
		public ButtonAttribute(string name)
		{
			Name = name;
		}

		/// <summary>
		/// Creates a button in the inspector with a custom name.
		/// </summary>
		/// <param name="name">Custom name for the button.</param>
		/// <param name="buttonSize">Size of the button.</param>
		public ButtonAttribute(string name, ButtonSizes buttonSize)
		{
			Name = name;
			ButtonHeight = (int)buttonSize;
		}

		/// <summary>
		/// Creates a button in the inspector with a custom name.
		/// </summary>
		/// <param name="name">Custom name for the button.</param>
		/// <param name="buttonSize">Size of the button in pixels.</param>
		public ButtonAttribute(string name, int buttonSize)
		{
			Name = name;
			ButtonHeight = buttonSize;
		}

		/// <summary>
		/// Creates a button in the inspector named after the method.
		/// </summary>
		/// <param name="parameterBtnStyle">Button style for methods with parameters.</param>
		public ButtonAttribute(ButtonStyle parameterBtnStyle)
		{
			Name = null;
			Style = parameterBtnStyle;
		}

		/// <summary>
		/// Creates a button in the inspector named after the method.
		/// </summary>
		/// <param name="buttonSize">The size of the button.</param>
		/// <param name="parameterBtnStyle">Button style for methods with parameters.</param>
		public ButtonAttribute(int buttonSize, ButtonStyle parameterBtnStyle)
		{
			ButtonHeight = buttonSize;
			Name = null;
			Style = parameterBtnStyle;
		}

		/// <summary>
		/// Creates a button in the inspector named after the method.
		/// </summary>
		/// <param name="size">The size of the button.</param>
		/// <param name="parameterBtnStyle">Button style for methods with parameters.</param>
		public ButtonAttribute(ButtonSizes size, ButtonStyle parameterBtnStyle)
		{
			ButtonHeight = (int)size;
			Name = null;
			Style = parameterBtnStyle;
		}

		/// <summary>
		/// Creates a button in the inspector with a custom name.
		/// </summary>
		/// <param name="name">Custom name for the button.</param>
		/// <param name="parameterBtnStyle">Button style for methods with parameters.</param>
		public ButtonAttribute(string name, ButtonStyle parameterBtnStyle)
		{
			Name = name;
			Style = parameterBtnStyle;
		}

		/// <summary>
		/// Creates a button in the inspector with a custom name.
		/// </summary>
		/// <param name="name">Custom name for the button.</param>
		/// <param name="buttonSize">Size of the button.</param>
		/// <param name="parameterBtnStyle">Button style for methods with parameters.</param>
		public ButtonAttribute(string name, ButtonSizes buttonSize, ButtonStyle parameterBtnStyle)
		{
			Name = name;
			ButtonHeight = (int)buttonSize;
			Style = parameterBtnStyle;
		}

		/// <summary>
		/// Creates a button in the inspector with a custom name.
		/// </summary>
		/// <param name="name">Custom name for the button.</param>
		/// <param name="buttonSize">Size of the button in pixels.</param>
		/// <param name="parameterBtnStyle">Button style for methods with parameters.</param>
		public ButtonAttribute(string name, int buttonSize, ButtonStyle parameterBtnStyle)
		{
			Name = name;
			ButtonHeight = buttonSize;
			Style = parameterBtnStyle;
		}

		/// <summary>
		/// Creates a button in the inspector with a custom icon.
		/// </summary>
		/// <param name="icon">The icon to be displayed inside the button.</param>
		/// <param name="iconAlignment">The alignment of the icon that is displayed inside the button.</param>
		public ButtonAttribute(SdfIconType icon, IconAlignment iconAlignment)
		{
			Icon = icon;
			IconAlignment = iconAlignment;
			Name = null;
		}

		/// <summary>
		/// Creates a button in the inspector with a custom icon.
		/// </summary>
		/// <param name="icon">The icon to be displayed inside the button.</param>
		public ButtonAttribute(SdfIconType icon)
		{
			Icon = icon;
			Name = null;
		}

		/// <summary>
		/// Creates a button in the inspector with a custom icon.
		/// </summary>
		/// <param name="icon">The icon to be displayed inside the button.</param>
		/// <param name="name">Custom name for the button.</param>
		public ButtonAttribute(SdfIconType icon, string name)
		{
			Name = name;
			Icon = icon;
		}
	}
}
