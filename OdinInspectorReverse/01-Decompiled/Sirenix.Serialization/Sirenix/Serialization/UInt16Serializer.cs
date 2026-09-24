#define UNITY_EDITOR
namespace Sirenix.Serialization
{
	/// <summary>
	/// Serializer for the <see cref="T:System.UInt16" /> type.
	/// </summary>
	/// <seealso cref="!:Serializer&lt;System.UInt16&gt;" />
	public sealed class UInt16Serializer : Serializer<ushort>
	{
		/// <summary>
		/// Reads a value of type <see cref="T:System.UInt16" />.
		/// </summary>
		/// <param name="reader">The reader to use.</param>
		/// <returns>
		/// The value which has been read.
		/// </returns>
		public override ushort ReadValue(IDataReader reader)
		{
			string name;
			EntryType entry = reader.PeekEntry(out name);
			if (entry == EntryType.Integer)
			{
				if (!reader.ReadUInt16(out var value))
				{
					reader.Context.Config.DebugContext.LogWarning("Failed to read entry '" + name + "' of type " + entry);
				}
				return value;
			}
			reader.Context.Config.DebugContext.LogWarning("Expected entry of type " + EntryType.Integer.ToString() + ", but got entry '" + name + "' of type " + entry);
			reader.SkipEntry();
			return 0;
		}

		/// <summary>
		/// Writes a value of type <see cref="T:System.UInt64" />.
		/// </summary>
		/// <param name="name">The name of the value to write.</param>
		/// <param name="value">The value to write.</param>
		/// <param name="writer">The writer to use.</param>
		public override void WriteValue(string name, ushort value, IDataWriter writer)
		{
			Serializer<ushort>.FireOnSerializedType();
			writer.WriteUInt16(name, value);
		}
	}
}
