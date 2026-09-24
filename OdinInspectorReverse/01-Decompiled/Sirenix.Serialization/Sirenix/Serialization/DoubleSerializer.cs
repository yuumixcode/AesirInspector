#define UNITY_EDITOR
namespace Sirenix.Serialization
{
	/// <summary>
	/// Serializer for the <see cref="T:System.Double" /> type.
	/// </summary>
	/// <seealso cref="!:Serializer&lt;System.Double&gt;" />
	public sealed class DoubleSerializer : Serializer<double>
	{
		/// <summary>
		/// Reads a value of type <see cref="T:System.Double" />.
		/// </summary>
		/// <param name="reader">The reader to use.</param>
		/// <returns>
		/// The value which has been read.
		/// </returns>
		public override double ReadValue(IDataReader reader)
		{
			string name;
			EntryType entry = reader.PeekEntry(out name);
			if (entry == EntryType.FloatingPoint || entry == EntryType.Integer)
			{
				if (!reader.ReadDouble(out var value))
				{
					reader.Context.Config.DebugContext.LogWarning("Failed to read entry '" + name + "' of type " + entry);
				}
				return value;
			}
			reader.Context.Config.DebugContext.LogWarning("Expected entry of type " + EntryType.FloatingPoint.ToString() + " or " + EntryType.Integer.ToString() + ", but got entry '" + name + "' of type " + entry);
			reader.SkipEntry();
			return 0.0;
		}

		/// <summary>
		/// Writes a value of type <see cref="T:System.Double" />.
		/// </summary>
		/// <param name="name">The name of the value to write.</param>
		/// <param name="value">The value to write.</param>
		/// <param name="writer">The writer to use.</param>
		public override void WriteValue(string name, double value, IDataWriter writer)
		{
			Serializer<double>.FireOnSerializedType();
			writer.WriteDouble(name, value);
		}
	}
}
