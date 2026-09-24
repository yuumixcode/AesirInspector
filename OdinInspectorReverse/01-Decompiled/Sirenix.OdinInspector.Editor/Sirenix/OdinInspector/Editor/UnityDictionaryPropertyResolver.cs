using System;
using System.Collections.Generic;
using Sirenix.Reflection.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// <para>
	/// This resolver has the unenviable job of handling Unity's dictionary serialization and translating it into Odin terms.
	/// Unity decided that dictionaries are secretly serialized as arrays, with a secret ordering that is invisible to the outside
	/// and only stored inside of Unity's serialization systems.
	/// </para>
	/// <para>
	/// This means Odin must query Unity's SerializedObject/SerializedProperties to determine the true indices of all keys in the 
	/// dictionary, which is necessary to, for example, correctly map prefab modification paths. 
	/// </para>
	/// <para>
	/// This resolver also prefers to always modify the dictionary directly through Unity SerializedProperty and then applying, 
	/// rather than directly editing the live C# dictionary object.
	/// </para>
	/// </summary>
	[ResolverPriority(-0.9)]
	public class UnityDictionaryPropertyResolver<TDictionary, TKey, TValue> : BaseKeyValueMapResolver<TDictionary>, IMaySupportPrefabModifications, IOverridesUnityPropertyNames where TDictionary : Dictionary<TKey, TValue>
	{
		private struct TempKeyInfo
		{
			public TKey Key;

			public bool IsInvalid;
		}

		private static readonly Type KeyType = typeof(TKey);

		private static readonly bool KeyIsNullable = KeyType.IsNullableType();

		private static readonly bool KeyIsEnum = KeyType.IsEnum;

		private const string UnityEntryKeyField = "key";

		private const string UnityEntryValueField = "value";

		private SerializedProperty cachedProp;

		private int lastUpdatedID = -1;

		private Dictionary<int, InspectorPropertyInfo> childInfos = new Dictionary<int, InspectorPropertyInfo>();

		private Dictionary<int, TempKeyInfo> tempKeys = new Dictionary<int, TempKeyInfo>();

		private Dictionary<TKey, int> keyToIndexMap = new Dictionary<TKey, int>();

		private Dictionary<int, TKey> indexToKeyMap = new Dictionary<int, TKey>();

		private Attribute[] childAttrs;

		private bool hasLoggedNoPropertyFoundError;

		private uint lastUpdatedDictHashCode;

		public bool MaySupportPrefabModifications => true;

		public override Type ElementType => typeof(EditableKeyValuePair<TKey, TValue>);

		public override bool CanResolveForPropertyFilter(InspectorProperty property)
		{
			if (SerializedProperty_Internals.HasContentHash && SerializedProperty_Internals.HasParent && SerializedProperty_Internals.HasBoxedValue && property.ValueEntry != null && property.ValueEntry.SerializationBackend.IsUnity && property.Tree.GetUnitySerializedObjectNoUpdate() != null && property.ValueEntry.BaseValueType == typeof(Dictionary<TKey, TValue>))
			{
				return !property.Attributes.HasAttribute<DrawWithUnityAttribute>();
			}
			return false;
		}

		protected override void Initialize()
		{
			ImmutableList<Attribute> propAttrs = base.Property.Attributes;
			List<Attribute> attrs = new List<Attribute>(propAttrs.Count);
			for (int i = 0; i < propAttrs.Count; i++)
			{
				Attribute attr = propAttrs[i];
				if (!attr.GetType().IsDefined(typeof(DontApplyToListElementsAttribute), inherit: true))
				{
					attrs.Add(attr);
				}
			}
			childAttrs = attrs.ToArray();
		}

		private SerializedProperty GetSerializedProperty()
		{
			SerializedObject serializedObject = base.Property.Tree.UnitySerializedObject;
			if (serializedObject == null)
			{
				return null;
			}
			if (cachedProp == null || !SerializedProperty_Internals.isValid(cachedProp) || cachedProp.propertyPath != base.Property.UnityPropertyPath || cachedProp.serializedObject != serializedObject)
			{
				cachedProp = serializedObject.FindProperty(base.Property.UnityPropertyPath);
			}
			if (cachedProp == null && !hasLoggedNoPropertyFoundError)
			{
				Debug.LogError("UnityDictionaryResolver could not find Unity property at path '" + base.Property.UnityPropertyPath + "'.");
				hasLoggedNoPropertyFoundError = true;
			}
			return cachedProp;
		}

		public void Update()
		{
			SerializedProperty prop = GetSerializedProperty();
			if (prop == null)
			{
				keyToIndexMap.Clear();
				indexToKeyMap.Clear();
				tempKeys.Clear();
				return;
			}
			uint hash = SerializedProperty_Internals.contentHash(prop);
			if (lastUpdatedDictHashCode == hash)
			{
				return;
			}
			lastUpdatedDictHashCode = hash;
			keyToIndexMap.Clear();
			indexToKeyMap.Clear();
			tempKeys.Clear();
			if (!prop.isArray)
			{
				Debug.LogError("SerializedProperty for Unity-serialized Dictionary was not an array. Something is deepy broken. Please report this to the Odin developers and put [DrawWithUnity] on the Unity-serialized dictionary at path " + base.Property.Tree.TargetType.GetNiceName() + "." + base.Property.UnityPropertyPath + " for now.");
				return;
			}
			int count = prop.arraySize;
			if (count <= 0)
			{
				return;
			}
			prop.Next(enterChildren: true);
			prop.Next(enterChildren: true);
			prop.Next(enterChildren: false);
			for (int i = 0; i < count; i++)
			{
				if (i > 0)
				{
					prop.Next(enterChildren: false);
				}
				prop.Next(enterChildren: true);
				if (!TryGetKeyValue(prop, out var key))
				{
					Debug.LogError("Odin cannot currently properly inspect the Unity-serialized dictionary at path " + base.Property.Tree.TargetType.GetNiceName() + "." + base.Property.UnityPropertyPath + " with a key type of " + typeof(TKey).GetNiceName() + ", as it cannot be fetched using SerializedProperty.boxedValue and there is no custom key handling logic for this key type yet. Please inform the developers, and meanwhile, you can put [DrawWithUnity] on this dictionary.");
					SerializedProperty_Internals.Parent(prop);
					SerializedProperty_Internals.Parent(prop);
					keyToIndexMap.Clear();
					indexToKeyMap.Clear();
					tempKeys.Clear();
					break;
				}
				if (KeyIsNullable && key == null)
				{
					tempKeys.Add(i, new TempKeyInfo
					{
						Key = key,
						IsInvalid = true
					});
				}
				else if (keyToIndexMap.ContainsKey(key))
				{
					tempKeys.Add(i, new TempKeyInfo
					{
						Key = key,
						IsInvalid = true
					});
				}
				else
				{
					keyToIndexMap.Add(key, i);
				}
				indexToKeyMap[i] = key;
				SerializedProperty_Internals.Parent(prop);
			}
			SerializedProperty_Internals.Parent(prop);
			SerializedProperty_Internals.Parent(prop);
		}

		private bool TryGetKeyValue(SerializedProperty prop, out TKey key)
		{
			try
			{
				object boxed = SerializedProperty_Internals.GetBoxedValue(prop);
				if (boxed == null)
				{
					key = default(TKey);
					return true;
				}
				if (boxed is TKey typedKey)
				{
					key = typedKey;
					return true;
				}
				key = (TKey)ConvertKey(boxed);
				return true;
			}
			catch
			{
				key = default(TKey);
				return false;
			}
		}

		private static object ConvertKey(object boxed)
		{
			if (KeyIsEnum)
			{
				return Enum.ToObject(KeyType, boxed);
			}
			if (KeyType == typeof(char))
			{
				return Convert.ToChar(boxed);
			}
			return Convert.ChangeType(boxed, KeyType);
		}

		public void EnsureUpdated()
		{
			if (base.Property.Tree.UpdateID != lastUpdatedID)
			{
				Update();
				lastUpdatedID = base.Property.Tree.UpdateID;
			}
		}

		public string GetUnityPropertyName(int index)
		{
			return $"Array.data[{index}]";
		}

		public override int ChildNameToIndex(string name)
		{
			StringSlice slice = name.Slice();
			return ChildNameToIndex(ref slice);
		}

		public override int ChildNameToIndex(ref StringSlice name)
		{
			return CollectionResolverUtilities.DefaultChildNameToIndex(ref name);
		}

		public override InspectorPropertyInfo GetChildInfo(int childIndex)
		{
			if (!childInfos.TryGetValue(childIndex, out var result))
			{
				result = InspectorPropertyInfo.CreateValue(CollectionResolverUtilities.DefaultIndexToChildName(childIndex), childIndex, base.Property.BaseValueEntry.SerializationBackend, new GetterSetter<TDictionary, EditableKeyValuePair<TKey, TValue>>(CreateGetter(childIndex), CreateSetter(childIndex)), childAttrs);
				childInfos[childIndex] = result;
			}
			return result;
		}

		private ValueGetter<TDictionary, EditableKeyValuePair<TKey, TValue>> CreateGetter(int childIndex)
		{
			return delegate(ref TDictionary dict)
			{
				EnsureUpdated();
				if (!indexToKeyMap.TryGetValue(childIndex, out var value))
				{
					Update();
					indexToKeyMap.TryGetValue(childIndex, out value);
				}
				TempKeyInfo value2;
				bool flag = tempKeys.TryGetValue(childIndex, out value2);
				TValue value3 = default(TValue);
				if (!KeyIsNullable || value != null)
				{
					TKey key = value;
					dict.TryGetValue(key, out value3);
				}
				return new EditableKeyValuePair<TKey, TValue>(flag ? value2.Key : value, value3, flag && value2.IsInvalid, flag);
			};
		}

		private ValueSetter<TDictionary, EditableKeyValuePair<TKey, TValue>> CreateSetter(int childIndex)
		{
			return delegate(ref TDictionary dict, EditableKeyValuePair<TKey, TValue> value)
			{
				EnsureUpdated();
				if (!indexToKeyMap.TryGetValue(childIndex, out var value2))
				{
					Update();
					indexToKeyMap.TryGetValue(childIndex, out value2);
				}
				TKey key = value.Key;
				TValue value3 = value.Value;
				bool flag = KeyIsNullable && value2 == null;
				bool flag2 = KeyIsNullable && key == null;
				TValue value4 = default(TValue);
				if (!flag)
				{
					TKey key2 = value2;
					dict.TryGetValue(key2, out value4);
				}
				bool flag3 = PropertyValueEntry<TKey>.EqualityComparer(value2, key);
				if (!flag3)
				{
					if (flag2 || dict.ContainsKey(key))
					{
						tempKeys[childIndex] = new TempKeyInfo
						{
							Key = key,
							IsInvalid = true
						};
					}
					else if (!ValueApplyIsTemporary)
					{
						tempKeys.Remove(childIndex);
						CollectionChangeInfo info = new CollectionChangeInfo
						{
							ChangeType = CollectionChangeType.SetKey,
							Key = key,
							Value = value3,
							SelectionIndex = 0
						};
						InvokeOnBeforeChange(info);
						if (TryGetEntryProperties(childIndex, out var arrayProp, out var keyProp, out var valueProp) && TrySetBoxedValue(keyProp, key) && TrySetBoxedValue(valueProp, value3))
						{
							ApplySerializedObject(arrayProp.serializedObject);
						}
						else
						{
							if (!flag)
							{
								TKey key3 = value2;
								dict.Remove(key3);
							}
							dict.Add(key, value3);
						}
						InvokeOnAfterChange(info);
					}
					else
					{
						tempKeys[childIndex] = new TempKeyInfo
						{
							Key = key,
							IsInvalid = false
						};
					}
				}
				else if (!flag2 && !PropertyValueEntry<TValue>.EqualityComparer(value4, value3))
				{
					CollectionChangeInfo info2 = new CollectionChangeInfo
					{
						ChangeType = CollectionChangeType.SetKey,
						Key = key,
						Value = value3,
						SelectionIndex = 0
					};
					InvokeOnBeforeChange(info2);
					if (TryGetEntryProperties(childIndex, out var arrayProp2, out var _, out var valueProp2) && TrySetBoxedValue(valueProp2, value3))
					{
						ApplySerializedObject(arrayProp2.serializedObject);
					}
					else
					{
						dict[key] = value3;
					}
					InvokeOnAfterChange(info2);
				}
				if (value.IsTempKey && flag3)
				{
					tempKeys.Remove(childIndex);
				}
			};
		}

		public override object GetKey(int selectionIndex, int childIndex)
		{
			EnsureUpdated();
			return indexToKeyMap[childIndex];
		}

		protected override void Add(TDictionary collection, object value)
		{
			KeyValuePair<TKey, TValue> pair = (KeyValuePair<TKey, TValue>)value;
			if (!TryAppendEntryAndApply(pair.Key, pair.Value))
			{
				collection.Add(pair.Key, pair.Value);
			}
		}

		protected override void Clear(TDictionary collection)
		{
			SerializedProperty prop = GetSerializedProperty();
			if (prop != null && prop.isArray)
			{
				prop.ClearArray();
				ApplySerializedObject(prop.serializedObject);
			}
			else
			{
				collection.Clear();
			}
		}

		protected override bool CollectionIsReadOnly(TDictionary collection)
		{
			return false;
		}

		protected override int GetChildCount(TDictionary value)
		{
			EnsureUpdated();
			return indexToKeyMap.Count;
		}

		protected override void Remove(TDictionary collection, object value)
		{
			TKey key = ((KeyValuePair<TKey, TValue>)value).Key;
			if (!TryDeleteEntryByKeyAndApply(key))
			{
				collection.Remove(key);
			}
		}

		protected override void RemoveKey(TDictionary map, object key)
		{
			TKey typedKey = (TKey)key;
			if (!TryDeleteEntryByKeyAndApply(typedKey))
			{
				map.Remove(typedKey);
			}
		}

		protected override void Set(TDictionary map, object key, object value)
		{
			TKey typedKey = (TKey)key;
			TValue typedValue = (TValue)value;
			if (!TrySetEntryValueOrAppendAndApply(typedKey, typedValue))
			{
				map[typedKey] = typedValue;
			}
		}

		private static bool TrySetBoxedValue(SerializedProperty prop, object boxedValue)
		{
			try
			{
				SerializedProperty_Internals.SetBoxedValue(prop, boxedValue);
				return true;
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
				return false;
			}
		}

		private bool TryGetEntryProperties(int arrayIndex, out SerializedProperty arrayProp, out SerializedProperty keyProp, out SerializedProperty valueProp)
		{
			arrayProp = null;
			keyProp = null;
			valueProp = null;
			SerializedProperty prop = GetSerializedProperty();
			if (prop == null || !prop.isArray)
			{
				return false;
			}
			if (arrayIndex < 0 || arrayIndex >= prop.arraySize)
			{
				return false;
			}
			SerializedProperty element = prop.GetArrayElementAtIndex(arrayIndex);
			if (element == null)
			{
				return false;
			}
			keyProp = element.FindPropertyRelative("key");
			valueProp = element.FindPropertyRelative("value");
			if (keyProp == null || valueProp == null)
			{
				return false;
			}
			arrayProp = prop;
			return true;
		}

		private bool TryAppendEntryAndApply(TKey key, TValue value)
		{
			SerializedProperty prop = GetSerializedProperty();
			if (prop == null || !prop.isArray)
			{
				return false;
			}
			SerializedProperty element = prop.GetArrayElementAtIndex(prop.arraySize++);
			if (element == null)
			{
				return false;
			}
			SerializedProperty keyProp = element.FindPropertyRelative("key");
			SerializedProperty valueProp = element.FindPropertyRelative("value");
			if (keyProp == null || valueProp == null)
			{
				return false;
			}
			if (!TrySetBoxedValue(keyProp, key) || !TrySetBoxedValue(valueProp, value))
			{
				return false;
			}
			ApplySerializedObject(prop.serializedObject);
			return true;
		}

		private bool TryDeleteEntryByKeyAndApply(TKey key)
		{
			SerializedProperty prop = GetSerializedProperty();
			if (prop == null || !prop.isArray)
			{
				return false;
			}
			if (KeyIsNullable && key == null)
			{
				return false;
			}
			EnsureUpdated();
			if (!keyToIndexMap.TryGetValue(key, out var index))
			{
				return false;
			}
			if (index < 0 || index >= prop.arraySize)
			{
				return false;
			}
			prop.DeleteArrayElementAtIndex(index);
			ApplySerializedObject(prop.serializedObject);
			return true;
		}

		private bool TrySetEntryValueOrAppendAndApply(TKey key, TValue value)
		{
			SerializedProperty prop = GetSerializedProperty();
			if (prop == null || !prop.isArray)
			{
				return false;
			}
			if (!KeyIsNullable || key != null)
			{
				EnsureUpdated();
				if (keyToIndexMap.TryGetValue(key, out var index) && index >= 0 && index < prop.arraySize)
				{
					SerializedProperty valueProp = prop.GetArrayElementAtIndex(index)?.FindPropertyRelative("value");
					if (valueProp == null)
					{
						return false;
					}
					if (!TrySetBoxedValue(valueProp, value))
					{
						return false;
					}
					ApplySerializedObject(prop.serializedObject);
					return true;
				}
			}
			return TryAppendEntryAndApply(key, value);
		}

		private void ApplySerializedObject(SerializedObject obj)
		{
			if (base.Property.Tree.RecordUndoForChanges)
			{
				obj.ApplyModifiedProperties();
			}
			else
			{
				obj.ApplyModifiedPropertiesWithoutUndo();
			}
		}
	}
}
