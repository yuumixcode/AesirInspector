using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.Serialization;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;

namespace Sirenix.OdinInspector.Editor
{
	[ResolverPriority(-1.0)]
	public class EditableKeyValuePairResolver<TKey, TValue> : OdinPropertyResolver<EditableKeyValuePair<TKey, TValue>>, IHasSpecialPropertyPaths, IMaySupportPrefabModifications
	{
		private static Dictionary<SerializationBackend, InspectorPropertyInfo[]> ChildInfos = new Dictionary<SerializationBackend, InspectorPropertyInfo[]>();

		private static readonly FieldInfo keyField = typeof(EditableKeyValuePair<TKey, TValue>).GetField("Key");

		private static readonly FieldInfo valueField = typeof(EditableKeyValuePair<TKey, TValue>).GetField("Value");

		private SerializationBackend backend;

		private bool useSpecialDictKeys;

		private bool maySupportPrefabModifications;

		public bool MaySupportPrefabModifications => maySupportPrefabModifications;

		public string GetSpecialChildPath(int childIndex)
		{
			if (useSpecialDictKeys)
			{
				TKey key = base.ValueEntry.SmartValue.Key;
				string keyStr = DictionaryKeyUtility.GetDictionaryKeyString(key);
				switch (childIndex)
				{
				case 0:
					return base.Property.Parent.Path + "." + keyStr + "#key";
				case 1:
					return base.Property.Parent.Path + "." + keyStr;
				}
			}
			else if (backend.IsUnity)
			{
				switch (childIndex)
				{
				case 0:
					return base.Property.Path + ".key";
				case 1:
					return base.Property.Path + ".value";
				}
			}
			else
			{
				switch (childIndex)
				{
				case 0:
					return base.Property.Path + ".Key";
				case 1:
					return base.Property.Path + ".Value";
				}
			}
			throw new ArgumentOutOfRangeException();
		}

		protected override void Initialize()
		{
			backend = base.Property.ValueEntry.SerializationBackend;
			InspectorProperty parent = base.Property.Parent;
			useSpecialDictKeys = parent != null && parent.ChildResolver is IKeyValueMapResolver && parent.ChildResolver.GetType().ImplementsOpenGenericClass(typeof(StrongDictionaryPropertyResolver<, , >));
			maySupportPrefabModifications = !useSpecialDictKeys || DictionaryKeyUtility.KeyTypeSupportsPersistentPaths(typeof(TKey));
			if (!ChildInfos.ContainsKey(backend))
			{
				InspectorPropertyInfo[] infos = ((!backend.IsUnity) ? InspectorPropertyInfoUtility.GetDefaultPropertiesForType(base.Property, typeof(EditableKeyValuePair<TKey, TValue>), includeSpeciallySerializedMembers: false) : new InspectorPropertyInfo[2]
				{
					InspectorPropertyInfo.CreateForMember(keyField, allowEditable: true, backend, null, base.Property.Tree.IsDesignerTree, "key"),
					InspectorPropertyInfo.CreateForMember(valueField, allowEditable: true, backend, null, base.Property.Tree.IsDesignerTree, "value")
				});
				ChildInfos[backend] = infos;
			}
		}

		public override InspectorPropertyInfo GetChildInfo(int childIndex)
		{
			return ChildInfos[backend][childIndex];
		}

		protected override int GetChildCount(EditableKeyValuePair<TKey, TValue> value)
		{
			return ChildInfos[backend].Length;
		}

		public override int ChildNameToIndex(string name)
		{
			StringSlice slice = name;
			return ChildNameToIndex(ref slice);
		}

		public override int ChildNameToIndex(ref StringSlice name)
		{
			if (name == "Key" || name == "key")
			{
				return 0;
			}
			if (name == "Value" || name == "#Value" || name == "value")
			{
				return 1;
			}
			return -1;
		}
	}
}
