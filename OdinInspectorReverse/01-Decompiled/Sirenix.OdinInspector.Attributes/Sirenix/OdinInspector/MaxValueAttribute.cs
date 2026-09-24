using System;
using System.Diagnostics;

namespace Sirenix.OdinInspector
{
	/// <summary>
	/// <para>MaxValue is used on primitive fields. It caps value of the field to a maximum value.</para>
	/// <para>Use this to define a maximum value for the field.</para>
	/// </summary>
	/// <remarks>
	/// <note type="note">Note that this attribute only works in the editor! Values changed from scripting will not be capped at a maximum.</note>
	/// </remarks>
	/// <example>
	/// <para>The following example shows a component where a speed value must be less than or equal to 200.</para>
	/// <code>
	/// public class Car : MonoBehaviour
	/// {
	/// 	// The speed of the car must be less than or equal to 200.
	/// 	[MaxValue(200)]
	/// 	public float Speed;
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// <para>The following example shows how MaxValue can be combined with <see cref="T:Sirenix.OdinInspector.MinValueAttribute" />.</para>
	/// <code>
	/// public class Health : MonoBehaviour
	/// {
	/// 	// The speed value must be between 0 and 200.
	/// 	[MinValue(0), MaxValue(200)]
	/// 	public float Speed;
	/// }
	/// </code>
	/// </example>
	/// <seealso cref="T:Sirenix.OdinInspector.MinValueAttribute" />
	[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
	[Conditional("UNITY_EDITOR")]
	public sealed class MaxValueAttribute : Attribute
	{
		/// <summary>
		/// The maximum value for the property.
		/// </summary>
		public double MaxValue;

		/// <summary>
		/// The string with which to resolve a maximum value. This could be a field, property or method name, or an expression.
		/// </summary>
		public string Expression;

		/// <summary>
		/// Sets a maximum value for the property in the inspector.
		/// </summary>
		/// <param name="maxValue">The max value.</param>
		public MaxValueAttribute(double maxValue)
		{
			MaxValue = maxValue;
		}

		/// <summary>
		/// Sets a maximum value for the property in the inspector.
		/// </summary>
		/// <param name="expression">The string with which to resolve a maximum value. This could be a field, property or method name, or an expression.</param>
		public MaxValueAttribute(string expression)
		{
			Expression = expression;
		}
	}
}
