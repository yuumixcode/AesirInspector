using System;
using System.Diagnostics;

namespace Sirenix.OdinInspector
{
	/// <summary>
	/// <para>LabelText is used to change the labels of properties.</para>
	/// <para>Use this if you want a different label than the name of the property.</para>
	/// </summary>
	/// <example>
	/// <para>The following example shows how LabelText is applied to a few property fields.</para>
	/// <code>
	/// public MyComponent : MonoBehaviour
	/// {
	///             		[LabelText("1")]
	///             		public int MyInt1;
	///
	///             		[LabelText("2")]
	///             		public int MyInt2;
	///
	///             		[LabelText("3")]
	///             		public int MyInt3;
	/// }
	/// </code>
	/// </example>
	/// <seealso cref="T:Sirenix.OdinInspector.TitleAttribute" />
	[DontApplyToListElements]
	[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
	[Conditional("UNITY_EDITOR")]
	public class LabelTextAttribute : Attribute
	{
		/// <summary>
		/// The new text of the label.
		/// </summary>
		public string Text;

		/// <summary>
		/// Whether the label text should be nicified before it is displayed, IE, "m_someField" becomes "Some Field".
		/// If the label text is resolved via a member reference, an expression, or the like, then the evaluated result
		/// of that member reference or expression will be nicified.
		/// </summary>
		public bool NicifyText;

		/// <summary>
		/// The icon to be displayed.
		/// </summary>
		public SdfIconType Icon;

		/// <summary> Supports a variety of color formats, including named colors (e.g. "red", "orange", "green", "blue"), hex codes (e.g. "#FF0000" and "#FF0000FF"), and RGBA (e.g. "RGBA(1,1,1,1)") or RGB (e.g. "RGB(1,1,1)"), including Odin attribute expressions (e.g "@this.MyColor"). Here are the available named colors: black, blue, clear, cyan, gray, green, grey, magenta, orange, purple, red, transparent, transparentBlack, transparentWhite, white, yellow, lightblue, lightcyan, lightgray, lightgreen, lightgrey, lightmagenta, lightorange, lightpurple, lightred, lightyellow, darkblue, darkcyan, darkgray, darkgreen, darkgrey, darkmagenta, darkorange, darkpurple, darkred, darkyellow. </summary>
		[ColorResolver]
		public string IconColor;

		/// <summary>
		/// Give a property a custom label.
		/// </summary>
		/// <param name="text">The new text of the label.</param>
		public LabelTextAttribute(string text)
		{
			Text = text;
		}

		/// <summary>
		/// Give a property a custom icon.
		/// </summary>
		/// <param name="icon">The icon to be shown next to the property.</param>
		public LabelTextAttribute(SdfIconType icon)
		{
			Icon = icon;
		}

		/// <summary>
		/// Give a property a custom label.
		/// </summary>
		/// <param name="text">The new text of the label.</param>
		/// <param name="nicifyText">Whether to nicify the label text.</param>
		public LabelTextAttribute(string text, bool nicifyText)
		{
			Text = text;
			NicifyText = nicifyText;
		}

		/// <summary>
		/// Give a property a custom label with a custom icon.
		/// </summary>
		/// <param name="text">The new text of the label.</param>
		/// <param name="icon">The icon to be displayed.</param>
		public LabelTextAttribute(string text, SdfIconType icon)
		{
			Text = text;
			Icon = icon;
		}

		/// <summary>
		/// Give a property a custom label with a custom icon.
		/// </summary>
		/// <param name="text">The new text of the label.</param>
		/// <param name="nicifyText">Whether to nicify the label text.</param>
		/// <param name="icon">The icon to be displayed.</param>
		public LabelTextAttribute(string text, bool nicifyText, SdfIconType icon)
		{
			Text = text;
			NicifyText = nicifyText;
			Icon = icon;
		}
	}
}
