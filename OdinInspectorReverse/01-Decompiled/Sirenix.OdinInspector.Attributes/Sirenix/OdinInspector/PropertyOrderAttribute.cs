using System;
using System.Diagnostics;

namespace Sirenix.OdinInspector
{
	/// <summary>
	/// <para>PropertyOrder is used on any property, and allows for ordering of properties.</para>
	/// <para>Use this to define in which order your properties are shown.</para>
	/// </summary>
	/// <remarks>
	/// <para>Lower order values will be drawn before higher values.</para>
	/// <note type="note">There is unfortunately no way of ensuring that properties are in the same order, as they appear in your class. PropertyOrder overcomes this.</note>
	/// </remarks>
	/// <example>
	/// <para>The following example shows how PropertyOrder is used to order properties in the inspector.</para>
	/// <code>
	/// public class MyComponent : MonoBehaviour
	/// {
	/// 	[PropertyOrder(1)]
	/// 	public int MySecondProperty;
	///
	/// 	[PropertyOrder(-1)]
	/// 	public int MyFirstProperty;
	/// }
	/// </code>
	/// </example>    
	[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
	[Conditional("UNITY_EDITOR")]
	public class PropertyOrderAttribute : Attribute
	{
		/// <summary>
		/// The order for the property.
		/// </summary>
		public float Order;

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Sirenix.OdinInspector.PropertyOrderAttribute" /> class.
		/// </summary>
		public PropertyOrderAttribute()
		{
		}

		/// <summary>
		/// Defines a custom order for the property.
		/// </summary>
		/// <param name="order">The order for the property.</param>
		public PropertyOrderAttribute(float order)
		{
			Order = order;
		}
	}
}
