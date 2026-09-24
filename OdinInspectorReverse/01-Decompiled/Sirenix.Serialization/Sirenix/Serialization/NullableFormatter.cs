namespace Sirenix.Serialization
{
	/// <summary>
	/// Formatter for all <see cref="T:System.Nullable`1" /> types.
	/// </summary>
	/// <typeparam name="T">The type that is nullable.</typeparam>
	/// <seealso cref="!:BaseFormatter&lt;T?&gt;" />
	public sealed class NullableFormatter<T> : BaseFormatter<T?> where T : struct
	{
		private static readonly Serializer<T> TSerializer;

		static NullableFormatter()
		{
			TSerializer = Serializer.Get<T>();
			new NullableFormatter<int>();
		}

		/// <summary>
		/// Creates a new instance of <see cref="T:Sirenix.Serialization.NullableFormatter`1" />.
		/// </summary>
		public NullableFormatter()
		{
		}

		/// <summary>
		/// Provides the actual implementation for deserializing a value of type <see cref="!:T" />.
		/// </summary>
		/// <param name="value">The uninitialized value to serialize into. This value will have been created earlier using <see cref="M:OdinSerializer.BaseFormatter`1.GetUninitializedObject" />.</param>
		/// <param name="reader">The reader to deserialize with.</param>
		protected override void DeserializeImplementation(ref T? value, IDataReader reader)
		{
			string name;
			EntryType entry = reader.PeekEntry(out name);
			if (entry == EntryType.Null)
			{
				value = null;
				reader.ReadNull();
			}
			else
			{
				value = TSerializer.ReadValue(reader);
			}
		}

		/// <summary>
		/// Provides the actual implementation for serializing a value of type <see cref="!:T" />.
		/// </summary>
		/// <param name="value">The value to serialize.</param>
		/// <param name="writer">The writer to serialize with.</param>
		protected override void SerializeImplementation(ref T? value, IDataWriter writer)
		{
			if (value.HasValue)
			{
				TSerializer.WriteValue(value.Value, writer);
			}
			else
			{
				writer.WriteNull(null);
			}
		}
	}
}
