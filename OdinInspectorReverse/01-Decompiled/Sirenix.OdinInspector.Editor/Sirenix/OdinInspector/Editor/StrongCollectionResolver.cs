using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Serialization;
using Sirenix.Utilities.Editor;

namespace Sirenix.OdinInspector.Editor
{
	[ResolverPriority(-2.0)]
	public class StrongCollectionResolver<TCollection, TElement> : BaseOrderedCollectionResolver<TCollection> where TCollection : ICollection<TElement>
	{
		private Dictionary<TCollection, TElement[]> elementsArrays = new Dictionary<TCollection, TElement[]>();

		private int lastUpdateId = -1;

		private Dictionary<int, InspectorPropertyInfo> childInfos = new Dictionary<int, InspectorPropertyInfo>();

		private HashSet<TCollection> seenHashset = new HashSet<TCollection>();

		private List<TCollection> toRemoveList = new List<TCollection>();

		public override Type ElementType => typeof(TElement);

		public override int ChildNameToIndex(string name)
		{
			return CollectionResolverUtilities.DefaultChildNameToIndex(name);
		}

		public override int ChildNameToIndex(ref StringSlice name)
		{
			return CollectionResolverUtilities.DefaultChildNameToIndex(ref name);
		}

		public override bool ChildPropertyRequiresRefresh(int index, InspectorPropertyInfo info)
		{
			return false;
		}

		public override InspectorPropertyInfo GetChildInfo(int childIndex)
		{
			if (childIndex < 0 || childIndex >= base.ChildCount)
			{
				throw new IndexOutOfRangeException();
			}
			if (!childInfos.TryGetValue(childIndex, out var result))
			{
				result = InspectorPropertyInfo.CreateValue(CollectionResolverUtilities.DefaultIndexToChildName(childIndex), childIndex, base.Property.BaseValueEntry.SerializationBackend, new GetterSetter<TCollection, TElement>(delegate(ref TCollection collection)
				{
					return GetElement(collection, childIndex);
				}, delegate(ref TCollection collection, TElement element)
				{
					SetElement(collection, element, childIndex);
				}), base.Property.Attributes.Where((Attribute attr) => !attr.GetType().IsDefined(typeof(DontApplyToListElementsAttribute), inherit: true)).ToArray());
				childInfos[childIndex] = result;
			}
			return result;
		}

		protected override void Initialize()
		{
			base.Initialize();
		}

		private TElement GetElement(TCollection collection, int index)
		{
			EnsureUpdated();
			if (elementsArrays.TryGetValue(collection, out var elements))
			{
				return elements[index];
			}
			return default(TElement);
		}

		private void SetElement(TCollection collection, TElement element, int index)
		{
			int count = collection.Count;
			using Buffer<TElement> copyBuffer = Buffer<TElement>.Claim(count);
			TElement[] array = copyBuffer.Array;
			collection.CopyTo(array, 0);
			collection.Clear();
			for (int i = 0; i < count; i++)
			{
				if (i == index)
				{
					collection.Add(element);
				}
				else
				{
					collection.Add(array[i]);
				}
			}
		}

		private void EnsureUpdated(bool force = false)
		{
			int treeId = base.Property.Tree.UpdateID;
			if (!force && lastUpdateId == treeId)
			{
				return;
			}
			seenHashset.Clear();
			toRemoveList.Clear();
			lastUpdateId = treeId;
			int count = base.ValueEntry.ValueCount;
			for (int i = 0; i < count; i++)
			{
				TCollection collection = base.ValueEntry.Values[i];
				if (collection != null)
				{
					seenHashset.Add(collection);
					if (!elementsArrays.TryGetValue(collection, out var elements) || elements.Length != collection.Count)
					{
						elements = new TElement[collection.Count];
						elementsArrays[collection] = elements;
					}
					TElement[] array = elements;
					collection.CopyTo(array, 0);
				}
			}
			foreach (TCollection col in elementsArrays.Keys)
			{
				if (!seenHashset.Contains(col))
				{
					toRemoveList.Add(col);
				}
			}
			for (int j = 0; j < toRemoveList.Count; j++)
			{
				elementsArrays.Remove(toRemoveList[j]);
			}
		}

		protected override void Add(TCollection collection, object value)
		{
			collection.Add((TElement)value);
		}

		protected override void Clear(TCollection collection)
		{
			collection.Clear();
		}

		protected override bool CollectionIsReadOnly(TCollection collection)
		{
			return collection.IsReadOnly;
		}

		protected override int GetChildCount(TCollection value)
		{
			return value.Count;
		}

		protected override void Remove(TCollection collection, object value)
		{
			collection.Remove((TElement)value);
		}

		protected override void InsertAt(TCollection collection, int index, object value)
		{
			int count = collection.Count;
			TElement tValue = (TElement)value;
			using Buffer<TElement> copyBuffer = Buffer<TElement>.Claim(count);
			TElement[] array = copyBuffer.Array;
			collection.CopyTo(array, 0);
			collection.Clear();
			for (int i = 0; i < count + 1; i++)
			{
				if (i == index)
				{
					collection.Add(tValue);
					continue;
				}
				int oldElementIndex = ((i >= index) ? (i - 1) : i);
				collection.Add(array[oldElementIndex]);
			}
		}

		protected override void RemoveAt(TCollection collection, int index)
		{
			int count = collection.Count;
			using Buffer<TElement> copyBuffer = Buffer<TElement>.Claim(count);
			TElement[] array = copyBuffer.Array;
			collection.CopyTo(array, 0);
			collection.Clear();
			for (int i = 0; i < count; i++)
			{
				if (i != index)
				{
					collection.Add(array[i]);
				}
			}
		}
	}
}
