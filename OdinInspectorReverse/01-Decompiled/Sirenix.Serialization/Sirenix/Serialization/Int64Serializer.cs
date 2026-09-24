#define UNITY_EDITOR
namespace Sirenix.Serialization
{
	/// <summary>
	/// Serializer for the <see cref="T:System.Int64" /> type.
	/// </summary>
	/// <seealso cref="!:Serializer&lt;System.Int64&gt;" />
	public sealed class Int64Serializer : Serializer<long>
	{
		/// <summary>
		/// Reads a value of type <see cref="T:System.Int64" />.
		/// </summary>
		/// <param name="reader">The reader to use.</param>
		/// <returns>
		/// The value which has been read.
		/// </returns>
		public override long ReadValue(IDataReader reader)
		{
			string name;
			EntryType entry = reader.PeekEntry(out name);
			if (entry == EntryType.Integer)
			{
				if (!reader.ReadInt64(out var value))
				{
					reader.Context.Config.DebugContext.LogWarning("Failed to read entry '" + name + "' of type " + entry);
				}
				return value;
			}
			reader.Context.Config.DebugContext.LogWarning("Expected entry of type " + EntryType.Integer.ToString() + ", but got entry '" + name + "' of type " + entry);
			reader.SkipEntry();
			return 0L;
		}

		/// <summary>
		/// Writes a value of type <see cref="T:System.Int64" />.
		/// </summary>
		/// <param name="name">The name of the value to write.</param>
		/// <param name="value">The value to write.</param>
		/// <param name="writer">The writer to use.</param>
		public override void WriteValue(string name, long value, IDataWriter writer)
		{
			Serializer<long>.FireOnSerializedType();
			writer.WriteInt64(name, value);
		}
	}
}
