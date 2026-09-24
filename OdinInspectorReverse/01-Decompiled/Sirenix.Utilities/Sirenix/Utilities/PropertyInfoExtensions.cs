using System;
using System.Reflection;

namespace Sirenix.Utilities
{
	/// <summary>
	/// PropertyInfo method extensions.
	/// </summary>
	public static class PropertyInfoExtensions
	{
		/// <summary>
		/// Determines whether a property is an auto property.
		/// </summary>
		public static bool IsAutoProperty(this PropertyInfo propInfo, bool allowVirtual = false)
		{
			if (!propInfo.CanWrite || !propInfo.CanRead)
			{
				return false;
			}
			if (!allowVirtual)
			{
				MethodInfo getter = propInfo.GetGetMethod(nonPublic: true);
				MethodInfo setter = propInfo.GetSetMethod(nonPublic: true);
				if ((getter != null && (getter.IsAbstract || getter.IsVirtual)) || (setter != null && (setter.IsAbstract || setter.IsVirtual)))
				{
					return false;
				}
			}
			BindingFlags flag = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;
			string compilerGeneratedName = "<" + propInfo.Name + ">";
			FieldInfo[] fields = propInfo.DeclaringType.GetFields(flag);
			for (int i = 0; i < fields.Length; i++)
			{
				if (fields[i].Name.Contains(compilerGeneratedName))
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Determines whether the specified property is an alias.
		/// </summary>
		/// <param name="propertyInfo">The property to check.</param>
		/// <returns>
		///   <c>true</c> if the specified property is an alias; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsAliasProperty(this PropertyInfo propertyInfo)
		{
			return propertyInfo is MemberAliasPropertyInfo;
		}

		/// <summary>
		/// Returns the original, backing property of an alias property if the property is an alias.
		/// </summary>
		/// <param name="propertyInfo">The property to check.</param>
		/// /// <param name="throwOnNotAliased">if set to <c>true</c> an exception will be thrown if the property is not aliased.</param>
		/// <returns></returns>
		/// <exception cref="T:System.ArgumentException">The property was not aliased; this only occurs if throwOnNotAliased is true.</exception>
		public static PropertyInfo DeAliasProperty(this PropertyInfo propertyInfo, bool throwOnNotAliased = false)
		{
			MemberAliasPropertyInfo aliasPropertyInfo = propertyInfo as MemberAliasPropertyInfo;
			if (aliasPropertyInfo != null)
			{
				while (aliasPropertyInfo.AliasedProperty is MemberAliasPropertyInfo)
				{
					aliasPropertyInfo = aliasPropertyInfo.AliasedProperty as MemberAliasPropertyInfo;
				}
				return aliasPropertyInfo.AliasedProperty;
			}
			if (throwOnNotAliased)
			{
				throw new ArgumentException("The property " + propertyInfo.GetNiceName() + " was not aliased.");
			}
			return propertyInfo;
		}
	}
}
