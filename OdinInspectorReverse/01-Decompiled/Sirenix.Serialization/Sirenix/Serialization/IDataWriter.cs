using System;
using System.ComponentModel;
using System.IO;

namespace Sirenix.Serialization
{
	/// <summary>
	/// Provides a set of methods for reading data stored in a format that can be read by a corresponding <see cref="T:Sirenix.Serialization.IDataReader" /> class.
	/// <para />
	/// If you implement this interface, it is VERY IMPORTANT that you implement each method to the *exact* specifications the documentation specifies.
	/// <para />
	/// It is strongly recommended to inherit from the <see cref="T:Sirenix.Serialization.BaseDataWriter" /> class if you wish to implement a new data writer.
	/// </summary>
	/// <seealso cref="T:System.IDisposable" />
	public interface IDataWriter : IDisposable
	{
		/// <summary>
		/// Gets or sets the reader's serialization binder.
		/// </summary>
		/// <value>
		/// The reader's serialization binder.
		/// </value>
		TwoWaySerializationBinder Binder { get; set; }

		/// <summary>
		/// Gets or sets the base stream of the writer.
		/// </summary>
		/// <value>
		/// The base stream of the writer.
		/// </value>
		[Obsolete("Data readers and writers don't necessarily have streams any longer, so this API has been made obsolete. Using this property may result in NotSupportedExceptions being thrown.", false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		Stream Stream { get; set; }

		/// <summary>
		/// Gets a value indicating whether the writer is in an array node.
		/// </summary>
		/// <value>
		/// <c>true</c> if the writer is in an array node; otherwise, <c>false</c>.
		/// </value>
		bool IsInArrayNode { get; }

		/// <summary>
		/// Gets the serialization context.
		/// </summary>
		/// <value>
		/// The serialization context.
		/// </value>
		SerializationContext Context { get; set; }

		/// <summary>
		/// Gets a dump of the data currently written by the writer. The format of this dump varies, but should be useful for debugging purposes.
		/// </summary>
		string GetDataDump();

		/// <summary>
		/// Flushes everything that has been written so far to the writer's base stream.
		/// </summary>
		void FlushToStream();

		/// <summary>
		/// Writes the beginning of a reference node.
		/// <para />
		/// This call MUST eventually be followed by a corresponding call to <see cref="M:Sirenix.Serialization.IDataWriter.EndNode(System.String)" />, with the same name.
		/// </summary>
		/// <param name="name">The name of the reference node.</param>
		/// <param name="type">The type of the reference node. If null, no type metadata will be written.</param>
		/// <param name="id">The id of the reference node. This id is acquired by calling <see cref="M:Sirenix.Serialization.SerializationContext.TryRegisterInternalReference(System.Object,System.Int32@)" />.</param>
		void BeginReferenceNode(string name, Type type, int id);

		/// <summary>
		/// Begins a struct/value type node. This is essentially the same as a reference node, except it has no internal reference id.
		/// <para />
		/// This call MUST eventually be followed by a corresponding call to <see cref="M:Sirenix.Serialization.IDataWriter.EndNode(System.String)" />, with the same name.
		/// </summary>
		/// <param name="name">The name of the struct node.</param>
		/// <param name="type">The type of the struct node. If null, no type metadata will be written.</param>
		void BeginStructNode(string name, Type type);

		/// <summary>
		/// Ends the current node with the given name. If the current node has another name, an <see cref="T:System.InvalidOperationException" /> is thrown.
		/// </summary>
		/// <param name="name">The name of the node to end. This has to be the name of the current node.</param>
		void EndNode(string name);

		/// <summary>
		/// Begins an array node of the given length.
		/// </summary>
		/// <param name="length">The length of the array to come.</param>
		void BeginArrayNode(long length);

		/// <summary>
		/// Ends the current array node, if the current node is an array node.
		/// </summary>
		void EndArrayNode();

		/// <summary>
		/// Writes a primitive array to the stream.
		/// </summary>
		/// <typeparam name="T">The element type of the primitive array. Valid element types can be determined using <see cref="M:Sirenix.Serialization.FormatterUtilities.IsPrimitiveArrayType(System.Type)" />.</typeparam>
		/// <param name="array">The primitive array to write.</param>
		void WritePrimitiveArray<T>(T[] array) where T : struct;

		/// <summary>
		/// Writes a null value to the stream.
		/// </summary>
		/// <param name="name">The name of the value. If this is null, no name will be written.</param>
		void WriteNull(string name);

		/// <summary>
		/// Writes an internal reference to the stream.
		/// </summary>
		/// <param name="name">The name of the value. If this is null, no name will be written.</param>
		/// <param name="id">The value to write.</param>
		void WriteInternalReference(string name, int id);

		/// <summary>
		/// Writes an external index reference to the stream.
		/// </summary>
		/// <param name="name">The name of the value. If this is null, no name will be written.</param>
		/// <param name="index">The value to write.</param>
		void WriteExternalReference(string name, int index);

		/// <summary>
		/// Writes an external guid reference to the stream.
		/// </summary>
		/// <param name="name">The name of the value. If this is null, no name will be written.</param>
		/// <param name="guid">The value to write.</param>
		void WriteExternalReference(string name, Guid guid);

		/// <summary>
		/// Writes an external string reference to the stream.
		/// </summary>
		/// <param name="name">The name of the value. If this is null, no name will be written.</param>
		/// <param name="id">The value to write.</param>
		void WriteExternalReference(string name, string id);

		/// <summary>
		/// Writes a <see cref="T:System.Char" /> value to the stream.
		/// </summary>
		/// <param name="name">The name of the value. If this is null, no name will be written.</param>
		/// <param name="value">The value to write.</param>
		void WriteChar(string name, char value);

		/// <summary>
		/// Writes a <see cref="T:System.String" /> value to the stream.
		/// </summary>
		/// <param name="name">The name of the value. If this is null, no name will be written.</param>
		/// <param name="value">The value to write.</param>
		void WriteString(string name, string value);

		/// <summary>
		/// Writes a <see cref="T:System.Guid" /> value to the stream.
		/// </summary>
		/// <param name="name">The name of the value. If this is null, no name will be written.</param>
		/// <param name="value">The value to write.</param>
		void WriteGuid(string name, Guid value);

		/// <summary>
		/// Writes an <see cref="T:System.SByte" /> value to the stream.
		/// </summary>
		/// <param name="name">The name of the value. If this is null, no name will be written.</param>
		/// <param name="value">The value to write.</param>
		void WriteSByte(string name, sbyte value);

		/// <summary>
		/// Writes a <see cref="T:System.Int16" /> value to the stream.
		/// </summary>
		/// <param name="name">The name of the value. If this is null, no name will be written.</param>
		/// <param name="value">The value to write.</param>
		void WriteInt16(string name, short value);

		/// <summary>
		/// Writes an <see cref="T:System.Int32" /> value to the stream.
		/// </summary>
		/// <param name="name">The name of the value. If this is null, no name will be written.</param>
		/// <param name="value">The value to write.</param>
		void WriteInt32(string name, int value);

		/// <summary>
		/// Writes a <see cref="T:System.Int64" /> value to the stream.
		/// </summary>
		/// <param name="name">The name of the value. If this is null, no name will be written.</param>
		/// <param name="value">The value to write.</param>
		void WriteInt64(string name, long value);

		/// <summary>
		/// Writes a <see cref="T:System.Byte" /> value to the stream.
		/// </summary>
		/// <param name="name">The name of the value. If this is null, no name will be written.</param>
		/// <param name="value">The value to write.</param>
		void WriteByte(string name, byte value);

		/// <summary>
		/// Writes an <see cref="T:System.UInt16" /> value to the stream.
		/// </summary>
		/// <param name="name">The name of the value. If this is null, no name will be written.</param>
		/// <param name="value">The value to write.</param>
		void WriteUInt16(string name, ushort value);

		/// <summary>
		/// Writes an <see cref="T:System.UInt32" /> value to the stream.
		/// </summary>
		/// <param name="name">The name of the value. If this is null, no name will be written.</param>
		/// <param name="value">The value to write.</param>
		void WriteUInt32(string name, uint value);

		/// <summary>
		/// Writes an <see cref="T:System.UInt64" /> value to the stream.
		/// </summary>
		/// <param name="name">The name of the value. If this is null, no name will be written.</param>
		/// <param name="value">The value to write.</param>
		void WriteUInt64(string name, ulong value);

		/// <summary>
		/// Writes a <see cref="T:System.Decimal" /> value to the stream.
		/// </summary>
		/// <param name="name">The name of the value. If this is null, no name will be written.</param>
		/// <param name="value">The value to write.</param>
		void WriteDecimal(string name, decimal value);

		/// <summary>
		/// Writes a <see cref="T:System.Single" /> value to the stream.
		/// </summary>
		/// <param name="name">The name of the value. If this is null, no name will be written.</param>
		/// <param name="value">The value to write.</param>
		void WriteSingle(string name, float value);

		/// <summary>
		/// Writes a <see cref="T:System.Double" /> value to the stream.
		/// </summary>
		/// <param name="name">The name of the value. If this is null, no name will be written.</param>
		/// <param name="value">The value to write.</param>
		void WriteDouble(string name, double value);

		/// <summary>
		/// Writes a <see cref="T:System.Boolean" /> value to the stream.
		/// </summary>
		/// <param name="name">The name of the value. If this is null, no name will be written.</param>
		/// <param name="value">The value to write.</param>
		void WriteBoolean(string name, bool value);

		/// <summary>
		/// Tells the writer that a new serialization session is about to begin, and that it should clear all cached values left over from any prior serialization sessions.
		/// This method is only relevant when the same writer is used to serialize several different, unrelated values.
		/// </summary>
		void PrepareNewSerializationSession();
	}
}
