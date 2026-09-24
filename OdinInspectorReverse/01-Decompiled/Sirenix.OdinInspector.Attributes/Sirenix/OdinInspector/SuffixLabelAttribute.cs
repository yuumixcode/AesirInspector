using System;
using System.Diagnostics;

namespace Sirenix.OdinInspector
{
	/// <summary>
	/// <para>The SuffixLabel attribute draws a label at the end of a property.</para>
	/// <para>Use this for conveying intend about a property. Is the distance measured in meters, kilometers, or in light years?.
	/// Is the angle measured in degrees or radians?
	/// Using SuffixLabel, you can place a neat label at the end of a property, to clearly show how the the property is used.</para>
	/// </summary>
	/// <example>
	/// <para>The following example demonstrates how SuffixLabel is used.</para>
	/// <code>
	///             	public class MyComponent : MonoBehaviour
	///             	{
	///             		// The SuffixLabel attribute draws a label at the end of a property.
	///             		// It's useful for conveying intend about a property.
	///             		// Fx, this field is supposed to have a prefab assigned.
	///             		[SuffixLabel("Prefab")]
	///             		public GameObject GameObject;
	///
	///             		// Using the Overlay property, the suffix label will be drawn on top of the property instead of behind it.
	///             		// Use this for a neat inline look.
	///             		[SuffixLabel("ms", Overlay = true)]
	///             		public float Speed;
	///
	///             		[SuffixLabel("radians", Overlay = true)]
	///             		public float Angle;
	///
	///             		// The SuffixLabel attribute also supports string member references by using $.
	///             		[SuffixLabel("$Suffix", Overlay = true)]
	///             		public string Suffix = "Dynamic suffix label";
	///             	}
	/// </code>
	/// </example>
	/// <seealso cref="T:Sirenix.OdinInspector.LabelTextAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.HideLabelAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.InlineButtonAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.LabelWidthAttribute" />
	[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
	[Conditional("UNITY_EDITOR")]
	public sealed class SuffixLabelAttribute : Attribute
	{
		/// <summary>
		/// The label displayed at the end of the property.
		/// </summary>
		public string Label;

		/// <summary>
		/// If <c>true</c> the suffix label will be drawn on top of the property, instead of after.
		/// </summary>
		public bool Overlay;

		/// <summary> Supports a variety of color formats, including named colors (e.g. "red", "orange", "green", "blue"), hex codes (e.g. "#FF0000" and "#FF0000FF"), and RGBA (e.g. "RGBA(1,1,1,1)") or RGB (e.g. "RGB(1,1,1)"), including Odin attribute expressions (e.g "@this.MyColor"). Here are the available named colors: black, blue, clear, cyan, gray, green, grey, magenta, orange, purple, red, transparent, transparentBlack, transparentWhite, white, yellow, lightblue, lightcyan, lightgray, lightgreen, lightgrey, lightmagenta, lightorange, lightpurple, lightred, lightyellow, darkblue, darkcyan, darkgray, darkgreen, darkgrey, darkmagenta, darkorange, darkpurple, darkred, darkyellow. </summary>
		[ColorResolver]
		public string IconColor;

		private SdfIconType icon;

		/// <summary>
		/// The icon to be displayed.
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
		/// Draws a label at the end of the property.
		/// </summary>
		/// <param name="label">The text of the label.</param>
		/// <param name="overlay">If <c>true</c> the suffix label will be drawn on top of the property, instead of after.</param>
		public SuffixLabelAttribute(string label, bool overlay = false)
		{
			Label = label;
			Overlay = overlay;
		}

		/// <summary>
		/// Draws a label at the end of the property.
		/// </summary>
		/// <param name="label">The text of the label.</param>
		/// <param name="icon">The icon to be displayed.</param>
		/// <param name="overlay">If <c>true</c> the suffix label will be drawn on top of the property, instead of after.</param>
		public SuffixLabelAttribute(string label, SdfIconType icon, bool overlay = false)
		{
			Label = label;
			Icon = icon;
			Overlay = overlay;
		}

		/// <summary>
		/// Draws a label at the end of the property.
		/// </summary>
		/// <param name="icon">The icon to be displayed.</param>
		public SuffixLabelAttribute(SdfIconType icon)
		{
			Icon = icon;
		}
	}
}
