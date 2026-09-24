using System;
using System.Collections.Generic;
using Sirenix.Utilities.Editor;

namespace Sirenix.OdinInspector.Editor
{
	public abstract class BaseMemberPropertyResolver<TValue> : OdinPropertyResolver<TValue>, IMaySupportPrefabModifications
	{
		private InspectorPropertyInfo[] infos;

		private Dictionary<StringSlice, int> namesToIndex;

		private bool initializing;

		public virtual bool MaySupportPrefabModifications => true;

		public sealed override InspectorPropertyInfo GetChildInfo(int childIndex)
		{
			if (infos == null)
			{
				LazyInitialize();
			}
			return infos[childIndex];
		}

		public sealed override int ChildNameToIndex(string name)
		{
			StringSlice slice = name;
			return ChildNameToIndex(ref slice);
		}

		public override int ChildNameToIndex(ref StringSlice name)
		{
			if (infos == null)
			{
				LazyInitialize();
			}
			if (namesToIndex.TryGetValue(name, out var result))
			{
				return result;
			}
			return -1;
		}

		protected sealed override int GetChildCount(TValue value)
		{
			if (infos == null)
			{
				LazyInitialize();
			}
			return infos.Length;
		}

		protected abstract InspectorPropertyInfo[] GetPropertyInfos();

		private void LazyInitialize()
		{
			if (initializing)
			{
				throw new Exception("Illegal API call was made: cannot query members of a property that are dependent on children being initialized, during the initialization of the property's children.");
			}
			initializing = true;
			try
			{
				infos = GetPropertyInfos();
			}
			finally
			{
				initializing = false;
			}
			namesToIndex = new Dictionary<StringSlice, int>(StringSliceEqualityComparer.Instance);
			for (int i = 0; i < infos.Length; i++)
			{
				InspectorPropertyInfo info = infos[i];
				namesToIndex[info.PropertyName] = i;
			}
		}
	}
	public abstract class BaseMemberPropertyResolver<TValue, TAttribute> : OdinPropertyResolver<TValue, TAttribute>, IMaySupportPrefabModifications where TAttribute : Attribute
	{
		private InspectorPropertyInfo[] infos;

		private Dictionary<StringSlice, int> namesToIndex;

		private bool initializing;

		public virtual bool MaySupportPrefabModifications => true;

		protected override void Initialize()
		{
			if (initializing)
			{
				throw new Exception("Illegal API call was made: cannot query members of a property that are dependent on children being initialized, during the initialization of the property's children.");
			}
			initializing = true;
			infos = GetPropertyInfos();
			initializing = false;
			namesToIndex = new Dictionary<StringSlice, int>(StringSliceEqualityComparer.Instance);
			for (int i = 0; i < infos.Length; i++)
			{
				InspectorPropertyInfo info = infos[i];
				namesToIndex[info.PropertyName] = i;
			}
		}

		public sealed override InspectorPropertyInfo GetChildInfo(int childIndex)
		{
			return infos[childIndex];
		}

		public sealed override int ChildNameToIndex(string name)
		{
			if (namesToIndex.TryGetValue(name, out var result))
			{
				return result;
			}
			return -1;
		}

		public override int ChildNameToIndex(ref StringSlice name)
		{
			if (namesToIndex.TryGetValue(name, out var result))
			{
				return result;
			}
			return -1;
		}

		protected sealed override int GetChildCount(TValue value)
		{
			return infos.Length;
		}

		protected abstract InspectorPropertyInfo[] GetPropertyInfos();
	}
}
