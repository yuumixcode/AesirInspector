using System;
using System.ComponentModel;

namespace Sirenix.Serialization
{
	/// <summary>
	/// Attribute indicating that a class which implements the <see cref="T:Sirenix.Serialization.IFormatter`1" /> interface somewhere in its hierarchy is a custom formatter for the type T.
	/// </summary>
	/// <seealso cref="T:System.Attribute" />
	[AttributeUsage(AttributeTargets.Class)]
	[Obsolete("Use a RegisterFormatterAttribute applied to the containing assembly instead.", true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class CustomFormatterAttribute : Attribute
	{
		/// <summary>
		/// The priority of the formatter. Of all the available custom formatters, the formatter with the highest priority is always chosen.
		/// </summary>
		public readonly int Priority;

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Sirenix.Serialization.CustomFormatterAttribute" /> class with priority 0.
		/// </summary>
		public CustomFormatterAttribute()
		{
			Priority = 0;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Sirenix.Serialization.CustomFormatterAttribute" /> class.
		/// </summary>
		/// <param name="priority">The priority of the formatter. Of all the available custom formatters, the formatter with the highest priority is always chosen.</param>
		public CustomFormatterAttribute(int priority = 0)
		{
			Priority = priority;
		}
	}
}
