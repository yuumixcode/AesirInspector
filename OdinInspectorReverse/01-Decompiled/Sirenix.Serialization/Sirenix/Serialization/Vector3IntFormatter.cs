using UnityEngine;

namespace Sirenix.Serialization
{
	/// <summary>
	/// Custom formatter for the <see cref="T:UnityEngine.Vector3Int" /> type.
	/// </summary>
	/// <seealso cref="!:Sirenix.Serialization.MinimalBaseFormatter&lt;UnityEngine.Vector3Int&gt;" />
	public class Vector3IntFormatter : MinimalBaseFormatter<Vector3Int>
	{
		private static readonly Serializer<int> Serializer = Sirenix.Serialization.Serializer.Get<int>();

		/// <summary>
		/// Reads into the specified value using the specified reader.
		/// </summary>
		/// <param name="value">The value to read into.</param>
		/// <param name="reader">The reader to use.</param>
		protected override void Read(ref Vector3Int value, IDataReader reader)
		{
			value.x = Serializer.ReadValue(reader);
			value.y = Serializer.ReadValue(reader);
			value.z = Serializer.ReadValue(reader);
		}

		/// <summary>
		/// Writes from the specified value using the specified writer.
		/// </summary>
		/// <param name="value">The value to write from.</param>
		/// <param name="writer">The writer to use.</param>
		protected override void Write(ref Vector3Int value, IDataWriter writer)
		{
			Serializer.WriteValue(value.x, writer);
			Serializer.WriteValue(value.y, writer);
			Serializer.WriteValue(value.z, writer);
		}
	}
}
