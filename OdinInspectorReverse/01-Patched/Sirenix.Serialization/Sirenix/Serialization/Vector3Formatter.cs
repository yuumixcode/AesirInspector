using UnityEngine;

namespace Sirenix.Serialization
{
	/// <summary>
	/// Custom formatter for the <see cref="T:UnityEngine.Vector3" /> type.
	/// </summary>
	/// <seealso cref="!:MinimalBaseFormatter&lt;UnityEngine.Vector3&gt;" />
	public class Vector3Formatter : MinimalBaseFormatter<Vector3>
	{
		private static readonly Serializer<float> FloatSerializer = Serializer.Get<float>();

		/// <summary>
		/// Reads into the specified value using the specified reader.
		/// </summary>
		/// <param name="value">The value to read into.</param>
		/// <param name="reader">The reader to use.</param>
		protected override void Read(ref Vector3 value, IDataReader reader)
		{
			value.x = FloatSerializer.ReadValue(reader);
			value.y = FloatSerializer.ReadValue(reader);
			value.z = FloatSerializer.ReadValue(reader);
		}

		/// <summary>
		/// Writes from the specified value using the specified writer.
		/// </summary>
		/// <param name="value">The value to write from.</param>
		/// <param name="writer">The writer to use.</param>
		protected override void Write(ref Vector3 value, IDataWriter writer)
		{
			FloatSerializer.WriteValue(value.x, writer);
			FloatSerializer.WriteValue(value.y, writer);
			FloatSerializer.WriteValue(value.z, writer);
		}
	}
}
