using System;
using System.Threading;

namespace Sirenix.Serialization.Utilities
{
	/// <summary>
	/// Provides an easy way of claiming and freeing cached values of any non-abstract reference type with a public parameterless constructor.
	/// <para />
	/// Cached types which implement the <see cref="T:Sirenix.Serialization.Utilities.ICacheNotificationReceiver" /> interface will receive notifications when they are claimed and freed.
	/// <para />
	/// Only one thread should be holding a given cache instance at a time if <see cref="T:Sirenix.Serialization.Utilities.ICacheNotificationReceiver" /> is implemented, since the invocation of 
	/// <see cref="M:Sirenix.Serialization.Utilities.ICacheNotificationReceiver.OnFreed" /> is not thread safe, IE, weird stuff might happen if multiple different threads are trying to free
	/// the same cache instance at the same time. This will practically never happen unless you're doing really strange stuff, but the case is documented here.
	/// </summary>
	/// <typeparam name="T">The type which is cached.</typeparam>
	/// <seealso cref="T:System.IDisposable" />
	public sealed class Cache<T> : ICache, IDisposable where T : class, new()
	{
		private static readonly bool IsNotificationReceiver = typeof(ICacheNotificationReceiver).IsAssignableFrom(typeof(T));

		private static object[] FreeValues = new object[4];

		private bool isFree;

		private static volatile int THREAD_LOCK_TOKEN = 0;

		private static int maxCacheSize = 5;

		/// <summary>
		/// The cached value.
		/// </summary>
		public T Value;

		/// <summary>
		/// Gets or sets the maximum size of the cache. This value can never go beneath 1.
		/// </summary>
		/// <value>
		/// The maximum size of the cache.
		/// </value>
		public static int MaxCacheSize
		{
			get
			{
				return maxCacheSize;
			}
			set
			{
				maxCacheSize = Math.Max(1, value);
			}
		}

		/// <summary>
		/// Gets a value indicating whether this cached value is free.
		/// </summary>
		/// <value>
		///   <c>true</c> if this cached value is free; otherwise, <c>false</c>.
		/// </value>
		public bool IsFree => isFree;

		object ICache.Value => Value;

		private Cache()
		{
			Value = new T();
			isFree = false;
		}

		/// <summary>
		/// Claims a cached value of type <see cref="!:T" />.
		/// </summary>
		/// <returns>A cached value of type <see cref="!:T" />.</returns>
		public static Cache<T> Claim()
		{
			Cache<T> result = null;
			while (Interlocked.CompareExchange(ref THREAD_LOCK_TOKEN, 1, 0) != 0)
			{
			}
			object[] freeValues = FreeValues;
			int length = freeValues.Length;
			for (int i = 0; i < length; i++)
			{
				result = (Cache<T>)freeValues[i];
				if (result != null)
				{
					freeValues[i] = null;
					result.isFree = false;
					break;
				}
			}
			THREAD_LOCK_TOKEN = 0;
			if (result == null)
			{
				result = new Cache<T>();
			}
			if (IsNotificationReceiver)
			{
				(result.Value as ICacheNotificationReceiver).OnClaimed();
			}
			return result;
		}

		/// <summary>
		/// Releases a cached value.
		/// </summary>
		/// <param name="cache">The cached value to release.</param>
		/// <exception cref="T:System.ArgumentNullException">The cached value to release is null.</exception>
		public static void Release(Cache<T> cache)
		{
			if (cache == null)
			{
				throw new ArgumentNullException("cache");
			}
			if (cache.isFree)
			{
				return;
			}
			if (IsNotificationReceiver)
			{
				(cache.Value as ICacheNotificationReceiver).OnFreed();
			}
			while (Interlocked.CompareExchange(ref THREAD_LOCK_TOKEN, 1, 0) != 0)
			{
			}
			if (cache.isFree)
			{
				THREAD_LOCK_TOKEN = 0;
				return;
			}
			cache.isFree = true;
			object[] freeValues = FreeValues;
			int length = freeValues.Length;
			bool added = false;
			for (int i = 0; i < length; i++)
			{
				if (freeValues[i] == null)
				{
					freeValues[i] = cache;
					added = true;
					break;
				}
			}
			if (!added && length < MaxCacheSize)
			{
				object[] newArr = new object[length * 2];
				for (int j = 0; j < length; j++)
				{
					newArr[j] = freeValues[j];
				}
				newArr[length] = cache;
				FreeValues = newArr;
			}
			THREAD_LOCK_TOKEN = 0;
		}

		/// <summary>
		/// Performs an implicit conversion from <see cref="T:Sirenix.Serialization.Utilities.Cache`1" /> to <see cref="!:T" />.
		/// </summary>
		/// <param name="cache">The cache to convert.</param>
		/// <returns>
		/// The result of the conversion.
		/// </returns>
		public static implicit operator T(Cache<T> cache)
		{
			if (cache == null)
			{
				return null;
			}
			return cache.Value;
		}

		/// <summary>
		/// Releases this cached value.
		/// </summary>
		public void Release()
		{
			Release(this);
		}

		/// <summary>
		/// Releases this cached value.
		/// </summary>
		void IDisposable.Dispose()
		{
			Release(this);
		}
	}
}
