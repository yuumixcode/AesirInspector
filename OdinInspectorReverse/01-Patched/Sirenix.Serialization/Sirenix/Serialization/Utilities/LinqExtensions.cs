using System;
using System.Collections.Generic;

namespace Sirenix.Serialization.Utilities
{
	/// <summary>
	/// Various LinQ extensions.
	/// </summary>
	internal static class LinqExtensions
	{
		/// <summary>
		/// Perform an action on each item.
		/// </summary>
		/// <param name="source">The source.</param>
		/// <param name="action">The action to perform.</param>
		public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T> action)
		{
			foreach (T item in source)
			{
				action(item);
			}
			return source;
		}

		/// <summary>
		/// Perform an action on each item.
		/// </summary>
		/// <param name="source">The source.</param>
		/// <param name="action">The action to perform.</param>
		public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T, int> action)
		{
			int counter = 0;
			foreach (T item in source)
			{
				action(item, counter++);
			}
			return source;
		}

		/// <summary>
		/// Add a collection to the end of another collection.
		/// </summary>
		/// <param name="source">The collection.</param>
		/// <param name="append">The collection to append.</param>
		public static IEnumerable<T> Append<T>(this IEnumerable<T> source, IEnumerable<T> append)
		{
			foreach (T item in source)
			{
				yield return item;
			}
			foreach (T item2 in append)
			{
				yield return item2;
			}
		}
	}
}
