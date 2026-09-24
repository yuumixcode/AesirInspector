using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using Sirenix.Reflection.Editor;
using Sirenix.Serialization;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// Represents the values of an <see cref="T:Sirenix.OdinInspector.Editor.InspectorProperty" />, and contains utilities for querying the values' type and getting and setting them.
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.IPropertyValueEntry" />
	public abstract class PropertyValueEntry : IPropertyValueEntry, IDisposable
	{
		private struct TypePairKey
		{
			public Type ParentType;

			public Type ValueType;
		}

		private class TypePairKeyComparer : IEqualityComparer<TypePairKey>
		{
			public bool Equals(TypePairKey x, TypePairKey y)
			{
				if ((object)x.ParentType == y.ParentType || x.ParentType == y.ParentType)
				{
					if ((object)x.ValueType != y.ValueType)
					{
						return x.ValueType == y.ValueType;
					}
					return true;
				}
				return false;
			}

			public int GetHashCode(TypePairKey obj)
			{
				return obj.ParentType.GetHashCode() ^ obj.ValueType.GetHashCode();
			}
		}

		/// <summary>
		/// Delegate type used for the events <see cref="E:Sirenix.OdinInspector.Editor.PropertyValueEntry.OnValueChanged" /> and <see cref="E:Sirenix.OdinInspector.Editor.PropertyValueEntry.OnChildValueChanged" />.
		/// </summary>
		public delegate void ValueChangedDelegate(int targetIndex);

		private static readonly Dictionary<TypePairKey, Type> GenericValueEntryVariants_Cache = new Dictionary<TypePairKey, Type>(new TypePairKeyComparer());

		private static readonly Dictionary<Type, Func<PropertyValueEntry>> GenericValueEntryVariants_EmittedCreator_Cache = new Dictionary<Type, Func<PropertyValueEntry>>(FastTypeComparer.Instance);

		private static readonly Dictionary<TypePairKey, Type> GenericAliasVariants_Cache = new Dictionary<TypePairKey, Type>(new TypePairKeyComparer());

		private static readonly Dictionary<Type, Func<PropertyValueEntry, IPropertyValueEntry>> GenericAlasVariants_EmittedCreator_Cache = new Dictionary<Type, Func<PropertyValueEntry, IPropertyValueEntry>>(FastTypeComparer.Instance);

		private static readonly Type[] TypeArrayWithOneElement_Cached = new Type[1];

		private InspectorProperty parentValueProperty;

		private InspectorProperty property;

		private bool isBaseEditable;

		private Type actualTypeOfValue;

		private bool baseValueIsValueType;

		private bool? childValueChangedFromPrefab;

		private bool? valueChangedFromPrefab;

		private bool? listLengthChangedFromPrefab;

		private string cachedUnityPropertyListLengthModificationPath;

		/// <summary>
		/// <para>The nearest parent property that has a value.
		/// That is, the property from which this value
		/// entry will fetch its parentvalues from in order
		/// to extract its own values.</para>
		///
		/// <para>If <see cref="P:Sirenix.OdinInspector.Editor.PropertyValueEntry.ParentValueProperty" /> is null, this is a root property.</para>
		/// </summary>
		protected InspectorProperty ParentValueProperty => parentValueProperty;

		/// <summary>
		/// Whether this value entry represents a boxed value type.
		/// </summary>
		protected bool IsBoxedValueType { get; private set; }

		/// <summary>
		/// The number of parallel values this entry represents. This will always be exactly equal to the count of <see cref="P:Sirenix.OdinInspector.Editor.PropertyTree.WeakTargets" />.
		/// </summary>
		public int ValueCount { get; private set; }

		/// <summary>
		/// Whether this value entry is editable or not.
		/// </summary>
		public bool IsEditable
		{
			get
			{
				if (isBaseEditable)
				{
					if (parentValueProperty != null)
					{
						IPropertyValueEntry parentValueEntry = parentValueProperty.ValueEntry;
						if (!parentValueEntry.IsEditable)
						{
							return false;
						}
						if (parentValueProperty.ChildResolver is ICollectionResolver parentResolver)
						{
							if (parentResolver.IsReadOnly)
							{
								return false;
							}
							return true;
						}
					}
					return true;
				}
				return false;
			}
		}

		/// <summary>
		/// If this value entry has the override type <see cref="F:Sirenix.OdinInspector.Editor.PropertyValueState.Reference" />, this is the path of the property it references.
		/// </summary>
		public string TargetReferencePath { get; private set; }

		/// <summary>
		/// <para>The actual serialization backend for this value entry, possibly inherited from the serialization backend of the root property this entry is a child of.</para>
		/// <para>Note that this is *not* always equal to <see cref="P:Sirenix.OdinInspector.Editor.InspectorPropertyInfo.SerializationBackend" />.</para>
		/// </summary>
		public SerializationBackend SerializationBackend { get; private set; }

		/// <summary>
		/// The property whose values this value entry represents.
		/// </summary>
		public InspectorProperty Property => property;

		/// <summary>
		/// Provides access to the weakly typed values of this value entry.
		/// </summary>
		public abstract IPropertyValueCollection WeakValues { get; }

		/// <summary>
		/// Whether this value entry has been changed from its prefab counterpart.
		/// </summary>
		public bool ValueChangedFromPrefab
		{
			get
			{
				if (!valueChangedFromPrefab.HasValue)
				{
					RefreshUnityPrefabModificationState();
				}
				if (!valueChangedFromPrefab.HasValue)
				{
					return false;
				}
				return valueChangedFromPrefab.Value;
			}
		}

		/// <summary>
		/// Whether a child of this value entry has been changed from its prefab counterpart.
		/// </summary>
		public bool ChildValueChangedFromPrefab
		{
			get
			{
				if (!childValueChangedFromPrefab.HasValue)
				{
					RefreshUnityPrefabModificationState();
				}
				if (!childValueChangedFromPrefab.HasValue)
				{
					return false;
				}
				return childValueChangedFromPrefab.Value;
			}
		}

		/// <summary>
		/// Whether this value entry has had its list length changed from its prefab counterpart.
		/// </summary>
		public bool ListLengthChangedFromPrefab
		{
			get
			{
				if (!listLengthChangedFromPrefab.HasValue)
				{
					RefreshUnityPrefabModificationState();
				}
				if (!listLengthChangedFromPrefab.HasValue)
				{
					return false;
				}
				return listLengthChangedFromPrefab.Value;
			}
		}

		/// <summary>
		/// Whether this value entry has had its dictionary values changes from its prefab counterpart.
		/// </summary>
		public bool DictionaryChangedFromPrefab { get; internal set; }

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
		/// The most precise known contained type of the value entry. If polymorphism is in effect, this will be some type derived from <see cref="P:Sirenix.OdinInspector.Editor.PropertyValueEntry.BaseValueType" />.
		/// </summary>
		public Type TypeOfValue
		{
			get
			{
				if (actualTypeOfValue == null)
				{
					actualTypeOfValue = BaseValueType;
				}
				return actualTypeOfValue;
			}
		}

		/// <summary>
		/// The base type of the value entry. If this is value entry represents a member value, this is the type of the member. If the value entry represents a collection element, this is the element type of the collection.
		/// </summary>
		public Type BaseValueType { get; private set; }

		/// <summary>
		/// The special state of the value entry.
		/// </summary>
		public PropertyValueState ValueState { get; private set; }

		/// <summary>
		/// Whether this value entry is an alias, or not. Value entry aliases are used to provide strongly typed value entries in the case of polymorphism.
		/// </summary>
		public bool IsAlias => false;

		/// <summary>
		/// The context container of this property.
		/// </summary>
		public PropertyContextContainer Context => Property.Context;

		/// <summary>
		/// Whether this type is marked as an atomic type using a <see cref="T:Sirenix.OdinInspector.Editor.IAtomHandler" />.
		/// </summary>
		public abstract bool IsMarkedAtomic { get; }

		/// <summary>
		/// An event that is invoked during <see cref="M:Sirenix.OdinInspector.Editor.PropertyValueEntry.ApplyChanges" />, when any values have changed.
		/// </summary>
		public event Action<int> OnValueChanged;

		/// <summary>
		/// An event that is invoked during <see cref="M:Sirenix.OdinInspector.Editor.PropertyValueEntry.ApplyChanges" />, when any child values have changed.
		/// </summary>
		public event Action<int> OnChildValueChanged;

		/// <summary>
		/// Updates the values contained in this value entry to the actual values in the target objects, and updates its state (override, type of value, etc.) accordingly.
		/// </summary>
		public void Update()
		{
			UpdateValues();
			if (!baseValueIsValueType && (SerializationBackend.SupportsPolymorphism || typeof(UnityEngine.Object).IsAssignableFrom(BaseValueType)))
			{
				Type type = GetMostPreciseContainedType();
				if (actualTypeOfValue != type)
				{
					actualTypeOfValue = type;
					IsBoxedValueType = BaseValueType == typeof(object) && type.IsValueType;
				}
			}
			ValueState = GetValueState();
			if (ValueState == PropertyValueState.Reference)
			{
				property.Tree.ObjectIsReferenced(WeakValues[0], out var targetReferencePath);
				TargetReferencePath = targetReferencePath;
			}
			else
			{
				TargetReferencePath = null;
			}
		}

		public void RefreshPrefabModificationState()
		{
			if (SerializationBackend == SerializationBackend.Odin)
			{
				PrefabModificationType? change = Property.Tree.PrefabModificationHandler.GetPrefabModificationType(Property);
				valueChangedFromPrefab = change == PrefabModificationType.Value;
				listLengthChangedFromPrefab = change == PrefabModificationType.ListLength;
				DictionaryChangedFromPrefab = change == PrefabModificationType.Dictionary;
				if (!change.HasValue)
				{
					return;
				}
				InspectorProperty curr = Property.Parent;
				while (curr != null)
				{
					bool? currChanged = curr.BaseValueEntry.childValueChangedFromPrefab;
					if (!currChanged.HasValue || !currChanged.Value)
					{
						curr.BaseValueEntry.childValueChangedFromPrefab = true;
						curr = curr.Parent;
						continue;
					}
					break;
				}
			}
			else if (SerializationBackend.IsUnity)
			{
				childValueChangedFromPrefab = null;
				valueChangedFromPrefab = null;
				listLengthChangedFromPrefab = null;
				DictionaryChangedFromPrefab = false;
			}
		}

		private void RefreshUnityPrefabModificationState()
		{
			int count = ValueCount;
			bool childrenHaveModifications = false;
			valueChangedFromPrefab = false;
			listLengthChangedFromPrefab = false;
			InspectorProperty prop = Property;
			if (prop.ChildResolver.IsCollection)
			{
				if (cachedUnityPropertyListLengthModificationPath == null)
				{
					cachedUnityPropertyListLengthModificationPath = prop.UnityPropertyPath + ".Array.size";
				}
				for (int i = 0; i < count; i++)
				{
					PropertyModification mod = prop.Tree.PrefabModificationHandler.GetUnityPropertyModification(cachedUnityPropertyListLengthModificationPath, i, out childrenHaveModifications);
					bool? flag = childValueChangedFromPrefab;
					childValueChangedFromPrefab = childrenHaveModifications | flag;
					if (mod != null)
					{
						listLengthChangedFromPrefab = true;
						break;
					}
				}
			}
			for (int j = 0; j < count; j++)
			{
				PropertyModification mod = prop.Tree.PrefabModificationHandler.GetUnityPropertyModification(prop.UnityPropertyPath, j, out childrenHaveModifications);
				bool? flag = childValueChangedFromPrefab;
				childValueChangedFromPrefab = childrenHaveModifications | flag;
				if (mod != null || (IsMarkedAtomic && childrenHaveModifications))
				{
					valueChangedFromPrefab = true;
					break;
				}
			}
		}

		/// <summary>
		/// <para>Checks whether the values in this value entry are equal to the values in another value entry.</para>
		/// <para>Note, both value entries must have the same value type, and must represent values that are .NET value types.</para>
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public abstract bool ValueTypeValuesAreEqual(IPropertyValueEntry other);

		/// <summary>
		/// Applies the changes made to this value entry to the target objects, and registers prefab modifications as necessary.
		/// </summary>
		/// <returns>
		/// True if any changes were made, otherwise, false.
		/// </returns>
		public abstract bool ApplyChanges();

		/// <summary>
		/// Determines the value state of this value entry.
		/// </summary>
		protected abstract PropertyValueState GetValueState();

		/// <summary>
		/// Determines what the most precise contained type is on this value entry.
		/// </summary>
		protected abstract Type GetMostPreciseContainedType();

		/// <summary>
		/// Updates all values in this value entry from the target tree values.
		/// </summary>
		protected abstract void UpdateValues();

		/// <summary>
		/// Initializes this value entry.
		/// </summary>
		protected abstract void Initialize();

		internal void TriggerOnValueChanged(int index)
		{
			Action action = delegate
			{
				if (this.OnValueChanged != null)
				{
					try
					{
						this.OnValueChanged(index);
					}
					catch (ExitGUIException ex)
					{
						throw ex;
					}
					catch (Exception ex2)
					{
						if (ex2.IsExitGUIException())
						{
							throw ex2.AsExitGUIException();
						}
						Debug.LogException(ex2);
					}
				}
				Property.Tree.InvokeOnPropertyValueChanged(Property, index);
			};
			if (Event.current != null && Event.current.type == EventType.Repaint)
			{
				action();
			}
			else
			{
				Property.Tree.DelayActionUntilRepaint(action);
			}
			if (ParentValueProperty != null)
			{
				ParentValueProperty.BaseValueEntry.TriggerOnChildValueChanged(index);
			}
		}

		internal void TriggerOnChildValueChanged(int index)
		{
			Property.Tree.DelayActionUntilRepaint(delegate
			{
				if (this.OnChildValueChanged != null)
				{
					try
					{
						this.OnChildValueChanged(index);
					}
					catch (ExitGUIException ex)
					{
						throw ex;
					}
					catch (Exception ex2)
					{
						if (ex2.IsExitGUIException())
						{
							throw ex2.AsExitGUIException();
						}
						Debug.LogException(ex2);
					}
				}
			});
			if (ParentValueProperty != null)
			{
				ParentValueProperty.BaseValueEntry.TriggerOnChildValueChanged(index);
			}
		}

		/// <summary>
		/// Creates an alias value entry of a given type, for a given value entry. This is used to implement polymorphism in Odin.
		/// </summary>
		public static IPropertyValueEntry CreateAlias(PropertyValueEntry entry, Type valueType)
		{
			if (entry == null)
			{
				throw new ArgumentNullException("entry");
			}
			if (valueType == null)
			{
				throw new ArgumentNullException("valueType");
			}
			TypePairKey typePairKey = new TypePairKey
			{
				ParentType = entry.BaseValueType,
				ValueType = valueType
			};
			if (!GenericAliasVariants_Cache.TryGetValue(typePairKey, out var aliasEntryType))
			{
				aliasEntryType = typeof(PropertyValueEntryAlias<, >).MakeGenericType(entry.BaseValueType, valueType);
				GenericAliasVariants_Cache.Add(typePairKey, aliasEntryType);
			}
			if (!GenericAlasVariants_EmittedCreator_Cache.TryGetValue(aliasEntryType, out var creator))
			{
				TypeArrayWithOneElement_Cached[0] = typeof(PropertyValueEntry);
				DynamicMethod method = new DynamicMethod("AliasCreator_" + Guid.NewGuid(), typeof(IPropertyValueEntry), TypeArrayWithOneElement_Cached);
				ILGenerator il = method.GetILGenerator();
				il.Emit(OpCodes.Ldarg_0);
				il.Emit(OpCodes.Newobj, aliasEntryType.GetConstructor(TypeArrayWithOneElement_Cached));
				il.Emit(OpCodes.Ret);
				creator = (Func<PropertyValueEntry, IPropertyValueEntry>)method.CreateDelegate(typeof(Func<PropertyValueEntry, IPropertyValueEntry>));
				GenericAlasVariants_EmittedCreator_Cache.Add(aliasEntryType, creator);
			}
			return creator(entry);
		}

		/// <summary>
		/// Creates a value entry for a given property, of a given value type. Note that the created value entry is returned un-updated, and needs to have <see cref="M:Sirenix.OdinInspector.Editor.PropertyValueEntry.Update" /> called on it before it can be used.
		/// </summary>
		internal static PropertyValueEntry Create(InspectorProperty property, Type valueType, bool isSecretRoot)
		{
			if (property == null)
			{
				throw new ArgumentNullException("property");
			}
			if (valueType == null)
			{
				throw new ArgumentNullException("valueType");
			}
			if (property.Info.PropertyType != PropertyType.Value)
			{
				throw new ArgumentException("Cannot create a " + typeof(PropertyValueEntry).Name + " for a property which is not a value property.");
			}
			InspectorProperty parentValueProperty = property.ParentValueProperty;
			Type parentType = (isSecretRoot ? typeof(int) : ((parentValueProperty == null) ? property.Tree.TargetType : parentValueProperty.ValueEntry.TypeOfValue));
			TypePairKey variantKey = new TypePairKey
			{
				ParentType = parentType,
				ValueType = valueType
			};
			if (!GenericValueEntryVariants_Cache.TryGetValue(variantKey, out var genericVariantType))
			{
				genericVariantType = typeof(PropertyValueEntry<, >).MakeGenericType(parentType, valueType);
				GenericValueEntryVariants_Cache.Add(variantKey, genericVariantType);
			}
			if (!GenericValueEntryVariants_EmittedCreator_Cache.TryGetValue(genericVariantType, out var creator))
			{
				DynamicMethod builder = new DynamicMethod("PropertyValueEntry_InstanceCreator_" + Guid.NewGuid(), typeof(PropertyValueEntry), Type.EmptyTypes);
				ILGenerator il = builder.GetILGenerator();
				il.Emit(OpCodes.Newobj, genericVariantType.GetConstructor(Type.EmptyTypes));
				il.Emit(OpCodes.Ret);
				creator = (Func<PropertyValueEntry>)builder.CreateDelegate(typeof(Func<PropertyValueEntry>));
				GenericValueEntryVariants_EmittedCreator_Cache.Add(genericVariantType, creator);
			}
			PropertyValueEntry result = creator();
			result.BaseValueType = valueType;
			result.property = property;
			result.ValueCount = property.Tree.WeakTargets.Count;
			result.parentValueProperty = parentValueProperty;
			result.baseValueIsValueType = valueType.IsValueType;
			result.IsBoxedValueType = result.BaseValueType == typeof(object) && result.TypeOfValue.IsValueType;
			if (parentValueProperty != null)
			{
				result.SerializationBackend = property.Info.SerializationBackend;
				result.isBaseEditable = parentValueProperty.BaseValueEntry.isBaseEditable && property.Info.IsEditable;
			}
			else
			{
				result.SerializationBackend = property.Info.SerializationBackend;
				result.isBaseEditable = property.Info.IsEditable;
			}
			result.Initialize();
			return result;
		}

		/// <summary>
		/// <para>Determines whether the value at the given selection index is different from the given prefab value, as is relevant for prefab modification checks.</para>
		/// <para>If the value is a reference type, null and type difference is checked. If value is a value type, a comparer from <see cref="M:Sirenix.Utilities.TypeExtensions.GetEqualityComparerDelegate``1" /> is used.</para>
		/// <para>This method is best ignored unless you know what you are doing.</para>
		/// </summary>
		/// <param name="value">The value to check differences against.</param>
		/// <param name="index">The selection index to compare against.</param>
		public abstract bool ValueIsPrefabDifferent(object value, int index);

		public void Dispose()
		{
			this.OnValueChanged = null;
			this.OnChildValueChanged = null;
		}
	}
	/// <summary>
	/// Represents the values of an <see cref="T:Sirenix.OdinInspector.Editor.InspectorProperty" />, and contains utilities for querying the values' type and getting and setting them.
	/// </summary>
	/// <typeparam name="TValue">The type of the value.</typeparam>
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.IPropertyValueEntry" />
	public abstract class PropertyValueEntry<TValue> : PropertyValueEntry, IPropertyValueEntry<TValue>, IPropertyValueEntry, IDisposable, IValueEntryActualValueSetter<TValue>, IValueEntryActualValueSetter
	{
		/// <summary>
		/// An equality comparer for comparing values of type <see cref="!:TValue" />. This is gotten using <see cref="M:Sirenix.Utilities.TypeExtensions.GetEqualityComparerDelegate``1" />.
		/// </summary>
		public static readonly Func<TValue, TValue, bool> EqualityComparer = TypeExtensions.GetEqualityComparerDelegate<TValue>();

		/// <summary>
		/// Whether <see cref="!:TValue" />.is a primitive type; that is, the type is primitive, a string, or an enum.
		/// </summary>
		protected static readonly bool ValueIsPrimitive = typeof(TValue).IsPrimitive || typeof(TValue) == typeof(string) || typeof(TValue).IsEnum;

		/// <summary>
		/// Whether <see cref="!:TValue" /> is a value type.
		/// </summary>
		protected static readonly bool ValueIsValueType = typeof(TValue).IsValueType;

		/// <summary>
		/// Whether the type of the value is marked atomic.
		/// </summary>
		protected static readonly bool ValueIsMarkedAtomic = typeof(TValue).IsMarkedAtomic();

		/// <summary>
		/// If the type of the value is marked atomic, this an instance of an atom handler for the value type.
		/// </summary>
		protected static readonly IAtomHandler<TValue> AtomHandler = (ValueIsMarkedAtomic ? AtomHandlerLocator.GetAtomHandler<TValue>() : null);

		private PropertyValueCollection<TValue> values;

		private bool isWaitingForDelayedValueSet;

		/// <summary>
		/// Whether <see cref="P:Sirenix.OdinInspector.Editor.PropertyValueEntry.TypeOfValue" /> is derived from <see cref="T:UnityEngine.Object" />.
		/// </summary>
		protected bool ValueIsUnityObject => typeof(UnityEngine.Object).IsAssignableFrom(base.TypeOfValue);

		/// <summary>
		/// Provides access to the weakly typed values of this value entry.
		/// </summary>
		public sealed override IPropertyValueCollection WeakValues => values;

		/// <summary>
		/// Provides access to the strongly typed values of this value entry.
		/// </summary>
		public IPropertyValueCollection<TValue> Values => values;

		/// <summary>
		/// Whether this type is marked as an atomic type using a <see cref="T:Sirenix.OdinInspector.Editor.IAtomHandler" />.
		/// </summary>
		public override bool IsMarkedAtomic => ValueIsMarkedAtomic;

		/// <summary>
		/// <para>A weakly typed smart value that represents the first element of the value entry's value collection, but has "smart logic" for setting the value that detects relevant changes and applies them in parallel.</para>
		/// <para>This lets you often just use the smart value instead of having to deal with the tedium of multiple parallel values.</para>
		/// </summary>
		public override object WeakSmartValue
		{
			get
			{
				return SmartValue;
			}
			set
			{
				try
				{
					SmartValue = (TValue)value;
				}
				catch (InvalidCastException)
				{
					if (value == null)
					{
						Debug.LogError("Invalid cast on set weak value! Could not cast value 'null' to the type '" + typeof(TValue).GetNiceName() + "' on property " + base.Property.Path + ".");
					}
					else
					{
						Debug.LogError("Invalid cast on set weak value! Could not cast value of type '" + value.GetType().GetNiceName() + "' to '" + typeof(TValue).GetNiceName() + "' on property " + base.Property.Path + ".");
					}
				}
			}
		}

		/// <summary>
		/// <para>A strongly typed smart value that represents the first element of the value entry's value collection, but has "smart logic" for setting the value that detects relevant changes and applies them in parallel.</para>
		/// <para>This lets you often just use the smart value instead of having to deal with the tedium of multiple parallel values.</para>
		/// </summary>
		public TValue SmartValue
		{
			get
			{
				return values[0];
			}
			set
			{
				if (isWaitingForDelayedValueSet)
				{
					return;
				}
				if (ValueIsMarkedAtomic)
				{
					if (AtomHandler.Compare(value, AtomValuesArray[0]))
					{
						return;
					}
					if (!base.IsEditable)
					{
						Debug.LogWarning("Tried to change value of non-editable property '" + base.Property.NiceName + "' of type '" + base.TypeOfValue.GetNiceName() + "' at path '" + base.Property.Path + "'.");
						if (!ValueIsValueType)
						{
							AtomHandler.Copy(ref AtomValuesArray[0], ref value);
						}
					}
					else
					{
						for (int i = 0; i < base.ValueCount; i++)
						{
							values[i] = value;
						}
					}
				}
				else if (ValueIsPrimitive || ValueIsValueType)
				{
					if (EqualityComparer(value, values[0]))
					{
						return;
					}
					if (!base.IsEditable)
					{
						Debug.LogWarning("Tried to change value of non-editable property '" + base.Property.NiceName + "' of type '" + base.TypeOfValue.GetNiceName() + "' at path '" + base.Property.Path + "'.");
					}
					else
					{
						for (int j = 0; j < base.ValueCount; j++)
						{
							values[j] = value;
						}
					}
				}
				else
				{
					if ((object)value == (object)SmartValue)
					{
						return;
					}
					if (!base.IsEditable)
					{
						Debug.LogWarning("Tried to change value of non-editable property '" + base.Property.NiceName + "' of type '" + base.TypeOfValue.GetNiceName() + "' at path '" + base.Property.Path + "'.");
					}
					else
					{
						Type currentType = ((SmartValue == null) ? typeof(TValue) : SmartValue.GetType());
						if (value != null && value.GetType() != currentType)
						{
							DelayedSmartValueReferenceSet(value);
						}
						else
						{
							SmartValueReferenceSet(value);
						}
					}
				}
			}
		}

		/// <summary>
		/// An array containing the original values as they were at the beginning of frame.
		/// </summary>
		protected TValue[] OriginalValuesArray { get; private set; }

		/// <summary>
		/// An array containing the current modified set of values.
		/// </summary>
		protected TValue[] InternalValuesArray { get; private set; }

		/// <summary>
		/// An array containing the current modified set of atomic values.
		/// </summary>
		protected TValue[] AtomValuesArray { get; private set; }

		/// <summary>
		/// An array containing the original set of atomic values.
		/// </summary>
		protected TValue[] OriginalAtomValuesArray { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Sirenix.OdinInspector.Editor.PropertyValueEntry`1" /> class.
		/// </summary>
		protected PropertyValueEntry()
		{
		}

		private void DelayedSmartValueReferenceSet(TValue value)
		{
			isWaitingForDelayedValueSet = true;
			base.Property.Tree.DelayActionUntilRepaint(delegate
			{
				isWaitingForDelayedValueSet = false;
				SmartValueReferenceSet(value);
			});
		}

		private void SmartValueReferenceSet(TValue value)
		{
			if (base.ValueCount == 1 || value == null)
			{
				for (int i = 0; i < base.ValueCount; i++)
				{
					values[i] = value;
				}
				return;
			}
			bool valueIsSet = false;
			if (base.Property.Tree.ObjectIsReferenced(value, out var seenBeforePath))
			{
				InspectorProperty referencedProperty = base.Property.Tree.GetPropertyAtPath(seenBeforePath);
				if (referencedProperty != null && referencedProperty.Info.PropertyType == PropertyType.Value && !referencedProperty.Info.TypeOfValue.IsValueType)
				{
					for (int j = 0; j < base.ValueCount; j++)
					{
						TValue mirroredValue = (TValue)referencedProperty.ValueEntry.WeakValues[j];
						values[j] = mirroredValue;
					}
					valueIsSet = true;
				}
			}
			if (!valueIsSet)
			{
				for (int k = 0; k < base.ValueCount; k++)
				{
					values[k] = value;
				}
			}
		}

		/// <summary>
		/// Initializes this value entry.
		/// </summary>
		protected override void Initialize()
		{
			OriginalValuesArray = new TValue[base.Property.Tree.WeakTargets.Count];
			InternalValuesArray = new TValue[base.Property.Tree.WeakTargets.Count];
			if (IsMarkedAtomic)
			{
				AtomValuesArray = new TValue[base.Property.Tree.WeakTargets.Count];
				OriginalAtomValuesArray = new TValue[base.Property.Tree.WeakTargets.Count];
			}
			values = new PropertyValueCollection<TValue>(base.Property, InternalValuesArray, OriginalValuesArray, AtomValuesArray, OriginalAtomValuesArray);
		}

		/// <summary>
		/// Sets the actual target tree value.
		/// </summary>
		protected abstract void SetActualValueImplementation(int index, TValue value);

		/// <summary>
		/// <para>Checks whether the values in this value entry are equal to the values in another value entry.</para>
		/// <para>Note, both value entries must have the same value type, and must represent values that are .NET value types.</para>
		/// </summary>
		public override bool ValueTypeValuesAreEqual(IPropertyValueEntry other)
		{
			if (!ValueIsValueType || !other.TypeOfValue.IsValueType || other.TypeOfValue != base.TypeOfValue)
			{
				return false;
			}
			IPropertyValueEntry<TValue> castOther = (IPropertyValueEntry<TValue>)other;
			if (other.ValueCount == 1 || other.ValueState == PropertyValueState.None)
			{
				TValue otherValue = castOther.Values[0];
				for (int i = 0; i < base.ValueCount; i++)
				{
					if (!EqualityComparer(Values[i], otherValue))
					{
						return false;
					}
				}
				return true;
			}
			if (base.ValueCount == 1 || base.ValueState == PropertyValueState.None)
			{
				TValue thisValue = Values[0];
				for (int j = 0; j < base.ValueCount; j++)
				{
					if (!EqualityComparer(thisValue, castOther.Values[j]))
					{
						return false;
					}
				}
				return true;
			}
			if (base.ValueCount == other.ValueCount)
			{
				for (int k = 0; k < base.ValueCount; k++)
				{
					if (!EqualityComparer(Values[k], castOther.Values[k]))
					{
						return false;
					}
				}
				return true;
			}
			return false;
		}

		void IValueEntryActualValueSetter<TValue>.SetActualValue(int index, TValue value)
		{
			InternalValuesArray[index] = value;
			SetActualValueImplementation(index, value);
		}

		void IValueEntryActualValueSetter.SetActualValue(int index, object value)
		{
			InternalValuesArray[index] = (TValue)value;
			SetActualValueImplementation(index, (TValue)value);
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

		/// <summary>
		/// <para>Determines whether the value at the given selection index is different from the given prefab value, as is relevant for prefab modification checks.</para>
		/// <para>If the value is a reference type, null and type difference is checked. If value is a value type, a comparer from <see cref="M:Sirenix.Utilities.TypeExtensions.GetEqualityComparerDelegate``1" /> is used.</para>
		/// <para>This method is best ignored unless you know what you are doing.</para>
		/// </summary>
		/// <param name="value">The value to check differences against.</param>
		/// <param name="index">The selection index to compare against.</param>
		public bool ValueIsPrefabDifferent(TValue value, int index)
		{
			TValue thisValue = Values[index];
			if (IsMarkedAtomic)
			{
				return !AtomHandler.Compare(value, thisValue);
			}
			if (ValueIsValueType)
			{
				if (ValueIsPrimitive)
				{
					return !EqualityComparer(value, thisValue);
				}
				return false;
			}
			if (typeof(TValue) == typeof(string))
			{
				return !EqualityComparer(value, thisValue);
			}
			if (ValueIsUnityObject)
			{
				return (object)thisValue != (object)value;
			}
			Type a = null;
			Type b = null;
			if (value != null)
			{
				a = value.GetType();
			}
			if (thisValue != null)
			{
				b = thisValue.GetType();
			}
			return a != b;
		}
	}
	/// <summary>
	/// Represents the values of an <see cref="T:Sirenix.OdinInspector.Editor.InspectorProperty" />, and contains utilities for querying the values' type and getting and setting them.
	/// </summary>
	/// <typeparam name="TParent">The type of the parent.</typeparam>
	/// <typeparam name="TValue">The type of the value.</typeparam>
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.IPropertyValueEntry" />
	public sealed class PropertyValueEntry<TParent, TValue> : PropertyValueEntry<TValue>
	{
		private IValueGetterSetter<TParent, TValue> getterSetter;

		private static readonly bool ParentIsValueType = typeof(TParent).IsValueType;

		/// <summary>
		/// The type from which this value entry comes. If this value entry represents a member value, this is the declaring type of the member. If the value entry represents a collection element, this is the type of the collection.
		/// </summary>
		public sealed override Type ParentType => typeof(TParent);

		/// <summary>
		/// Determines what the most precise contained type is on this value entry.
		/// </summary>
		protected sealed override Type GetMostPreciseContainedType()
		{
			if (PropertyValueEntry<TValue>.ValueIsValueType)
			{
				return typeof(TValue);
			}
			TValue[] values = base.InternalValuesArray;
			Type type = null;
			for (int i = 0; i < values.Length; i++)
			{
				object value = values[i];
				if (value == null)
				{
					return base.Property.Info.TypeOfValue;
				}
				if (i == 0)
				{
					type = value.GetType();
				}
				else if (type != value.GetType())
				{
					return base.Property.Info.TypeOfValue;
				}
			}
			return type;
		}

		/// <summary>
		/// Initializes this value entry.
		/// </summary>
		protected override void Initialize()
		{
			base.Initialize();
			if (base.Property.Info.IsUnityPropertyOnly)
			{
				getterSetter = new UnityPropertyGetterSetter<TParent, TValue>(base.Property);
			}
			else if (!base.Property.Info.TryGetStrongGetterSetter(out getterSetter))
			{
				base.Property.Info.TryGetStrongGetterSetter(out getterSetter);
				throw new InvalidOperationException("Could not get proper value getter setter for property '" + base.Property.Path + "'.");
			}
		}

		/// <summary>
		/// Updates all values in this value entry from the target tree values.
		/// </summary>
		protected sealed override void UpdateValues()
		{
			if (base.Property.Tree.IsDesignerTree)
			{
				for (int i = 0; i < base.ValueCount; i++)
				{
					if (PropertyValueEntry<TValue>.ValueIsMarkedAtomic)
					{
						base.AtomValuesArray[i] = default(TValue);
						base.OriginalAtomValuesArray[i] = default(TValue);
					}
					base.OriginalValuesArray[i] = default(TValue);
					base.InternalValuesArray[i] = default(TValue);
				}
			}
			else
			{
				for (int j = 0; j < base.ValueCount; j++)
				{
					TParent parent = GetParent(j);
					if (parent == null)
					{
						parent = GetParent(j);
					}
					TValue value = getterSetter.GetValue(ref parent);
					if (PropertyValueEntry<TValue>.ValueIsMarkedAtomic)
					{
						PropertyValueEntry<TValue>.AtomHandler.Copy(ref value, ref base.AtomValuesArray[j]);
						PropertyValueEntry<TValue>.AtomHandler.Copy(ref value, ref base.OriginalAtomValuesArray[j]);
					}
					else if (value is UnityEngine.Object obj && obj.SafeIsUnityNull())
					{
						UnityEngine.Object newValue = OdinEntityId.FromObject(obj).ToObject();
						if (newValue != null && (object)value != newValue)
						{
							value = ((!(newValue is TValue newTValue)) ? default(TValue) : newTValue);
						}
					}
					base.OriginalValuesArray[j] = value;
					base.InternalValuesArray[j] = value;
				}
			}
			base.Values.MarkClean();
		}

		/// <summary>
		/// Determines the value state of this value entry.
		/// </summary>
		protected sealed override PropertyValueState GetValueState()
		{
			TValue[] values = base.InternalValuesArray;
			if (!PropertyValueEntry<TValue>.ValueIsValueType && !PropertyValueEntry<TValue>.ValueIsPrimitive && !PropertyValueEntry<TValue>.ValueIsMarkedAtomic)
			{
				TValue value = values[0];
				if (value == null || (base.ValueIsUnityObject && (UnityEngine.Object)(object)value == null))
				{
					for (int i = 1; i < values.Length; i++)
					{
						if (base.ValueIsUnityObject)
						{
							if ((UnityEngine.Object)(object)values[i] != null)
							{
								return PropertyValueState.ReferenceValueConflict;
							}
						}
						else if (values[i] != null)
						{
							return PropertyValueState.ReferenceValueConflict;
						}
					}
					return PropertyValueState.NullReference;
				}
				if (!base.ValueIsUnityObject && base.Property.Tree.ObjectIsReferenced(value, out var referencePath) && referencePath != base.Property.Path)
				{
					bool valueWasNull = false;
					for (int j = 1; j < values.Length; j++)
					{
						TValue v = values[j];
						string otherReferencePath;
						if (v == null)
						{
							valueWasNull = true;
						}
						else if (!base.Property.Tree.ObjectIsReferenced(v, out otherReferencePath) || otherReferencePath != referencePath)
						{
							return PropertyValueState.ReferencePathConflict;
						}
					}
					if (valueWasNull)
					{
						return PropertyValueState.ReferenceValueConflict;
					}
					return PropertyValueState.Reference;
				}
				InspectorProperty prop = base.Property;
				PropertyTree tree = prop.Tree;
				Type type = value.GetType();
				bool isReferenceValueConflict = false;
				tree.ForceRegisterObjectReference(value, prop);
				for (int k = 1; k < values.Length; k++)
				{
					TValue v2 = values[k];
					bool isNull = v2 == null;
					if (!isNull)
					{
						tree.ForceRegisterObjectReference(v2, prop);
					}
					if (isNull || v2.GetType() != type)
					{
						isReferenceValueConflict = true;
					}
					if (base.ValueIsUnityObject && (object)value != (object)v2)
					{
						isReferenceValueConflict = true;
					}
				}
				if (isReferenceValueConflict)
				{
					return PropertyValueState.ReferenceValueConflict;
				}
				if (base.Property.ChildResolver is ICollectionResolver collectionResolver && collectionResolver.CheckHasLengthConflict())
				{
					return PropertyValueState.CollectionLengthConflict;
				}
				return PropertyValueState.None;
			}
			if (PropertyValueEntry<TValue>.ValueIsMarkedAtomic)
			{
				TValue value2 = values[0];
				if (!PropertyValueEntry<TValue>.ValueIsValueType && value2 == null)
				{
					for (int l = 1; l < values.Length; l++)
					{
						if (values[l] != null)
						{
							return PropertyValueState.ReferenceValueConflict;
						}
					}
					return PropertyValueState.NullReference;
				}
				for (int m = 1; m < values.Length; m++)
				{
					if (!PropertyValueEntry<TValue>.AtomHandler.Compare(value2, values[m]))
					{
						return PropertyValueState.PrimitiveValueConflict;
					}
				}
				return PropertyValueState.None;
			}
			if (PropertyValueEntry<TValue>.ValueIsPrimitive || PropertyValueEntry<TValue>.ValueIsValueType)
			{
				TValue value3 = values[0];
				for (int n = 1; n < values.Length; n++)
				{
					if (!PropertyValueEntry<TValue>.EqualityComparer(value3, values[n]))
					{
						return PropertyValueState.PrimitiveValueConflict;
					}
				}
				return PropertyValueState.None;
			}
			return PropertyValueState.None;
		}

		/// <summary>
		/// Applies the changes made to this value entry to the target objects, and registers prefab modifications as necessary.
		/// </summary>
		/// <returns>
		/// True if any changes were made, otherwise, false.
		/// </returns>
		public sealed override bool ApplyChanges()
		{
			bool changed = false;
			PropertyTree tree = base.Property.Tree;
			if (tree.IsDesignerTree)
			{
				base.Values.MarkClean();
				return true;
			}
			if (base.Values.AreDirty)
			{
				base.Property.RecordForUndo();
				changed = true;
				for (int i = 0; i < base.ValueCount; i++)
				{
					if (GetParent(i) == null && (!base.Property.Tree.IsStatic || (base.Property.ParentValueProperty != null && !base.Property.ParentValueProperty.IsTreeRoot)))
					{
						Debug.LogError("Parent is null!");
						continue;
					}
					TValue value = base.InternalValuesArray[i];
					SetActualValueImplementation(i, value);
				}
				base.Values.MarkClean();
				for (int j = 0; j < base.ValueCount; j++)
				{
					TriggerOnValueChanged(j);
				}
				base.Property.Update(forceUpdate: true);
				for (int k = 0; k < base.ValueCount; k++)
				{
					if (base.SerializationBackend == SerializationBackend.Odin && tree.PrefabModificationHandler.HasPrefabs && tree.PrefabModificationHandler.TargetPrefabs[k] != null)
					{
						tree.PrefabModificationHandler.RegisterPrefabValueModification(base.Property, k);
					}
				}
			}
			return changed;
		}

		/// <summary>
		/// Gets the parent value at the given index.
		/// </summary>
		private TParent GetParent(int index)
		{
			if (base.Property == base.Property.Tree.RootProperty)
			{
				return (TParent)(object)index;
			}
			if (base.ParentValueProperty != null)
			{
				IPropertyValueEntry<TParent> parentValueEntry = (IPropertyValueEntry<TParent>)base.ParentValueProperty.ValueEntry;
				return parentValueEntry.Values[index];
			}
			return (TParent)base.Property.Tree.WeakTargets[index];
		}

		protected override void SetActualValueImplementation(int index, TValue value)
		{
			TParent parent = GetParent(index);
			if (ParentIsValueType)
			{
				getterSetter.SetValue(ref parent, value);
				if (base.ParentValueProperty != null)
				{
					((IValueEntryActualValueSetter<TParent>)base.ParentValueProperty.ValueEntry).SetActualValue(index, parent);
				}
			}
			else
			{
				getterSetter.SetValue(ref parent, value);
			}
		}
	}
}
