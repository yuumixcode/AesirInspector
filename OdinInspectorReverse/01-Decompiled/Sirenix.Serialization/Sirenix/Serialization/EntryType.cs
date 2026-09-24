namespace Sirenix.Serialization
{
	/// <summary>
	/// An entry type which is part of a stream being read by a <see cref="T:Sirenix.Serialization.IDataReader" />.
	/// </summary>
	public enum EntryType : byte
	{
		/// <summary>
		/// Could not parse entry.
		/// </summary>
		Invalid,
		/// <summary>
		/// Entry is a primitive value of type string or char.
		/// </summary>
		String,
		/// <summary>
		/// Entry is a primitive value of type guid.
		/// </summary>
		Guid,
		/// <summary>
		/// Entry is a primitive value of type sbyte, byte, short, ushort, int, uint, long or ulong.
		/// </summary>
		Integer,
		/// <summary>
		/// Entry is a primitive value of type float, double or decimal.
		/// </summary>
		FloatingPoint,
		/// <summary>
		/// Entry is a primitive boolean value.
		/// </summary>
		Boolean,
		/// <summary>
		/// Entry is a null value.
		/// </summary>
		Null,
		/// <summary>
		/// Entry marks the start of a node, IE, a complex type that contains values of its own.
		/// </summary>
		StartOfNode,
		/// <summary>
		/// Entry marks the end of a node, IE, a complex type that contains values of its own.
		/// </summary>
		EndOfNode,
		/// <summary>
		/// Entry contains an ID that is a reference to a node defined previously in the stream.
		/// </summary>
		InternalReference,
		/// <summary>
		/// Entry contains the index of an external object in the DeserializationContext.
		/// </summary>
		ExternalReferenceByIndex,
		/// <summary>
		/// Entry contains the guid of an external object in the DeserializationContext.
		/// </summary>
		ExternalReferenceByGuid,
		/// <summary>
		/// Entry marks the start of an array.
		/// </summary>
		StartOfArray,
		/// <summary>
		/// Entry marks the end of an array.
		/// </summary>
		EndOfArray,
		/// <summary>
		/// Entry marks a primitive array.
		/// </summary>
		PrimitiveArray,
		/// <summary>
		/// Entry indicating that the reader has reached the end of the data stream.
		/// </summary>
		EndOfStream,
		/// <summary>
		/// Entry contains the string id of an external object in the DeserializationContext.
		/// </summary>
		ExternalReferenceByString
	}
}
