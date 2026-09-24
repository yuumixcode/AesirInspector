#define UNITY_EDITOR
namespace Sirenix.Serialization
{
	/// <summary>
	/// Serializer for the <see cref="T:System.Single" /> type.
	/// </summary>
	/// <seealso cref="!:Serializer&lt;System.Single&gt;" />
	public sealed class SingleSerializer : Serializer<float>
	{
		/// <summary>
		/// Reads a value of type <see cref="T:System.Single" />.
		/// </summary>
		/// <param name="reader">The reader to use.</param>
		/// <returns>
		/// The value which has been read.
		/// </returns>
		public override float ReadValue(IDataReader reader)
		{
			string name;
			EntryType entry = reader.PeekEntry(out name);
			if (entry == EntryType.FloatingPoint || entry == EntryType.Integer)
			{
				if (!reader.ReadSingle(out var value))
				{
					reader.Context.Config.DebugContext.LogWarning("Failed to read entry '" + name + "' of type " + entry);
				}
				return value;
			}
			reader.Context.Config.DebugContext.LogWarning("Expected entry of type " + EntryType.FloatingPoint.ToString() + " or " + EntryType.Integer.ToString() + ", but got entry '" + name + "' of type " + entry);
			reader.SkipEntry();
			return 0f;
		}

		/// <summary>
		/// Writes a value of type <see cref="T:System.Single" />.
		/// </summary>
		/// <param name="name">The name of the value to write.</param>
		/// <param name="value">The value to write.</param>
		/// <param name="writer">The writer to use.</param>
		public override void WriteValue(string name, float value, IDataWriter writer)
		{
			Serializer<float>.FireOnSerializedType();
			writer.WriteSingle(name, value);
		}
	}
}
