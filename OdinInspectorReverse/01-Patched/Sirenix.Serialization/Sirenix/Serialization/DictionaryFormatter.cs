using System;
using System.Collections.Generic;

namespace Sirenix.Serialization
{
	/// <summary>
	/// Custom generic formatter for the generic type definition <see cref="T:System.Collections.Generic.Dictionary`2" />.
	/// </summary>
	/// <typeparam name="TKey">The type of the dictionary key.</typeparam>
	/// <typeparam name="TValue">The type of the dictionary value.</typeparam>
	/// <seealso cref="!:BaseFormatter&lt;System.Collections.Generic.Dictionary&lt;TKey, TValue&gt;&gt;" />
	public sealed class DictionaryFormatter<TKey, TValue> : BaseFormatter<Dictionary<TKey, TValue>>
	{
		private static readonly bool KeyIsValueType;

		private static readonly Serializer<IEqualityComparer<TKey>> EqualityComparerSerializer;

		private static readonly Serializer<TKey> KeyReaderWriter;

		private static readonly Serializer<TValue> ValueReaderWriter;

		static DictionaryFormatter()
		{
			KeyIsValueType = typeof(TKey).IsValueType;
			EqualityComparerSerializer = Serializer.Get<IEqualityComparer<TKey>>();
			KeyReaderWriter = Serializer.Get<TKey>();
			ValueReaderWriter = Serializer.Get<TValue>();
			new DictionaryFormatter<int, string>();
		}

		/// <summary>
		/// Creates a new instance of <see cref="T:Sirenix.Serialization.DictionaryFormatter`2" />.
		/// </summary>
		public DictionaryFormatter()
		{
		}

		/// <summary>
		/// Returns null.
		/// </summary>
		/// <returns>
		/// A value of null.
		/// </returns>
		protected override Dictionary<TKey, TValue> GetUninitializedObject()
		{
			return null;
		}

		/// <summary>
		/// Provides the actual implementation for deserializing a value of type <see cref="!:T" />.
		/// </summary>
		/// <param name="value">The uninitialized value to serialize into. This value will have been created earlier using <see cref="M:Sirenix.Serialization.BaseFormatter`1.GetUninitializedObject" />.</param>
		/// <param name="reader">The reader to deserialize with.</param>
		protected override void DeserializeImplementation(ref Dictionary<TKey, TValue> value, IDataReader reader)
		{
			string name;
			EntryType entry = reader.PeekEntry(out name);
			IEqualityComparer<TKey> comparer = null;
			if (name == "comparer" || entry != EntryType.StartOfArray)
			{
				comparer = EqualityComparerSerializer.ReadValue(reader);
				entry = reader.PeekEntry(out name);
			}
			if (entry == EntryType.StartOfArray)
			{
				try
				{
					reader.EnterArray(out var length);
					value = ((comparer == null) ? new Dictionary<TKey, TValue>((int)length) : new Dictionary<TKey, TValue>((int)length, comparer));
					RegisterReferenceID(value, reader);
					for (int i = 0; i < length; i++)
					{
						if (reader.PeekEntry(out name) == EntryType.EndOfArray)
						{
							reader.Context.Config.DebugContext.LogError("Reached end of array after " + i + " elements, when " + length + " elements were expected.");
							break;
						}
						bool exitNode = true;
						try
						{
							reader.EnterNode(out var _);
							TKey key = KeyReaderWriter.ReadValue(reader);
							TValue val = ValueReaderWriter.ReadValue(reader);
							if (!KeyIsValueType && key == null)
							{
								reader.Context.Config.DebugContext.LogWarning("Dictionary key of type '" + typeof(TKey).FullName + "' was null upon deserialization. A key has gone missing.");
								continue;
							}
							value[key] = val;
						}
						catch (SerializationAbortException ex)
						{
							exitNode = false;
							throw ex;
						}
						catch (Exception exception)
						{
							reader.Context.Config.DebugContext.LogException(exception);
						}
						finally
						{
							if (exitNode)
							{
								reader.ExitNode();
							}
						}
						if (!reader.IsInArrayNode)
						{
							reader.Context.Config.DebugContext.LogError("Reading array went wrong. Data dump: " + reader.GetDataDump());
							break;
						}
					}
					return;
				}
				finally
				{
					reader.ExitArray();
				}
			}
			reader.SkipEntry();
		}

		/// <summary>
		/// Provides the actual implementation for serializing a value of type <see cref="!:T" />.
		/// </summary>
		/// <param name="value">The value to serialize.</param>
		/// <param name="writer">The writer to serialize with.</param>
		protected override void SerializeImplementation(ref Dictionary<TKey, TValue> value, IDataWriter writer)
		{
			try
			{
				if (value.Comparer != null)
				{
					EqualityComparerSerializer.WriteValue("comparer", value.Comparer, writer);
				}
				writer.BeginArrayNode(value.Count);
				foreach (KeyValuePair<TKey, TValue> pair in value)
				{
					bool endNode = true;
					try
					{
						writer.BeginStructNode(null, null);
						KeyReaderWriter.WriteValue("$k", pair.Key, writer);
						ValueReaderWriter.WriteValue("$v", pair.Value, writer);
					}
					catch (SerializationAbortException ex)
					{
						endNode = false;
						throw ex;
					}
					catch (Exception exception)
					{
						writer.Context.Config.DebugContext.LogException(exception);
					}
					finally
					{
						if (endNode)
						{
							writer.EndNode(null);
						}
					}
				}
			}
			finally
			{
				writer.EndArrayNode();
			}
		}
	}
}
