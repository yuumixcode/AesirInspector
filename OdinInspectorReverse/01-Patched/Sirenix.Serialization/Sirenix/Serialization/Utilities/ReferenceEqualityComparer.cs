using System;
using System.Collections.Generic;

namespace Sirenix.Serialization.Utilities
{
	/// <summary>
	/// Compares objects by reference only, ignoring equality operators completely. This is used by the property tree reference dictionaries to keep track of references.
	/// </summary>
	internal class ReferenceEqualityComparer<T> : IEqualityComparer<T> where T : class
	{
		/// <summary>
		/// A default, cached instance of this generic variant of the reference equality comparer.
		/// </summary>
		public static readonly ReferenceEqualityComparer<T> Default = new ReferenceEqualityComparer<T>();

		/// <summary>
		/// Returns true if the object references are equal.
		/// </summary>
		public bool Equals(T x, T y)
		{
			return x == y;
		}

		/// <summary>
		/// Returns the result of the object's own GetHashCode method.
		/// </summary>
		public int GetHashCode(T obj)
		{
			try
			{
				return obj.GetHashCode();
			}
			catch (NullReferenceException)
			{
				return -1;
			}
		}
	}
}
