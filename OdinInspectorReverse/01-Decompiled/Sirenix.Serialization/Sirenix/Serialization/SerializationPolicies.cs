using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Sirenix.Serialization.Utilities;
using UnityEngine;

namespace Sirenix.Serialization
{
	/// <summary>
	/// Contains a set of default implementations of the <see cref="T:Sirenix.Serialization.ISerializationPolicy" /> interface.
	/// <para />
	/// NOTE: Policies are not necessarily compatible with each other in intuitive ways.
	/// Data serialized with the <see cref="P:Sirenix.Serialization.SerializationPolicies.Everything" /> policy
	/// will for example fail to deserialize auto-properties with <see cref="P:Sirenix.Serialization.SerializationPolicies.Strict" />,
	/// even if only strict data is needed.
	/// It is best to ensure that you always use the same policy for serialization and deserialization.
	/// <para />
	/// This class and all of its policies are thread-safe.
	/// </summary>
	public static class SerializationPolicies
	{
		private static readonly object LOCK = new object();

		private static volatile ISerializationPolicy everythingPolicy;

		private static volatile ISerializationPolicy unityPolicy;

		private static volatile ISerializationPolicy strictPolicy;

		/// <summary>
		/// All fields not marked with <see cref="T:System.NonSerializedAttribute" /> are serialized. If a field is marked with both <see cref="T:System.NonSerializedAttribute" /> and <see cref="T:Sirenix.Serialization.OdinSerializeAttribute" />, then the field will be serialized.
		/// </summary>
		public static ISerializationPolicy Everything
		{
			get
			{
				if (everythingPolicy == null)
				{
					lock (LOCK)
					{
						if (everythingPolicy == null)
						{
							everythingPolicy = new CustomSerializationPolicy("OdinSerializerPolicies.Everything", allowNonSerializableTypes: true, delegate(MemberInfo member)
							{
								if (!(member is FieldInfo))
								{
									return false;
								}
								return member.IsDefined<OdinSerializeAttribute>(inherit: true) || !member.IsDefined<NonSerializedAttribute>(inherit: true);
							});
						}
					}
				}
				return everythingPolicy;
			}
		}

		/// <summary>
		/// Public fields, as well as fields or auto-properties marked with <see cref="T:UnityEngine.SerializeField" /> or <see cref="T:Sirenix.Serialization.OdinSerializeAttribute" /> and not marked with <see cref="T:System.NonSerializedAttribute" />, are serialized.
		/// <para />
		/// There are two exceptions:
		/// <para />1) All fields in tuples, as well as in private nested types marked as compiler generated (e.g. lambda capture classes) are also serialized.
		/// <para />2) Virtual auto-properties are never serialized. Note that properties specified by an implemented interface are automatically marked virtual by the compiler.
		/// </summary>
		public static ISerializationPolicy Unity
		{
			get
			{
				if (unityPolicy == null)
				{
					lock (LOCK)
					{
						if (unityPolicy == null)
						{
							Type tupleInterface = typeof(string).Assembly.GetType("System.ITuple") ?? typeof(string).Assembly.GetType("System.ITupleInternal");
							unityPolicy = new CustomSerializationPolicy("OdinSerializerPolicies.Unity", allowNonSerializableTypes: true, delegate(MemberInfo member)
							{
								if (member is PropertyInfo)
								{
									PropertyInfo propertyInfo = member as PropertyInfo;
									if (propertyInfo.GetGetMethod(nonPublic: true) == null || propertyInfo.GetSetMethod(nonPublic: true) == null)
									{
										return false;
									}
								}
								if (member.IsDefined<NonSerializedAttribute>(inherit: true) && !member.IsDefined<OdinSerializeAttribute>())
								{
									return false;
								}
								if (member is FieldInfo && ((member as FieldInfo).IsPublic || (member.DeclaringType.IsNestedPrivate && member.DeclaringType.IsDefined<CompilerGeneratedAttribute>()) || (tupleInterface != null && tupleInterface.IsAssignableFrom(member.DeclaringType))))
								{
									return true;
								}
								return member.IsDefined<SerializeField>(inherit: false) || member.IsDefined<OdinSerializeAttribute>(inherit: false) || (UnitySerializationUtility.SerializeReferenceAttributeType != null && member.IsDefined(UnitySerializationUtility.SerializeReferenceAttributeType, inherit: false));
							});
						}
					}
				}
				return unityPolicy;
			}
		}

		/// <summary>
		/// Only fields and auto-properties marked with <see cref="T:UnityEngine.SerializeField" /> or <see cref="T:Sirenix.Serialization.OdinSerializeAttribute" /> and not marked with <see cref="T:System.NonSerializedAttribute" /> are serialized.
		/// <para />
		/// There are two exceptions:
		/// <para />1) All fields in private nested types marked as compiler generated (e.g. lambda capture classes) are also serialized.
		/// <para />2) Virtual auto-properties are never serialized. Note that properties specified by an implemented interface are automatically marked virtual by the compiler.
		/// </summary>
		public static ISerializationPolicy Strict
		{
			get
			{
				if (strictPolicy == null)
				{
					lock (LOCK)
					{
						if (strictPolicy == null)
						{
							strictPolicy = new CustomSerializationPolicy("OdinSerializerPolicies.Strict", allowNonSerializableTypes: true, delegate(MemberInfo member)
							{
								if (member is PropertyInfo && !((PropertyInfo)member).IsAutoProperty())
								{
									return false;
								}
								if (member.IsDefined<NonSerializedAttribute>())
								{
									return false;
								}
								if (member is FieldInfo && member.DeclaringType.IsNestedPrivate && member.DeclaringType.IsDefined<CompilerGeneratedAttribute>())
								{
									return true;
								}
								return member.IsDefined<SerializeField>(inherit: false) || member.IsDefined<OdinSerializeAttribute>(inherit: false) || (UnitySerializationUtility.SerializeReferenceAttributeType != null && member.IsDefined(UnitySerializationUtility.SerializeReferenceAttributeType, inherit: false));
							});
						}
					}
				}
				return strictPolicy;
			}
		}

		/// <summary>
		/// Tries to get a serialization policy by its id, in case a serialization graph has the policy used for serialization stored by name.
		/// </summary>
		public static bool TryGetByID(string name, out ISerializationPolicy policy)
		{
			switch (name)
			{
			case "OdinSerializerPolicies.Everything":
				policy = Everything;
				break;
			case "OdinSerializerPolicies.Unity":
				policy = Unity;
				break;
			case "OdinSerializerPolicies.Strict":
				policy = Strict;
				break;
			default:
				policy = null;
				break;
			}
			return policy != null;
		}
	}
}
