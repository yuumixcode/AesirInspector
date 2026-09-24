using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.Utilities;

namespace Sirenix.OdinInspector.Editor
{
	[OdinCacheableProcessor]
	[ResolverPriority(1000.0)]
	public class TypeDefinitionGroupAttributeProcessor : OdinAttributeProcessor
	{
		private static readonly Dictionary<Type, bool> HadResultCache = new Dictionary<Type, bool>(FastTypeComparer.Instance);

		public override bool CanProcessSelfAttributes(InspectorProperty property)
		{
			return false;
		}

		public override bool CanProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member)
		{
			Type current = member.GetReturnType();
			if (current == null)
			{
				return false;
			}
			if (HadResultCache.TryGetValue(current, out var result))
			{
				return result;
			}
			return true;
		}

		public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
		{
			Type original = member.GetReturnType();
			Type current = original;
			if (current == null)
			{
				return;
			}
			bool wasInCache = false;
			if (HadResultCache.TryGetValue(original, out var hadResult))
			{
				if (!hadResult)
				{
					return;
				}
				wasInCache = true;
			}
			while (!(current == null))
			{
				AssemblyCategory flag = AssemblyUtilities.GetAssemblyCategory(current.Assembly);
				if ((flag & AssemblyCategory.ProjectSpecific) == 0)
				{
					break;
				}
				if (current.IsDefined(typeof(PropertyGroupAttribute), inherit: false))
				{
					hadResult = true;
					object[] attrs = current.GetCustomAttributes(typeof(PropertyGroupAttribute), inherit: false);
					for (int i = 0; i < attrs.Length; i++)
					{
						attributes.Add(attrs[i] as Attribute);
					}
				}
				current = current.BaseType;
			}
			if (!wasInCache)
			{
				HadResultCache.Add(original, hadResult);
			}
		}
	}
}
