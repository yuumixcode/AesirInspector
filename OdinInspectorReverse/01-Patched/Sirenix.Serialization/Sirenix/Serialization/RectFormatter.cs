using UnityEngine;

namespace Sirenix.Serialization
{
	/// <summary>
	/// Custom formatter for the <see cref="T:UnityEngine.Rect" /> type.
	/// </summary>
	/// <seealso cref="!:MinimalBaseFormatter&lt;UnityEngine.Rect&gt;" />
	public class RectFormatter : MinimalBaseFormatter<Rect>
	{
		private static readonly Serializer<float> FloatSerializer = Serializer.Get<float>();

		/// <summary>
		/// Reads into the specified value using the specified reader.
		/// </summary>
		/// <param name="value">The value to read into.</param>
		/// <param name="reader">The reader to use.</param>
		protected override void Read(ref Rect value, IDataReader reader)
		{
			value.x = FloatSerializer.ReadValue(reader);
			value.y = FloatSerializer.ReadValue(reader);
			value.width = FloatSerializer.ReadValue(reader);
			value.height = FloatSerializer.ReadValue(reader);
		}

		/// <summary>
		/// Writes from the specified value using the specified writer.
		/// </summary>
		/// <param name="value">The value to write from.</param>
		/// <param name="writer">The writer to use.</param>
		protected override void Write(ref Rect value, IDataWriter writer)
		{
			FloatSerializer.WriteValue(value.x, writer);
			FloatSerializer.WriteValue(value.y, writer);
			FloatSerializer.WriteValue(value.width, writer);
			FloatSerializer.WriteValue(value.height, writer);
		}
	}
}
