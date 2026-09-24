using System;
using System.Reflection;

namespace Sirenix.OdinInspector.Editor
{
	public class StaticInspectorSerializationBackend : SerializationBackend
	{
		public static readonly StaticInspectorSerializationBackend Default = new StaticInspectorSerializationBackend();

		public override bool SupportsGenerics => true;

		public override bool SupportsPolymorphism => true;

		public override bool SupportsCyclicReferences => true;

		public override bool IsUnity => false;

		public override bool CanSerializeMember(MemberInfo member)
		{
			return true;
		}

		public override bool CanSerializeType(Type type)
		{
			return true;
		}
	}
}
