using System;
using System.ComponentModel;

namespace Sirenix.Serialization
{
	/// <summary>
	/// Attribute indicating that a generic type definition class which implements the <see cref="T:Sirenix.Serialization.IFormatter`1" /> interface somewhere in its hierarchy is a custom formatter for *any variation* of the generic type definition T.
	/// <para />
	/// The formatter's generic type parameters are mapped onto the serialized type's generic type parameters.
	/// <para />
	/// For example, <see cref="T:Sirenix.Serialization.DictionaryFormatter`2" /> implements <see cref="T:Sirenix.Serialization.IFormatter`1" />, where T is <see cref="T:System.Collections.Generic.Dictionary`2" />.
	/// </summary>
	/// <seealso cref="T:Sirenix.Serialization.CustomFormatterAttribute" />
	[AttributeUsage(AttributeTargets.Class)]
	[Obsolete("Use a RegisterFormatterAttribute applied to the containing assembly instead.", true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class CustomGenericFormatterAttribute : CustomFormatterAttribute
	{
		/// <summary>
		/// The generic type definition of the serialized type.
		/// </summary>
		public readonly Type SerializedGenericTypeDefinition;

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Sirenix.Serialization.CustomGenericFormatterAttribute" /> class.
		/// </summary>
		/// <param name="serializedGenericTypeDefinition">The generic type definition of the serialized type.</param>
		/// <param name="priority">The priority of the formatter. Of all the available custom formatters, the formatter with the highest priority is always chosen.</param>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="serializedGenericTypeDefinition" /> was null.</exception>
		/// <exception cref="T:System.ArgumentException">The type given in <paramref name="serializedGenericTypeDefinition" /> is not a generic type definition.</exception>
		public CustomGenericFormatterAttribute(Type serializedGenericTypeDefinition, int priority = 0)
			: base(priority)
		{
			if (serializedGenericTypeDefinition == null)
			{
				throw new ArgumentNullException();
			}
			if (!serializedGenericTypeDefinition.IsGenericTypeDefinition)
			{
				throw new ArgumentException("The type " + serializedGenericTypeDefinition.Name + " is not a generic type definition.");
			}
			SerializedGenericTypeDefinition = serializedGenericTypeDefinition;
		}
	}
}
