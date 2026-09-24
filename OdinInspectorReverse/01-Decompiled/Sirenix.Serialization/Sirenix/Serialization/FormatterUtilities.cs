using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Sirenix.Serialization.Utilities;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace Sirenix.Serialization
{
	/// <summary>
	/// Provides an array of utility methods which are commonly used by serialization formatters.
	/// </summary>
	[InitializeOnLoad]
	public static class FormatterUtilities
	{
		private static readonly DoubleLookupDictionary<ISerializationPolicy, Type, MemberInfo[]> MemberArrayCache;

		private static readonly DoubleLookupDictionary<ISerializationPolicy, Type, Dictionary<string, MemberInfo>> MemberMapCache;

		private static readonly object LOCK;

		private static readonly HashSet<Type> PrimitiveArrayTypes;

		private static readonly FieldInfo UnityObjectRuntimeErrorStringField;

		private const string UnityObjectRuntimeErrorString = "The variable nullValue of {0} has not been assigned.\r\nYou probably need to assign the nullValue variable of the {0} script in the inspector.";

		static FormatterUtilities()
		{
			MemberArrayCache = new DoubleLookupDictionary<ISerializationPolicy, Type, MemberInfo[]>();
			MemberMapCache = new DoubleLookupDictionary<ISerializationPolicy, Type, Dictionary<string, MemberInfo>>();
			LOCK = new object();
			PrimitiveArrayTypes = new HashSet<Type>(FastTypeComparer.Instance)
			{
				typeof(char),
				typeof(sbyte),
				typeof(short),
				typeof(int),
				typeof(long),
				typeof(byte),
				typeof(ushort),
				typeof(uint),
				typeof(ulong),
				typeof(decimal),
				typeof(bool),
				typeof(float),
				typeof(double),
				typeof(Guid)
			};
			UnityObjectRuntimeErrorStringField = typeof(UnityEngine.Object).GetField("m_UnityRuntimeErrorString", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (UnityObjectRuntimeErrorStringField == null)
			{
				Debug.LogWarning("A change in Unity has hindered the Serialization system's ability to create proper fake Unity null values; the UnityEngine.Object.m_UnityRuntimeErrorString field has been renamed or removed.");
			}
		}

		/// <summary>
		/// Gets a map of all serializable members on the given type. This will also properly map names extracted from <see cref="T:UnityEngine.Serialization.FormerlySerializedAsAttribute" /> and <see cref="T:Sirenix.Serialization.PreviouslySerializedAsAttribute" /> to their corresponding members.
		/// </summary>
		/// <param name="type">The type to get a map for.</param>
		/// <param name="policy">The serialization policy to use. If null, <see cref="P:Sirenix.Serialization.SerializationPolicies.Strict" /> is used.</param>
		/// <returns>A map of all serializable members on the given type.</returns>
		public static Dictionary<string, MemberInfo> GetSerializableMembersMap(Type type, ISerializationPolicy policy)
		{
			if (policy == null)
			{
				policy = SerializationPolicies.Strict;
			}
			Dictionary<string, MemberInfo> result;
			lock (LOCK)
			{
				if (!MemberMapCache.TryGetInnerValue(policy, type, out result))
				{
					result = FindSerializableMembersMap(type, policy);
					MemberMapCache.AddInner(policy, type, result);
				}
			}
			return result;
		}

		/// <summary>
		/// Gets an array of all serializable members on the given type.
		/// </summary>
		/// <param name="type">The type to get serializable members for.</param>
		/// <param name="policy">The serialization policy to use. If null, <see cref="P:Sirenix.Serialization.SerializationPolicies.Strict" /> is used.</param>
		/// <returns>An array of all serializable members on the given type.</returns>
		public static MemberInfo[] GetSerializableMembers(Type type, ISerializationPolicy policy)
		{
			if (policy == null)
			{
				policy = SerializationPolicies.Strict;
			}
			MemberInfo[] result;
			lock (LOCK)
			{
				if (!MemberArrayCache.TryGetInnerValue(policy, type, out result))
				{
					List<MemberInfo> list = new List<MemberInfo>();
					FindSerializableMembers(type, list, policy);
					result = list.ToArray();
					MemberArrayCache.AddInner(policy, type, result);
				}
			}
			return result;
		}

		/// <summary>
		/// Creates a fake Unity null value of a given type, for the given <see cref="T:UnityEngine.Object" />-derived owning type.
		/// <para />
		/// Unity uses these kinds of values to indicate missing object references.
		/// </summary>
		/// <param name="nullType">Type of the null value.</param>
		/// <param name="owningType">Type of the owning value. This is the value which changes the <see cref="T:UnityEngine.MissingReferenceException" /> which you get.</param>
		/// <returns>A fake Unity null value of a given type.</returns>
		/// <exception cref="T:System.ArgumentNullException">The nullType or owningType parameter is null.</exception>
		/// <exception cref="T:System.ArgumentException">
		/// The type given in the nullType parameter is not a Unity object.
		/// or
		/// The type given in the owningType parameter is not a Unity object.
		/// </exception>
		public static UnityEngine.Object CreateUnityNull(Type nullType, Type owningType)
		{
			if (nullType == null || owningType == null)
			{
				throw new ArgumentNullException();
			}
			if (!nullType.ImplementsOrInherits(typeof(UnityEngine.Object)))
			{
				throw new ArgumentException("Type " + nullType.Name + " is not a Unity object.");
			}
			if (!owningType.ImplementsOrInherits(typeof(UnityEngine.Object)))
			{
				throw new ArgumentException("Type " + owningType.Name + " is not a Unity object.");
			}
			UnityEngine.Object nullValue = (UnityEngine.Object)FormatterServices.GetUninitializedObject(nullType);
			if (UnityObjectRuntimeErrorStringField != null)
			{
				UnityObjectRuntimeErrorStringField.SetValue(nullValue, string.Format(CultureInfo.InvariantCulture, "The variable nullValue of {0} has not been assigned.\r\nYou probably need to assign the nullValue variable of the {0} script in the inspector.", owningType.Name));
			}
			return nullValue;
		}

		/// <summary>
		/// Determines whether a given type is a primitive type to the serialization system.
		/// <para />
		/// The following criteria are checked: type.IsPrimitive or type.IsEnum, or type is a <see cref="T:System.Decimal" />, <see cref="T:System.String" /> or <see cref="T:System.Guid" />.
		/// </summary>
		/// <param name="type">The type to check.</param>
		/// <returns><c>true</c> if the given type is a primitive type; otherwise, <c>false</c>.</returns>
		public static bool IsPrimitiveType(Type type)
		{
			if (!type.IsPrimitive && !type.IsEnum && !(type == typeof(decimal)) && !(type == typeof(string)))
			{
				return type == typeof(Guid);
			}
			return true;
		}

		/// <summary>
		/// Determines whether a given type is a primitive array type. Namely, arrays with primitive array types as elements are primitive arrays.
		/// <para />
		/// The following types are primitive array types: <see cref="T:System.Char" />, <see cref="T:System.SByte" />, <see cref="T:System.Int16" />, <see cref="T:System.Int32" />, <see cref="T:System.Int64" />, <see cref="T:System.Byte" />, <see cref="T:System.UInt16" />, <see cref="T:System.UInt32" />, <see cref="T:System.UInt64" />, <see cref="T:System.Decimal" />, <see cref="T:System.Boolean" />, <see cref="T:System.Single" />, <see cref="T:System.Double" /> and <see cref="T:System.Guid" />.
		/// </summary>
		/// <param name="type">The type to check.</param>
		/// <returns><c>true</c> if the given type is a primitive array type; otherwise, <c>false</c>.</returns>
		public static bool IsPrimitiveArrayType(Type type)
		{
			return PrimitiveArrayTypes.Contains(type);
		}

		/// <summary>
		/// Gets the type contained in the given <see cref="T:System.Reflection.MemberInfo" />. Currently only <see cref="T:System.Reflection.FieldInfo" /> and <see cref="T:System.Reflection.PropertyInfo" /> is supported.
		/// </summary>
		/// <param name="member">The <see cref="T:System.Reflection.MemberInfo" /> to get the contained type of.</param>
		/// <returns>The type contained in the given <see cref="T:System.Reflection.MemberInfo" />.</returns>
		/// <exception cref="T:System.ArgumentException">Can't get the contained type of the given <see cref="T:System.Reflection.MemberInfo" /> type.</exception>
		public static Type GetContainedType(MemberInfo member)
		{
			if (member is FieldInfo)
			{
				return (member as FieldInfo).FieldType;
			}
			if (member is PropertyInfo)
			{
				return (member as PropertyInfo).PropertyType;
			}
			throw new ArgumentException("Can't get the contained type of a " + member.GetType().Name);
		}

		/// <summary>
		/// Gets the value contained in a given <see cref="T:System.Reflection.MemberInfo" />. Currently only <see cref="T:System.Reflection.FieldInfo" /> and <see cref="T:System.Reflection.PropertyInfo" /> is supported.
		/// </summary>
		/// <param name="member">The <see cref="T:System.Reflection.MemberInfo" /> to get the value of.</param>
		/// <param name="obj">The instance to get the value from.</param>
		/// <returns>The value contained in the given <see cref="T:System.Reflection.MemberInfo" />.</returns>
		/// <exception cref="T:System.ArgumentException">Can't get the value of the given <see cref="T:System.Reflection.MemberInfo" /> type.</exception>
		public static object GetMemberValue(MemberInfo member, object obj)
		{
			if (member is FieldInfo)
			{
				return (member as FieldInfo).GetValue(obj);
			}
			if (member is PropertyInfo)
			{
				return (member as PropertyInfo).GetGetMethod(nonPublic: true).Invoke(obj, null);
			}
			throw new ArgumentException("Can't get the value of a " + member.GetType().Name);
		}

		/// <summary>
		/// Sets the value of a given MemberInfo. Currently only <see cref="T:System.Reflection.FieldInfo" /> and <see cref="T:System.Reflection.PropertyInfo" /> is supported.
		/// </summary>
		/// <param name="member">The <see cref="T:System.Reflection.MemberInfo" /> to set the value of.</param>
		/// <param name="obj">The object to set the value on.</param>
		/// <param name="value">The value to set.</param>
		/// <exception cref="T:System.ArgumentException">
		/// Property has no setter
		/// or
		/// Can't set the value of the given <see cref="T:System.Reflection.MemberInfo" /> type.
		/// </exception>
		public static void SetMemberValue(MemberInfo member, object obj, object value)
		{
			if (member is FieldInfo)
			{
				(member as FieldInfo).SetValue(obj, value);
				return;
			}
			if (member is PropertyInfo)
			{
				MethodInfo method = (member as PropertyInfo).GetSetMethod(nonPublic: true);
				if (method != null)
				{
					method.Invoke(obj, new object[1] { value });
					return;
				}
				throw new ArgumentException("Property " + member.Name + " has no setter");
			}
			throw new ArgumentException("Can't set the value of a " + member.GetType().Name);
		}

		private static Dictionary<string, MemberInfo> FindSerializableMembersMap(Type type, ISerializationPolicy policy)
		{
			Dictionary<string, MemberInfo> map = GetSerializableMembers(type, policy).ToDictionary((MemberInfo n) => n.Name, (MemberInfo n) => n);
			foreach (MemberInfo member in map.Values.ToList())
			{
				IEnumerable<FormerlySerializedAsAttribute> serializedAsAttributes = member.GetAttributes<FormerlySerializedAsAttribute>();
				foreach (FormerlySerializedAsAttribute attr in serializedAsAttributes)
				{
					if (!map.ContainsKey(attr.oldName))
					{
						map.Add(attr.oldName, member);
					}
				}
			}
			return map;
		}

		private static void FindSerializableMembers(Type type, List<MemberInfo> members, ISerializationPolicy policy)
		{
			if (type.BaseType != typeof(object) && type.BaseType != null)
			{
				FindSerializableMembers(type.BaseType, members, policy);
			}
			foreach (MemberInfo member in from n in type.GetMembers(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
				where n is FieldInfo || n is PropertyInfo
				select n)
			{
				if (policy.ShouldSerializeMember(member))
				{
					bool nameAlreadyExists = members.Any((MemberInfo n) => n.Name == member.Name);
					if (MemberIsPrivate(member) && nameAlreadyExists)
					{
						members.Add(GetPrivateMemberAlias(member));
					}
					else if (nameAlreadyExists)
					{
						members.Add(GetPrivateMemberAlias(member));
					}
					else
					{
						members.Add(member);
					}
				}
			}
		}

		/// <summary>
		/// Gets an aliased version of a member, with the declaring type name included in the member name, so that there are no conflicts with private fields and properties with the same name in different classes in the same inheritance hierarchy.
		/// <para />
		/// Marked internal in Odin because this method MUST NOT BE CALLED FROM ODIN'S INSPECTOR CODE.
		/// Odin has its own version of this, and there must be no conflict. These aliases must not be
		/// mixed into Odin's own. Use InspectorPropertyInfoUtility.GetPrivateMemberAlias instead.
		/// </summary>
		internal static MemberInfo GetPrivateMemberAlias(MemberInfo member, string prefixString = null, string separatorString = null)
		{
			if (member is FieldInfo)
			{
				if (separatorString != null)
				{
					return new MemberAliasFieldInfo(member as FieldInfo, prefixString ?? member.DeclaringType.Name, separatorString);
				}
				return new MemberAliasFieldInfo(member as FieldInfo, prefixString ?? member.DeclaringType.Name);
			}
			if (member is PropertyInfo)
			{
				if (separatorString != null)
				{
					return new MemberAliasPropertyInfo(member as PropertyInfo, prefixString ?? member.DeclaringType.Name, separatorString);
				}
				return new MemberAliasPropertyInfo(member as PropertyInfo, prefixString ?? member.DeclaringType.Name);
			}
			if (member is MethodInfo)
			{
				if (separatorString != null)
				{
					return new MemberAliasMethodInfo(member as MethodInfo, prefixString ?? member.DeclaringType.Name, separatorString);
				}
				return new MemberAliasMethodInfo(member as MethodInfo, prefixString ?? member.DeclaringType.Name);
			}
			throw new NotImplementedException();
		}

		private static bool MemberIsPrivate(MemberInfo member)
		{
			if (member is FieldInfo)
			{
				return (member as FieldInfo).IsPrivate;
			}
			if (member is PropertyInfo)
			{
				PropertyInfo prop = member as PropertyInfo;
				MethodInfo getter = prop.GetGetMethod();
				MethodInfo setter = prop.GetSetMethod();
				if (getter != null && setter != null && getter.IsPrivate)
				{
					return setter.IsPrivate;
				}
				return false;
			}
			if (member is MethodInfo)
			{
				return (member as MethodInfo).IsPrivate;
			}
			throw new NotImplementedException();
		}
	}
}
