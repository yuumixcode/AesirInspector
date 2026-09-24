using System;
using System.Globalization;

namespace Sirenix.Serialization
{
	/// <summary>
	/// Custom formatter for the <see cref="T:System.DateTimeOffset" /> type.
	/// </summary>
	/// <seealso cref="!:MinimalBaseFormatter&lt;System.DateTimeOffset&gt;" />
	public sealed class DateTimeOffsetFormatter : MinimalBaseFormatter<DateTimeOffset>
	{
		/// <summary>
		/// Reads into the specified value using the specified reader.
		/// </summary>
		/// <param name="value">The value to read into.</param>
		/// <param name="reader">The reader to use.</param>
		protected override void Read(ref DateTimeOffset value, IDataReader reader)
		{
			if (reader.PeekEntry(out var _) == EntryType.String)
			{
				reader.ReadString(out var str);
				DateTimeOffset.TryParse(str, out value);
			}
		}

		/// <summary>
		/// Writes from the specified value using the specified writer.
		/// </summary>
		/// <param name="value">The value to write from.</param>
		/// <param name="writer">The writer to use.</param>
		protected override void Write(ref DateTimeOffset value, IDataWriter writer)
		{
			writer.WriteString(null, value.ToString("O", CultureInfo.InvariantCulture));
		}
	}
}
