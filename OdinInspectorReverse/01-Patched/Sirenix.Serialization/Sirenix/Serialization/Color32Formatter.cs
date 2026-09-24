using UnityEngine;

namespace Sirenix.Serialization
{
	/// <summary>
	/// Custom formatter for the <see cref="T:UnityEngine.Color32" /> type.
	/// </summary>
	/// <seealso cref="!:MinimalBaseFormatter&lt;UnityEngine.Color32&gt;" />
	public class Color32Formatter : MinimalBaseFormatter<Color32>
	{
		private static readonly Serializer<byte> ByteSerializer = Serializer.Get<byte>();

		/// <summary>
		/// Reads into the specified value using the specified reader.
		/// </summary>
		/// <param name="value">The value to read into.</param>
		/// <param name="reader">The reader to use.</param>
		protected override void Read(ref Color32 value, IDataReader reader)
		{
			value.r = ByteSerializer.ReadValue(reader);
			value.g = ByteSerializer.ReadValue(reader);
			value.b = ByteSerializer.ReadValue(reader);
			value.a = ByteSerializer.ReadValue(reader);
		}

		/// <summary>
		/// Writes from the specified value using the specified writer.
		/// </summary>
		/// <param name="value">The value to write from.</param>
		/// <param name="writer">The writer to use.</param>
		protected override void Write(ref Color32 value, IDataWriter writer)
		{
			ByteSerializer.WriteValue(value.r, writer);
			ByteSerializer.WriteValue(value.g, writer);
			ByteSerializer.WriteValue(value.b, writer);
			ByteSerializer.WriteValue(value.a, writer);
		}
	}
}
