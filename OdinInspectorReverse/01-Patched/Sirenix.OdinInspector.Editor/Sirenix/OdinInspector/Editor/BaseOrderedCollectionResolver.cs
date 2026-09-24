using System;

namespace Sirenix.OdinInspector.Editor
{
	public abstract class BaseOrderedCollectionResolver<TCollection> : BaseCollectionResolver<TCollection>, IOrderedCollectionResolver, ICollectionResolver, IApplyableResolver, IRefreshableResolver
	{
		public void QueueInsertAt(int index, object[] values)
		{
			for (int i = 0; i < values.Length; i++)
			{
				QueueInsertAt(index, values[i], i);
			}
		}

		public void QueueInsertAt(int index, object value, int selectionIndex)
		{
			EnqueueChange(delegate
			{
				InsertAt((TCollection)base.Property.BaseValueEntry.WeakValues[selectionIndex], index, value);
			}, new CollectionChangeInfo
			{
				ChangeType = CollectionChangeType.Insert,
				Value = value,
				Index = index,
				SelectionIndex = selectionIndex
			});
		}

		public void QueueRemoveAt(int index)
		{
			if (index < 0)
			{
				throw new IndexOutOfRangeException();
			}
			int count = base.Property.Tree.WeakTargets.Count;
			for (int i = 0; i < count; i++)
			{
				QueueRemoveAt(index, i);
			}
		}

		public void QueueRemoveAt(int index, int selectionIndex)
		{
			EnqueueChange(delegate
			{
				RemoveAt((TCollection)base.Property.BaseValueEntry.WeakValues[selectionIndex], index);
			}, new CollectionChangeInfo
			{
				ChangeType = CollectionChangeType.RemoveIndex,
				Index = index,
				SelectionIndex = selectionIndex
			});
		}

		protected abstract void InsertAt(TCollection collection, int index, object value);

		protected abstract void RemoveAt(TCollection collection, int index);
	}
}
