using System;
using System.Collections.Generic;

namespace Sirenix.Utilities
{
	/// <summary>
	/// Not yet documented.
	/// </summary>
	[Serializable]
	public class DoubleLookupDictionary<TFirstKey, TSecondKey, TValue> : Dictionary<TFirstKey, Dictionary<TSecondKey, TValue>>
	{
		private readonly IEqualityComparer<TSecondKey> secondKeyComparer;

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public new Dictionary<TSecondKey, TValue> this[TFirstKey firstKey]
		{
			get
			{
				if (!TryGetValue(firstKey, out var innerDict))
				{
					innerDict = new Dictionary<TSecondKey, TValue>(secondKeyComparer);
					Add(firstKey, innerDict);
				}
				return innerDict;
			}
		}

		public DoubleLookupDictionary()
		{
			secondKeyComparer = EqualityComparer<TSecondKey>.Default;
		}

		public DoubleLookupDictionary(IEqualityComparer<TFirstKey> firstKeyComparer, IEqualityComparer<TSecondKey> secondKeyComparer)
			: base(firstKeyComparer)
		{
			this.secondKeyComparer = secondKeyComparer;
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public int InnerCount(TFirstKey firstKey)
		{
			if (TryGetValue(firstKey, out var innerDict))
			{
				return innerDict.Count;
			}
			return 0;
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public int TotalInnerCount()
		{
			int count = 0;
			if (base.Count > 0)
			{
				foreach (Dictionary<TSecondKey, TValue> innerDict in base.Values)
				{
					count += innerDict.Count;
				}
			}
			return count;
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public bool ContainsKeys(TFirstKey firstKey, TSecondKey secondKey)
		{
			if (TryGetValue(firstKey, out var innerDict))
			{
				return innerDict.ContainsKey(secondKey);
			}
			return false;
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public bool TryGetInnerValue(TFirstKey firstKey, TSecondKey secondKey, out TValue value)
		{
			if (TryGetValue(firstKey, out var innerDict) && innerDict.TryGetValue(secondKey, out value))
			{
				return true;
			}
			value = default(TValue);
			return false;
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public TValue AddInner(TFirstKey firstKey, TSecondKey secondKey, TValue value)
		{
			if (ContainsKeys(firstKey, secondKey))
			{
				throw new ArgumentException("An element with the same keys already exists in the " + GetType().GetNiceName() + ".");
			}
			return this[firstKey][secondKey] = value;
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public bool RemoveInner(TFirstKey firstKey, TSecondKey secondKey)
		{
			if (TryGetValue(firstKey, out var innerDict))
			{
				bool removed = innerDict.Remove(secondKey);
				if (innerDict.Count == 0)
				{
					Remove(firstKey);
				}
				return removed;
			}
			return false;
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public void RemoveWhere(Func<TValue, bool> predicate)
		{
			List<TFirstKey> toRemoveBufferFirstKey = new List<TFirstKey>();
			List<TSecondKey> toRemoveBufferSecondKey = new List<TSecondKey>();
			foreach (KeyValuePair<TFirstKey, Dictionary<TSecondKey, TValue>> outerDictionary in this.GFIterator())
			{
				foreach (KeyValuePair<TSecondKey, TValue> innerKeyPair in outerDictionary.Value.GFIterator())
				{
					if (predicate(innerKeyPair.Value))
					{
						toRemoveBufferFirstKey.Add(outerDictionary.Key);
						toRemoveBufferSecondKey.Add(innerKeyPair.Key);
					}
				}
			}
			for (int i = 0; i < toRemoveBufferFirstKey.Count; i++)
			{
				RemoveInner(toRemoveBufferFirstKey[i], toRemoveBufferSecondKey[i]);
			}
		}
	}
}
