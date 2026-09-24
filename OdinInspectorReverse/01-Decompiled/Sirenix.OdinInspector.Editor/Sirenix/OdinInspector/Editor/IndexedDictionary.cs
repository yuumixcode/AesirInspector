using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.Serialization;

namespace Sirenix.OdinInspector.Editor
{
	[AlwaysFormatsSelf]
	internal class IndexedDictionary : IEnumerable<KeyValuePair<ContextKey, GlobalPersistentContext>>, IEnumerable, ISelfFormatter
	{
		private class CKC : IEqualityComparer<ContextKey>
		{
			public bool Equals(ContextKey x, ContextKey y)
			{
				if (x.Key1234 == y.Key1234)
				{
					return x.Key5 == y.Key5;
				}
				return false;
			}

			public int GetHashCode(ContextKey obj)
			{
				return obj.GetHashCode();
			}
		}

		private Dictionary<ContextKey, GlobalPersistentContext> dictionary;

		private List<ContextKey> indexer;

		private static readonly Dictionary<Type, Type> GlobalPersistentContext_GenericVariantCache = new Dictionary<Type, Type>(FastTypeComparer.Instance);

		private static readonly Serializer<Type> TypeSerializer = Serializer.Get<Type>();

		public int Count => dictionary.Count;

		public GlobalPersistentContext this[ContextKey key]
		{
			get
			{
				return dictionary[key];
			}
			set
			{
				if (dictionary.ContainsKey(key))
				{
					dictionary[key] = value;
				}
				else
				{
					Add(key, value);
				}
			}
		}

		public IndexedDictionary()
		{
			dictionary = new Dictionary<ContextKey, GlobalPersistentContext>(0, new CKC());
			indexer = new List<ContextKey>(0);
		}

		public KeyValuePair<ContextKey, GlobalPersistentContext> Get(int index)
		{
			ContextKey k = indexer[index];
			dictionary.TryGetValue(k, out var val);
			return new KeyValuePair<ContextKey, GlobalPersistentContext>(k, val);
		}

		public ContextKey GeContextKey(int index)
		{
			return indexer[index];
		}

		public void Add(ContextKey key, GlobalPersistentContext value)
		{
			dictionary.Add(key, value);
			indexer.Add(key);
		}

		public void Clear()
		{
			indexer.Clear();
			dictionary.Clear();
		}

		public void RemoveAt(int index)
		{
			if (index >= 0 && index < Count)
			{
				ContextKey k = indexer[index];
				if (!dictionary.Remove(k))
				{
					throw new IndexOutOfRangeException(index.ToString());
				}
				indexer.RemoveAt(index);
			}
		}

		public bool TryGetValue(ContextKey key, out GlobalPersistentContext value)
		{
			return dictionary.TryGetValue(key, out value);
		}

		public IEnumerator<KeyValuePair<ContextKey, GlobalPersistentContext>> GetEnumerator()
		{
			return ((IEnumerable<KeyValuePair<ContextKey, GlobalPersistentContext>>)dictionary).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<KeyValuePair<ContextKey, GlobalPersistentContext>>)dictionary).GetEnumerator();
		}

		public void Serialize(IDataWriter writer)
		{
			writer.BeginArrayNode(indexer.Count);
			for (int i = 0; i < indexer.Count; i++)
			{
				writer.BeginStructNode(null, null);
				ContextKey key = indexer[i];
				GlobalPersistentContext value = Get(i).Value;
				key.Serialize(writer);
				if (value == null)
				{
					writer.WriteNull(null);
				}
				else
				{
					TypeSerializer.WriteValue(value.ValueType, writer);
					value.Serialize(writer);
				}
				writer.EndNode(null);
			}
			writer.EndArrayNode();
		}

		public void Deserialize(IDataReader reader)
		{
			reader.EnterArray(out var length);
			indexer = new List<ContextKey>((int)length);
			dictionary = new Dictionary<ContextKey, GlobalPersistentContext>((int)length, new CKC());
			for (int i = 0; i < length; i++)
			{
				reader.EnterNode(out var _);
				ContextKey key = default(ContextKey);
				key.Deserialize(reader);
				string whoCaresLess;
				EntryType nextEntry = reader.PeekEntry(out whoCaresLess);
				if (nextEntry == EntryType.Null)
				{
					reader.ReadNull();
				}
				else
				{
					Type type = TypeSerializer.ReadValue(reader);
					GlobalPersistentContext value = null;
					if (type != null)
					{
						Type contextType;
						lock (GlobalPersistentContext_GenericVariantCache)
						{
							if (!GlobalPersistentContext_GenericVariantCache.TryGetValue(type, out contextType))
							{
								contextType = typeof(GlobalPersistentContext<>).MakeGenericType(type);
								GlobalPersistentContext_GenericVariantCache.Add(type, contextType);
							}
						}
						value = (GlobalPersistentContext)Activator.CreateInstance(contextType);
						value.Deserialize(reader);
						Add(key, value);
					}
				}
				reader.ExitNode();
			}
			reader.ExitArray();
		}
	}
}
