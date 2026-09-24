#define UNITY_EDITOR
namespace Sirenix.Serialization
{
	/// <summary>
	/// Serializer for the <see cref="T:System.UInt64" /> type.
	/// </summary>
	/// <seealso cref="!:Serializer&lt;System.UInt64&gt;" />
	public sealed class UInt64Serializer : Serializer<ulong>
	{
		/// <summary>
		/// Reads a value of type <see cref="T:System.UInt64" />.
		/// </summary>
		/// <param name="reader">The reader to use.</param>
		/// <returns>
		/// The value which has been read.
		/// </returns>
		public override ulong ReadValue(IDataReader reader)
		{
			string name;
			EntryType entry = reader.PeekEntry(out name);
			if (entry == EntryType.Integer)
			{
				if (!reader.ReadUInt64(out var value))
				{
					reader.Context.Config.DebugContext.LogWarning("Failed to read entry '" + name + "' of type " + entry);
				}
				return value;
			}
			reader.Context.Config.DebugContext.LogWarning("Expected entry of type " + EntryType.Integer.ToString() + ", but got entry '" + name + "' of type " + entry);
			reader.SkipEntry();
			return 0uL;
		}

		/// <summary>
		/// Writes a value of type <see cref="T:System.UInt64" />.
		/// </summary>
		/// <param name="name">The name of the value to write.</param>
		/// <param name="value">The value to write.</param>
		/// <param name="writer">The writer to use.</param>
		public override void WriteValue(string name, ulong value, IDataWriter writer)
		{
			Serializer<ulong>.FireOnSerializedType();
			writer.WriteUInt64(name, value);
		}
	}
}
