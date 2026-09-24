using System;
using System.Collections.Generic;

namespace Sirenix.OdinValidator.Editor
{
	internal class MiniPool<T>
	{
		private object LOCK = new object();

		private Func<T> createNew;

		private readonly Queue<T> pool = new Queue<T>();

		public MiniPool(Func<T> createNew)
		{
			this.createNew = createNew;
		}

		public void Return(T item)
		{
			lock (LOCK)
			{
				pool.Enqueue(item);
			}
		}

		public T Get()
		{
			lock (LOCK)
			{
				if (pool.Count > 0)
				{
					return pool.Dequeue();
				}
				return createNew();
			}
		}
	}
}
