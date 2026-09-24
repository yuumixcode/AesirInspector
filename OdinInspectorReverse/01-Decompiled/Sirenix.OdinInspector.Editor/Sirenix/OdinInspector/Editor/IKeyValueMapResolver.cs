namespace Sirenix.OdinInspector.Editor
{
	public interface IKeyValueMapResolver : ICollectionResolver, IApplyableResolver, IRefreshableResolver
	{
		object GetKey(int selectionIndex, int childIndex);

		void QueueSet(object[] keys, object[] values);

		void QueueSet(object key, object value, int selectionIndex);

		void QueueRemoveKey(object[] keys);

		void QueueRemoveKey(object key, int selectionIndex);
	}
}
