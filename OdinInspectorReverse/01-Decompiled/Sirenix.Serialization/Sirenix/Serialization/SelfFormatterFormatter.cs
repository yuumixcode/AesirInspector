namespace Sirenix.Serialization
{
	/// <summary>
	/// Formatter for types that implement the <see cref="T:Sirenix.Serialization.ISelfFormatter" /> interface.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <seealso cref="T:Sirenix.Serialization.BaseFormatter`1" />
	public sealed class SelfFormatterFormatter<T> : BaseFormatter<T> where T : ISelfFormatter
	{
		/// <summary>
		/// Calls <see cref="M:Sirenix.Serialization.ISelfFormatter.Deserialize(Sirenix.Serialization.IDataReader)" />  on the value to deserialize.
		/// </summary>
		protected override void DeserializeImplementation(ref T value, IDataReader reader)
		{
			value.Deserialize(reader);
		}

		/// <summary>
		/// Calls <see cref="M:Sirenix.Serialization.ISelfFormatter.Serialize(Sirenix.Serialization.IDataWriter)" />  on the value to deserialize.
		/// </summary>
		protected override void SerializeImplementation(ref T value, IDataWriter writer)
		{
			value.Serialize(writer);
		}
	}
}
