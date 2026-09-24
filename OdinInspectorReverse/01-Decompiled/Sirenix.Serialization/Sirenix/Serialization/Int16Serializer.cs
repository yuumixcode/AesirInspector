#define UNITY_EDITOR
namespace Sirenix.Serialization
{
	/// <summary>
	/// Serializer for the <see cref="T:System.Int16" /> type.
	/// </summary>
	/// <seealso cref="!:Serializer&lt;System.Int16&gt;" />
	public sealed class Int16Serializer : Serializer<short>
	{
		/// <summary>
		/// Reads a value of type <see cref="T:System.Int16" />.
		/// </summary>
		/// <param name="reader">The reader to use.</param>
		/// <returns>
		/// The value which has been read.
		/// </returns>
		public override short ReadValue(IDataReader reader)
		{
			string name;
			EntryType entry = reader.PeekEntry(out name);
			if (entry == EntryType.Integer)
			{
				if (!reader.ReadInt16(out var value))
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
		/// Writes a value of type <see cref="T:System.Int16" />.
		/// </summary>
		/// <param name="name">The name of the value to write.</param>
		/// <param name="value">The value to write.</param>
		/// <param name="writer">The writer to use.</param>
		public override void WriteValue(string name, short value, IDataWriter writer)
		{
			Serializer<short>.FireOnSerializedType();
			writer.WriteInt16(name, value);
		}
	}
}
