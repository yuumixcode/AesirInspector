using System;
using System.Collections.Generic;
using Sirenix.Serialization.Utilities;

namespace Sirenix.Serialization
{
	/// <summary>
	/// Custom Odin serialization formatter for <see cref="T:Sirenix.Serialization.Utilities.DoubleLookupDictionary`3" />.
	/// </summary>
	/// <typeparam name="TPrimary">Type of primary key.</typeparam>
	/// <typeparam name="TSecondary">Type of secondary key.</typeparam>
	/// <typeparam name="TValue">Type of value.</typeparam>
	internal sealed class DoubleLookupDictionaryFormatter<TPrimary, TSecondary, TValue> : BaseFormatter<DoubleLookupDictionary<TPrimary, TSecondary, TValue>>
	{
		private static readonly Serializer<TPrimary> PrimaryReaderWriter;

		private static readonly Serializer<Dictionary<TSecondary, TValue>> InnerReaderWriter;

		static DoubleLookupDictionaryFormatter()
		{
			PrimaryReaderWriter = Serializer.Get<TPrimary>();
			InnerReaderWriter = Serializer.Get<Dictionary<TSecondary, TValue>>();
			new DoubleLookupDictionaryFormatter<int, int, string>();
		}

		/// <summary>
		/// Creates a new instance of <see cref="T:Sirenix.Serialization.DoubleLookupDictionaryFormatter`3" />.
		/// </summary>
		public DoubleLookupDictionaryFormatter()
		{
		}

		/// <summary>
		/// Returns null.
		/// </summary>
		protected override DoubleLookupDictionary<TPrimary, TSecondary, TValue> GetUninitializedObject()
		{
			return null;
		}

		/// <summary>
		/// Provides the actual implementation for serializing a value of type <see cref="!:T" />.
		/// </summary>
		/// <param name="value">The value to serialize.</param>
		/// <param name="writer">The writer to serialize with.</param>
		protected override void SerializeImplementation(ref DoubleLookupDictionary<TPrimary, TSecondary, TValue> value, IDataWriter writer)
		{
			try
			{
				writer.BeginArrayNode(value.Count);
				bool endNode = true;
				foreach (KeyValuePair<TPrimary, Dictionary<TSecondary, TValue>> pair in value)
				{
					try
					{
						writer.BeginStructNode(null, null);
						PrimaryReaderWriter.WriteValue("$k", pair.Key, writer);
						InnerReaderWriter.WriteValue("$v", pair.Value, writer);
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

		/// <summary>
		/// Provides the actual implementation for deserializing a value of type <see cref="!:T" />.
		/// </summary>
		/// <param name="value">The uninitialized value to serialize into. This value will have been created earlier using <see cref="M:OdinSerializer.BaseFormatter`1.GetUninitializedObject" />.</param>
		/// <param name="reader">The reader to deserialize with.</param>
		protected override void DeserializeImplementation(ref DoubleLookupDictionary<TPrimary, TSecondary, TValue> value, IDataReader reader)
		{
			string name;
			EntryType entry = reader.PeekEntry(out name);
			if (entry == EntryType.StartOfArray)
			{
				try
				{
					reader.EnterArray(out var length);
					value = new DoubleLookupDictionary<TPrimary, TSecondary, TValue>();
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
							TPrimary key = PrimaryReaderWriter.ReadValue(reader);
							Dictionary<TSecondary, TValue> inner = InnerReaderWriter.ReadValue(reader);
							value.Add(key, inner);
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
	}
}
