using System;

namespace Sirenix.Serialization
{
	/// <summary>
	/// Custom formatter for the <see cref="T:System.TimeSpan" /> type.
	/// </summary>
	/// <seealso cref="!:MinimalBaseFormatter&lt;System.TimeSpan&gt;" />
	public sealed class TimeSpanFormatter : MinimalBaseFormatter<TimeSpan>
	{
		/// <summary>
		/// Reads into the specified value using the specified reader.
		/// </summary>
		/// <param name="value">The value to read into.</param>
		/// <param name="reader">The reader to use.</param>
		protected override void Read(ref TimeSpan value, IDataReader reader)
		{
			if (reader.PeekEntry(out var _) == EntryType.Integer)
			{
				reader.ReadInt64(out var ticks);
				value = new TimeSpan(ticks);
			}
		}

		/// <summary>
		/// Writes from the specified value using the specified writer.
		/// </summary>
		/// <param name="value">The value to write from.</param>
		/// <param name="writer">The writer to use.</param>
		protected override void Write(ref TimeSpan value, IDataWriter writer)
		{
			writer.WriteInt64(null, value.Ticks);
		}
	}
}
