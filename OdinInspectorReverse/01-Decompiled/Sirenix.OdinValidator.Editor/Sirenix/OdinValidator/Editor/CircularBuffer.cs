using System;

namespace Sirenix.OdinValidator.Editor
{
	internal class CircularBuffer<T>
	{
		public T[] Buffer;

		public int Position;

		public int Length => Math.Min(Buffer.Length, Position);

		public T this[int index]
		{
			get
			{
				int p = (Position - index - 1) % Buffer.Length;
				if (p < 0)
				{
					p += Buffer.Length;
				}
				return Buffer[p];
			}
		}

		public CircularBuffer(int capacity)
		{
			Buffer = new T[capacity];
		}

		public void Add(T item)
		{
			Buffer[Position++ % Buffer.Length] = item;
		}
	}
}
