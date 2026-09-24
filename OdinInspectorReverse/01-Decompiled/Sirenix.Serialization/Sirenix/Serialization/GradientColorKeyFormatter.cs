using UnityEngine;

namespace Sirenix.Serialization
{
	/// <summary>
	/// Custom formatter for the <see cref="T:UnityEngine.GradientColorKey" /> type.
	/// </summary>
	/// <seealso cref="!:MinimalBaseFormatter&lt;UnityEngine.GradientColorKey&gt;" />
	public class GradientColorKeyFormatter : MinimalBaseFormatter<GradientColorKey>
	{
		private static readonly Serializer<Color> ColorSerializer = Serializer.Get<Color>();

		private static readonly Serializer<float> FloatSerializer = Serializer.Get<float>();

		/// <summary>
		/// Reads into the specified value using the specified reader.
		/// </summary>
		/// <param name="value">The value to read into.</param>
		/// <param name="reader">The reader to use.</param>
		protected override void Read(ref GradientColorKey value, IDataReader reader)
		{
			value.color = ColorSerializer.ReadValue(reader);
			value.time = FloatSerializer.ReadValue(reader);
		}

		/// <summary>
		/// Writes from the specified value using the specified writer.
		/// </summary>
		/// <param name="value">The value to write from.</param>
		/// <param name="writer">The writer to use.</param>
		protected override void Write(ref GradientColorKey value, IDataWriter writer)
		{
			ColorSerializer.WriteValue(value.color, writer);
			FloatSerializer.WriteValue(value.time, writer);
		}
	}
}
