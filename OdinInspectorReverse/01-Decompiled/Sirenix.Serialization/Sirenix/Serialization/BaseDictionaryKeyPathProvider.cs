using System.Collections.Generic;

namespace Sirenix.Serialization
{
	/// <summary>
	/// Not yet documented.
	/// </summary>
	/// <typeparam name="T">Not yet documented.</typeparam>
	public abstract class BaseDictionaryKeyPathProvider<T> : IDictionaryKeyPathProvider<T>, IDictionaryKeyPathProvider, IComparer<T>
	{
		/// <summary>
		/// Not yet documented.
		/// </summary>
		public abstract string ProviderID { get; }

		/// <summary>
		/// Not yet documented.
		/// </summary>
		/// <param name="pathStr">Not yet documented.</param>
		/// <returns>Not yet documented.</returns>
		public abstract T GetKeyFromPathString(string pathStr);

		/// <summary>
		/// Not yet documented.
		/// </summary>
		/// <param name="key">Not yet documented.</param>
		/// <returns>Not yet documented.</returns>
		public abstract string GetPathStringFromKey(T key);

		/// <summary>
		/// Not yet documented.
		/// </summary>
		/// <param name="x">Not yet documented.</param>
		/// <param name="y">Not yet documented.</param>
		/// <returns>Not yet documented.</returns>
		public abstract int Compare(T x, T y);

		int IDictionaryKeyPathProvider.Compare(object x, object y)
		{
			return Compare((T)x, (T)y);
		}

		object IDictionaryKeyPathProvider.GetKeyFromPathString(string pathStr)
		{
			return GetKeyFromPathString(pathStr);
		}

		string IDictionaryKeyPathProvider.GetPathStringFromKey(object key)
		{
			return GetPathStringFromKey((T)key);
		}
	}
}
