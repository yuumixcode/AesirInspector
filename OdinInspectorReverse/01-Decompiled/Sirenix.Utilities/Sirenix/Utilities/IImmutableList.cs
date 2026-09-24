using System.Collections;
using System.Collections.Generic;

namespace Sirenix.Utilities
{
	/// <summary>
	/// Interface for immutable list.
	/// </summary>
	public interface IImmutableList : IList, ICollection, IEnumerable
	{
	}
	/// <summary>
	/// Interface for generic immutable list.
	/// </summary>
	public interface IImmutableList<T> : IImmutableList, IList, ICollection, IEnumerable, IList<T>, ICollection<T>, IEnumerable<T>
	{
		/// <summary>
		/// Index accessor.
		/// </summary>
		new T this[int index] { get; }
	}
}
