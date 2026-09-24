using System.Collections.Generic;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal class FixedDictionary<TKey, TValue>
	{
		public List<TKey> KeyAddOrder;

		public Dictionary<TKey, TValue> Dictionary;

		public int Capacity => KeyAddOrder.Capacity;

		public TValue this[TKey key] => Dictionary[key];

		public FixedDictionary(int capacity)
		{
			KeyAddOrder = new List<TKey>(capacity);
			Dictionary = new Dictionary<TKey, TValue>(capacity);
		}

		public void Add(TKey key, TValue value)
		{
			if (Dictionary.ContainsKey(key))
			{
				Dictionary[key] = value;
				return;
			}
			if (Dictionary.Count == Capacity)
			{
				TKey lastAddedKey = KeyAddOrder[0];
				KeyAddOrder.RemoveAt(0);
				Dictionary.Remove(lastAddedKey);
			}
			KeyAddOrder.Add(key);
			Dictionary[key] = value;
		}

		public bool TryGetValue(TKey key, out TValue value)
		{
			return Dictionary.TryGetValue(key, out value);
		}

		public bool Remove(TKey key)
		{
			bool result = Dictionary.Remove(key);
			if (result)
			{
				KeyAddOrder.Remove(key);
			}
			return result;
		}

		public void Clear()
		{
			KeyAddOrder.Clear();
			Dictionary.Clear();
		}
	}
}
