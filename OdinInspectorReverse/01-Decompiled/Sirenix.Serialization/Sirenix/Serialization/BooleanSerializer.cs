#define UNITY_EDITOR
namespace Sirenix.Serialization
{
	/// <summary>
	/// Serializer for the <see cref="T:System.Boolean" /> type.
	/// </summary>
	/// <seealso cref="!:Serializer&lt;System.Boolean&gt;" />
	public sealed class BooleanSerializer : Serializer<bool>
	{
		/// <summary>
		/// Reads a value of type <see cref="T:System.Boolean" />.
		/// </summary>
		/// <param name="reader">The reader to use.</param>
		/// <returns>
		/// The value which has been read.
		/// </returns>
		public override bool ReadValue(IDataReader reader)
		{
			string name;
			EntryType entry = reader.PeekEntry(out name);
			if (entry == EntryType.Boolean)
			{
				if (!reader.ReadBoolean(out var value))
				{
					reader.Context.Config.DebugContext.LogWarning("Failed to read entry '" + name + "' of type " + entry);
				}
				return value;
			}
			reader.Context.Config.DebugContext.LogWarning("Expected entry of type " + EntryType.Boolean.ToString() + ", but got entry '" + name + "' of type " + entry);
			reader.SkipEntry();
			return false;
		}

		/// <summary>
		/// Writes a value of type <see cref="T:System.Boolean" />.
		/// </summary>
		/// <param name="name">The name of the value to write.</param>
		/// <param name="value">The value to write.</param>
		/// <param name="writer">The writer to use.</param>
		public override void WriteValue(string name, bool value, IDataWriter writer)
		{
			Serializer<bool>.FireOnSerializedType();
			writer.WriteBoolean(name, value);
		}
	}
}
