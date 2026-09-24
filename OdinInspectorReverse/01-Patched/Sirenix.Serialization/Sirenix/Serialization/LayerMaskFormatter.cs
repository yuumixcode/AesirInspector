using UnityEngine;

namespace Sirenix.Serialization
{
	/// <summary>
	/// Custom formatter for the <see cref="T:UnityEngine.LayerMask" /> type.
	/// </summary>
	/// <seealso cref="!:MinimalBaseFormatter&lt;UnityEngine.LayerMask&gt;" />
	public class LayerMaskFormatter : MinimalBaseFormatter<LayerMask>
	{
		private static readonly Serializer<int> IntSerializer = Serializer.Get<int>();

		/// <summary>
		/// Reads into the specified value using the specified reader.
		/// </summary>
		/// <param name="value">The value to read into.</param>
		/// <param name="reader">The reader to use.</param>
		protected override void Read(ref LayerMask value, IDataReader reader)
		{
			value.value = IntSerializer.ReadValue(reader);
		}

		/// <summary>
		/// Writes from the specified value using the specified writer.
		/// </summary>
		/// <param name="value">The value to write from.</param>
		/// <param name="writer">The writer to use.</param>
		protected override void Write(ref LayerMask value, IDataWriter writer)
		{
			IntSerializer.WriteValue(value.value, writer);
		}
	}
}
