using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Sirenix.Utilities;

namespace Sirenix.OdinInspector.Editor
{
	[OdinDontRegister]
	public class StaticRootPropertyResolver<T> : BaseMemberPropertyResolver<T>
	{
		private Type targetType;

		private PropertyContext<bool> allowObsoleteMembers;

		protected override bool AllowNullValues => true;

		protected override InspectorPropertyInfo[] GetPropertyInfos()
		{
			targetType = base.ValueEntry.TypeOfValue;
			IEnumerable<MemberInfo> members = targetType.GetAllMembers(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			List<InspectorPropertyInfo> infos = new List<InspectorPropertyInfo>();
			allowObsoleteMembers = base.Property.Context.GetGlobal("ALLOW_OBSOLETE_STATIC_MEMBERS", defaultValue: false);
			foreach (MemberInfo member in members.Where(Filter).OrderBy(Order))
			{
				List<Attribute> attributes = new List<Attribute>();
				InspectorPropertyInfoUtility.ProcessAttributes(base.Property, member, attributes);
				if (member is MethodInfo)
				{
					if ((member as MethodInfo).IsGenericMethodDefinition)
					{
						continue;
					}
					if (!attributes.HasAttribute<ButtonAttribute>() && !attributes.HasAttribute<OnInspectorGUIAttribute>())
					{
						attributes.Add(new ButtonAttribute(ButtonSizes.Medium));
					}
				}
				SerializationBackend backend = ((member is MethodInfo) ? SerializationBackend.None : StaticInspectorSerializationBackend.Default);
				InspectorPropertyInfo info = InspectorPropertyInfo.CreateForMember(member, allowEditable: true, backend, attributes);
				InspectorPropertyInfo previousPropertyWithName = null;
				int previousPropertyIndex = -1;
				for (int j = 0; j < infos.Count; j++)
				{
					if (infos[j].PropertyName == info.PropertyName)
					{
						previousPropertyIndex = j;
						previousPropertyWithName = infos[j];
						break;
					}
				}
				if (previousPropertyWithName != null)
				{
					bool createAlias = true;
					if (member.SignaturesAreEqual(previousPropertyWithName.GetMemberInfo()))
					{
						createAlias = false;
						infos.RemoveAt(previousPropertyIndex);
					}
					if (createAlias)
					{
						MemberInfo alias = InspectorPropertyInfoUtility.GetPrivateMemberAlias(previousPropertyWithName.GetMemberInfo(), previousPropertyWithName.TypeOfOwner.GetNiceName(), " -> ");
						infos[previousPropertyIndex] = InspectorPropertyInfo.CreateForMember(alias, allowEditable: true, backend, attributes);
					}
				}
				infos.Add(info);
			}
			return InspectorPropertyInfoUtility.BuildPropertyGroupsAndFinalize(base.Property, targetType, infos, includeSpeciallySerializedMembers: false);
		}

		private int Order(MemberInfo arg1)
		{
			if (arg1 is FieldInfo)
			{
				return 1;
			}
			if (arg1 is PropertyInfo)
			{
				return 2;
			}
			if (arg1 is MethodInfo)
			{
				return 3;
			}
			return 4;
		}

		private bool Filter(MemberInfo member)
		{
			if (member.DeclaringType == typeof(object) && targetType != typeof(object))
			{
				return false;
			}
			if (!(member is FieldInfo) && !(member is PropertyInfo) && !(member is MethodInfo))
			{
				return false;
			}
			if (member is FieldInfo && (member as FieldInfo).IsSpecialName)
			{
				return false;
			}
			if (member is MethodInfo && (member as MethodInfo).IsSpecialName)
			{
				return false;
			}
			if (member is PropertyInfo && (member as PropertyInfo).IsSpecialName)
			{
				return false;
			}
			if (member.IsDefined<CompilerGeneratedAttribute>())
			{
				return false;
			}
			if (!allowObsoleteMembers.Value && member.IsDefined<ObsoleteAttribute>())
			{
				return false;
			}
			return true;
		}
	}
}
