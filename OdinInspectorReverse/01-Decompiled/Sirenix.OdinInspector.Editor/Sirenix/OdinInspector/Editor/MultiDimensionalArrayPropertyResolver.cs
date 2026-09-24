using System;
using System.Collections;
using Sirenix.Utilities.Editor;

namespace Sirenix.OdinInspector.Editor
{
	[ResolverPriority(-1.0)]
	public sealed class MultiDimensionalArrayPropertyResolver<T> : OdinPropertyResolver<T> where T : IList
	{
		public override bool CanResolveForPropertyFilter(InspectorProperty property)
		{
			Type type = property.ValueEntry.TypeOfValue;
			if (type.IsArray)
			{
				return type.GetArrayRank() > 1;
			}
			return false;
		}

		public override int ChildNameToIndex(string name)
		{
			return -1;
		}

		public override int ChildNameToIndex(ref StringSlice name)
		{
			return -1;
		}

		public override InspectorPropertyInfo GetChildInfo(int childIndex)
		{
			throw new NotSupportedException();
		}

		protected override int GetChildCount(T value)
		{
			return 0;
		}
	}
}
