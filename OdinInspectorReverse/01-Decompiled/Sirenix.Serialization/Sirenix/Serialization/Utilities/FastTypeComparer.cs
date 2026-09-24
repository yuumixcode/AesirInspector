using System;
using System.Collections.Generic;

namespace Sirenix.Serialization.Utilities
{
	/// <summary>
	/// Compares types by reference before comparing them using the default type equality operator.
	/// This can constitute a *significant* speedup when used as the comparer for dictionaries.
	/// </summary>
	/// <seealso cref="!:System.Collections.Generic.IEqualityComparer&lt;System.Type&gt;" />
	public class FastTypeComparer : IEqualityComparer<Type>
	{
		public static readonly FastTypeComparer Instance = new FastTypeComparer();

		public bool Equals(Type x, Type y)
		{
			if ((object)x == y)
			{
				return true;
			}
			return x == y;
		}

		public int GetHashCode(Type obj)
		{
			return obj.GetHashCode();
		}
	}
}
