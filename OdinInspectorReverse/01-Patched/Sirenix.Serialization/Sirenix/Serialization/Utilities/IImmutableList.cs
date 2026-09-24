using System.Collections;
using System.Collections.Generic;

namespace Sirenix.Serialization.Utilities
{
	/// <summary>
	/// Interface for immutable list.
	/// </summary>
	internal interface IImmutableList : IList, ICollection, IEnumerable
	{
	}
	/// <summary>
	/// Interface for generic immutable list.
	/// </summary>
	internal interface IImmutableList<T> : IImmutableList, IList, ICollection, IEnumerable, IList<T>, ICollection<T>, IEnumerable<T>
	{
		/// <summary>
		/// Index accessor.
		/// </summary>
		new T this[int index] { get; }
	}
}
