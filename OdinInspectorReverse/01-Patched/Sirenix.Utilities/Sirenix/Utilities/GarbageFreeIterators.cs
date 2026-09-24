using System;
using System.Collections.Generic;

namespace Sirenix.Utilities
{
	/// <summary>
	/// Garbage free enumerator methods.
	/// </summary>
	public static class GarbageFreeIterators
	{
		/// <summary>
		/// List iterator.
		/// </summary>
		public struct ListIterator<T> : IDisposable
		{
			private bool isNull;

			private List<T> list;

			private List<T>.Enumerator enumerator;

			/// <summary>
			/// Gets the current value.
			/// </summary>
			public T Current => enumerator.Current;

			/// <summary>
			/// Creates a list iterator.
			/// </summary>
			public ListIterator(List<T> list)
			{
				isNull = list == null;
				if (isNull)
				{
					this.list = null;
					enumerator = default(List<T>.Enumerator);
				}
				else
				{
					this.list = list;
					enumerator = this.list.GetEnumerator();
				}
			}

			/// <summary>
			/// Gets the enumerator.
			/// </summary>
			public ListIterator<T> GetEnumerator()
			{
				return this;
			}

			/// <summary>
			/// Moves to the next value.
			/// </summary>
			public bool MoveNext()
			{
				if (isNull)
				{
					return false;
				}
				return enumerator.MoveNext();
			}

			/// <summary>
			/// Disposes the iterator.
			/// </summary>
			public void Dispose()
			{
				enumerator.Dispose();
			}
		}

		/// <summary>
		/// Hashset iterator.
		/// </summary>
		public struct HashsetIterator<T> : IDisposable
		{
			private bool isNull;

			private HashSet<T> hashset;

			private HashSet<T>.Enumerator enumerator;

			/// <summary>
			/// Gets the current value.
			/// </summary>
			public T Current => enumerator.Current;

			/// <summary>
			/// Creates a hashset iterator.
			/// </summary>
			public HashsetIterator(HashSet<T> hashset)
			{
				isNull = hashset == null;
				if (isNull)
				{
					this.hashset = null;
					enumerator = default(HashSet<T>.Enumerator);
				}
				else
				{
					this.hashset = hashset;
					enumerator = this.hashset.GetEnumerator();
				}
			}

			/// <summary>
			/// Gets the enumerator.
			/// </summary>
			public HashsetIterator<T> GetEnumerator()
			{
				return this;
			}

			/// <summary>
			/// Moves to the next value.
			/// </summary>
			public bool MoveNext()
			{
				if (isNull)
				{
					return false;
				}
				return enumerator.MoveNext();
			}

			/// <summary>
			/// Disposes the iterator.
			/// </summary>
			public void Dispose()
			{
				enumerator.Dispose();
			}
		}

		/// <summary>
		/// Dictionary iterator.
		/// </summary>
		public struct DictionaryIterator<T1, T2> : IDisposable
		{
			private Dictionary<T1, T2> dictionary;

			private Dictionary<T1, T2>.Enumerator enumerator;

			private bool isNull;

			/// <summary>
			/// Gets the current value.
			/// </summary>
			public KeyValuePair<T1, T2> Current => enumerator.Current;

			/// <summary>
			/// Creates a dictionary iterator.
			/// </summary>
			public DictionaryIterator(Dictionary<T1, T2> dictionary)
			{
				isNull = dictionary == null;
				if (isNull)
				{
					this.dictionary = null;
					enumerator = default(Dictionary<T1, T2>.Enumerator);
				}
				else
				{
					this.dictionary = dictionary;
					enumerator = this.dictionary.GetEnumerator();
				}
			}

			/// <summary>
			/// Gets the enumerator.
			/// </summary>
			public DictionaryIterator<T1, T2> GetEnumerator()
			{
				return this;
			}

			/// <summary>
			/// Moves to the next value.
			/// </summary>
			public bool MoveNext()
			{
				if (isNull)
				{
					return false;
				}
				return enumerator.MoveNext();
			}

			/// <summary>
			/// Disposes the iterator.
			/// </summary>
			public void Dispose()
			{
				enumerator.Dispose();
			}
		}

		/// <summary>
		/// Dictionary value iterator.
		/// </summary>
		public struct DictionaryValueIterator<T1, T2> : IDisposable
		{
			private Dictionary<T1, T2> dictionary;

			private Dictionary<T1, T2>.Enumerator enumerator;

			private bool isNull;

			/// <summary>
			/// Gets the current value.
			/// </summary>
			public T2 Current => enumerator.Current.Value;

			/// <summary>
			/// Creates a dictionary value iterator.
			/// </summary>
			public DictionaryValueIterator(Dictionary<T1, T2> dictionary)
			{
				isNull = dictionary == null;
				if (isNull)
				{
					this.dictionary = null;
					enumerator = default(Dictionary<T1, T2>.Enumerator);
				}
				else
				{
					this.dictionary = dictionary;
					enumerator = this.dictionary.GetEnumerator();
				}
			}

			/// <summary>
			/// Gets the enumerator.
			/// </summary>
			public DictionaryValueIterator<T1, T2> GetEnumerator()
			{
				return this;
			}

			/// <summary>
			/// Moves to the next value.
			/// </summary>
			public bool MoveNext()
			{
				if (isNull)
				{
					return false;
				}
				return enumerator.MoveNext();
			}

			/// <summary>
			/// Disposes the iterator.
			/// </summary>
			public void Dispose()
			{
				enumerator.Dispose();
			}
		}

		/// <summary>
		/// Garbage free enumerator for lists.
		/// </summary>
		public static ListIterator<T> GFIterator<T>(this List<T> list)
		{
			return new ListIterator<T>(list);
		}

		/// <summary>
		/// Garbage free enumerator for dictionaries.
		/// </summary>
		public static DictionaryIterator<T1, T2> GFIterator<T1, T2>(this Dictionary<T1, T2> dictionary)
		{
			return new DictionaryIterator<T1, T2>(dictionary);
		}

		/// <summary>
		/// Garbage free enumator for dictionary values.
		/// </summary>
		public static DictionaryValueIterator<T1, T2> GFValueIterator<T1, T2>(this Dictionary<T1, T2> dictionary)
		{
			return new DictionaryValueIterator<T1, T2>(dictionary);
		}

		/// <summary>
		/// Garbage free enumerator for hashsets.
		/// </summary>
		public static HashsetIterator<T> GFIterator<T>(this HashSet<T> hashset)
		{
			return new HashsetIterator<T>(hashset);
		}
	}
}
