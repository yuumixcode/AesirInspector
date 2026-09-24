namespace Sirenix.Serialization
{
	/// <summary>
	/// Formatter for all non-primitive one-dimensional arrays.
	/// </summary>
	/// <typeparam name="T">The element type of the formatted array.</typeparam>
	/// <seealso cref="!:BaseFormatter&lt;T[]&gt;" />
	public sealed class ArrayFormatter<T> : BaseFormatter<T[]>
	{
		private static Serializer<T> valueReaderWriter = Serializer.Get<T>();

		/// <summary>
		/// Returns null.
		/// </summary>
		/// <returns>
		/// A null value.
		/// </returns>
		protected override T[] GetUninitializedObject()
		{
			return null;
		}

		/// <summary>
		/// Provides the actual implementation for deserializing a value of type <see cref="!:T" />.
		/// </summary>
		/// <param name="value">The uninitialized value to serialize into. This value will have been created earlier using <see cref="M:Sirenix.Serialization.BaseFormatter`1.GetUninitializedObject" />.</param>
		/// <param name="reader">The reader to deserialize with.</param>
		protected override void DeserializeImplementation(ref T[] value, IDataReader reader)
		{
			string name;
			EntryType entry = reader.PeekEntry(out name);
			if (entry == EntryType.StartOfArray)
			{
				reader.EnterArray(out var length);
				value = new T[length];
				RegisterReferenceID(value, reader);
				for (int i = 0; i < length; i++)
				{
					if (reader.PeekEntry(out name) == EntryType.EndOfArray)
					{
						reader.Context.Config.DebugContext.LogError("Reached end of array after " + i + " elements, when " + length + " elements were expected.");
						break;
					}
					value[i] = valueReaderWriter.ReadValue(reader);
					if (reader.PeekEntry(out name) == EntryType.EndOfStream)
					{
						break;
					}
				}
				reader.ExitArray();
			}
			else
			{
				reader.SkipEntry();
			}
		}

		/// <summary>
		/// Provides the actual implementation for serializing a value of type <see cref="!:T" />.
		/// </summary>
		/// <param name="value">The value to serialize.</param>
		/// <param name="writer">The writer to serialize with.</param>
		protected override void SerializeImplementation(ref T[] value, IDataWriter writer)
		{
			try
			{
				writer.BeginArrayNode(value.Length);
				for (int i = 0; i < value.Length; i++)
				{
					valueReaderWriter.WriteValue(value[i], writer);
				}
			}
			finally
			{
				writer.EndArrayNode();
			}
		}
	}
}
