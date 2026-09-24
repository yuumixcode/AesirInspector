using UnityEngine;

namespace Sirenix.Serialization
{
	/// <summary>
	/// Custom formatter for the <see cref="T:UnityEngine.GradientAlphaKey" /> type.
	/// </summary>
	/// <seealso cref="!:MinimalBaseFormatter&lt;UnityEngine.GradientAlphaKey&gt;" />
	public class GradientAlphaKeyFormatter : MinimalBaseFormatter<GradientAlphaKey>
	{
		private static readonly Serializer<float> FloatSerializer = Serializer.Get<float>();

		/// <summary>
		/// Reads into the specified value using the specified reader.
		/// </summary>
		/// <param name="value">The value to read into.</param>
		/// <param name="reader">The reader to use.</param>
		protected override void Read(ref GradientAlphaKey value, IDataReader reader)
		{
			value.alpha = FloatSerializer.ReadValue(reader);
			value.time = FloatSerializer.ReadValue(reader);
		}

		/// <summary>
		/// Writes from the specified value using the specified writer.
		/// </summary>
		/// <param name="value">The value to write from.</param>
		/// <param name="writer">The writer to use.</param>
		protected override void Write(ref GradientAlphaKey value, IDataWriter writer)
		{
			FloatSerializer.WriteValue(value.alpha, writer);
			FloatSerializer.WriteValue(value.time, writer);
		}
	}
}
