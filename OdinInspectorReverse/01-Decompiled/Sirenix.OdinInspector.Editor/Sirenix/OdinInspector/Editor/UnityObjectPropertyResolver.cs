using System;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	public class UnityObjectPropertyResolver<T> : OdinPropertyResolver<T>, IMaySupportPrefabModifications where T : UnityEngine.Object
	{
		public bool MaySupportPrefabModifications => true;

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

		public override bool CanResolveForPropertyFilter(InspectorProperty property)
		{
			return property != property.Tree.RootProperty;
		}
	}
}
