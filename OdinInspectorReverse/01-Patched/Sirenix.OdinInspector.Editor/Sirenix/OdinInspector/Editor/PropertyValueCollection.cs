using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.Utilities;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// Represents a weakly typed collection of values for a <see cref="T:Sirenix.OdinInspector.Editor.PropertyValueEntry" /> - one value per selected inspector target.
	/// </summary>
	public abstract class PropertyValueCollection : IPropertyValueCollection, IList, ICollection, IEnumerable
	{
		/// <summary>
		/// The property whose values are represented.
		/// </summary>
		protected readonly InspectorProperty Property;

		/// <summary>
		/// Whether the values have been changed since <see cref="M:Sirenix.OdinInspector.Editor.PropertyValueCollection.MarkClean" /> was last called.
		/// </summary>
		public abstract bool AreDirty { get; }

		/// <summary>
		/// The number of values in the collection.
		/// </summary>
		public abstract int Count { get; }

		bool ICollection.IsSynchronized => IsSynchronized;

		object ICollection.SyncRoot => SyncRoot;

		bool IList.IsFixedSize => true;

		bool IList.IsReadOnly => false;

		/// <summary>
		/// Gets a value indicating whether this instance is synchronized.
		/// </summary>
		/// <value>
		///   <c>true</c> if this instance is synchronized; otherwise, <c>false</c>.
		/// </value>
		protected abstract bool IsSynchronized { get; }

		/// <summary>
		/// Gets the synchronization root object.
		/// </summary>
		/// <value>
		/// The synchronization root object.
		/// </value>
		protected abstract object SyncRoot { get; }

		IImmutableList IPropertyValueCollection.Original => WeakOriginal;

		/// <summary>
		/// The original values of the (loosely typed) value collection, such as they were immediately after the last <see cref="M:Sirenix.OdinInspector.Editor.PropertyValueEntry.Update" /> call.
		/// </summary>
		protected abstract IImmutableList WeakOriginal { get; }

		/// <summary>
		/// Gets or sets the weakly typed <see cref="T:System.Object" /> at the specified index.
		/// </summary>
		/// <value>
		/// The <see cref="T:System.Object" /> value.
		/// </value>
		/// <param name="index">The index to set.</param>
		public object this[int index]
		{
			get
			{
				return GetWeakValue(index);
			}
			set
			{
				SetWeakValue(index, value);
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Sirenix.OdinInspector.Editor.PropertyValueCollection" /> class.
		/// </summary>
		/// <param name="property">The property to represent.</param>
		/// <exception cref="T:System.ArgumentNullException">property is null</exception>
		protected PropertyValueCollection(InspectorProperty property)
		{
			if (property == null)
			{
				throw new ArgumentNullException("property");
			}
			Property = property;
		}

		/// <summary>
		/// Gets an enumerator for the collection.
		/// </summary>
		public abstract IEnumerator GetEnumerator();

		void ICollection.CopyTo(Array array, int index)
		{
			CopyTo(array, index);
		}

		int IList.Add(object value)
		{
			throw new InvalidOperationException("Cannot add elements to a property value collection.");
		}

		void IList.Clear()
		{
			throw new InvalidOperationException("Cannot remove elements from a property value collection.");
		}

		bool IList.Contains(object value)
		{
			return Contains(value);
		}

		int IList.IndexOf(object value)
		{
			return IndexOf(value);
		}

		void IList.Insert(int index, object value)
		{
			throw new InvalidOperationException("Cannot add elements to a property value collection.");
		}

		void IList.Remove(object value)
		{
			throw new InvalidOperationException("Cannot remove elements from a property value collection.");
		}

		void IList.RemoveAt(int index)
		{
			throw new InvalidOperationException("Cannot remove elements from a property value collection.");
		}

		/// <summary>
		/// Marks the value collection as being clean again. This is typically called at the end of the current GUI frame, during <see cref="M:Sirenix.OdinInspector.Editor.PropertyValueEntry.ApplyChanges" />.
		/// </summary>
		public abstract void MarkClean();

		/// <summary>
		/// Reverts the value collection to its origin values (found in <see cref="!:Original" />) from the last <see cref="M:Sirenix.OdinInspector.Editor.PropertyValueEntry.Update" /> call, and marks the value collection as being clean again.
		/// </summary>
		public abstract void RevertUnappliedValues();

		/// <summary>
		/// Determines whether the collection contains the specified value.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>
		///   <c>true</c> if the collection contains the specified value; otherwise, <c>false</c>.
		/// </returns>
		protected abstract bool Contains(object value);

		/// <summary>
		/// Copies the collection to an array.
		/// </summary>
		/// <param name="array">The array to copy to.</param>
		/// <param name="index">The index to copy from.</param>
		protected abstract void CopyTo(Array array, int index);

		/// <summary>
		/// Gets the index of the given value, or -1 if the value was not found.
		/// </summary>
		/// <param name="value">The value to get the index of.</param>
		/// <returns>The index of the given value, or -1 if the value was not found.</returns>
		protected abstract int IndexOf(object value);

		/// <summary>
		/// Gets the weakly typed value at the given index.
		/// </summary>
		/// <param name="index">The index of the value to get.</param>
		/// <returns>The weakly typed value at the given index</returns>
		protected abstract object GetWeakValue(int index);

		/// <summary>
		/// Sets the weakly typed value at the given index.
		/// </summary>
		/// <param name="index">The index to set the value of.</param>
		/// <param name="value">The value to set.</param>
		protected abstract void SetWeakValue(int index, object value);

		/// <summary>
		/// <para>Force sets the value, ignoring whether it is editable or not.</para>
		/// <para>Note that this will fail on list element value entries where <see cref="!:IPropertyValueEntry.ListIsReadOnly" /> is true on the parent value entry.</para>
		/// </summary>
		/// <param name="index">The selection index of the value.</param>
		/// <param name="value">The value to be set.</param>
		public abstract void ForceSetValue(int index, object value);

		/// <summary>
		/// Marks the value collection as being dirty, regardless of any value changes.
		/// </summary>
		public abstract void ForceMarkDirty();
	}
	/// <summary>
	/// Represents a strongly typed collection of values for a <see cref="T:Sirenix.OdinInspector.Editor.PropertyValueEntry`1" /> - one value per selected inspector target.
	/// </summary>
	/// <typeparam name="TValue">The element type of the collection.</typeparam>
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.IPropertyValueCollection" />
	public sealed class PropertyValueCollection<TValue> : PropertyValueCollection, IPropertyValueCollection<TValue>, IPropertyValueCollection, IList, ICollection, IEnumerable, IList<TValue>, ICollection<TValue>, IEnumerable<TValue>
	{
		private static readonly Func<TValue, TValue, bool> EqualityComparer = TypeExtensions.GetEqualityComparerDelegate<TValue>();

		private static readonly bool IsMarkedAtomic = typeof(TValue).IsMarkedAtomic();

		private static readonly IAtomHandler<TValue> AtomHandler = (IsMarkedAtomic ? AtomHandlerLocator.GetAtomHandler<TValue>() : null);

		private bool areDirty;

		private TValue[] values;

		private TValue[] originalValues;

		private TValue[] atomValues;

		private TValue[] originalAtomValues;

		private IImmutableList<TValue> originalValuesImmutable;

		/// <summary>
		/// Whether the values have been changed since <see cref="M:Sirenix.OdinInspector.Editor.PropertyValueCollection`1.MarkClean" /> was last called.
		/// </summary>
		public override bool AreDirty => areDirty;

		/// <summary>
		/// The number of values in the collection.
		/// </summary>
		public override int Count => values.Length;

		/// <summary>
		/// Gets a value indicating whether this instance is synchronized.
		/// </summary>
		/// <value>
		///   <c>true</c> if this instance is synchronized; otherwise, <c>false</c>.
		/// </value>
		protected override bool IsSynchronized => values.IsSynchronized;

		/// <summary>
		/// Gets the synchronization root object.
		/// </summary>
		/// <value>
		/// The synchronization root object.
		/// </value>
		protected override object SyncRoot => values.SyncRoot;

		bool ICollection<TValue>.IsReadOnly => false;

		/// <summary>
		/// The original values of the (loosely typed) value collection, such as they were immediately after the last <see cref="M:Sirenix.OdinInspector.Editor.PropertyValueEntry.Update" /> call.
		/// </summary>
		protected override IImmutableList WeakOriginal => originalValuesImmutable;

		IImmutableList<TValue> IPropertyValueCollection<TValue>.Original => originalValuesImmutable;

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
				return values[index];
			}
			set
			{
				if (IsMarkedAtomic)
				{
					if (!AtomHandler.Compare(value, atomValues[index]))
					{
						if (!Property.ValueEntry.IsEditable)
						{
							Debug.LogWarning("Cannot edit values on the property " + Property.Name + " with path " + Property.Path + " of type " + Property.ValueEntry.TypeOfValue.GetNiceName() + ".");
						}
						else
						{
							MarkDirty();
							AtomHandler.Copy(ref value, ref atomValues[index]);
							values[index] = value;
						}
					}
				}
				else if (!EqualityComparer(value, originalValues[index]))
				{
					if (!Property.ValueEntry.IsEditable)
					{
						Debug.LogWarning("Cannot edit values on the property " + Property.Name + " with path " + Property.Path + " of type " + Property.ValueEntry.TypeOfValue.GetNiceName() + ".");
					}
					else
					{
						MarkDirty();
						values[index] = value;
					}
				}
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Sirenix.OdinInspector.Editor.PropertyValueCollection`1" /> class.
		/// </summary>
		/// <param name="property">The property.</param>
		/// <param name="internalArray">The internal array.</param>
		/// <param name="originalArray">The original array.</param>
		/// <param name="atomArray">The internal atom array.</param>
		/// <param name="originalAtomArray">The original atom array.</param>
		public PropertyValueCollection(InspectorProperty property, TValue[] internalArray, TValue[] originalArray, TValue[] atomArray, TValue[] originalAtomArray)
			: base(property)
		{
			_ = IsMarkedAtomic;
			values = internalArray;
			originalValues = originalArray;
			originalValuesImmutable = new ImmutableList<TValue>(originalValues);
			atomValues = atomArray;
			originalAtomValues = originalAtomArray;
		}

		/// <summary>
		/// Gets an enumerator for the collection.
		/// </summary>
		public override IEnumerator GetEnumerator()
		{
			return values.GetEnumerator();
		}

		/// <summary>
		/// Marks the value collection as being clean again. This is typically called at the end of the current GUI frame, during <see cref="M:Sirenix.OdinInspector.Editor.PropertyValueEntry.ApplyChanges" />.
		/// </summary>
		public override void MarkClean()
		{
			areDirty = false;
		}

		/// <summary>
		/// Reverts the value collection to its origin values (found in <see cref="!:Original" />) from the last <see cref="M:Sirenix.OdinInspector.Editor.PropertyValueEntry.Update" /> call, and marks the value collection as being clean again.
		/// </summary>
		public override void RevertUnappliedValues()
		{
			if (IsMarkedAtomic)
			{
				for (int i = 0; i < values.Length; i++)
				{
					AtomHandler.Copy(ref originalAtomValues[i], ref originalValues[i]);
					AtomHandler.Copy(ref originalAtomValues[i], ref atomValues[i]);
				}
			}
			for (int j = 0; j < values.Length; j++)
			{
				values[j] = originalValues[j];
			}
			areDirty = false;
		}

		/// <summary>
		/// Copies the collection to an array.
		/// </summary>
		/// <param name="array">The array to copy to.</param>
		/// <param name="index">The index to copy from.</param>
		protected override void CopyTo(Array array, int index)
		{
			values.CopyTo(array, index);
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
			return this[index];
		}

		/// <summary>
		/// Sets the weakly typed value at the given index.
		/// </summary>
		/// <param name="index">The index to set the value of.</param>
		/// <param name="value">The value to set.</param>
		protected override void SetWeakValue(int index, object value)
		{
			TValue strongVal;
			try
			{
				strongVal = (TValue)value;
			}
			catch (InvalidCastException innerException)
			{
				if (value != null)
				{
					throw new InvalidCastException("Cannot cast type '" + value.GetType().GetNiceName() + "' to expected type '" + typeof(TValue).GetNiceName() + "'", innerException);
				}
				strongVal = default(TValue);
			}
			this[index] = strongVal;
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
			TValue tValue = (TValue)value;
			for (int i = 0; i < Count; i++)
			{
				if (EqualityComparer(values[i], tValue))
				{
					return true;
				}
			}
			return false;
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
			return ((IList<TValue>)values).IndexOf((TValue)value);
		}

		int IList<TValue>.IndexOf(TValue item)
		{
			return ((IList<TValue>)values).IndexOf(item);
		}

		void IList<TValue>.Insert(int index, TValue item)
		{
			throw new InvalidOperationException("Cannot add elements to a property value collection.");
		}

		void IList<TValue>.RemoveAt(int index)
		{
			throw new InvalidOperationException("Cannot remove elements from a property value collection.");
		}

		void ICollection<TValue>.Add(TValue item)
		{
			throw new InvalidOperationException("Cannot add elements to a property value collection.");
		}

		void ICollection<TValue>.Clear()
		{
			throw new InvalidOperationException("Cannot remove elements from a property value collection.");
		}

		bool ICollection<TValue>.Contains(TValue item)
		{
			return ((ICollection<TValue>)values).Contains(item);
		}

		void ICollection<TValue>.CopyTo(TValue[] array, int arrayIndex)
		{
			((ICollection<TValue>)values).CopyTo(array, arrayIndex);
		}

		bool ICollection<TValue>.Remove(TValue item)
		{
			throw new InvalidOperationException("Cannot remove elements from a property value collection.");
		}

		IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
		{
			return ((IEnumerable<TValue>)values).GetEnumerator();
		}

		/// <summary>
		/// <para>Force sets the value, ignoring whether it is editable or not.</para>
		/// <para>Note that this will fail on list element value entries where <see cref="!:IPropertyValueEntry.ListIsReadOnly" /> is true on the parent value entry.</para>
		/// </summary>
		/// <param name="index">The selection index of the value.</param>
		/// <param name="value">The value to be set.</param>
		public override void ForceSetValue(int index, object value)
		{
			ForceSetValue(index, (TValue)value);
		}

		/// <summary>
		/// <para>Force sets the value, ignoring whether it is editable or not.</para>
		/// <para>Note that this will fail on list element value entries where <see cref="!:IPropertyValueEntry.ListIsReadOnly" /> is true on the parent value entry.</para>
		/// </summary>
		/// <param name="index">The selection index of the value.</param>
		/// <param name="value">The value to be set.</param>
		/// <exception cref="T:System.NotImplementedException"></exception>
		public void ForceSetValue(int index, TValue value)
		{
			MarkDirty();
			values[index] = value;
		}

		/// <summary>
		/// Marks the value collection as being dirty, regardless of any value changes.
		/// </summary>
		public override void ForceMarkDirty()
		{
			MarkDirty();
		}

		private void MarkDirty()
		{
			if (!areDirty)
			{
				areDirty = true;
				Property.Tree.RegisterPropertyDirty(Property);
			}
		}
	}
}
