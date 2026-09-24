using System.Collections.Generic;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	[ResolverPriority(2.0)]
	internal class ScriptableObjectTableListResolver<T> : BaseMemberPropertyResolver<T> where T : ScriptableObject
	{
		private List<OdinPropertyProcessor> processors;

		public override bool CanResolveForPropertyFilter(InspectorProperty property)
		{
			if (property.Parent != null && property.Parent.GetAttribute<TableListAttribute>() != null)
			{
				return property.Parent.ChildResolver is IOrderedCollectionResolver;
			}
			return false;
		}

		protected override InspectorPropertyInfo[] GetPropertyInfos()
		{
			if (processors == null)
			{
				processors = OdinPropertyProcessorLocator.GetMemberProcessors(base.Property);
			}
			bool includeSpeciallySerializedMembers = InspectorPropertyInfoUtility.TypeDefinesShowOdinSerializedPropertiesInInspectorAttribute_Cached(typeof(T));
			List<InspectorPropertyInfo> infos = InspectorPropertyInfoUtility.CreateMemberProperties(base.Property, typeof(T), includeSpeciallySerializedMembers);
			for (int i = 0; i < processors.Count; i++)
			{
				ProcessedMemberPropertyResolverExtensions.ProcessingOwnerType = typeof(T);
				processors[i].ProcessMemberProperties(infos);
			}
			return InspectorPropertyInfoUtility.BuildPropertyGroupsAndFinalize(base.Property, typeof(T), infos, includeSpeciallySerializedMembers);
		}
	}
}
