namespace Sirenix.Serialization
{
	/// <summary>
	/// A formatter for empty types. It writes no data, and skips all data that is to be read, deserializing a "default" value.
	/// </summary>
	public class EmptyTypeFormatter<T> : EasyBaseFormatter<T>
	{
		/// <summary>
		/// Skips the entry to read.
		/// </summary>
		protected override void ReadDataEntry(ref T value, string entryName, EntryType entryType, IDataReader reader)
		{
			reader.SkipEntry();
		}

		/// <summary>
		/// Does nothing at all.
		/// </summary>
		protected override void WriteDataEntries(ref T value, IDataWriter writer)
		{
		}
	}
}
