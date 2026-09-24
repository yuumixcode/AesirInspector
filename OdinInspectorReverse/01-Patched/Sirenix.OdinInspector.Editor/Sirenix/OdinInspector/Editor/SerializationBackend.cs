using System;
using System.Reflection;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// Class that describes the different possible serialization backends that a property can have,
	/// and specifies the capabilities of each backend.
	/// </summary>
	public abstract class SerializationBackend
	{
		/// <summary>
		/// The property is serialized by Unity's polymorphic serialization backend via the [SerializeReference] attribute. Polymorphism, null values and cyclical references are supported.
		/// </summary>
		public static readonly SerializationBackend UnityPolymorphic = new UnityPolymorphicSerializationBackend();

		/// <summary>
		/// The property is serialized by Unity's classic serialization backend. Polymorphism, null values and types such as <see cref="T:System.Collections.Generic.Dictionary`2" /> are not supported.
		/// </summary>
		public static readonly SerializationBackend Unity = new UnityClassicSerializationBackend();

		/// <summary>
		/// The property is serialized by Odin. Polymorphism, null values and types such as <see cref="T:System.Collections.Generic.Dictionary`2" /> are supported.
		/// </summary>
		public static readonly SerializationBackend Odin = new OdinSerializationBackend();

		/// <summary>
		/// <para>The property is not serialized by anything - possibly because it is a method, possibly because it is a field or property shown in the inspector without being serialized.</para>
		/// <para>In the case of fields or properties, polymorphism, null values and types such as <see cref="T:System.Collections.Generic.Dictionary`2" /> are supported, but will not be saved.</para>
		/// </summary>
		public static readonly SerializationBackend None = new NoneSerializationBackend();

		public abstract bool SupportsGenerics { get; }

		public abstract bool SupportsPolymorphism { get; }

		public abstract bool SupportsCyclicReferences { get; }

		public abstract bool IsUnity { get; }

		public abstract bool CanSerializeType(Type type);

		public abstract bool CanSerializeMember(MemberInfo member);
	}
}
