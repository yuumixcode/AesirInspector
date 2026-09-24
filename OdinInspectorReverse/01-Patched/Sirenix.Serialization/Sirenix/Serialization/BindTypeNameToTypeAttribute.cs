using System;

namespace Sirenix.Serialization
{
	/// <summary>
	/// An attribute that lets you help the DefaultSerializationBinder bind type names to types. This is useful if you're renaming a type,
	/// that would result in data loss, and what to specify the new type name to avoid loss of data.
	/// </summary>
	/// <seealso cref="T:Sirenix.Serialization.DefaultSerializationBinder" />
	/// <example>
	/// <code>
	/// [assembly: OdinSerializer.BindTypeNameToType("Namespace.OldTypeName", typeof(Namespace.NewTypeName))]
	/// //[assembly: OdinSerializer.BindTypeNameToType("Namespace.OldTypeName, OldFullAssemblyName", typeof(Namespace.NewTypeName))]
	///
	/// namespace Namespace
	/// {
	///     public class SomeComponent : SerializedMonoBehaviour
	///     {
	///         public IInterface test; // Contains an instance of OldTypeName;
	///     }
	///
	///     public interface IInterface { }
	///
	///     public class NewTypeName : IInterface { }
	///
	///     //public class OldTypeName : IInterface { }
	/// }
	/// </code>
	/// </example>
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	public sealed class BindTypeNameToTypeAttribute : Attribute
	{
		internal readonly Type NewType;

		internal readonly string OldTypeName;

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Sirenix.Serialization.BindTypeNameToTypeAttribute" /> class.
		/// </summary>
		/// <param name="oldFullTypeName">Old old full type name. If it's moved to new a new assembly you must specify the old assembly name as well. See example code in the documentation.</param>
		/// <param name="newType">The new type.</param>
		public BindTypeNameToTypeAttribute(string oldFullTypeName, Type newType)
		{
			OldTypeName = oldFullTypeName;
			NewType = newType;
		}
	}
}
