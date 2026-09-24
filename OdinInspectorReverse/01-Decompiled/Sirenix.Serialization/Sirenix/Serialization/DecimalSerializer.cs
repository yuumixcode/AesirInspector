#define UNITY_EDITOR
namespace Sirenix.Serialization
{
	/// <summary>
	/// Serializer for the <see cref="T:System.Decimal" /> type.
	/// </summary>
	/// <seealso cref="!:Serializer&lt;System.Decimal&gt;" />
	public sealed class DecimalSerializer : Serializer<decimal>
	{
		/// <summary>
		/// Reads a value of type <see cref="T:System.Decimal" />.
		/// </summary>
		/// <param name="reader">The reader to use.</param>
		/// <returns>
		/// The value which has been read.
		/// </returns>
		public override decimal ReadValue(IDataReader reader)
		{
			string name;
			EntryType entry = reader.PeekEntry(out name);
			if (entry == EntryType.FloatingPoint || entry == EntryType.Integer)
			{
				if (!reader.ReadDecimal(out var value))
				{
					reader.Context.Config.DebugContext.LogWarning("Failed to read entry of type " + entry);
				}
				return value;
			}
			reader.Context.Config.DebugContext.LogWarning("Expected entry of type " + EntryType.FloatingPoint.ToString() + " or " + EntryType.Integer.ToString() + ", but got entry of type " + entry);
			reader.SkipEntry();
			return 0m;
		}

		/// <summary>
		/// Writes a value of type <see cref="T:System.Decimal" />.
		/// </summary>
		/// <param name="name">The name of the value to write.</param>
		/// <param name="value">The value to write.</param>
		/// <param name="writer">The writer to use.</param>
		public override void WriteValue(string name, decimal value, IDataWriter writer)
		{
			Serializer<decimal>.FireOnSerializedType();
			writer.WriteDecimal(name, value);
		}
	}
}
