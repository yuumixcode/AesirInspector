using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Serialization;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	public class HashSetResolver<TCollection, TElement> : BaseCollectionResolver<TCollection> where TCollection : HashSet<TElement>
	{
		private Dictionary<TCollection, List<TElement>> elementsArrays = new Dictionary<TCollection, List<TElement>>();

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
				}), base.Property.Attributes.Where((Attribute attr) => !attr.GetType().IsDefined(typeof(DontApplyToListElementsAttribute), inherit: true)).AppendWith(new DelayedAttribute()).AppendWith(new SuppressInvalidAttributeErrorAttribute())
					.ToArray());
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
			if (elementsArrays.TryGetValue(collection, out var elements) && !elements.Contains(element))
			{
				elements[index] = element;
				collection.Clear();
				collection.AddRange(elements);
				EnsureUpdated(force: true);
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
					if (!elementsArrays.TryGetValue(collection, out var elements))
					{
						elements = new List<TElement>(collection.Count);
						elementsArrays[collection] = elements;
					}
					elements.Clear();
					elements.AddRange(collection);
					DictionaryKeyUtility.KeyComparer<TElement> comparer = DictionaryKeyUtility.KeyComparer<TElement>.Default;
					elements.Sort(comparer);
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
			return false;
		}

		protected override int GetChildCount(TCollection value)
		{
			return value.Count;
		}

		protected override void Remove(TCollection collection, object value)
		{
			collection.Remove((TElement)value);
		}
	}
}
