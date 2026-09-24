namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// Custom types used by the <see cref="!:TemporaryPropertyContext&lt;T&gt;" /> can choose to implement the ITemporaryContext
	/// interface in order to be notified when the context gets reset.
	/// </summary>
	public interface ITemporaryContext
	{
		/// <summary>
		/// Called by <see cref="!:TemporaryPropertyContext&lt;T&gt;" /> when the context gets reset.
		/// </summary>
		void Reset();
	}
}
