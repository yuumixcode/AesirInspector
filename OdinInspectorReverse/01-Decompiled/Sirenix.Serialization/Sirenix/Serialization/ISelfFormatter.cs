namespace Sirenix.Serialization
{
	/// <summary>
	/// Specifies that a type is capable of serializing itself using an <see cref="T:Sirenix.Serialization.IDataWriter" /> and an
	/// <see cref="T:Sirenix.Serialization.IDataReader" />.
	/// <para />
	/// The deserialized type instance will be created without a constructor call using the
	/// <see cref="M:System.Runtime.Serialization.FormatterServices.GetUninitializedObject(System.Type)" />
	/// method if it is a reference type, otherwise it will be created using default(type).
	/// <para />
	/// Use <see cref="T:Sirenix.Serialization.AlwaysFormatsSelfAttribute" /> to specify that a class which implements this
	/// interface should *always* format itself regardless of other formatters being specified.
	/// </summary>
	public interface ISelfFormatter
	{
		/// <summary>
		/// Serializes the instance's data using the given writer.
		/// </summary>
		void Serialize(IDataWriter writer);

		/// <summary>
		/// Deserializes data into the instance using the given reader.
		/// </summary>
		void Deserialize(IDataReader reader);
	}
}
