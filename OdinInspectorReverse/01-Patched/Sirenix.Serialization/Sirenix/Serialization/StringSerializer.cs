#define UNITY_EDITOR
namespace Sirenix.Serialization
{
	/// <summary>
	/// Serializer for the <see cref="T:System.String" /> type.
	/// </summary>
	/// <seealso cref="!:Serializer&lt;System.String&gt;" />
	public sealed class StringSerializer : Serializer<string>
	{
		/// <summary>
		/// Reads a value of type <see cref="T:System.String" />.
		/// </summary>
		/// <param name="reader">The reader to use.</param>
		/// <returns>
		/// The value which has been read.
		/// </returns>
		public override string ReadValue(IDataReader reader)
		{
			string name;
			EntryType entry = reader.PeekEntry(out name);
			switch (entry)
			{
			case EntryType.String:
			{
				if (!reader.ReadString(out var value))
				{
					reader.Context.Config.DebugContext.LogWarning("Failed to read entry '" + name + "' of type " + entry);
				}
				return value;
			}
			case EntryType.Null:
				if (!reader.ReadNull())
				{
					reader.Context.Config.DebugContext.LogWarning("Failed to read entry '" + name + "' of type " + entry);
				}
				return null;
			default:
				reader.Context.Config.DebugContext.LogWarning("Expected entry of type " + EntryType.String.ToString() + " or " + EntryType.Null.ToString() + ", but got entry '" + name + "' of type " + entry);
				reader.SkipEntry();
				return null;
			}
		}

		/// <summary>
		/// Writes a value of type <see cref="T:System.String" />.
		/// </summary>
		/// <param name="name">The name of the value to write.</param>
		/// <param name="value">The value to write.</param>
		/// <param name="writer">The writer to use.</param>
		public override void WriteValue(string name, string value, IDataWriter writer)
		{
			Serializer<string>.FireOnSerializedType();
			if (value == null)
			{
				writer.WriteNull(name);
			}
			else
			{
				writer.WriteString(name, value);
			}
		}
	}
}
