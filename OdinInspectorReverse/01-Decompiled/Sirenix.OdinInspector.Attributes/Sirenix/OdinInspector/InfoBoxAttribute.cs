using System;
using System.Diagnostics;

namespace Sirenix.OdinInspector
{
	/// <summary>
	/// <para>InfoBox is used on any property, and display a text box above the property in the inspector.</para>
	/// <para>Use this to add comments or warn about the use of different properties.</para>
	/// </summary>
	/// <example>
	/// <para>The following example shows different info box types.</para>
	/// <code>
	///             	public class MyComponent : MonoBehaviour
	///             	{
	///             		[InfoBox("This is an int property")]
	///             		public int MyInt;
	///
	///             		[InfoBox("This info box is a warning", InfoMessageType.Warning)]
	///             		public float MyFloat;
	///
	///             		[InfoBox("This info box is an error", InfoMessageType.Error)]
	///             		public object MyObject;
	///
	/// 	[InfoBox("This info box is just a box", InfoMessageType.None)]
	///             		public Vector3 MyVector;
	///             	}
	/// </code>
	/// </example>
	/// <example>
	/// <para>The following example how info boxes can be hidden by fields and properties.</para>
	/// <code>
	/// public class MyComponent : MonoBehaviour
	/// {
	///             		[InfoBox("This info box is hidden by an instance field.", "InstanceShowInfoBoxField")]
	///             		public int MyInt;
	///             		public bool InstanceShowInfoBoxField;
	///
	///             		[InfoBox("This info box is hideable by a static field.", "StaticShowInfoBoxField")]
	///             		public float MyFloat;
	///             		public static bool StaticShowInfoBoxField;
	///
	///             		[InfoBox("This info box is hidden by an instance property.", "InstanceShowInfoBoxProperty")]
	///             		public int MyOtherInt;
	/// 	public bool InstanceShowInfoBoxProperty { get; set; }
	///
	///             		[InfoBox("This info box is hideable by a static property.", "StaticShowInfoBoxProperty")]
	///             		public float MyOtherFloat;
	///             		public static bool StaticShowInfoBoxProperty { get; set; }
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// <para>The following example shows how info boxes can be hidden by functions.</para>
	/// <code>
	///             	public class MyComponent : MonoBehaviour
	///             	{
	///             		[InfoBox("This info box is hidden by an instance function.", "InstanceShowFunction")]
	///             		public int MyInt;
	///             		public bool InstanceShowFunction()
	///             		{
	///             			return this.MyInt == 0;
	///             		}
	///
	///             		[InfoBox("This info box is hidden by a static function.", "StaticShowFunction")]
	///             		public short MyShort;
	///             		public bool StaticShowFunction()
	///             		{
	///             			return true;
	///             		}
	///
	///             		// You can also specify a function with the same type of parameter.
	///             		// Use this to specify the same function, for multiple different properties.
	///             		[InfoBox("This info box is hidden by an instance function with a parameter.", "InstanceShowParameterFunction")]
	///             		public GameObject MyGameObject;
	///             		public bool InstanceShowParameterFunction(GameObject property)
	///             		{
	///             			return property != null;
	///             		}
	///
	///             		[InfoBox("This info box is hidden by a static function with a parameter.", "StaticShowParameterFunction")]
	///             		public Vector3 MyVector;
	///             		public bool StaticShowParameterFunction(Vector3 property)
	///             		{
	///             			return property.magnitude == 0f;
	///             		}
	///             	}
	/// </code>
	/// </example>
	/// <seealso cref="T:Sirenix.OdinInspector.RequiredAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.ValidateInputAttribute" />
	[DontApplyToListElements]
	[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
	[Conditional("UNITY_EDITOR")]
	public sealed class InfoBoxAttribute : Attribute
	{
		/// <summary>
		/// The message to display in the info box.
		/// </summary>
		public string Message;

		/// <summary>
		/// The type of the message box.
		/// </summary>
		public InfoMessageType InfoMessageType;

		/// <summary>
		/// Optional member field, property or function to show and hide the info box.
		/// </summary>
		public string VisibleIf;

		/// <summary>
		/// When <c>true</c> the InfoBox will ignore the GUI.enable flag and always draw as enabled.
		/// </summary>
		public bool GUIAlwaysEnabled;

		/// <summary>Supports a variety of color formats, including named colors (e.g. "red", "orange", "green", "blue"), hex codes (e.g. "#FF0000" and "#FF0000FF"), and RGBA (e.g. "RGBA(1,1,1,1)") or RGB (e.g. "RGB(1,1,1)"), including Odin attribute expressions (e.g "@this.MyColor"). Here are the available named colors: black, blue, clear, cyan, gray, green, grey, magenta, orange, purple, red, transparent, transparentBlack, transparentWhite, white, yellow, lightblue, lightcyan, lightgray, lightgreen, lightgrey, lightmagenta, lightorange, lightpurple, lightred, lightyellow, darkblue, darkcyan, darkgray, darkgreen, darkgrey, darkmagenta, darkorange, darkpurple, darkred, darkyellow.</summary>
		[ColorResolver]
		public string IconColor;

		private SdfIconType icon;

		/// <summary>
		/// The icon to be displayed next to the message.
		/// </summary>
		[ShowInInspector]
		[OdinDesignerBinding(new string[] { "icon", "HasDefinedIcon" })]
		public SdfIconType Icon
		{
			get
			{
				return icon;
			}
			set
			{
				icon = value;
				HasDefinedIcon = true;
			}
		}

		public bool HasDefinedIcon { get; private set; }

		/// <summary>
		/// Displays an info box above the property.
		/// </summary>
		/// <param name="message">The message for the message box. Supports referencing a member string field, property or method by using $.</param>
		/// <param name="infoMessageType">The type of the message box.</param>
		/// <param name="visibleIfMemberName">Name of member bool to show or hide the message box.</param>
		public InfoBoxAttribute(string message, InfoMessageType infoMessageType = InfoMessageType.Info, string visibleIfMemberName = null)
		{
			Message = message;
			InfoMessageType = infoMessageType;
			VisibleIf = visibleIfMemberName;
		}

		/// <summary>
		/// Displays an info box above the property.
		/// </summary>
		/// <param name="message">The message for the message box. Supports referencing a member string field, property or method by using $.</param>
		/// <param name="visibleIfMemberName">Name of member bool to show or hide the message box.</param>
		public InfoBoxAttribute(string message, string visibleIfMemberName)
		{
			Message = message;
			InfoMessageType = InfoMessageType.Info;
			VisibleIf = visibleIfMemberName;
		}

		/// <summary>
		/// Displays an info box above the property.
		/// </summary>
		/// <param name="message">The message for the message box. Supports referencing a member string field, property or method by using $.</param>
		/// <param name="icon">The icon to be displayed next to the message.</param>
		/// <param name="visibleIfMemberName">Name of member bool to show or hide the message box.</param>
		public InfoBoxAttribute(string message, SdfIconType icon, string visibleIfMemberName = null)
		{
			Message = message;
			Icon = icon;
			VisibleIf = visibleIfMemberName;
			InfoMessageType = InfoMessageType.None;
		}
	}
}
