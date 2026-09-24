using System;
using System.Collections;

namespace Sirenix.Serialization
{
	/// <summary>
	/// Custom formatter for the type <see cref="T:System.Collections.ArrayList" />.
	/// </summary>
	/// <seealso cref="!:BaseFormatter&lt;System.Collections.Generic.List&lt;T&gt;&gt;" />
	public class ArrayListFormatter : BaseFormatter<ArrayList>
	{
		private static readonly Serializer<object> ObjectSerializer = Serializer.Get<object>();

		/// <summary>
		/// Returns null.
		/// </summary>
		/// <returns>
		/// A null value.
		/// </returns>
		protected override ArrayList GetUninitializedObject()
		{
			return null;
		}

		/// <summary>
		/// Provides the actual implementation for deserializing a value of type <see cref="!:T" />.
		/// </summary>
		/// <param name="value">The uninitialized value to serialize into. This value will have been created earlier using <see cref="M:Sirenix.Serialization.BaseFormatter`1.GetUninitializedObject" />.</param>
		/// <param name="reader">The reader to deserialize with.</param>
		protected override void DeserializeImplementation(ref ArrayList value, IDataReader reader)
		{
			string name;
			EntryType entry = reader.PeekEntry(out name);
			if (entry == EntryType.StartOfArray)
			{
				try
				{
					reader.EnterArray(out var length);
					value = new ArrayList((int)length);
					RegisterReferenceID(value, reader);
					for (int i = 0; i < length; i++)
					{
						if (reader.PeekEntry(out name) == EntryType.EndOfArray)
						{
							reader.Context.Config.DebugContext.LogError("Reached end of array after " + i + " elements, when " + length + " elements were expected.");
							break;
						}
						value.Add(ObjectSerializer.ReadValue(reader));
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
		protected override void SerializeImplementation(ref ArrayList value, IDataWriter writer)
		{
			try
			{
				writer.BeginArrayNode(value.Count);
				for (int i = 0; i < value.Count; i++)
				{
					try
					{
						ObjectSerializer.WriteValue(value[i], writer);
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
