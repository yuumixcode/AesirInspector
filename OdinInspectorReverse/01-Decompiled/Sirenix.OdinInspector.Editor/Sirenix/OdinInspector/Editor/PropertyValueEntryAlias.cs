using System;
using Sirenix.Utilities;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// A polymorphic alias for a <see cref="T:Sirenix.OdinInspector.Editor.PropertyValueEntry" /> instance, used to implement strongly typed polymorphism in Odin.
	/// </summary>
	public abstract class PropertyValueEntryAlias : IPropertyValueEntry, IDisposable, IValueEntryActualValueSetter
	{
		/// <summary>
		/// The number of parallel values this entry represents. This will always be exactly equal to the count of <see cref="P:Sirenix.OdinInspector.Editor.PropertyTree.WeakTargets" />.
		/// </summary>
		public abstract int ValueCount { get; }

		/// <summary>
		/// Whether this value entry is editable or not.
		/// </summary>
		public abstract bool IsEditable { get; }

		/// <summary>
		/// If this value entry has the override type <see cref="F:Sirenix.OdinInspector.Editor.PropertyValueState.Reference" />, this is the path of the property it references.
		/// </summary>
		public abstract string TargetReferencePath { get; }

		/// <summary>
		/// <para>The actual serialization backend for this value entry, possibly inherited from the serialization backend of the root property this entry is a child of.</para>
		/// <para>Note that this is *not* always equal to <see cref="P:Sirenix.OdinInspector.Editor.InspectorPropertyInfo.SerializationBackend" />.</para>
		/// </summary>
		public abstract SerializationBackend SerializationBackend { get; }

		/// <summary>
		/// The property whose values this value entry represents.
		/// </summary>
		public abstract InspectorProperty Property { get; }

		/// <summary>
		/// Provides access to the weakly typed values of this value entry.
		/// </summary>
		public abstract IPropertyValueCollection WeakValues { get; }

		/// <summary>
		/// Whether this value entry has been changed from its prefab counterpart.
		/// </summary>
		public abstract bool ValueChangedFromPrefab { get; }

		/// <summary>
		/// Whether a child of this value entry has been changed from its prefab counterpart.
		/// </summary>
		public abstract bool ChildValueChangedFromPrefab { get; }

		/// <summary>
		/// Whether this value entry has had its list length changed from its prefab counterpart.
		/// </summary>
		public abstract bool ListLengthChangedFromPrefab { get; }

		/// <summary>
		/// Whether this value entry has had its dictionary values changes from its prefab counterpart.
		/// </summary>
		public abstract bool DictionaryChangedFromPrefab { get; }

		/// <summary>
		/// <para>A weakly typed smart value that represents the first element of the value entry's value collection, but has "smart logic" for setting the value that detects relevant changes and applies them in parallel.</para>
		/// <para>This lets you often just use the smart value instead of having to deal with the tedium of multiple parallel values.</para>
		/// </summary>
		public abstract object WeakSmartValue { get; set; }

		/// <summary>
		/// The type from which this value entry comes. If this value entry represents a member value, this is the declaring type of the member. If the value entry represents a collection element, this is the type of the collection.
		/// </summary>
		public abstract Type ParentType { get; }

		/// <summary>
		/// The most precise known contained type of the value entry. If polymorphism is in effect, this will be some type derived from <see cref="P:Sirenix.OdinInspector.Editor.PropertyValueEntryAlias.BaseValueType" />.
		/// </summary>
		public abstract Type TypeOfValue { get; }

		/// <summary>
		/// The base type of the value entry. If this is value entry represents a member value, this is the type of the member. If the value entry represents a collection element, this is the element type of the collection.
		/// </summary>
		public abstract Type BaseValueType { get; }

		/// <summary>
		/// The generic value type of the value entry alias itself, IE, the PropertyValueEntryAlias implements the IPropertyValueEntry&lt;TValue&gt; interface - this member returns TValue regardless of the actual type of the backing objects. This is a performance shortcut to checking TValue using reflection, used internally by the property system to determine whether a value entry alias needs to be swapped out with an alias for a different type.
		/// </summary>
		public abstract Type TValueGenericTypeArgument { get; }

		/// <summary>
		/// The special state of the value entry.
		/// </summary>
		public abstract PropertyValueState ValueState { get; }

		/// <summary>
		/// Whether this value entry is an alias, or not. Value entry aliases are used to provide strongly typed value entries in the case of polymorphism.
		/// </summary>
		public bool IsAlias => true;

		/// <summary>
		/// The context container of this property.
		/// </summary>
		public PropertyContextContainer Context => Property.Context;

		/// <summary>
		/// Whether this type is marked as an atomic type using a <see cref="T:Sirenix.OdinInspector.Editor.IAtomHandler" />.
		/// </summary>
		public abstract bool IsMarkedAtomic { get; }

		/// <summary>
		/// An event that is invoked during <see cref="M:Sirenix.OdinInspector.Editor.PropertyValueEntryAlias.ApplyChanges" />, when any values have changed.
		/// </summary>
		public abstract event Action<int> OnValueChanged;

		/// <summary>
		/// An event that is invoked during <see cref="M:Sirenix.OdinInspector.Editor.PropertyValueEntryAlias.ApplyChanges" />, when any child values have changed.
		/// </summary>
		public abstract event Action<int> OnChildValueChanged;

		/// <summary>
		/// Applies the changes made to this value entry to the target objects, and registers prefab modifications as necessary.
		/// </summary>
		/// <returns>
		/// True if any changes were made, otherwise, false.
		/// </returns>
		public abstract bool ApplyChanges();

		/// <summary>
		/// Updates the values contained in this value entry to the actual values in the target objects, and updates its state (override, type of value, etc.) accordingly.
		/// </summary>
		public abstract void Update();

		/// <summary>
		/// <para>Checks whether the values in this value entry are equal to the values in another value entry.</para>
		/// <para>Note, both value entries must have the same value type, and must represent values that are .NET value types.</para>
		/// </summary>
		public abstract bool ValueTypeValuesAreEqual(IPropertyValueEntry other);

		void IValueEntryActualValueSetter.SetActualValue(int index, object value)
		{
			SetActualValue(index, value);
		}

		/// <summary>
		/// Sets the actual value of a value entry, for a given selection index.
		/// </summary>
		protected abstract void SetActualValue(int index, object value);

		/// <summary>
		/// <para>Determines whether the value at the given selection index is different from the given prefab value, as is relevant for prefab modification checks.</para>
		/// <para>If the value is a reference type, null and type difference is checked. If value is a value type, a comparer from <see cref="M:Sirenix.Utilities.TypeExtensions.GetEqualityComparerDelegate``1" /> is used.</para>
		/// <para>This method is best ignored unless you know what you are doing.</para>
		/// </summary>
		/// <param name="value">The value to check differences against.</param>
		/// <param name="index">The selection index to compare against.</param>
		public abstract bool ValueIsPrefabDifferent(object value, int index);

		public abstract void Dispose();
	}
	/// <summary>
	/// A polymorphic alias for a <see cref="T:Sirenix.OdinInspector.Editor.PropertyValueEntry" /> instance, used to implement strongly typed polymorphism in Odin.
	/// </summary>
	public sealed class PropertyValueEntryAlias<TActualValue, TValue> : PropertyValueEntryAlias, IPropertyValueEntry<TValue>, IPropertyValueEntry, IDisposable, IValueEntryActualValueSetter<TValue>, IValueEntryActualValueSetter
	{
		private static readonly bool ValueIsMarkedAtomic = typeof(TValue).IsMarkedAtomic();

		private static readonly IAtomHandler<TValue> AtomHandler = (ValueIsMarkedAtomic ? AtomHandlerLocator.GetAtomHandler<TValue>() : null);

		private static readonly bool ValueIsPrimitive = typeof(TValue).IsPrimitive || typeof(TValue) == typeof(string) || typeof(TValue).IsEnum;

		private static readonly bool ValueIsValueType = typeof(TValue).IsValueType;

		private static readonly Func<TValue, TValue, bool> EqualityComparer = PropertyValueEntry<TValue>.EqualityComparer;

		private PropertyValueEntry<TActualValue> entry;

		private PropertyValueCollectionAlias<TActualValue, TValue> aliasValues;

		private TValue[] atomicValues;

		private TValue[] originalAtomicValues;

		/// <summary>
		/// Provides access to the strongly typed values of this value entry.
		/// </summary>
		public IPropertyValueCollection<TValue> Values => aliasValues;

		/// <summary>
		/// <para>A strongly typed smart value that represents the first element of the value entry's value collection, but has "smart logic" for setting the value that detects relevant changes and applies them in parallel.</para>
		/// <para>This lets you often just use the smart value instead of having to deal with the tedium of multiple parallel values.</para>
		/// </summary>
		public TValue SmartValue
		{
			get
			{
				TActualValue value = entry.SmartValue;
				if (value is TValue)
				{
					object obj = value;
					return (TValue)((obj is TValue) ? obj : null);
				}
				if (!ValueIsValueType && value == null)
				{
					return default(TValue);
				}
				string[] obj2 = new string[5] { "Cannot cast from actual type '", null, null, null, null };
				TActualValue smartValue = entry.SmartValue;
				obj2[1] = ((smartValue == null) ? null : smartValue.GetType()?.GetNiceFullName()) ?? "null value";
				obj2[2] = "' to expected alias type '";
				obj2[3] = typeof(TValue).GetNiceFullName();
				obj2[4] = "'.";
				throw new InvalidCastException(string.Concat(obj2));
			}
			set
			{
				if (ValueIsMarkedAtomic)
				{
					if (AtomHandler.Compare(value, atomicValues[0]))
					{
						return;
					}
					if (!IsEditable)
					{
						Debug.LogWarning("Tried to change value of non-editable property '" + Property.NiceName + "' of type '" + TypeOfValue.GetNiceName() + "' at path '" + Property.Path + "'.");
						if (!ValueIsValueType)
						{
							AtomHandler.Copy(ref atomicValues[0], ref value);
						}
					}
					else
					{
						for (int i = 0; i < ValueCount; i++)
						{
							Values[i] = value;
						}
					}
				}
				else if (ValueIsPrimitive || ValueIsValueType)
				{
					TActualValue existingValue = entry.Values[0];
					if (existingValue == null || existingValue.GetType() != typeof(TValue) || !EqualityComparer(value, (TValue)(object)existingValue))
					{
						entry.SmartValue = (TActualValue)(object)value;
					}
				}
				else
				{
					entry.SmartValue = (TActualValue)(object)value;
				}
			}
		}

		/// <summary>
		/// <para>A weakly typed smart value that represents the first element of the value entry's value collection, but has "smart logic" for setting the value that detects relevant changes and applies them in parallel.</para>
		/// <para>This lets you often just use the smart value instead of having to deal with the tedium of multiple parallel values.</para>
		/// </summary>
		public override object WeakSmartValue
		{
			get
			{
				return entry.WeakSmartValue;
			}
			set
			{
				try
				{
					SmartValue = (TValue)value;
				}
				catch (InvalidCastException)
				{
					entry.WeakSmartValue = value;
				}
				catch (NullReferenceException)
				{
					entry.WeakSmartValue = value;
				}
			}
		}

		/// <summary>
		/// The number of parallel values this entry represents. This will always be exactly equal to the count of <see cref="P:Sirenix.OdinInspector.Editor.PropertyTree.WeakTargets" />.
		/// </summary>
		public override int ValueCount => entry.ValueCount;

		/// <summary>
		/// Whether this value entry is editable or not.
		/// </summary>
		public override bool IsEditable => entry.IsEditable;

		/// <summary>
		/// If this value entry has the override type <see cref="F:Sirenix.OdinInspector.Editor.PropertyValueState.Reference" />, this is the path of the property it references.
		/// </summary>
		public override string TargetReferencePath => entry.TargetReferencePath;

		/// <summary>
		/// <para>The actual serialization backend for this value entry, possibly inherited from the serialization backend of the root property this entry is a child of.</para>
		/// <para>Note that this is *not* always equal to <see cref="P:Sirenix.OdinInspector.Editor.InspectorPropertyInfo.SerializationBackend" />.</para>
		/// </summary>
		public override SerializationBackend SerializationBackend => entry.SerializationBackend;

		/// <summary>
		/// The property whose values this value entry represents.
		/// </summary>
		public override InspectorProperty Property => entry.Property;

		/// <summary>
		/// Provides access to the weakly typed values of this value entry.
		/// </summary>
		public override IPropertyValueCollection WeakValues => entry.WeakValues;

		/// <summary>
		/// Whether this value entry has been changed from its prefab counterpart.
		/// </summary>
		public override bool ValueChangedFromPrefab => entry.ValueChangedFromPrefab;

		/// <summary>
		/// Whether a child of this value entry has been changed from its prefab counterpart.
		/// </summary>
		public override bool ChildValueChangedFromPrefab => entry.ChildValueChangedFromPrefab;

		/// <summary>
		/// Whether this value entry has had its list length changed from its prefab counterpart.
		/// </summary>
		public override bool ListLengthChangedFromPrefab => entry.ListLengthChangedFromPrefab;

		/// <summary>
		/// Whether this value entry has had its dictionary values changes from its prefab counterpart.
		/// </summary>
		public override bool DictionaryChangedFromPrefab => entry.DictionaryChangedFromPrefab;

		/// <summary>
		/// The type from which this value entry comes. If this value entry represents a member value, this is the declaring type of the member. If the value entry represents a collection element, this is the type of the collection.
		/// </summary>
		public override Type ParentType => entry.ParentType;

		/// <summary>
		/// The most precise known contained type of the value entry. If polymorphism is in effect, this will be some type derived from <see cref="P:Sirenix.OdinInspector.Editor.PropertyValueEntryAlias`2.BaseValueType" />.
		/// </summary>
		public override Type TypeOfValue => entry.TypeOfValue;

		/// <summary>
		/// The base type of the value entry. If this is value entry represents a member value, this is the type of the member. If the value entry represents a collection element, this is the element type of the collection.
		/// </summary>
		public override Type BaseValueType => entry.BaseValueType;

		/// <summary>
		/// The generic value type of the value entry alias itself, IE, the PropertyValueEntryAlias implements the IPropertyValueEntry&lt;TValue&gt; interface - this member returns TValue regardless of the actual type of the backing objects. This is a performance shortcut to checking TValue using reflection, used internally by the property system to determine whether a value entry alias needs to be swapped out with an alias for a different type.
		/// </summary>
		public override Type TValueGenericTypeArgument => typeof(TValue);

		/// <summary>
		/// The special state of the value entry.
		/// </summary>
		public override PropertyValueState ValueState => entry.ValueState;

		/// <summary>
		/// Whether this type is marked as an atomic type using a <see cref="T:Sirenix.OdinInspector.Editor.IAtomHandler" />.
		/// </summary>
		public override bool IsMarkedAtomic => ValueIsMarkedAtomic;

		/// <summary>
		/// An event that is invoked during <see cref="M:Sirenix.OdinInspector.Editor.PropertyValueEntryAlias`2.ApplyChanges" />, when any values have changed.
		/// </summary>
		public override event Action<int> OnValueChanged
		{
			add
			{
				entry.OnValueChanged += value;
			}
			remove
			{
				entry.OnValueChanged -= value;
			}
		}

		/// <summary>
		/// An event that is invoked during <see cref="M:Sirenix.OdinInspector.Editor.PropertyValueEntryAlias`2.ApplyChanges" />, when any child values have changed.
		/// </summary>
		public override event Action<int> OnChildValueChanged
		{
			add
			{
				entry.OnChildValueChanged += value;
			}
			remove
			{
				entry.OnChildValueChanged -= value;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Sirenix.OdinInspector.Editor.PropertyValueEntryAlias`2" /> class.
		/// </summary>
		/// <param name="valueEntry">The value entry to alias.</param>
		/// <exception cref="T:System.ArgumentNullException">valueEntry is null</exception>
		public PropertyValueEntryAlias(PropertyValueEntry valueEntryWeak)
		{
			if (!(valueEntryWeak is PropertyValueEntry<TActualValue> valueEntry))
			{
				throw new ArgumentNullException("valueEntry");
			}
			entry = valueEntry;
			if (ValueIsMarkedAtomic)
			{
				atomicValues = new TValue[entry.ValueCount];
				originalAtomicValues = new TValue[entry.ValueCount];
			}
			aliasValues = new PropertyValueCollectionAlias<TActualValue, TValue>(valueEntry.Property, entry.Values, atomicValues, originalAtomicValues);
		}

		/// <summary>
		/// Applies the changes made to this value entry to the target objects, and registers prefab modifications as necessary.
		/// </summary>
		/// <returns>
		/// True if any changes were made, otherwise, false.
		/// </returns>
		public override bool ApplyChanges()
		{
			return entry.ApplyChanges();
		}

		/// <summary>
		/// Updates the values contained in this value entry to the actual values in the target objects, and updates its state (override, type of value, etc.) accordingly.
		/// </summary>
		public override void Update()
		{
			entry.Update();
			if (!ValueIsMarkedAtomic)
			{
				return;
			}
			for (int i = 0; i < ValueCount; i++)
			{
				try
				{
					TActualValue val = entry.Values[i];
					if (!(val is TValue value))
					{
						throw new InvalidCastException();
					}
					AtomHandler.Copy(ref value, ref atomicValues[i]);
					AtomHandler.Copy(ref value, ref originalAtomicValues[i]);
				}
				catch (InvalidCastException)
				{
				}
			}
		}

		/// <summary>
		/// <para>Checks whether the values in this value entry are equal to the values in another value entry.</para>
		/// <para>Note, both value entries must have the same value type, and must represent values that are .NET value types.</para>
		/// </summary>
		public override bool ValueTypeValuesAreEqual(IPropertyValueEntry other)
		{
			if (!TypeOfValue.IsValueType || !other.TypeOfValue.IsValueType || other.TypeOfValue != TypeOfValue)
			{
				return false;
			}
			IPropertyValueEntry<TValue> castOther = (IPropertyValueEntry<TValue>)other;
			if (other.ValueCount == 1 || other.ValueState == PropertyValueState.None)
			{
				TValue otherValue = castOther.Values[0];
				for (int i = 0; i < ValueCount; i++)
				{
					if (!PropertyValueEntry<TValue>.EqualityComparer(Values[i], otherValue))
					{
						return false;
					}
				}
				return true;
			}
			if (ValueCount == 1 || ValueState == PropertyValueState.None)
			{
				TValue thisValue = Values[0];
				for (int j = 0; j < ValueCount; j++)
				{
					if (!PropertyValueEntry<TValue>.EqualityComparer(thisValue, castOther.Values[j]))
					{
						return false;
					}
				}
				return true;
			}
			if (ValueCount == other.ValueCount)
			{
				for (int k = 0; k < ValueCount; k++)
				{
					if (!PropertyValueEntry<TValue>.EqualityComparer(Values[k], castOther.Values[k]))
					{
						return false;
					}
				}
				return true;
			}
			return false;
		}

		/// <summary>
		/// Sets the actual value of a value entry, for a given selection index.
		/// </summary>
		protected sealed override void SetActualValue(int index, object value)
		{
			((IValueEntryActualValueSetter)entry).SetActualValue(index, value);
		}

		void IValueEntryActualValueSetter<TValue>.SetActualValue(int index, TValue value)
		{
			((IValueEntryActualValueSetter)entry).SetActualValue(index, (object)value);
		}

		/// <summary>
		/// <para>Determines whether the value at the given selection index is different from the given prefab value, as is relevant for prefab modification checks.</para>
		/// <para>If the value is a reference type, null and type difference is checked. If value is a value type, a comparer from <see cref="M:Sirenix.Utilities.TypeExtensions.GetEqualityComparerDelegate``1" /> is used.</para>
		/// <para>This method is best ignored unless you know what you are doing.</para>
		/// </summary>
		/// <param name="value">The value to check differences against.</param>
		/// <param name="index">The selection index to compare against.</param>
		public bool ValueIsPrefabDifferent(TValue value, int index)
		{
			if (ValueIsMarkedAtomic)
			{
				return !AtomHandler.Compare(value, Values[index]);
			}
			if (value == null)
			{
				return entry.ValueIsPrefabDifferent(default(TActualValue), index);
			}
			if (!(value is TActualValue val))
			{
				throw new InvalidCastException();
			}
			return entry.ValueIsPrefabDifferent(val, index);
		}

		/// <summary>
		/// <para>Determines whether the value at the given selection index is different from the given prefab value, as is relevant for prefab modification checks.</para>
		/// <para>If the value is a reference type, null and type difference is checked. If value is a value type, a comparer from <see cref="M:Sirenix.Utilities.TypeExtensions.GetEqualityComparerDelegate``1" /> is used.</para>
		/// <para>This method is best ignored unless you know what you are doing.</para>
		/// </summary>
		/// <param name="value">The value to check differences against.</param>
		/// <param name="index">The selection index to compare against.</param>
		public override bool ValueIsPrefabDifferent(object value, int index)
		{
			if (value == null)
			{
				if (ValueIsValueType)
				{
					return true;
				}
			}
			else if (ValueIsValueType)
			{
				if (typeof(TValue) != value.GetType())
				{
					return true;
				}
			}
			else if (!typeof(TValue).IsAssignableFrom(value.GetType()))
			{
				return true;
			}
			return ValueIsPrefabDifferent((TValue)value, index);
		}

		public override void Dispose()
		{
			entry.Dispose();
		}
	}
}
