namespace Sirenix.Serialization
{
	/// <summary>
	/// Formatter for all primitive one-dimensional arrays.
	/// </summary>
	/// <typeparam name="T">The element type of the formatted array. This type must be an eligible primitive array type, as determined by <see cref="M:Sirenix.Serialization.FormatterUtilities.IsPrimitiveArrayType(System.Type)" />.</typeparam>
	/// <seealso cref="!:MinimalBaseFormatter&lt;T[]&gt;" />
	public sealed class PrimitiveArrayFormatter<T> : MinimalBaseFormatter<T[]> where T : struct
	{
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
		/// Reads into the specified value using the specified reader.
		/// </summary>
		/// <param name="value">The value to read into.</param>
		/// <param name="reader">The reader to use.</param>
		protected override void Read(ref T[] value, IDataReader reader)
		{
			if (reader.PeekEntry(out var _) == EntryType.PrimitiveArray)
			{
				reader.ReadPrimitiveArray<T>(out value);
				RegisterReferenceID(value, reader);
			}
			else
			{
				reader.SkipEntry();
			}
		}

		/// <summary>
		/// Writes from the specified value using the specified writer.
		/// </summary>
		/// <param name="value">The value to write from.</param>
		/// <param name="writer">The writer to use.</param>
		protected override void Write(ref T[] value, IDataWriter writer)
		{
			writer.WritePrimitiveArray(value);
		}
	}
}
