#define UNITY_EDITOR
namespace Sirenix.Serialization
{
	/// <summary>
	/// Serializer for the <see cref="T:System.Char" /> type.
	/// </summary>
	/// <seealso cref="!:Serializer&lt;System.Char&gt;" />
	public sealed class CharSerializer : Serializer<char>
	{
		/// <summary>
		/// Reads a value of type <see cref="T:System.Char" />.
		/// </summary>
		/// <param name="reader">The reader to use.</param>
		/// <returns>
		/// The value which has been read.
		/// </returns>
		public override char ReadValue(IDataReader reader)
		{
			string name;
			EntryType entry = reader.PeekEntry(out name);
			if (entry == EntryType.String)
			{
				if (!reader.ReadChar(out var value))
				{
					reader.Context.Config.DebugContext.LogWarning("Failed to read entry '" + name + "' of type " + entry);
				}
				return value;
			}
			reader.Context.Config.DebugContext.LogWarning("Expected entry of type " + EntryType.String.ToString() + ", but got entry '" + name + "' of type " + entry);
			reader.SkipEntry();
			return '\0';
		}

		/// <summary>
		/// Writes a value of type <see cref="T:System.Char" />.
		/// </summary>
		/// <param name="name">The name of the value to write.</param>
		/// <param name="value">The value to write.</param>
		/// <param name="writer">The writer to use.</param>
		public override void WriteValue(string name, char value, IDataWriter writer)
		{
			Serializer<char>.FireOnSerializedType();
			writer.WriteChar(name, value);
		}
	}
}
