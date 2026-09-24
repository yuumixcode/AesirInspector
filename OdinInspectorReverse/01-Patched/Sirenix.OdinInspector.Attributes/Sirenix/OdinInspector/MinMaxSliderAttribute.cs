using System;
using System.ComponentModel;
using System.Diagnostics;

namespace Sirenix.OdinInspector
{
	/// <summary>
	/// <para>Draw a special slider the user can use to specify a range between a min and a max value.</para>
	/// <para>Uses a Vector2 where x is min and y is max.</para>
	/// </summary>
	/// <example>
	/// <para>The following example shows how MinMaxSlider is used.</para>
	/// <code>
	/// public class Player : MonoBehaviour
	/// {
	/// 	[MinMaxSlider(4, 5)]
	/// 	public Vector2 SpawnRadius;
	/// }
	/// </code>
	/// </example>
	[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
	[Conditional("UNITY_EDITOR")]
	public sealed class MinMaxSliderAttribute : Attribute
	{
		/// <summary>
		/// The hardcoded min value for the slider.
		/// </summary>
		public float MinValue;

		/// <summary>
		/// The hardcoded max value for the slider.
		/// </summary>
		public float MaxValue;

		/// <summary>
		/// A resolved string that should evaluate to a float value, which is used as the min bounds.
		/// </summary>
		public string MinValueGetter;

		/// <summary>
		/// A resolved string that should evaluate to a float value, which is used as the max bounds.
		/// </summary>
		public string MaxValueGetter;

		/// <summary>
		/// A resolved string that should evaluate to a Vector2 value, which is used as the min/max bounds. If this is non-null, it overrides the behaviour of the MinValue, MinValueGetter, MaxValue and MaxValueGetter members.
		/// </summary>
		public string MinMaxValueGetter;

		/// <summary>
		/// Draw float fields for min and max value.
		/// </summary>
		public bool ShowFields;

		/// <summary>
		/// The name of a field, property or method to get the min value from. Obsolete; use MinValueGetter instead.
		/// </summary>
		[Obsolete("Use the MinValueGetter member instead.", false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public string MinMember
		{
			get
			{
				return MinValueGetter;
			}
			set
			{
				MinValueGetter = value;
			}
		}

		/// <summary>
		/// The name of a field, property or method to get the max value from. Obsolete; use MaxValueGetter instead.
		/// </summary>
		[Obsolete("Use the MaxValueGetter member instead.", false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public string MaxMember
		{
			get
			{
				return MaxValueGetter;
			}
			set
			{
				MaxValueGetter = value;
			}
		}

		/// <summary>
		/// The name of a Vector2 field, property or method to get the min max values from. Obsolete; use MinMaxValueGetter instead.
		/// </summary>
		[Obsolete("Use the MinMaxValueGetter member instead.", false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public string MinMaxMember
		{
			get
			{
				return MinMaxValueGetter;
			}
			set
			{
				MinMaxValueGetter = value;
			}
		}

		/// <summary>
		/// Draws a min-max slider in the inspector. X will be set to min, and Y will be set to max.
		/// </summary>
		/// <param name="minValue">The min value.</param>
		/// <param name="maxValue">The max value.</param>
		/// <param name="showFields">If <c>true</c> number fields will drawn next to the MinMaxSlider.</param>
		public MinMaxSliderAttribute(float minValue, float maxValue, bool showFields = false)
		{
			MinValue = minValue;
			MaxValue = maxValue;
			ShowFields = showFields;
		}

		/// <summary>
		/// Draws a min-max slider in the inspector. X will be set to min, and Y will be set to max.
		/// </summary>
		/// <param name="minValueGetter">A resolved string that should evaluate to a float value, which is used as the min bounds.</param>
		/// <param name="maxValue">The max value.</param>
		/// <param name="showFields">If <c>true</c> number fields will drawn next to the MinMaxSlider.</param>
		public MinMaxSliderAttribute(string minValueGetter, float maxValue, bool showFields = false)
		{
			MinValueGetter = minValueGetter;
			MaxValue = maxValue;
			ShowFields = showFields;
		}

		/// <summary>
		/// Draws a min-max slider in the inspector. X will be set to min, and Y will be set to max.
		/// </summary>
		/// <param name="minValue">The min value.</param>
		/// <param name="maxValueGetter">A resolved string that should evaluate to a float value, which is used as the max bounds.</param>
		/// <param name="showFields">If <c>true</c> number fields will drawn next to the MinMaxSlider.</param>
		public MinMaxSliderAttribute(float minValue, string maxValueGetter, bool showFields = false)
		{
			MinValue = minValue;
			MaxValueGetter = maxValueGetter;
			ShowFields = showFields;
		}

		/// <summary>
		/// Draws a min-max slider in the inspector. X will be set to min, and Y will be set to max.
		/// </summary>
		/// <param name="minValueGetter">A resolved string that should evaluate to a float value, which is used as the min bounds.</param>
		/// <param name="maxValueGetter">A resolved string that should evaluate to a float value, which is used as the max bounds.</param>
		/// <param name="showFields">If <c>true</c> number fields will drawn next to the MinMaxSlider.</param>
		public MinMaxSliderAttribute(string minValueGetter, string maxValueGetter, bool showFields = false)
		{
			MinValueGetter = minValueGetter;
			MaxValueGetter = maxValueGetter;
			ShowFields = showFields;
		}

		/// <summary>
		/// Draws a min-max slider in the inspector. X will be set to min, and Y will be set to max.
		/// </summary>
		/// <param name="minMaxValueGetter">A resolved string that should evaluate to a Vector2 value, which is used as the min/max bounds. If this is non-null, it overrides the behaviour of the MinValue, MinValueGetter, MaxValue and MaxValueGetter members.</param>
		/// <param name="showFields">If <c>true</c> number fields will drawn next to the MinMaxSlider.</param>
		public MinMaxSliderAttribute(string minMaxValueGetter, bool showFields = false)
		{
			MinMaxValueGetter = minMaxValueGetter;
			ShowFields = showFields;
		}
	}
}
