using System;
using System.Reflection;
using Sirenix.Serialization;
using Sirenix.Utilities;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// The property is serialized by Unity's classic serialization backend. Polymorphism, null values and types such as <see cref="T:System.Collections.Generic.Dictionary`2" /> are not supported.
	/// </summary>
	public class UnityClassicSerializationBackend : SerializationBackend
	{
		public override bool SupportsGenerics => UnityVersion.IsVersionOrGreater(2020, 1);

		public override bool SupportsPolymorphism => false;

		public override bool SupportsCyclicReferences => false;

		public override bool IsUnity => true;

		public override string ToString()
		{
			return "Unity (Classic)";
		}

		public override bool CanSerializeMember(MemberInfo member)
		{
			return UnitySerializationUtility.GuessIfUnityWillSerialize(member);
		}

		public override bool CanSerializeType(Type type)
		{
			return UnitySerializationUtility.GuessIfUnityWillSerialize(type);
		}
	}
}
