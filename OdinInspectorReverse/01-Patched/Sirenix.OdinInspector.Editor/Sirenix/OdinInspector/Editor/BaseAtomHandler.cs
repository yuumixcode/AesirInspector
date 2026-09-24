using System;

namespace Sirenix.OdinInspector.Editor
{
	public abstract class BaseAtomHandler<T> : IAtomHandler<T>, IAtomHandler
	{
		private static readonly bool IsValueType = typeof(T).IsValueType;

		public Type AtomType => typeof(T);

		public bool Compare(T a, T b)
		{
			if (IsValueType)
			{
				return CompareImplementation(a, b);
			}
			if ((object)a == (object)b)
			{
				return true;
			}
			if (a == null || b == null)
			{
				return false;
			}
			return CompareImplementation(a, b);
		}

		protected abstract bool CompareImplementation(T a, T b);

		public abstract T CreateInstance();

		public bool Compare(object a, object b)
		{
			return Compare((T)a, (T)b);
		}

		public void Copy(ref T from, ref T to)
		{
			if (IsValueType)
			{
				CopyImplementation(ref from, ref to);
			}
			else if ((object)from != (object)to)
			{
				if (from == null)
				{
					to = default(T);
				}
				else if (to == null)
				{
					to = CreateInstance();
					CopyImplementation(ref from, ref to);
				}
				else
				{
					CopyImplementation(ref from, ref to);
				}
			}
		}

		protected abstract void CopyImplementation(ref T from, ref T to);

		public void Copy(ref object from, ref object to)
		{
			T tFrom = (T)from;
			T tTo = (T)to;
			Copy(ref tFrom, ref tTo);
			from = tFrom;
			to = tTo;
		}

		object IAtomHandler.CreateInstance()
		{
			return CreateInstance();
		}
	}
}
