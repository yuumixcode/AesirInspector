using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.Utilities;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// <para>Represents an alias for a strongly typed collection of values for a <see cref="T:Sirenix.OdinInspector.Editor.PropertyValueEntry`1" /> - one value per selected inspector target.</para>
	/// <para>This class ensures that polymorphism works in the inspector, and can be strongly typed in applicable cases.</para>
	/// </summary>
	/// <typeparam name="TActualValue">The type of the aliased collection.</typeparam>
	/// <typeparam name="TValue">The polymorphic type of this collection, which is assignable to <see cref="!:TActualValue" />.</typeparam>
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.PropertyValueCollection" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.IPropertyValueCollection`1" />
	public sealed class PropertyValueCollectionAlias<TActualValue, TValue> : PropertyValueCollection, IPropertyValueCollection<TValue>, IPropertyValueCollection, IList, ICollection, IEnumerable, IList<TValue>, ICollection<TValue>, IEnumerable<TValue>
	{
		private class OriginalValuesAlias : IImmutableList<TValue>, IImmutableList, IList, ICollection, IEnumerable, IList<TValue>, ICollection<TValue>, IEnumerable<TValue>
		{
			private IImmutableList<TActualValue> aliased;

			TValue IImmutableList<TValue>.this[int index]
			{
				get
				{
					throw new NotImplementedException();
				}
			}

			object IList.this[int index]
			{
				get
				{
					return aliased[index];
				}
				set
				{
					throw new NotSupportedException();
				}
			}

			TValue IList<TValue>.this[int index]
			{
				get
				{
					TActualValue val = aliased[index];
					if (val is TValue)
					{
						object obj = val;
						return (TValue)((obj is TValue) ? obj : null);
					}
					throw new InvalidCastException();
				}
				set
				{
					throw new NotSupportedException();
				}
			}

			bool IList.IsFixedSize => aliased.IsFixedSize;

			bool IList.IsReadOnly => ((IList)aliased).IsReadOnly;

			bool ICollection<TValue>.IsReadOnly => ((IList)aliased).IsReadOnly;

			int ICollection.Count => ((ICollection)aliased).Count;

			int ICollection<TValue>.Count => ((ICollection)aliased).Count;

			bool ICollection.IsSynchronized => aliased.IsSynchronized;

			object ICollection.SyncRoot => aliased.SyncRoot;

			public OriginalValuesAlias(IImmutableList<TActualValue> aliased)
			{
				this.aliased = aliased;
			}

			int IList.Add(object value)
			{
				return aliased.Add(value);
			}

			void ICollection<TValue>.Add(TValue item)
			{
				aliased.Add(item);
			}

			void IList.Clear()
			{
				((IList)aliased).Clear();
			}

			void ICollection<TValue>.Clear()
			{
				((IList)aliased).Clear();
			}

			bool IList.Contains(object value)
			{
				return aliased.Contains(value);
			}

			bool ICollection<TValue>.Contains(TValue item)
			{
				return (aliased as ICollection<TValue>).Contains(item);
			}

			void ICollection.CopyTo(Array array, int index)
			{
				aliased.CopyTo(array, index);
			}

			void ICollection<TValue>.CopyTo(TValue[] array, int arrayIndex)
			{
				aliased.CopyTo(array, arrayIndex);
			}

			IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
			{
				return (aliased as IEnumerable<TValue>).GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return ((IEnumerable)aliased).GetEnumerator();
			}

			int IList.IndexOf(object value)
			{
				return aliased.IndexOf(value);
			}

			int IList<TValue>.IndexOf(TValue item)
			{
				return aliased.IndexOf(item);
			}

			void IList.Insert(int index, object value)
			{
				aliased.Insert(index, value);
			}

			void IList<TValue>.Insert(int index, TValue item)
			{
				aliased.Insert(index, item);
			}

			void IList.Remove(object value)
			{
				aliased.Remove(value);
			}

			bool ICollection<TValue>.Remove(TValue item)
			{
				if (!(item is TActualValue val))
				{
					throw new InvalidCastException();
				}
				return aliased.Remove(val);
			}

			void IList.RemoveAt(int index)
			{
				((IList)aliased).RemoveAt(index);
			}

			void IList<TValue>.RemoveAt(int index)
			{
				(aliased as IList<TValue>).RemoveAt(index);
			}
		}

		private static readonly bool IsMarkedAtomic = typeof(TValue).IsMarkedAtomic();

		private static readonly IAtomHandler<TValue> AtomHandler = (IsMarkedAtomic ? AtomHandlerLocator.GetAtomHandler<TValue>() : null);

		private static readonly bool IsValueType = typeof(TValue).IsValueType;

		private IPropertyValueCollection<TActualValue> aliased;

		private OriginalValuesAlias originalValuesAlias;

		private TValue[] atomicValues;

		private TValue[] originalAtomicValues;

		/// <summary>
		/// Whether the values have been changed since <see cref="M:Sirenix.OdinInspector.Editor.PropertyValueCollectionAlias`2.MarkClean" /> was last called.
		/// </summary>
		public override bool AreDirty => aliased.AreDirty;

		/// <summary>
		/// The number of values in the collection.
		/// </summary>
		public override int Count => aliased.Count;

		/// <summary>
		/// Gets a value indicating whether this instance is synchronized.
		/// </summary>
		/// <value>
		///   <c>true</c> if this instance is synchronized; otherwise, <c>false</c>.
		/// </value>
		protected override bool IsSynchronized => aliased.IsSynchronized;

		/// <summary>
		/// Gets the synchronization root object.
		/// </summary>
		/// <value>
		/// The synchronization root object.
		/// </value>
		protected override object SyncRoot => aliased.SyncRoot;

		int ICollection<TValue>.Count => aliased.Count;

		bool ICollection<TValue>.IsReadOnly => false;

		/// <summary>
		/// The original values of the (loosely typed) value collection, such as they were immediately after the last <see cref="M:Sirenix.OdinInspector.Editor.PropertyValueEntry.Update" /> call.
		/// </summary>
		protected override IImmutableList WeakOriginal => aliased.Original;

		/// <summary>
		/// The original values of the value collection, such as they were immediately after the last <see cref="M:Sirenix.OdinInspector.Editor.PropertyValueEntry.Update" /> call.
		/// </summary>
		public IImmutableList<TValue> Original => originalValuesAlias;

		/// <summary>
		/// Gets or sets the <see cref="!:TValue" /> at the specified index.
		/// </summary>
		/// <value>
		/// The <see cref="!:TValue" />.
		/// </value>
		/// <param name="index">The index.</param>
		/// <returns></returns>
		public new TValue this[int index]
		{
			get
			{
				try
				{
					TActualValue val = aliased[index];
					if (val is TValue val2)
					{
						return val2;
					}
					throw new InvalidCastException();
				}
				catch
				{
					return default(TValue);
				}
			}
			set
			{
				if (IsMarkedAtomic)
				{
					if (!AtomHandler.Compare(value, atomicValues[index]))
					{
						AtomHandler.Copy(ref value, ref atomicValues[index]);
						if (IsValueType)
						{
							aliased.ForceSetValue(index, value);
						}
						aliased.ForceMarkDirty();
					}
				}
				else
				{
					if (!(value is TActualValue val))
					{
						string fromType = value?.GetType().GetNiceFullName() ?? "null";
						string toType = typeof(TActualValue).GetNiceFullName();
						throw new InvalidCastException("Could not cast from " + fromType + " to " + toType);
					}
					aliased[index] = val;
				}
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Sirenix.OdinInspector.Editor.PropertyValueCollectionAlias`2" /> class.
		/// </summary>
		/// <param name="property">The property.</param>
		/// <param name="aliasedCollection">The aliased collection.</param>
		/// <param name="atomArray">Not yet documented.</param>
		/// <param name="originalAtomArray">Not yet documented.</param>
		/// <exception cref="T:System.ArgumentException">aliasedCollection</exception>
		public PropertyValueCollectionAlias(InspectorProperty property, IPropertyValueCollection<TActualValue> aliasedCollection, TValue[] atomArray, TValue[] originalAtomArray)
			: base(property)
		{
			_ = IsMarkedAtomic;
			aliased = aliasedCollection;
			originalValuesAlias = new OriginalValuesAlias(aliased.Original);
			atomicValues = atomArray;
			originalAtomicValues = originalAtomArray;
		}

		/// <summary>
		/// Gets an enumerator for the collection.
		/// </summary>
		/// <returns></returns>
		public override IEnumerator GetEnumerator()
		{
			return aliased.GetEnumerator();
		}

		/// <summary>
		/// Marks the value collection as being clean again. This is typically called at the end of the current GUI frame, during <see cref="M:Sirenix.OdinInspector.Editor.PropertyValueEntry.ApplyChanges" />.
		/// </summary>
		public override void MarkClean()
		{
			aliased.MarkClean();
		}

		/// <summary>
		/// Reverts the value collection to its origin values (found in <see cref="P:Sirenix.OdinInspector.Editor.PropertyValueCollectionAlias`2.Original" />) from the last <see cref="M:Sirenix.OdinInspector.Editor.PropertyValueEntry.Update" /> call, and marks the value collection as being clean again.
		/// </summary>
		public override void RevertUnappliedValues()
		{
			aliased.RevertUnappliedValues();
			if (!IsMarkedAtomic)
			{
				return;
			}
			for (int i = 0; i < aliased.Count; i++)
			{
				TActualValue val = aliased[i];
				if (!(val is TValue value))
				{
					throw new InvalidCastException();
				}
				AtomHandler.Copy(ref originalAtomicValues[i], ref atomicValues[i]);
				AtomHandler.Copy(ref originalAtomicValues[i], ref value);
				if (IsValueType)
				{
					aliased.ForceSetValue(i, value);
				}
			}
			aliased.MarkClean();
		}

		/// <summary>
		/// Determines whether the collection contains the specified value.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>
		///   <c>true</c> if the collection contains the specified value; otherwise, <c>false</c>.
		/// </returns>
		protected override bool Contains(object value)
		{
			return aliased.Contains(value);
		}

		/// <summary>
		/// Gets the index of the given value, or -1 if the value was not found.
		/// </summary>
		/// <param name="value">The value to get the index of.</param>
		/// <returns>
		/// The index of the given value, or -1 if the value was not found.
		/// </returns>
		protected override int IndexOf(object value)
		{
			return aliased.IndexOf(value);
		}

		/// <summary>
		/// Copies the collection to an array.
		/// </summary>
		/// <param name="array">The array to copy to.</param>
		/// <param name="index">The index to copy from.</param>
		protected override void CopyTo(Array array, int index)
		{
			aliased.CopyTo(array, index);
		}

		/// <summary>
		/// Gets the weakly typed value at the given index.
		/// </summary>
		/// <param name="index">The index of the value to get.</param>
		/// <returns>
		/// The weakly typed value at the given index
		/// </returns>
		protected override object GetWeakValue(int index)
		{
			return aliased[index];
		}

		/// <summary>
		/// Sets the weakly typed value at the given index.
		/// </summary>
		/// <param name="index">The index to set the value of.</param>
		/// <param name="value">The value to set.</param>
		protected override void SetWeakValue(int index, object value)
		{
			aliased[index] = (TActualValue)value;
		}

		int IList<TValue>.IndexOf(TValue item)
		{
			throw new NotImplementedException();
		}

		void IList<TValue>.Insert(int index, TValue item)
		{
			throw new NotImplementedException();
		}

		void IList<TValue>.RemoveAt(int index)
		{
			throw new NotImplementedException();
		}

		void ICollection<TValue>.Add(TValue item)
		{
			throw new NotImplementedException();
		}

		void ICollection<TValue>.Clear()
		{
			throw new NotImplementedException();
		}

		bool ICollection<TValue>.Contains(TValue item)
		{
			throw new NotImplementedException();
		}

		void ICollection<TValue>.CopyTo(TValue[] array, int arrayIndex)
		{
			throw new NotImplementedException();
		}

		bool ICollection<TValue>.Remove(TValue item)
		{
			throw new NotImplementedException();
		}

		IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// <para>Force sets the value, ignoring whether it is editable or not.</para>
		/// <para>Note that this will fail on list element value entries where <see cref="M:Sirenix.OdinInspector.Editor.IPropertyValueEntry.ListIsReadOnly" /> is true on the parent value entry.</para>
		/// </summary>
		/// <param name="index">The selection index of the value.</param>
		/// <param name="value">The value to be set.</param>
		public void ForceSetValue(int index, TValue value)
		{
			aliased.ForceSetValue(index, value);
		}

		/// <summary>
		/// <para>Force sets the value, ignoring whether it is editable or not.</para>
		/// <para>Note that this will fail on list element value entries where <see cref="!:IPropertyValueEntry.ListIsReadOnly" /> is true on the parent value entry.</para>
		/// </summary>
		/// <param name="index">The selection index of the value.</param>
		/// <param name="value">The value to be set.</param>
		public override void ForceSetValue(int index, object value)
		{
			aliased.ForceSetValue(index, (TActualValue)value);
		}

		/// <summary>
		/// Marks the value collection as being dirty, regardless of any value changes.
		/// </summary>
		public override void ForceMarkDirty()
		{
			aliased.ForceMarkDirty();
		}
	}
}
