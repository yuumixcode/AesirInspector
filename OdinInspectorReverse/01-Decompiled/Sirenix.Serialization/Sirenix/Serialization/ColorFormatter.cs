using UnityEngine;

namespace Sirenix.Serialization
{
	/// <summary>
	/// Custom formatter for the <see cref="T:UnityEngine.Color" /> type.
	/// </summary>
	/// <seealso cref="!:MinimalBaseFormatter&lt;UnityEngine.Color&gt;" />
	public class ColorFormatter : MinimalBaseFormatter<Color>
	{
		private static readonly Serializer<float> FloatSerializer = Serializer.Get<float>();

		/// <summary>
		/// Reads into the specified value using the specified reader.
		/// </summary>
		/// <param name="value">The value to read into.</param>
		/// <param name="reader">The reader to use.</param>
		protected override void Read(ref Color value, IDataReader reader)
		{
			value.r = FloatSerializer.ReadValue(reader);
			value.g = FloatSerializer.ReadValue(reader);
			value.b = FloatSerializer.ReadValue(reader);
			value.a = FloatSerializer.ReadValue(reader);
		}

		/// <summary>
		/// Writes from the specified value using the specified writer.
		/// </summary>
		/// <param name="value">The value to write from.</param>
		/// <param name="writer">The writer to use.</param>
		protected override void Write(ref Color value, IDataWriter writer)
		{
			FloatSerializer.WriteValue(value.r, writer);
			FloatSerializer.WriteValue(value.g, writer);
			FloatSerializer.WriteValue(value.b, writer);
			FloatSerializer.WriteValue(value.a, writer);
		}
	}
}
