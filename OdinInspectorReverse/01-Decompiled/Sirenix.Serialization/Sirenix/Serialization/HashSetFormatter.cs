using System;
using System.Collections.Generic;

namespace Sirenix.Serialization
{
	/// <summary>
	/// Custom generic formatter for the generic type definition <see cref="T:System.Collections.Generic.HashSet`1" />.
	/// </summary>
	/// <typeparam name="T">The element type of the formatted list.</typeparam>
	/// <seealso cref="!:BaseFormatter&lt;System.Collections.Generic.HashSet&lt;T&gt;&gt;" />
	public class HashSetFormatter<T> : BaseFormatter<HashSet<T>>
	{
		private static readonly Serializer<T> TSerializer;

		static HashSetFormatter()
		{
			TSerializer = Serializer.Get<T>();
			new HashSetFormatter<int>();
		}

		/// <summary>
		/// Returns null.
		/// </summary>
		/// <returns>
		/// A null value.
		/// </returns>
		protected override HashSet<T> GetUninitializedObject()
		{
			return null;
		}

		/// <summary>
		/// Provides the actual implementation for deserializing a value of type <see cref="!:T" />.
		/// </summary>
		/// <param name="value">The uninitialized value to serialize into. This value will have been created earlier using <see cref="M:Sirenix.Serialization.BaseFormatter`1.GetUninitializedObject" />.</param>
		/// <param name="reader">The reader to deserialize with.</param>
		protected override void DeserializeImplementation(ref HashSet<T> value, IDataReader reader)
		{
			string name;
			EntryType entry = reader.PeekEntry(out name);
			if (entry == EntryType.StartOfArray)
			{
				try
				{
					reader.EnterArray(out var length);
					value = new HashSet<T>();
					RegisterReferenceID(value, reader);
					for (int i = 0; i < length; i++)
					{
						if (reader.PeekEntry(out name) == EntryType.EndOfArray)
						{
							reader.Context.Config.DebugContext.LogError("Reached end of array after " + i + " elements, when " + length + " elements were expected.");
							break;
						}
						value.Add(TSerializer.ReadValue(reader));
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
		protected override void SerializeImplementation(ref HashSet<T> value, IDataWriter writer)
		{
			try
			{
				writer.BeginArrayNode(value.Count);
				foreach (T item in value)
				{
					try
					{
						TSerializer.WriteValue(item, writer);
					}
					catch (Exception exception)
					{
						writer.Context.Config.DebugContext.LogException(exception);
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
