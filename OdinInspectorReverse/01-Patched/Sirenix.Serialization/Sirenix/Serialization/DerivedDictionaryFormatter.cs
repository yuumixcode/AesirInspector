using System;
using System.Collections.Generic;
using System.Reflection;

namespace Sirenix.Serialization
{
	/// <summary>
	/// Emergency hack class to support serialization of types derived from dictionary
	/// </summary>
	internal sealed class DerivedDictionaryFormatter<TDictionary, TKey, TValue> : BaseFormatter<TDictionary> where TDictionary : Dictionary<TKey, TValue>, new()
	{
		private static readonly bool KeyIsValueType;

		private static readonly Serializer<IEqualityComparer<TKey>> EqualityComparerSerializer;

		private static readonly Serializer<TKey> KeyReaderWriter;

		private static readonly Serializer<TValue> ValueReaderWriter;

		private static readonly ConstructorInfo ComparerConstructor;

		static DerivedDictionaryFormatter()
		{
			KeyIsValueType = typeof(TKey).IsValueType;
			EqualityComparerSerializer = Serializer.Get<IEqualityComparer<TKey>>();
			KeyReaderWriter = Serializer.Get<TKey>();
			ValueReaderWriter = Serializer.Get<TValue>();
			ComparerConstructor = typeof(TDictionary).GetConstructor(new Type[1] { typeof(IEqualityComparer<TKey>) });
			new DerivedDictionaryFormatter<Dictionary<int, string>, int, string>();
		}

		/// <summary>
		/// Returns null.
		/// </summary>
		/// <returns>
		/// A value of null.
		/// </returns>
		protected override TDictionary GetUninitializedObject()
		{
			return null;
		}

		/// <summary>
		/// Provides the actual implementation for deserializing a value of type <see cref="!:T" />.
		/// </summary>
		/// <param name="value">The uninitialized value to serialize into. This value will have been created earlier using <see cref="M:Sirenix.Serialization.BaseFormatter`1.GetUninitializedObject" />.</param>
		/// <param name="reader">The reader to deserialize with.</param>
		protected override void DeserializeImplementation(ref TDictionary value, IDataReader reader)
		{
			string name;
			EntryType entry = reader.PeekEntry(out name);
			IEqualityComparer<TKey> comparer = null;
			if (name == "comparer" || entry == EntryType.StartOfNode)
			{
				comparer = EqualityComparerSerializer.ReadValue(reader);
				entry = reader.PeekEntry(out name);
			}
			if (entry == EntryType.StartOfArray)
			{
				try
				{
					reader.EnterArray(out var length);
					if (comparer != null && ComparerConstructor != null)
					{
						value = (TDictionary)ComparerConstructor.Invoke(new object[1] { comparer });
					}
					else
					{
						value = new TDictionary();
					}
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
		protected override void SerializeImplementation(ref TDictionary value, IDataWriter writer)
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
