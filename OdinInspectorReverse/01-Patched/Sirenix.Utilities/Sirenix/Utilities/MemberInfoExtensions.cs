using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Sirenix.Utilities
{
	/// <summary>
	/// MemberInfo method extensions.
	/// </summary>
	public static class MemberInfoExtensions
	{
		/// <summary>
		/// Returns true if the attribute whose type is specified by the generic argument is defined on this member
		/// </summary>
		public static bool IsDefined<T>(this ICustomAttributeProvider member, bool inherit) where T : Attribute
		{
			try
			{
				return member.IsDefined(typeof(T), inherit);
			}
			catch
			{
				return false;
			}
		}

		/// <summary>
		/// Returns true if the attribute whose type is specified by the generic argument is defined on this member
		/// </summary>
		public static bool IsDefined<T>(this ICustomAttributeProvider member) where T : Attribute
		{
			return member.IsDefined<T>(inherit: false);
		}

		/// <summary>
		/// Returns the first found custom attribute of type T on this member
		/// Returns null if none was found
		/// </summary>
		public static T GetAttribute<T>(this ICustomAttributeProvider member, bool inherit) where T : Attribute
		{
			T[] all = member.GetAttributes<T>(inherit).ToArray();
			if (all != null && all.Length != 0)
			{
				return all[0];
			}
			return null;
		}

		/// <summary>
		/// Returns the first found non-inherited custom attribute of type T on this member
		/// Returns null if none was found
		/// </summary>
		public static T GetAttribute<T>(this ICustomAttributeProvider member) where T : Attribute
		{
			return member.GetAttribute<T>(inherit: false);
		}

		/// <summary>
		/// Gets all attributes of the specified generic type.
		/// </summary>
		/// <param name="member">The member.</param>
		public static IEnumerable<T> GetAttributes<T>(this ICustomAttributeProvider member) where T : Attribute
		{
			return member.GetAttributes<T>(inherit: false);
		}

		/// <summary>
		/// Gets all attributes of the specified generic type.
		/// </summary>
		/// <param name="member">The member.</param>
		/// <param name="inherit">If true, specifies to also search the ancestors of element for custom attributes.</param>
		public static IEnumerable<T> GetAttributes<T>(this ICustomAttributeProvider member, bool inherit) where T : Attribute
		{
			try
			{
				return member.GetCustomAttributes(typeof(T), inherit).Cast<T>();
			}
			catch
			{
				return new T[0];
			}
		}

		/// <summary>
		/// Gets all attribute instances defined on a MemeberInfo.
		/// </summary>
		/// <param name="member">The member.</param>
		public static Attribute[] GetAttributes(this ICustomAttributeProvider member)
		{
			try
			{
				return member.GetAttributes<Attribute>().ToArray();
			}
			catch
			{
				return new Attribute[0];
			}
		}

		/// <summary>
		/// Gets all attribute instances on a MemberInfo.
		/// </summary>
		/// <param name="member">The member.</param>
		/// <param name="inherit">If true, specifies to also search the ancestors of element for custom attributes.</param>
		public static Attribute[] GetAttributes(this ICustomAttributeProvider member, bool inherit)
		{
			try
			{
				return member.GetAttributes<Attribute>(inherit).ToArray();
			}
			catch
			{
				return new Attribute[0];
			}
		}

		/// <summary>
		/// If this member is a method, returns the full method name (name + params) otherwise the member name paskal splitted
		/// </summary>
		public static string GetNiceName(this MemberInfo member)
		{
			MethodBase method = member as MethodBase;
			string result = ((!(method != null)) ? member.Name : method.GetFullName());
			return result.ToTitleCase();
		}

		/// <summary>
		/// Determines whether a FieldInfo, PropertyInfo or MethodInfo is static.
		/// </summary>
		/// <param name="member">The member.</param>
		/// <returns>
		///   <c>true</c> if the specified member is static; otherwise, <c>false</c>.
		/// </returns>
		/// <exception cref="T:System.NotSupportedException"></exception>
		public static bool IsStatic(this MemberInfo member)
		{
			FieldInfo field = member as FieldInfo;
			if (field != null)
			{
				return field.IsStatic;
			}
			PropertyInfo property = member as PropertyInfo;
			if (property != null)
			{
				if (!property.CanRead)
				{
					return property.GetSetMethod(nonPublic: true).IsStatic;
				}
				return property.GetGetMethod(nonPublic: true).IsStatic;
			}
			MethodBase method = member as MethodBase;
			if (method != null)
			{
				return method.IsStatic;
			}
			EventInfo @event = member as EventInfo;
			if (@event != null)
			{
				return @event.GetRaiseMethod(nonPublic: true).IsStatic;
			}
			Type type = member as Type;
			if (type != null)
			{
				if (type.IsSealed)
				{
					return type.IsAbstract;
				}
				return false;
			}
			string message = string.Format(CultureInfo.InvariantCulture, "Unable to determine IsStatic for member {0}.{1}MemberType was {2} but only fields, properties, methods, events and types are supported.", member.DeclaringType.FullName, member.Name, member.GetType().FullName);
			throw new NotSupportedException(message);
		}

		/// <summary>
		/// Determines whether the specified member is an alias.
		/// </summary>
		/// <param name="memberInfo">The member to check.</param>
		/// <returns>
		///   <c>true</c> if the specified member is an alias; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsAlias(this MemberInfo memberInfo)
		{
			if (!(memberInfo is MemberAliasFieldInfo) && !(memberInfo is MemberAliasPropertyInfo))
			{
				return memberInfo is MemberAliasMethodInfo;
			}
			return true;
		}

		/// <summary>
		/// Returns the original, backing member of an alias member if the member is an alias.
		/// </summary>
		/// <param name="memberInfo">The member to check.</param>
		/// /// <param name="throwOnNotAliased">if set to <c>true</c> an exception will be thrown if the member is not aliased.</param>
		/// <returns></returns>
		/// <exception cref="T:System.ArgumentException">The member was not aliased; this only occurs if throwOnNotAliased is true.</exception>
		public static MemberInfo DeAlias(this MemberInfo memberInfo, bool throwOnNotAliased = false)
		{
			MemberAliasFieldInfo aliasFieldInfo = memberInfo as MemberAliasFieldInfo;
			if (aliasFieldInfo != null)
			{
				return aliasFieldInfo.AliasedField;
			}
			MemberAliasPropertyInfo aliasPropertyInfo = memberInfo as MemberAliasPropertyInfo;
			if (aliasPropertyInfo != null)
			{
				return aliasPropertyInfo.AliasedProperty;
			}
			MemberAliasMethodInfo aliasMethodInfo = memberInfo as MemberAliasMethodInfo;
			if (aliasMethodInfo != null)
			{
				return aliasMethodInfo.AliasedMethod;
			}
			if (throwOnNotAliased)
			{
				throw new ArgumentException("The member " + memberInfo.GetNiceName() + " was not aliased.");
			}
			return memberInfo;
		}

		public static bool SignaturesAreEqual(this MemberInfo a, MemberInfo b)
		{
			if (a.MemberType != b.MemberType)
			{
				return false;
			}
			if (a.Name != b.Name)
			{
				return false;
			}
			if (a.GetReturnType() != b.GetReturnType())
			{
				return false;
			}
			if (a.IsStatic() != b.IsStatic())
			{
				return false;
			}
			MethodInfo m1 = a as MethodInfo;
			MethodInfo m2 = b as MethodInfo;
			if (m1 != null)
			{
				if (m1.IsPublic != m2.IsPublic)
				{
					return false;
				}
				if (m1.IsPrivate != m2.IsPrivate)
				{
					return false;
				}
				if (m1.IsPublic != m2.IsPublic)
				{
					return false;
				}
				ParameterInfo[] p1 = m1.GetParameters();
				ParameterInfo[] p2 = m2.GetParameters();
				if (p1.Length != p2.Length)
				{
					return false;
				}
				for (int i = 0; i < p1.Length; i++)
				{
					if (p1[i].ParameterType != p2[i].ParameterType)
					{
						return false;
					}
				}
			}
			PropertyInfo p3 = a as PropertyInfo;
			PropertyInfo p4 = b as PropertyInfo;
			if (p3 != null)
			{
				MethodInfo[] p1a = p3.GetAccessors(nonPublic: true);
				MethodInfo[] p2a = p4.GetAccessors(nonPublic: true);
				if (p1a.Length != p2a.Length)
				{
					return false;
				}
				if (p1a[0].IsPublic != p2a[0].IsPublic)
				{
					return false;
				}
				if (p1a.Length > 1 && p1a[1].IsPublic != p2a[1].IsPublic)
				{
					return false;
				}
			}
			return true;
		}
	}
}
