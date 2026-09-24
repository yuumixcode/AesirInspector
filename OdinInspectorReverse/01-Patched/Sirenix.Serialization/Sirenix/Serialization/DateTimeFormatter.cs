using System;

namespace Sirenix.Serialization
{
	/// <summary>
	/// Custom formatter for the <see cref="T:System.DateTime" /> type.
	/// </summary>
	/// <seealso cref="!:MinimalBaseFormatter&lt;System.DateTime&gt;" />
	public sealed class DateTimeFormatter : MinimalBaseFormatter<DateTime>
	{
		/// <summary>
		/// Reads into the specified value using the specified reader.
		/// </summary>
		/// <param name="value">The value to read into.</param>
		/// <param name="reader">The reader to use.</param>
		protected override void Read(ref DateTime value, IDataReader reader)
		{
			if (reader.PeekEntry(out var _) == EntryType.Integer)
			{
				reader.ReadInt64(out var binary);
				value = DateTime.FromBinary(binary);
			}
		}

		/// <summary>
		/// Writes from the specified value using the specified writer.
		/// </summary>
		/// <param name="value">The value to write from.</param>
		/// <param name="writer">The writer to use.</param>
		protected override void Write(ref DateTime value, IDataWriter writer)
		{
			writer.WriteInt64(null, value.ToBinary());
		}
	}
}
