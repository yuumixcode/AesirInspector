using System;
using System.Collections.Generic;

namespace Sirenix.Serialization
{
	/// <summary>
	/// Provides a way of claiming and releasing cached array buffers.
	/// </summary>
	/// <typeparam name="T">The element type of the array to buffer.</typeparam>
	/// <seealso cref="T:System.IDisposable" />
	public sealed class Buffer<T> : IDisposable
	{
		private static readonly object LOCK = new object();

		private static readonly List<Buffer<T>> FreeBuffers = new List<Buffer<T>>();

		private int count;

		private T[] array;

		private volatile bool isFree;

		/// <summary>
		/// Gets the total element count of the buffered array. This will always be a power of two.
		/// </summary>
		/// <value>
		/// The total element count of the buffered array.
		/// </value>
		/// <exception cref="T:System.InvalidOperationException">Cannot access a buffer while it is freed.</exception>
		public int Count
		{
			get
			{
				if (isFree)
				{
					throw new InvalidOperationException("Cannot access a buffer while it is freed.");
				}
				return count;
			}
		}

		/// <summary>
		/// Gets the buffered array.
		/// </summary>
		/// <value>
		/// The buffered array.
		/// </value>
		/// <exception cref="T:System.InvalidOperationException">Cannot access a buffer while it is freed.</exception>
		public T[] Array
		{
			get
			{
				if (isFree)
				{
					throw new InvalidOperationException("Cannot access a buffer while it is freed.");
				}
				return array;
			}
		}

		/// <summary>
		/// Gets a value indicating whether this buffer is free.
		/// </summary>
		/// <value>
		///   <c>true</c> if this buffer is free; otherwise, <c>false</c>.
		/// </value>
		public bool IsFree => isFree;

		private Buffer(int count)
		{
			array = new T[count];
			this.count = count;
			isFree = false;
		}

		/// <summary>
		/// Claims a buffer with the specified minimum capacity. Note: buffers always have a capacity equal to or larger than 256.
		/// </summary>
		/// <param name="minimumCapacity">The minimum capacity.</param>
		/// <returns>A buffer which has a capacity equal to or larger than the specified minimum capacity.</returns>
		/// <exception cref="T:System.ArgumentException">Requested size of buffer must be larger than 0.</exception>
		public static Buffer<T> Claim(int minimumCapacity)
		{
			if (minimumCapacity < 0)
			{
				throw new ArgumentException("Requested size of buffer must be larger than or equal to 0.");
			}
			if (minimumCapacity < 256)
			{
				minimumCapacity = 256;
			}
			Buffer<T> result = null;
			lock (LOCK)
			{
				for (int i = 0; i < FreeBuffers.Count; i++)
				{
					Buffer<T> buffer = FreeBuffers[i];
					if (buffer != null && buffer.count >= minimumCapacity)
					{
						result = buffer;
						result.isFree = false;
						FreeBuffers[i] = null;
						break;
					}
				}
			}
			if (result == null)
			{
				result = new Buffer<T>(NextPowerOfTwo(minimumCapacity));
			}
			return result;
		}

		/// <summary>
		/// Frees the specified buffer.
		/// </summary>
		/// <param name="buffer">The buffer to free.</param>
		/// <exception cref="T:System.ArgumentNullException">The buffer argument is null.</exception>
		public static void Free(Buffer<T> buffer)
		{
			if (buffer == null)
			{
				throw new ArgumentNullException("buffer");
			}
			if (buffer.isFree)
			{
				return;
			}
			lock (LOCK)
			{
				if (buffer.isFree)
				{
					return;
				}
				buffer.isFree = true;
				bool added = false;
				for (int i = 0; i < FreeBuffers.Count; i++)
				{
					if (FreeBuffers[i] == null)
					{
						FreeBuffers[i] = buffer;
						added = true;
						break;
					}
				}
				if (!added)
				{
					FreeBuffers.Add(buffer);
				}
			}
		}

		/// <summary>
		/// Frees this buffer.
		/// </summary>
		public void Free()
		{
			Free(this);
		}

		/// <summary>
		/// Frees this buffer.
		/// </summary>
		public void Dispose()
		{
			Free(this);
		}

		private static int NextPowerOfTwo(int v)
		{
			v--;
			v |= v >> 1;
			v |= v >> 2;
			v |= v >> 4;
			v |= v >> 8;
			v |= v >> 16;
			v++;
			return v;
		}
	}
}
