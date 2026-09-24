using System;
using System.Reflection;
using Sirenix.Serialization;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// The property is serialized by Odin. Polymorphism, null values and types such as <see cref="T:System.Collections.Generic.Dictionary`2" /> are supported.
	/// </summary>
	public class OdinSerializationBackend : SerializationBackend
	{
		public override bool SupportsGenerics => true;

		public override bool SupportsPolymorphism => true;

		public override bool SupportsCyclicReferences => true;

		public override bool IsUnity => false;

		public override string ToString()
		{
			return "Odin";
		}

		public override bool CanSerializeMember(MemberInfo member)
		{
			return SerializationPolicies.Unity.ShouldSerializeMember(member);
		}

		public override bool CanSerializeType(Type type)
		{
			return true;
		}
	}
}
