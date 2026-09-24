using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities.Editor;

namespace Sirenix.OdinInspector.Editor
{
	[ResolverPriority(-4.9)]
	public class WeakListPropertyResolver<TList> : BaseOrderedCollectionResolver<TList>, IMaySupportPrefabModifications where TList : IList
	{
		private Dictionary<int, InspectorPropertyInfo> childInfos = new Dictionary<int, InspectorPropertyInfo>();

		public bool MaySupportPrefabModifications => true;

		public override Type ElementType => typeof(object);

		public override InspectorPropertyInfo GetChildInfo(int childIndex)
		{
			if (childIndex < 0 || childIndex >= base.ChildCount)
			{
				throw new IndexOutOfRangeException();
			}
			if (!childInfos.TryGetValue(childIndex, out var result))
			{
				result = InspectorPropertyInfo.CreateValue(CollectionResolverUtilities.DefaultIndexToChildName(childIndex), childIndex, base.Property.BaseValueEntry.SerializationBackend, new GetterSetter<TList, object>(delegate(ref TList list)
				{
					int index = childIndex;
					return list[index];
				}, delegate(ref TList list, object element)
				{
					int index = childIndex;
					list[index] = element;
				}), base.Property.Attributes.Where((Attribute attr) => !attr.GetType().IsDefined(typeof(DontApplyToListElementsAttribute), inherit: true)).ToArray());
				childInfos[childIndex] = result;
			}
			return result;
		}

		public override bool ChildPropertyRequiresRefresh(int index, InspectorPropertyInfo info)
		{
			return false;
		}

		public override int ChildNameToIndex(string name)
		{
			return CollectionResolverUtilities.DefaultChildNameToIndex(name);
		}

		public override int ChildNameToIndex(ref StringSlice name)
		{
			return CollectionResolverUtilities.DefaultChildNameToIndex(ref name);
		}

		protected override int GetChildCount(TList value)
		{
			return value.Count;
		}

		protected override void Add(TList collection, object value)
		{
			collection.Add(value);
		}

		protected override void InsertAt(TList collection, int index, object value)
		{
			collection.Insert(index, value);
		}

		protected override void Remove(TList collection, object value)
		{
			collection.Remove(value);
		}

		protected override void RemoveAt(TList collection, int index)
		{
			collection.RemoveAt(index);
		}

		protected override void Clear(TList collection)
		{
			collection.Clear();
		}

		protected override bool CollectionIsReadOnly(TList collection)
		{
			return collection.IsReadOnly;
		}
	}
}
