using System;
using System.Collections.Generic;
using Sirenix.Serialization;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;

namespace Sirenix.OdinInspector.Editor
{
	[ResolverPriority(-1.0)]
	public class StrongDictionaryPropertyResolver<TDictionary, TKey, TValue> : BaseKeyValueMapResolver<TDictionary>, IHasSpecialPropertyPaths, IPathRedirector, IMaySupportPrefabModifications where TDictionary : IDictionary<TKey, TValue>
	{
		private struct TempKeyInfo
		{
			public TKey Key;

			public bool IsInvalid;
		}

		private int lastUpdateID;

		private Dictionary<int, InspectorPropertyInfo> childInfos = new Dictionary<int, InspectorPropertyInfo>();

		private Dictionary<int, TempKeyInfo> tempKeys = new Dictionary<int, TempKeyInfo>();

		private Dictionary<TDictionary, int> dictIndexMap = new Dictionary<TDictionary, int>();

		private List<TKey>[] keys;

		private List<TKey>[] oldKeys;

		private List<Attribute> childAttrs;

		private static readonly bool KeyTypeSupportsPersistentPaths = DictionaryKeyUtility.KeyTypeSupportsPersistentPaths(typeof(TKey));

		public bool MaySupportPrefabModifications => KeyTypeSupportsPersistentPaths;

		public override Type ElementType => typeof(EditableKeyValuePair<TKey, TValue>);

		protected override void Initialize()
		{
			base.Initialize();
			keys = new List<TKey>[base.Property.Tree.WeakTargets.Count];
			oldKeys = new List<TKey>[base.Property.Tree.WeakTargets.Count];
			for (int i = 0; i < keys.Length; i++)
			{
				keys[i] = new List<TKey>();
				oldKeys[i] = new List<TKey>();
			}
			ImmutableList<Attribute> propAttrs = base.Property.Attributes;
			List<Attribute> attrs = new List<Attribute>(propAttrs.Count);
			for (int j = 0; j < propAttrs.Count; j++)
			{
				Attribute attr = propAttrs[j];
				if (!attr.GetType().IsDefined(typeof(DontApplyToListElementsAttribute), inherit: true))
				{
					attrs.Add(attr);
				}
			}
			childAttrs = attrs;
		}

		public bool TryGetRedirectedProperty(string childName, out InspectorProperty property)
		{
			EnsureUpdated();
			property = null;
			if (childName.Length == 0 || childName[0] != '{')
			{
				return false;
			}
			try
			{
				bool isKey = FastEndsWith(childName, "#key");
				if (isKey)
				{
					childName = childName.Substring(0, childName.Length - 4);
				}
				TKey key = (TKey)DictionaryKeyUtility.GetDictionaryKeyValue(childName, typeof(TKey));
				List<TKey> keyList = keys[0];
				for (int i = 0; i < keyList.Count; i++)
				{
					if (PropertyValueEntry<TKey>.EqualityComparer(key, keyList[i]))
					{
						property = (isKey ? base.Property.Children[i].Children["Key"] : base.Property.Children[i].Children["Value"]);
						return true;
					}
				}
			}
			catch (Exception)
			{
				return false;
			}
			return false;
		}

		public string GetSpecialChildPath(int childIndex)
		{
			EnsureUpdated();
			List<TKey> keys = this.keys[0];
			if (childIndex >= keys.Count)
			{
				Update();
				keys = this.keys[0];
			}
			TKey key = this.keys[0][childIndex];
			return base.Property.Path + "." + DictionaryKeyUtility.GetDictionaryKeyString(key) + "#entry";
		}

		public override object GetKey(int selectionIndex, int childIndex)
		{
			EnsureUpdated();
			return keys[selectionIndex][childIndex];
		}

		public override InspectorPropertyInfo GetChildInfo(int childIndex)
		{
			EnsureUpdated();
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
				List<TKey> list = keys[dictIndexMap[dict]];
				if (childIndex >= list.Count)
				{
					Update();
					list = keys[dictIndexMap[dict]];
				}
				TKey val = list[childIndex];
				dict.TryGetValue(val, out var value);
				TempKeyInfo value2;
				bool flag = tempKeys.TryGetValue(childIndex, out value2);
				return new EditableKeyValuePair<TKey, TValue>(flag ? value2.Key : val, value, flag && value2.IsInvalid, flag);
			};
		}

		private ValueSetter<TDictionary, EditableKeyValuePair<TKey, TValue>> CreateSetter(int childIndex)
		{
			return delegate(ref TDictionary dict, EditableKeyValuePair<TKey, TValue> value)
			{
				EnsureUpdated();
				int num = dictIndexMap[dict];
				List<TKey> list = keys[num];
				if (childIndex >= list.Count)
				{
					Update();
					num = dictIndexMap[dict];
					list = keys[num];
				}
				TKey val = list[childIndex];
				dict.TryGetValue(val, out var value2);
				TKey key = value.Key;
				TValue value3 = value.Value;
				bool flag = PropertyValueEntry<TKey>.EqualityComparer(val, key);
				if (!flag)
				{
					if (dict.ContainsKey(key))
					{
						tempKeys[childIndex] = new TempKeyInfo
						{
							Key = key,
							IsInvalid = true
						};
					}
					else if (!ValueApplyIsTemporary)
					{
						bool flag2 = base.Property.SupportsPrefabModifications && base.ValueEntry.SerializationBackend == SerializationBackend.Odin;
						tempKeys.Remove(childIndex);
						CollectionChangeInfo info = new CollectionChangeInfo
						{
							ChangeType = CollectionChangeType.RemoveKey,
							Key = val,
							SelectionIndex = num
						};
						InvokeOnBeforeChange(info);
						dict.Remove(val);
						InvokeOnAfterChange(info);
						CollectionChangeInfo info2 = new CollectionChangeInfo
						{
							ChangeType = CollectionChangeType.SetKey,
							Key = key,
							Value = value3,
							SelectionIndex = dictIndexMap[dict]
						};
						InvokeOnBeforeChange(info2);
						dict.Add(key, value3);
						InvokeOnAfterChange(info2);
						if (flag2)
						{
							for (int i = 0; i < base.Property.Tree.WeakTargets.Count; i++)
							{
								base.Property.Tree.PrefabModificationHandler.RegisterPrefabDictionaryRemoveKeyModification(base.Property, i, val);
								base.Property.Tree.PrefabModificationHandler.RegisterPrefabDictionaryAddKeyModification(base.Property, i, key);
							}
						}
						childInfos.Clear();
						base.Property.Children.ClearAndDisposeChildren();
						if (flag2)
						{
							Update();
							string dictionaryKeyString = DictionaryKeyUtility.GetDictionaryKeyString(key);
							InspectorProperty inspectorProperty = base.Property.Children[dictionaryKeyString];
							inspectorProperty.Update(forceUpdate: true);
							foreach (InspectorProperty current in inspectorProperty.Children.Recurse())
							{
								current.Update(forceUpdate: true);
							}
						}
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
				else if (!PropertyValueEntry<TValue>.EqualityComparer(value2, value3))
				{
					CollectionChangeInfo info3 = new CollectionChangeInfo
					{
						ChangeType = CollectionChangeType.SetKey,
						Key = key,
						Value = value3,
						SelectionIndex = dictIndexMap[dict]
					};
					InvokeOnBeforeChange(info3);
					dict[key] = value3;
					InvokeOnAfterChange(info3);
				}
				if (value.IsTempKey && flag)
				{
					tempKeys.Remove(childIndex);
				}
			};
		}

		protected override int GetChildCount(TDictionary value)
		{
			return value.Count;
		}

		protected override void OnCollectionChangesApplied()
		{
			base.OnCollectionChangesApplied();
			if (base.Property.SupportsPrefabModifications && base.Property.ValueEntry.SerializationBackend == SerializationBackend.Odin)
			{
				int count = base.Property.Tree.WeakTargets.Count;
				for (int i = 0; i < count; i++)
				{
					base.Property.Tree.PrefabModificationHandler.RegisterPrefabDictionaryDeltaModification(base.Property, i);
				}
			}
		}

		protected override void Add(TDictionary collection, object value)
		{
			KeyValuePair<TKey, TValue> pair = (KeyValuePair<TKey, TValue>)value;
			collection.Add(pair);
			HandleAddSetPrefabValueModification(pair.Key);
		}

		protected override void Remove(TDictionary collection, object value)
		{
			collection.Remove(((KeyValuePair<TKey, TValue>)value).Key);
		}

		protected override void RemoveKey(TDictionary map, object key)
		{
			map.Remove((TKey)key);
		}

		protected override void Set(TDictionary map, object key, object value)
		{
			map[(TKey)key] = (TValue)value;
			HandleAddSetPrefabValueModification(key);
		}

		protected override void Clear(TDictionary collection)
		{
			collection.Clear();
		}

		protected override bool CollectionIsReadOnly(TDictionary collection)
		{
			return collection.IsReadOnly;
		}

		private void HandleAddSetPrefabValueModification(object key)
		{
			if (!base.Property.SupportsPrefabModifications || base.Property.ValueEntry.SerializationBackend != SerializationBackend.Odin)
			{
				return;
			}
			Update();
			int count = base.Property.Tree.WeakTargets.Count;
			for (int i = 0; i < count; i++)
			{
				base.Property.Tree.PrefabModificationHandler.RegisterPrefabDictionaryAddKeyModification(base.Property, i, key);
				InspectorProperty child = base.Property.Children[DictionaryKeyUtility.GetDictionaryKeyString(key)];
				if (child != null)
				{
					base.Property.Tree.PrefabModificationHandler.RegisterPrefabValueModification(child, i, forceImmediate: true);
				}
			}
		}

		private void EnsureUpdated()
		{
			if (base.Property.Tree.UpdateID != lastUpdateID)
			{
				Update();
			}
		}

		private void Update()
		{
			dictIndexMap.Clear();
			lastUpdateID = base.Property.Tree.UpdateID;
			for (int i = 0; i < keys.Length; i++)
			{
				List<TKey> oldKeyList = keys[i];
				List<TKey> keyList = oldKeys[i];
				oldKeys[i] = oldKeyList;
				keys[i] = keyList;
				keyList.Clear();
				TDictionary dict = (TDictionary)base.Property.ValueEntry.WeakValues[i];
				if (dict == null)
				{
					continue;
				}
				dictIndexMap[dict] = i;
				if (dict is Dictionary<TKey, TValue> castDict)
				{
					foreach (KeyValuePair<TKey, TValue> item in castDict.GFIterator())
					{
						keyList.Add(item.Key);
					}
				}
				else
				{
					foreach (TKey key in dict.Keys)
					{
						keyList.Add(key);
					}
				}
				if (keyList.Count > 1)
				{
					DictionaryKeyUtility.KeyComparer<TKey> comparer = DictionaryKeyUtility.KeyComparer<TKey>.Default;
					TKey a = keyList[0];
					for (int j = 1; j < keyList.Count; j++)
					{
						TKey b = keyList[j];
						if (comparer.Compare(a, b) > 0)
						{
							keyList.Sort(comparer);
							break;
						}
						a = b;
					}
				}
				if (keyList.Count != oldKeyList.Count)
				{
					childInfos.Clear();
					base.Property.Children.ClearAndDisposeChildren();
					continue;
				}
				for (int k = 0; k < keyList.Count; k++)
				{
					if (!PropertyValueEntry<TKey>.EqualityComparer(keyList[k], oldKeyList[k]))
					{
						childInfos.Clear();
						base.Property.Children.ClearAndDisposeChildren();
						break;
					}
				}
			}
		}

		public override int ChildNameToIndex(string name)
		{
			return CollectionResolverUtilities.DefaultChildNameToIndex(name);
		}

		public override int ChildNameToIndex(ref StringSlice name)
		{
			return CollectionResolverUtilities.DefaultChildNameToIndex(ref name);
		}

		private static bool FastEndsWith(string str, string endsWith)
		{
			if (str.Length < endsWith.Length)
			{
				return false;
			}
			int start = str.Length - endsWith.Length;
			for (int i = 0; i < endsWith.Length; i++)
			{
				if (str[start + i] != endsWith[i])
				{
					return false;
				}
			}
			return true;
		}
	}
}
