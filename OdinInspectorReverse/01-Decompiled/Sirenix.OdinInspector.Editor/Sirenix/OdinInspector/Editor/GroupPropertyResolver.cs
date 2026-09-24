using System.Collections.Generic;
using Sirenix.Utilities.Editor;

namespace Sirenix.OdinInspector.Editor
{
	[ResolverPriority(-5.0)]
	public class GroupPropertyResolver : OdinPropertyResolver
	{
		private InspectorPropertyInfo[] groupInfos;

		private Dictionary<StringSlice, int> nameToIndexMap = new Dictionary<StringSlice, int>(StringSliceEqualityComparer.Instance);

		public override bool CanResolveForPropertyFilter(InspectorProperty property)
		{
			return property.Info.PropertyType == PropertyType.Group;
		}

		protected override void Initialize()
		{
			base.Initialize();
			groupInfos = base.Property.Info.GetGroupInfos();
			for (int i = 0; i < groupInfos.Length; i++)
			{
				nameToIndexMap[groupInfos[i].PropertyName] = i;
			}
		}

		public override int ChildNameToIndex(string name)
		{
			if (nameToIndexMap.TryGetValue(name, out var index))
			{
				return index;
			}
			return -1;
		}

		public override int ChildNameToIndex(ref StringSlice name)
		{
			if (nameToIndexMap.TryGetValue(name, out var index))
			{
				return index;
			}
			return -1;
		}

		public override InspectorPropertyInfo GetChildInfo(int childIndex)
		{
			return groupInfos[childIndex];
		}

		protected override int CalculateChildCount()
		{
			return groupInfos.Length;
		}
	}
}
