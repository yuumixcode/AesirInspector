using System.Reflection;

namespace Sirenix.Serialization
{
	/// <summary>
	/// Defines which members to serialize and deserialize when there aren't any custom formatters for a type.
	/// Usually, it governs the behaviour of the <see cref="T:Sirenix.Serialization.FormatterEmitter" /> and <see cref="T:Sirenix.Serialization.ReflectionFormatter`1" /> classes.
	/// </summary>
	public interface ISerializationPolicy
	{
		/// <summary>
		/// Gets the identifier of the policy. This can be stored in the serialization metadata, so the policy used to serialize can be recovered upon deserialization without knowing the policy ahead of time. This ID should preferably be unique.
		/// </summary>
		/// <value>
		/// The identifier of the policy.
		/// </value>
		string ID { get; }

		/// <summary>
		/// Gets a value indicating whether to allow non serializable types. (Types which are not decorated with <see cref="T:System.SerializableAttribute" />.)
		/// </summary>
		/// <value>
		/// <c>true</c> if serializable types are allowed; otherwise, <c>false</c>.
		/// </value>
		bool AllowNonSerializableTypes { get; }

		/// <summary>
		/// Gets a value indicating whether a given <see cref="T:System.Reflection.MemberInfo" /> should be serialized or not.
		/// </summary>
		/// <param name="member">The member to check.</param>
		/// <returns><c>true</c> if the given member should be serialized, otherwise, <c>false</c>.</returns>
		bool ShouldSerializeMember(MemberInfo member);
	}
}
