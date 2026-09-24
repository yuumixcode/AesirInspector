using System;
using System.Reflection;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// The property is serialized by Unity's polymorphic serialization backend via the [SerializeReference] attribute. Polymorphism, null values and cyclical references are supported.
	/// </summary>
	public class UnityPolymorphicSerializationBackend : SerializationBackend
	{
		public static readonly Type SerializeReferenceAttribute = typeof(SerializeField).Assembly.GetType("UnityEngine.SerializeReference");

		public override bool SupportsGenerics => true;

		public override bool SupportsPolymorphism => true;

		public override bool SupportsCyclicReferences => true;

		public override bool IsUnity => true;

		public override string ToString()
		{
			return "Unity (Polymorphic)";
		}

		public override bool CanSerializeMember(MemberInfo member)
		{
			if (SerializeReferenceAttribute == null)
			{
				return false;
			}
			try
			{
				return member is FieldInfo && member.IsDefined(SerializeReferenceAttribute, inherit: false);
			}
			catch
			{
				return false;
			}
		}

		public override bool CanSerializeType(Type type)
		{
			return SerializeReferenceAttribute != null;
		}
	}
}
