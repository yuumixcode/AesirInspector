namespace Sirenix.OdinInspector.Editor
{
	public interface IOrderedCollectionResolver : ICollectionResolver, IApplyableResolver, IRefreshableResolver
	{
		void QueueRemoveAt(int index);

		void QueueRemoveAt(int index, int selectionIndex);

		void QueueInsertAt(int index, object[] values);

		void QueueInsertAt(int index, object value, int selectionIndex);
	}
}
