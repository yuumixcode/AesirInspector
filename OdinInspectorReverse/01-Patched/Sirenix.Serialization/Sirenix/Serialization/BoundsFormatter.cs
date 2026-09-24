using UnityEngine;

namespace Sirenix.Serialization
{
	/// <summary>
	/// Custom formatter for the <see cref="T:UnityEngine.Bounds" /> type.
	/// </summary>
	/// <seealso cref="!:MinimalBaseFormatter&lt;UnityEngine.Bounds&gt;" />
	public class BoundsFormatter : MinimalBaseFormatter<Bounds>
	{
		private static readonly Serializer<Vector3> Vector3Serializer = Serializer.Get<Vector3>();

		/// <summary>
		/// Reads into the specified value using the specified reader.
		/// </summary>
		/// <param name="value">The value to read into.</param>
		/// <param name="reader">The reader to use.</param>
		protected override void Read(ref Bounds value, IDataReader reader)
		{
			value.center = Vector3Serializer.ReadValue(reader);
			value.size = Vector3Serializer.ReadValue(reader);
		}

		/// <summary>
		/// Writes from the specified value using the specified writer.
		/// </summary>
		/// <param name="value">The value to write from.</param>
		/// <param name="writer">The writer to use.</param>
		protected override void Write(ref Bounds value, IDataWriter writer)
		{
			Vector3Serializer.WriteValue(value.center, writer);
			Vector3Serializer.WriteValue(value.size, writer);
		}
	}
}
