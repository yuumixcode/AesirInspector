using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.Utilities;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Validation
{
	public class DefaultValidationMemberSelector : IMemberSelector
	{
		public static readonly DefaultValidationMemberSelector Instance = new DefaultValidationMemberSelector();

		private static Dictionary<Type, List<MemberInfo>> ResultCache = new Dictionary<Type, List<MemberInfo>>(FastTypeComparer.Instance);

		private static readonly object LOCK = new object();

		public IList<MemberInfo> SelectMembers(Type type)
		{
			List<MemberInfo> result;
			lock (LOCK)
			{
				if (!ResultCache.TryGetValue(type, out result))
				{
					result = ScanForMembers(type);
					ResultCache[type] = result;
				}
			}
			return result;
		}

		private static List<MemberInfo> ScanForMembers(Type type)
		{
			List<MemberInfo> result = new List<MemberInfo>();
			foreach (MemberInfo member in type.GetAllMembers(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy))
			{
				if (member.DeclaringType == typeof(UnityEngine.Object))
				{
					continue;
				}
				if (member is FieldInfo)
				{
					FieldInfo field = member as FieldInfo;
					if (!field.IsStatic || field.IsDefined<ShowInInspectorAttribute>())
					{
						result.Add(member);
					}
				}
				else if (member is PropertyInfo)
				{
					PropertyInfo prop = member as PropertyInfo;
					if (!prop.IsStatic() || prop.IsDefined<ShowInInspectorAttribute>())
					{
						result.Add(prop);
					}
				}
			}
			return result;
		}
	}
}
