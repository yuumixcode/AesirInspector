namespace Sirenix.OdinInspector.Editor
{
	public abstract class BaseKeyValueMapResolver<TMap> : BaseCollectionResolver<TMap>, IKeyValueMapResolver, ICollectionResolver, IApplyableResolver, IRefreshableResolver
	{
		public virtual bool ValueApplyIsTemporary { get; set; }

		public override bool ChildPropertyRequiresRefresh(int index, InspectorPropertyInfo info)
		{
			return GetChildInfo(index) != info;
		}

		public void QueueRemoveKey(object[] keys)
		{
			for (int i = 0; i < keys.Length; i++)
			{
				QueueRemoveKey(keys[i], i);
			}
		}

		public void QueueRemoveKey(object key, int selectionIndex)
		{
			EnqueueChange(delegate
			{
				RemoveKey((TMap)base.Property.BaseValueEntry.WeakValues[selectionIndex], key);
			}, new CollectionChangeInfo
			{
				ChangeType = CollectionChangeType.RemoveKey,
				Key = key,
				SelectionIndex = selectionIndex
			});
		}

		public void QueueSet(object[] keys, object[] values)
		{
			for (int i = 0; i < values.Length; i++)
			{
				QueueSet(keys[i], values[i], i);
			}
		}

		public void QueueSet(object key, object value, int selectionIndex)
		{
			EnqueueChange(delegate
			{
				Set((TMap)base.Property.BaseValueEntry.WeakValues[selectionIndex], key, value);
			}, new CollectionChangeInfo
			{
				ChangeType = CollectionChangeType.SetKey,
				Key = key,
				Value = value,
				SelectionIndex = selectionIndex
			});
		}

		protected abstract void Set(TMap map, object key, object value);

		protected abstract void RemoveKey(TMap map, object key);

		public abstract object GetKey(int selectionIndex, int childIndex);
	}
}
