using System;
using Sirenix.Serialization;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	public struct EditableKeyValuePair<TKey, TValue> : IEquatable<EditableKeyValuePair<TKey, TValue>>
	{
		[DoesNotSupportPrefabModifications]
		[ShowInInspector]
		[DisableContextMenu(true, false)]
		[Delayed]
		[OdinSerialize]
		[Space(2f)]
		[SuppressInvalidAttributeError]
		public TKey Key;

		[ShowInInspector]
		[OmitFromPrefabModificationPaths]
		[OdinSerialize]
		public TValue Value;

		[NonSerialized]
		public bool IsTempKey;

		[NonSerialized]
		public bool IsInvalidKey;

		public EditableKeyValuePair(TKey key, TValue value, bool isInvalidKey, bool isTempKey)
		{
			Key = key;
			Value = value;
			IsInvalidKey = isInvalidKey;
			IsTempKey = isTempKey;
		}

		public bool Equals(EditableKeyValuePair<TKey, TValue> other)
		{
			return PropertyValueEntry<TKey>.EqualityComparer(Key, other.Key);
		}
	}
}
