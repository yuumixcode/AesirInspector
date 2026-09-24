namespace Sirenix.Serialization.Utilities
{
	/// <summary>
	/// Provides notification callbacks for values that are cached using the <see cref="T:Sirenix.Serialization.Utilities.Cache`1" /> class.
	/// </summary>
	public interface ICacheNotificationReceiver
	{
		/// <summary>
		/// Called when the cached value is freed.
		/// </summary>
		void OnFreed();

		/// <summary>
		/// Called when the cached value is claimed.
		/// </summary>
		void OnClaimed();
	}
}
