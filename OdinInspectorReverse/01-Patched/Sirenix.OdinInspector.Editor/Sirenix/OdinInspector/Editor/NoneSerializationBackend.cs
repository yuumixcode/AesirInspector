using System;
using System.Reflection;
using Sirenix.Utilities;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// <para>The property is not serialized by anything - possibly because it is a method, possibly because it is a field or property shown in the inspector without being serialized.</para>
	/// <para>In the case of fields or properties, polymorphism, null values and types such as <see cref="T:System.Collections.Generic.Dictionary`2" /> are supported, but will not be saved.</para>
	/// </summary>
	public class NoneSerializationBackend : SerializationBackend
	{
		public override bool SupportsGenerics => true;

		public override bool SupportsPolymorphism => true;

		public override bool SupportsCyclicReferences => true;

		public override bool IsUnity => false;

		public override string ToString()
		{
			return "None";
		}

		public override bool CanSerializeMember(MemberInfo member)
		{
			if (!SerializationBackend.Odin.CanSerializeMember(member))
			{
				return member.IsDefined<ShowInInspectorAttribute>(inherit: true);
			}
			return true;
		}

		public override bool CanSerializeType(Type type)
		{
			return true;
		}
	}
}
