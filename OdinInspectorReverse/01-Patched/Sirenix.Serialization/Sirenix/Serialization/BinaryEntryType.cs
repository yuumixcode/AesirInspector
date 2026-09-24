namespace Sirenix.Serialization
{
	/// <summary>
	/// Entry types in the binary format written by <see cref="T:Sirenix.Serialization.BinaryDataWriter" />.
	/// </summary>
	public enum BinaryEntryType : byte
	{
		/// <summary>
		/// An invalid entry.
		/// </summary>
		Invalid,
		/// <summary>
		/// Entry denoting a named start of a reference node.
		/// </summary>
		NamedStartOfReferenceNode,
		/// <summary>
		/// Entry denoting an unnamed start of a reference node.
		/// </summary>
		UnnamedStartOfReferenceNode,
		/// <summary>
		/// Entry denoting a named start of a struct node.
		/// </summary>
		NamedStartOfStructNode,
		/// <summary>
		/// Entry denoting an unnamed start of a struct node.
		/// </summary>
		UnnamedStartOfStructNode,
		/// <summary>
		/// Entry denoting an end of node.
		/// </summary>
		EndOfNode,
		/// <summary>
		/// Entry denoting the start of an array.
		/// </summary>
		StartOfArray,
		/// <summary>
		/// Entry denoting the end of an array.
		/// </summary>
		EndOfArray,
		/// <summary>
		/// Entry denoting a primitive array.
		/// </summary>
		PrimitiveArray,
		/// <summary>
		/// Entry denoting a named internal reference.
		/// </summary>
		NamedInternalReference,
		/// <summary>
		/// Entry denoting an unnamed internal reference.
		/// </summary>
		UnnamedInternalReference,
		/// <summary>
		/// Entry denoting a named external reference by index.
		/// </summary>
		NamedExternalReferenceByIndex,
		/// <summary>
		/// Entry denoting an unnamed external reference by index.
		/// </summary>
		UnnamedExternalReferenceByIndex,
		/// <summary>
		/// Entry denoting a named external reference by guid.
		/// </summary>
		NamedExternalReferenceByGuid,
		/// <summary>
		/// Entry denoting an unnamed external reference by guid.
		/// </summary>
		UnnamedExternalReferenceByGuid,
		/// <summary>
		/// Entry denoting a named sbyte.
		/// </summary>
		NamedSByte,
		/// <summary>
		/// Entry denoting an unnamed sbyte.
		/// </summary>
		UnnamedSByte,
		/// <summary>
		/// Entry denoting a named byte.
		/// </summary>
		NamedByte,
		/// <summary>
		/// Entry denoting an unnamed byte.
		/// </summary>
		UnnamedByte,
		/// <summary>
		/// Entry denoting a named short.
		/// </summary>
		NamedShort,
		/// <summary>
		/// Entry denoting an unnamed short.
		/// </summary>
		UnnamedShort,
		/// <summary>
		/// Entry denoting a named ushort.
		/// </summary>
		NamedUShort,
		/// <summary>
		/// Entry denoting an unnamed ushort.
		/// </summary>
		UnnamedUShort,
		/// <summary>
		/// Entry denoting a named int.
		/// </summary>
		NamedInt,
		/// <summary>
		/// Entry denoting an unnamed int.
		/// </summary>
		UnnamedInt,
		/// <summary>
		/// Entry denoting a named uint.
		/// </summary>
		NamedUInt,
		/// <summary>
		/// Entry denoting an unnamed uint.
		/// </summary>
		UnnamedUInt,
		/// <summary>
		/// Entry denoting a named long.
		/// </summary>
		NamedLong,
		/// <summary>
		/// Entry denoting an unnamed long.
		/// </summary>
		UnnamedLong,
		/// <summary>
		/// Entry denoting a named ulong.
		/// </summary>
		NamedULong,
		/// <summary>
		/// Entry denoting an unnamed ulong.
		/// </summary>
		UnnamedULong,
		/// <summary>
		/// Entry denoting a named float.
		/// </summary>
		NamedFloat,
		/// <summary>
		/// Entry denoting an unnamed float.
		/// </summary>
		UnnamedFloat,
		/// <summary>
		/// Entry denoting a named double.
		/// </summary>
		NamedDouble,
		/// <summary>
		/// Entry denoting an unnamed double.
		/// </summary>
		UnnamedDouble,
		/// <summary>
		/// Entry denoting a named decimal.
		/// </summary>
		NamedDecimal,
		/// <summary>
		/// Entry denoting an unnamed decimal.
		/// </summary>
		UnnamedDecimal,
		/// <summary>
		/// Entry denoting a named char.
		/// </summary>
		NamedChar,
		/// <summary>
		/// Entry denoting an unnamed char.
		/// </summary>
		UnnamedChar,
		/// <summary>
		/// Entry denoting a named string.
		/// </summary>
		NamedString,
		/// <summary>
		/// Entry denoting an unnamed string.
		/// </summary>
		UnnamedString,
		/// <summary>
		/// Entry denoting a named guid.
		/// </summary>
		NamedGuid,
		/// <summary>
		/// Entry denoting an unnamed guid.
		/// </summary>
		UnnamedGuid,
		/// <summary>
		/// Entry denoting a named boolean.
		/// </summary>
		NamedBoolean,
		/// <summary>
		/// Entry denoting an unnamed boolean.
		/// </summary>
		UnnamedBoolean,
		/// <summary>
		/// Entry denoting a named null.
		/// </summary>
		NamedNull,
		/// <summary>
		/// Entry denoting an unnamed null.
		/// </summary>
		UnnamedNull,
		/// <summary>
		/// Entry denoting a type name.
		/// </summary>
		TypeName,
		/// <summary>
		/// Entry denoting a type id.
		/// </summary>
		TypeID,
		/// <summary>
		/// Entry denoting that the end of the stream has been reached.
		/// </summary>
		EndOfStream,
		/// <summary>
		/// Entry denoting a named external reference by string.
		/// </summary>
		NamedExternalReferenceByString,
		/// <summary>
		/// Entry denoting an unnamed external reference by string.
		/// </summary>
		UnnamedExternalReferenceByString
	}
}
