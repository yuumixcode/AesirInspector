using System;
using System.Collections.Generic;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;

namespace Sirenix.OdinInspector.Editor
{
	[ResolverPriority(-1.0)]
	public class StrongListPropertyResolver<TList, TElement> : BaseOrderedCollectionResolver<TList>, IMaySupportPrefabModifications where TList : IList<TElement>
	{
		private static bool IsArray = typeof(TList).IsArray;

		private Dictionary<int, InspectorPropertyInfo> childInfos = new Dictionary<int, InspectorPropertyInfo>();

		private List<Attribute> childAttrs;

		public bool MaySupportPrefabModifications => true;

		public override Type ElementType => typeof(TElement);

		protected override void Initialize()
		{
			base.Initialize();
			ImmutableList<Attribute> propAttrs = base.Property.Attributes;
			List<Attribute> attrs = new List<Attribute>(propAttrs.Count);
			for (int i = 0; i < propAttrs.Count; i++)
			{
				Attribute attr = propAttrs[i];
				if (!attr.GetType().IsDefined(typeof(DontApplyToListElementsAttribute), inherit: true))
				{
					attrs.Add(attr);
				}
			}
			childAttrs = attrs;
		}

		public override InspectorPropertyInfo GetChildInfo(int childIndex)
		{
			if (childIndex < 0 || childIndex >= base.ChildCount)
			{
				throw new IndexOutOfRangeException();
			}
			if (!childInfos.TryGetValue(childIndex, out var result))
			{
				result = InspectorPropertyInfo.CreateValue(CollectionResolverUtilities.DefaultIndexToChildName(childIndex), childIndex, base.Property.BaseValueEntry.SerializationBackend, new GetterSetter<TList, TElement>(delegate(ref TList list)
				{
					int index = childIndex;
					return list[index];
				}, delegate(ref TList list, TElement element)
				{
					int index = childIndex;
					list[index] = element;
				}), childAttrs);
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
			if (IsArray)
			{
				TList newArray = (TList)(object)ArrayUtilities.CreateNewArrayWithAddedElement((TElement[])(object)collection, (TElement)value);
				ReplaceArray(collection, newArray);
			}
			else
			{
				collection.Add((TElement)value);
			}
		}

		protected override void InsertAt(TList collection, int index, object value)
		{
			if (IsArray)
			{
				TList newArray = (TList)(object)ArrayUtilities.CreateNewArrayWithInsertedElement((TElement[])(object)collection, index, (TElement)value);
				ReplaceArray(collection, newArray);
			}
			else
			{
				collection.Insert(index, (TElement)value);
			}
		}

		protected override void Remove(TList collection, object value)
		{
			if (IsArray)
			{
				TElement item = (TElement)value;
				int index = collection.IndexOf(item);
				if (index >= 0)
				{
					TList newArray = (TList)(object)ArrayUtilities.CreateNewArrayWithRemovedElement((TElement[])(object)collection, index);
					ReplaceArray(collection, newArray);
				}
			}
			else
			{
				TElement item2 = (TElement)value;
				collection.Remove(item2);
			}
		}

		protected override void RemoveAt(TList collection, int index)
		{
			if (IsArray)
			{
				TList newArray = (TList)(object)ArrayUtilities.CreateNewArrayWithRemovedElement((TElement[])(object)collection, index);
				ReplaceArray(collection, newArray);
			}
			else
			{
				collection.RemoveAt(index);
			}
		}

		protected override void Clear(TList collection)
		{
			if (IsArray)
			{
				ReplaceArray(collection, (TList)(object)new TElement[0]);
			}
			else
			{
				collection.Clear();
			}
		}

		protected override bool CollectionIsReadOnly(TList collection)
		{
			if (IsArray)
			{
				return false;
			}
			return collection.IsReadOnly;
		}

		private void ReplaceArray(TList oldArray, TList newArray)
		{
			if (!base.Property.ValueEntry.SerializationBackend.SupportsCyclicReferences)
			{
				for (int i = 0; i < base.ValueEntry.ValueCount; i++)
				{
					if ((object)base.ValueEntry.Values[i] == (object)oldArray)
					{
						base.ValueEntry.Values[i] = newArray;
						(base.ValueEntry as IValueEntryActualValueSetter).SetActualValue(i, newArray);
					}
				}
			}
			else
			{
				ReplaceArrayRecursive(base.Property.Tree.RootProperty, oldArray, newArray);
			}
		}

		private void ReplaceArrayRecursive(InspectorProperty prop, TList oldArray, TList newArray)
		{
			if (prop.Info.PropertyType == PropertyType.Value && !prop.Info.TypeOfValue.IsValueType)
			{
				IPropertyValueEntry valueEntry = prop.ValueEntry;
				if (valueEntry.SerializationBackend.SupportsCyclicReferences)
				{
					for (int i = 0; i < valueEntry.ValueCount; i++)
					{
						object obj = valueEntry.WeakValues[i];
						if ((object)oldArray == obj)
						{
							valueEntry.WeakValues[i] = newArray;
							(valueEntry as IValueEntryActualValueSetter).SetActualValue(i, newArray);
						}
					}
				}
			}
			if (prop.ChildResolver is ICollectionResolver)
			{
				prop.Children.Update();
			}
			for (int j = 0; j < prop.Children.Count; j++)
			{
				ReplaceArrayRecursive(prop.Children[j], oldArray, newArray);
			}
		}
	}
}
