using System;
using Sirenix.Utilities;

namespace Sirenix.OdinInspector.Editor
{
	public abstract class EquatableStructAtomHandler<T> : BaseAtomHandler<T> where T : struct
	{
		private static readonly Func<T, T, bool> Comparer;

		static EquatableStructAtomHandler()
		{
			Comparer = TypeExtensions.GetEqualityComparerDelegate<T>();
		}

		protected override bool CompareImplementation(T a, T b)
		{
			return Comparer(a, b);
		}

		protected override void CopyImplementation(ref T from, ref T to)
		{
			to = from;
		}

		public override T CreateInstance()
		{
			return default(T);
		}
	}
}
