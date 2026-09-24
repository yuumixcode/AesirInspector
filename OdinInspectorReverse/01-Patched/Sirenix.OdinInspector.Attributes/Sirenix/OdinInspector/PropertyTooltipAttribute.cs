using System;
using System.Diagnostics;

namespace Sirenix.OdinInspector
{
	/// <summary>
	/// <para>PropertyTooltip is used on any property, and creates tooltips for when hovering the property in the inspector.</para>
	/// <para>Use this to explain the purpose, or how to use a property.</para>
	/// </summary>
	/// <remarks>
	/// <para>This is similar to Unity's <see cref="T:UnityEngine.TooltipAttribute" /> but can be applied to both fields and properties.</para>
	/// </remarks>
	/// <example>
	/// <para>The following example shows how PropertyTooltip is applied to various properties.</para>
	/// <code>
	///             	public class MyComponent : MonoBehaviour
	///             	{
	///             		[PropertyTooltip("This is an int property.")]
	///             		public int MyField;
	///
	///             		[ShowInInspector, PropertyTooltip("This is another int property.")]
	///             		public int MyProperty { get; set; }
	///             	}
	/// </code>
	/// </example>
	/// <seealso cref="T:UnityEngine.TooltipAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.ShowInInspectorAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.PropertySpaceAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.PropertyRangeAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.PropertyOrderAttribute" />
	[DontApplyToListElements]
	[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
	[Conditional("UNITY_EDITOR")]
	public sealed class PropertyTooltipAttribute : Attribute
	{
		/// <summary>
		/// The message shown in the tooltip.
		/// </summary>
		public string Tooltip;

		/// <summary>
		/// Adds a tooltip to the property in the inspector.
		/// </summary>
		/// <param name="tooltip">The message shown in the tooltip.</param>
		public PropertyTooltipAttribute(string tooltip)
		{
			Tooltip = tooltip;
		}
	}
}
