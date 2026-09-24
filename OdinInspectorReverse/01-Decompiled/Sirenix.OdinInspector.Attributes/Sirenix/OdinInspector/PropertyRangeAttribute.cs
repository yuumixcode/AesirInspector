using System;
using System.ComponentModel;
using System.Diagnostics;

namespace Sirenix.OdinInspector
{
	/// <summary>
	/// <para>PropertyRange attribute creates a slider control to set the value of a property to between the specified range.</para>
	/// <para>This is equivalent to Unity's Range attribute, but this attribute can be applied to both fields and property.</para>
	/// </summary>
	/// <example>The following example demonstrates how PropertyRange is used.</example>
	/// <code>
	/// public class MyComponent : MonoBehaviour
	/// {
	/// 	[PropertyRange(0, 100)]
	/// 	public int MyInt;
	///
	/// 	[PropertyRange(-100, 100)]
	/// 	public float MyFloat;
	///
	/// 	[PropertyRange(-100, -50)]
	/// 	public decimal MyDouble;
	///
	///     // This attribute also supports dynamically referencing members by name to assign the min and max values for the range field.
	///     [PropertyRange("DynamicMin", "DynamicMax"]
	///     public float MyDynamicValue;
	///
	///     public float DynamicMin, DynamicMax;
	/// }
	/// </code>
	/// <seealso cref="T:Sirenix.OdinInspector.ShowInInspectorAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.PropertySpaceAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.PropertyTooltipAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.PropertyOrderAttribute" />
	[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
	[Conditional("UNITY_EDITOR")]
	public sealed class PropertyRangeAttribute : Attribute
	{
		/// <summary>
		/// The minimum value.
		/// </summary>
		public double Min;

		/// <summary>
		/// The maximum value.
		/// </summary>
		public double Max;

		/// <summary>
		/// A resolved string that should evaluate to a float value, and will be used as the min bounds.
		/// </summary>
		public string MinGetter;

		/// <summary>
		/// A resolved string that should evaluate to a float value, and will be used as the max bounds.
		/// </summary>
		public string MaxGetter;

		/// <summary>
		/// The name of a field, property or method to get the min value from. Obsolete; use the MinGetter member instead.
		/// </summary>
		[Obsolete("Use the MinGetter member instead.", false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public string MinMember
		{
			get
			{
				return MinGetter;
			}
			set
			{
				MinGetter = value;
			}
		}

		/// <summary>
		/// The name of a field, property or method to get the max value from. Obsolete; use the MaxGetter member instead.
		/// </summary>
		[Obsolete("Use the MaxGetter member instead.", false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public string MaxMember
		{
			get
			{
				return MaxGetter;
			}
			set
			{
				MaxGetter = value;
			}
		}

		/// <summary>
		/// Creates a slider control to set the value of the property to between the specified range..
		/// </summary>
		/// <param name="min">The minimum value.</param>
		/// <param name="max">The maximum value.</param>
		public PropertyRangeAttribute(double min, double max)
		{
			Min = ((min < max) ? min : max);
			Max = ((max > min) ? max : min);
		}

		/// <summary>
		/// Creates a slider control to set the value of the property to between the specified range..
		/// </summary>
		/// <param name="minGetter">A resolved string that should evaluate to a float value, and will be used as the min bounds.</param>
		/// <param name="max">The maximum value.</param>
		public PropertyRangeAttribute(string minGetter, double max)
		{
			MinGetter = minGetter;
			Max = max;
		}

		/// <summary>
		/// Creates a slider control to set the value of the property to between the specified range..
		/// </summary>
		/// <param name="min">The minimum value.</param>
		/// <param name="maxGetter">A resolved string that should evaluate to a float value, and will be used as the max bounds.</param>
		public PropertyRangeAttribute(double min, string maxGetter)
		{
			Min = min;
			MaxGetter = maxGetter;
		}

		/// <summary>
		/// Creates a slider control to set the value of the property to between the specified range..
		/// </summary>
		/// <param name="minGetter">A resolved string that should evaluate to a float value, and will be used as the min bounds.</param>
		/// <param name="maxGetter">A resolved string that should evaluate to a float value, and will be used as the max bounds.</param>
		public PropertyRangeAttribute(string minGetter, string maxGetter)
		{
			MinGetter = minGetter;
			MaxGetter = maxGetter;
		}
	}
}
