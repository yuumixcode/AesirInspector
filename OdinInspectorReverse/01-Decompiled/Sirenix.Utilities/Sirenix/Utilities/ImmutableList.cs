using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sirenix.Utilities
{
	/// <summary>
	/// Immutable list wraps another list, and allows for reading the inner list, without the ability to change it.
	/// </summary>
	[Serializable]
	public sealed class ImmutableList : IImmutableList<object>, IImmutableList, IList, ICollection, IEnumerable, IList<object>, ICollection<object>, IEnumerable<object>
	{
		[SerializeField]
		private IList innerList;

		/// <summary>
		/// Number of items in the list.
		/// </summary>
		public int Count => innerList.Count;

		/// <summary>
		/// Immutable list cannot be changed directly, so it's size is always fixed.
		/// </summary>
		public bool IsFixedSize => true;

		/// <summary>
		/// Immutable list are always readonly.
		/// </summary>
		public bool IsReadOnly => true;

		/// <summary>
		/// Returns <c>true</c> if the inner list is synchronized.
		/// </summary>
		public bool IsSynchronized => innerList.IsSynchronized;

		/// <summary>
		/// Gets the sync root object.
		/// </summary>
		public object SyncRoot => innerList.SyncRoot;

		object IList.this[int index]
		{
			get
			{
				return innerList[index];
			}
			set
			{
				throw new NotSupportedException("Immutable Lists cannot be edited.");
			}
		}

		object IList<object>.this[int index]
		{
			get
			{
				return innerList[index];
			}
			set
			{
				throw new NotSupportedException("Immutable Lists cannot be edited.");
			}
		}

		/// <summary>
		/// Index accessor.
		/// </summary>
		/// <param name="index">Index.</param>
		public object this[int index] => innerList[index];

		/// <summary>
		/// Creates an immutable list around another list.
		/// </summary>
		public ImmutableList(IList innerList)
		{
			if (innerList == null)
			{
				throw new ArgumentNullException("innerList");
			}
			this.innerList = innerList;
		}

		/// <summary>
		/// Returns <c>true</c> if the item is contained in the list.
		/// </summary>
		/// <param name="value">The item's value.</param>
		public bool Contains(object value)
		{
			return innerList.Contains(value);
		}

		/// <summary>
		/// Copy the list to an array,
		/// </summary>
		/// <param name="array">Target array.</param>
		/// <param name="arrayIndex">Index.</param>
		public void CopyTo(object[] array, int arrayIndex)
		{
			innerList.CopyTo(array, arrayIndex);
		}

		/// <summary>
		/// Copy the list to an array,
		/// </summary>
		/// <param name="array">Target array.</param>
		/// <param name="index">Index.</param>
		public void CopyTo(Array array, int index)
		{
			innerList.CopyTo(array, index);
		}

		/// <summary>
		/// Gets an enumerator.
		/// </summary>
		public IEnumerator GetEnumerator()
		{
			return innerList.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		IEnumerator<object> IEnumerable<object>.GetEnumerator()
		{
			foreach (object inner in innerList)
			{
				yield return inner;
			}
		}

		int IList.Add(object value)
		{
			throw new NotSupportedException("Immutable Lists cannot be edited.");
		}

		void IList.Clear()
		{
			throw new NotSupportedException("Immutable Lists cannot be edited.");
		}

		void IList.Insert(int index, object value)
		{
			throw new NotSupportedException("Immutable Lists cannot be edited.");
		}

		void IList.Remove(object value)
		{
			throw new NotSupportedException("Immutable Lists cannot be edited.");
		}

		void IList.RemoveAt(int index)
		{
			throw new NotSupportedException("Immutable Lists cannot be edited.");
		}

		/// <summary>
		/// Get the index of a value.
		/// </summary>
		/// <param name="value">The item's value.</param>
		public int IndexOf(object value)
		{
			return innerList.IndexOf(value);
		}

		/// <summary>
		/// Immutable list cannot be edited.
		/// </summary>
		/// <param name="index">Index.</param>
		void IList<object>.RemoveAt(int index)
		{
			throw new NotSupportedException("Immutable Lists cannot be edited.");
		}

		/// <summary>
		/// Immutable list cannot be edited.
		/// </summary>
		/// <param name="index">Index.</param>
		/// <param name="item">Item.</param>
		void IList<object>.Insert(int index, object item)
		{
			throw new NotSupportedException("Immutable Lists cannot be edited.");
		}

		/// <summary>
		/// Immutable list cannot be edited.
		/// </summary>
		/// <param name="item">Item.</param>
		void ICollection<object>.Add(object item)
		{
			throw new NotSupportedException("Immutable Lists cannot be edited.");
		}

		/// <summary>
		/// Immutable list cannot be edited.
		/// </summary>
		void ICollection<object>.Clear()
		{
			throw new NotSupportedException("Immutable Lists cannot be edited.");
		}

		/// <summary>
		/// Immutable list cannot be edited.
		/// </summary>
		/// <param name="item">Item.</param>
		bool ICollection<object>.Remove(object item)
		{
			throw new NotSupportedException("Immutable Lists cannot be edited.");
		}
	}
	/// <summary>
	/// Not yet documented.
	/// </summary>
	[Serializable]
	public sealed class ImmutableList<T> : IImmutableList<T>, IImmutableList, IList, ICollection, IEnumerable, IList<T>, ICollection<T>, IEnumerable<T>
	{
		[SerializeField]
		private IList<T> innerList;

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public int Count => innerList.Count;

		bool ICollection.IsSynchronized => false;

		object ICollection.SyncRoot => null;

		bool IList.IsFixedSize => true;

		bool IList.IsReadOnly => true;

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public bool IsReadOnly => true;

		object IList.this[int index]
		{
			get
			{
				return this[index];
			}
			set
			{
				throw new NotSupportedException("Immutable Lists cannot be edited.");
			}
		}

		T IList<T>.this[int index]
		{
			get
			{
				return innerList[index];
			}
			set
			{
				throw new NotSupportedException("Immutable Lists cannot be edited.");
			}
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public T this[int index] => innerList[index];

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public ImmutableList(IList<T> innerList)
		{
			if (innerList == null)
			{
				throw new ArgumentNullException("innerList");
			}
			this.innerList = innerList;
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public bool Contains(T item)
		{
			return innerList.Contains(item);
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public void CopyTo(T[] array, int arrayIndex)
		{
			innerList.CopyTo(array, arrayIndex);
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public IEnumerator<T> GetEnumerator()
		{
			return innerList.GetEnumerator();
		}

		void ICollection.CopyTo(Array array, int index)
		{
			innerList.CopyTo((T[])array, index);
		}

		void ICollection<T>.Add(T item)
		{
			throw new NotSupportedException("Immutable Lists cannot be edited.");
		}

		void ICollection<T>.Clear()
		{
			throw new NotSupportedException("Immutable Lists cannot be edited.");
		}

		bool ICollection<T>.Remove(T item)
		{
			throw new NotSupportedException("Immutable Lists cannot be edited.");
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		int IList.Add(object value)
		{
			throw new NotSupportedException("Immutable Lists cannot be edited.");
		}

		void IList.Clear()
		{
			throw new NotSupportedException("Immutable Lists cannot be edited.");
		}

		bool IList.Contains(object value)
		{
			return innerList.Contains((T)value);
		}

		int IList.IndexOf(object value)
		{
			return innerList.IndexOf((T)value);
		}

		void IList.Insert(int index, object value)
		{
			throw new NotSupportedException("Immutable Lists cannot be edited.");
		}

		void IList.Remove(object value)
		{
			throw new NotSupportedException("Immutable Lists cannot be edited.");
		}

		void IList<T>.Insert(int index, T item)
		{
			throw new NotSupportedException("Immutable Lists cannot be edited.");
		}

		void IList.RemoveAt(int index)
		{
			throw new NotSupportedException("Immutable Lists cannot be edited.");
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public int IndexOf(T item)
		{
			return innerList.IndexOf(item);
		}

		void IList<T>.RemoveAt(int index)
		{
			throw new NotSupportedException("Immutable Lists cannot be edited.");
		}
	}
	/// <summary>
	/// Immutable list wraps another list, and allows for reading the inner list, without the ability to change it.
	/// </summary>
	[Serializable]
	public sealed class ImmutableList<TList, TElement> : IImmutableList<TElement>, IImmutableList, IList, ICollection, IEnumerable, IList<TElement>, ICollection<TElement>, IEnumerable<TElement> where TList : IList<TElement>
	{
		private TList innerList;

		/// <summary>
		/// Number of items in the list.
		/// </summary>
		public int Count => innerList.Count;

		bool ICollection.IsSynchronized => false;

		object ICollection.SyncRoot => null;

		bool IList.IsFixedSize => true;

		bool IList.IsReadOnly => true;

		/// <summary>
		/// Immutable list are always readonly.
		/// </summary>
		public bool IsReadOnly => true;

		object IList.this[int index]
		{
			get
			{
				return this[index];
			}
			set
			{
				throw new NotSupportedException("Immutable Lists cannot be edited.");
			}
		}

		TElement IList<TElement>.this[int index]
		{
			get
			{
				return innerList[index];
			}
			set
			{
				throw new NotSupportedException("Immutable Lists cannot be edited.");
			}
		}

		/// <summary>
		/// Index accessor.
		/// </summary>
		/// <param name="index">Index.</param>
		public TElement this[int index] => innerList[index];

		/// <summary>
		/// Creates an immutable list around another list.
		/// </summary>
		public ImmutableList(TList innerList)
		{
			if (innerList == null)
			{
				throw new ArgumentNullException("innerList");
			}
			this.innerList = innerList;
		}

		/// <summary>
		/// Returns <c>true</c> if the item is contained in the list.
		/// </summary>
		public bool Contains(TElement item)
		{
			return innerList.Contains(item);
		}

		/// <summary>
		/// Copies the list to an array.
		/// </summary>
		public void CopyTo(TElement[] array, int arrayIndex)
		{
			innerList.CopyTo(array, arrayIndex);
		}

		/// <summary>
		/// Gets an enumerator.
		/// </summary>
		public IEnumerator<TElement> GetEnumerator()
		{
			return innerList.GetEnumerator();
		}

		void ICollection.CopyTo(Array array, int index)
		{
			ref TList reference = ref innerList;
			TElement[] array2 = (TElement[])array;
			reference.CopyTo(array2, index);
		}

		void ICollection<TElement>.Add(TElement item)
		{
			throw new NotSupportedException("Immutable Lists cannot be edited.");
		}

		void ICollection<TElement>.Clear()
		{
			throw new NotSupportedException("Immutable Lists cannot be edited.");
		}

		bool ICollection<TElement>.Remove(TElement item)
		{
			throw new NotSupportedException("Immutable Lists cannot be edited.");
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		int IList.Add(object value)
		{
			throw new NotSupportedException("Immutable Lists cannot be edited.");
		}

		void IList.Clear()
		{
			throw new NotSupportedException("Immutable Lists cannot be edited.");
		}

		bool IList.Contains(object value)
		{
			ref TList reference = ref innerList;
			TElement item = (TElement)value;
			return reference.Contains(item);
		}

		int IList.IndexOf(object value)
		{
			ref TList reference = ref innerList;
			TElement item = (TElement)value;
			return reference.IndexOf(item);
		}

		void IList.Insert(int index, object value)
		{
			throw new NotSupportedException("Immutable Lists cannot be edited.");
		}

		void IList.Remove(object value)
		{
			throw new NotSupportedException("Immutable Lists cannot be edited.");
		}

		void IList<TElement>.Insert(int index, TElement item)
		{
			throw new NotSupportedException("Immutable Lists cannot be edited.");
		}

		void IList.RemoveAt(int index)
		{
			throw new NotSupportedException("Immutable Lists cannot be edited.");
		}

		/// <summary>
		/// Gets the index of an item.
		/// </summary>
		public int IndexOf(TElement item)
		{
			return innerList.IndexOf(item);
		}

		void IList<TElement>.RemoveAt(int index)
		{
			throw new NotSupportedException("Immutable Lists cannot be edited.");
		}
	}
}
