using System;
using System.IO;

namespace Sirenix.Serialization
{
	/// <summary>
	/// Provides basic functionality and overridable abstract methods for implementing a data writer.
	/// <para />
	/// If you inherit this class, it is VERY IMPORTANT that you implement each abstract method to the *exact* specifications the documentation specifies.
	/// </summary>
	/// <seealso cref="T:Sirenix.Serialization.BaseDataReaderWriter" />
	/// <seealso cref="T:Sirenix.Serialization.IDataWriter" />
	public abstract class BaseDataWriter : BaseDataReaderWriter, IDataWriter, IDisposable
	{
		private SerializationContext context;

		private Stream stream;

		/// <summary>
		/// Gets or sets the base stream of the writer.
		/// </summary>
		/// <value>
		/// The base stream of the writer.
		/// </value>
		/// <exception cref="T:System.ArgumentNullException">value</exception>
		/// <exception cref="T:System.ArgumentException">Cannot write to stream</exception>
		public virtual Stream Stream
		{
			get
			{
				return stream;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("value");
				}
				if (!value.CanWrite)
				{
					throw new ArgumentException("Cannot write to stream");
				}
				stream = value;
			}
		}

		/// <summary>
		/// Gets the serialization context.
		/// </summary>
		/// <value>
		/// The serialization context.
		/// </value>
		public SerializationContext Context
		{
			get
			{
				if (context == null)
				{
					context = new SerializationContext();
				}
				return context;
			}
			set
			{
				context = value;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Sirenix.Serialization.BaseDataWriter" /> class.
		/// </summary>
		/// <param name="stream">The base stream of the writer.</param>
		/// <param name="context">The serialization context to use.</param>
		/// <exception cref="T:System.ArgumentNullException">The stream or context is null.</exception>
		/// <exception cref="T:System.ArgumentException">Cannot write to the stream.</exception>
		protected BaseDataWriter(Stream stream, SerializationContext context)
		{
			this.context = context;
			if (stream != null)
			{
				Stream = stream;
			}
		}

		/// <summary>
		/// Flushes everything that has been written so far to the writer's base stream.
		/// </summary>
		public virtual void FlushToStream()
		{
			Stream.Flush();
		}

		/// <summary>
		/// Writes the beginning of a reference node.
		/// <para />
		/// This call MUST eventually be followed by a corresponding call to <see cref="M:Sirenix.Serialization.IDataWriter.EndNode(System.String)" />, with the same name.
		/// </summary>
		/// <param name="name">The name of the reference node.</param>
		/// <param name="type">The type of the reference node. If null, no type metadata will be written.</param>
		/// <param name="id">The id of the reference node. This id is acquired by calling <see cref="M:Sirenix.Serialization.SerializationContext.TryRegisterInternalReference(System.Object,System.Int32@)" />.</param>
		public abstract void BeginReferenceNode(string name, Type type, int id);

		/// <summary>
		/// Begins a struct/value type node. This is essentially the same as a reference node, except it has no internal reference id.
		/// <para />
		/// This call MUST eventually be followed by a corresponding call to <see cref="M:Sirenix.Serialization.IDataWriter.EndNode(System.String)" />, with the same name.
		/// </summary>
		/// <param name="name">The name of the struct node.</param>
		/// <param name="type">The type of the struct node. If null, no type metadata will be written.</param>
		public abstract void BeginStructNode(string name, Type type);

		/// <summary>
		/// Ends the current node with the given name. If the current node has another name, an <see cref="T:System.InvalidOperationException" /> is thrown.
		/// </summary>
		/// <param name="name">The name of the node to end. This has to be the name of the current node.</param>
		public abstract void EndNode(string name);

		/// <summary>
		/// Begins an array node of the given length.
		/// </summary>
		/// <param name="length">The length of the array to come.</param>
		public abstract void BeginArrayNode(long length);

		/// <summary>
		/// Ends the current array node, if the current node is an array node.
		/// </summary>
		public abstract void EndArrayNode();

		/// <summary>
		/// Writes a primitive array to the stream.
		/// </summary>
		/// <typeparam name="T">The element type of the primitive array. Valid element types can be determined using <see cref="M:Sirenix.Serialization.FormatterUtilities.IsPrimitiveArrayType(System.Type)" />.</typeparam>
		/// <param name="array">The primitive array to write.</param>
		public abstract void WritePrimitiveArray<T>(T[] array) where T : struct;

		/// <summary>
		/// Writes a null value to the stream.
		/// </summary>
		/// <param name="name">The name of the value. If this is null, no name will be written.</param>
		public abstract void WriteNull(string name);

		/// <summary>
		/// Writes an internal reference to the stream.
		/// </summary>
		/// <param name="name">The name of the value. If this is null, no name will be written.</param>
		/// <param name="id">The value to write.</param>
		public abstract void WriteInternalReference(string name, int id);

		/// <summary>
		/// Writes an external index reference to the stream.
		/// </summary>
		/// <param name="name">The name of the value. If this is null, no name will be written.</param>
		/// <param name="index">The value to write.</param>
		public abstract void WriteExternalReference(string name, int index);

		/// <summary>
		/// Writes an external guid reference to the stream.
		/// </summary>
		/// <param name="name">The name of the value. If this is null, no name will be written.</param>
		/// <param name="guid">The value to write.</param>
		public abstract void WriteExternalReference(string name, Guid guid);

		/// <summary>
		/// Writes an external string reference to the stream.
		/// </summary>
		/// <param name="name">The name of the value. If this is null, no name will be written.</param>
		/// <param name="id">The value to write.</param>
		public abstract void WriteExternalReference(string name, string id);

		/// <summary>
		/// Writes a <see cref="T:System.Char" /> value to the stream.
		/// </summary>
		/// <param name="name">The name of the value. If this is null, no name will be written.</param>
		/// <param name="value">The value to write.</param>
		public abstract void WriteChar(string name, char value);

		/// <summary>
		/// Writes a <see cref="T:System.String" /> value to the stream.
		/// </summary>
		/// <param name="name">The name of the value. If this is null, no name will be written.</param>
		/// <param name="value">The value to write.</param>
		public abstract void WriteString(string name, string value);

		/// <summary>
		/// Writes a <see cref="T:System.Guid" /> value to the stream.
		/// </summary>
		/// <param name="name">The name of the value. If this is null, no name will be written.</param>
		/// <param name="value">The value to write.</param>
		public abstract void WriteGuid(string name, Guid value);

		/// <summary>
		/// Writes an <see cref="T:System.SByte" /> value to the stream.
		/// </summary>
		/// <param name="name">The name of the value. If this is null, no name will be written.</param>
		/// <param name="value">The value to write.</param>
		public abstract void WriteSByte(string name, sbyte value);

		/// <summary>
		/// Writes a <see cref="T:System.Int16" /> value to the stream.
		/// </summary>
		/// <param name="name">The name of the value. If this is null, no name will be written.</param>
		/// <param name="value">The value to write.</param>
		public abstract void WriteInt16(string name, short value);

		/// <summary>
		/// Writes an <see cref="T:System.Int32" /> value to the stream.
		/// </summary>
		/// <param name="name">The name of the value. If this is null, no name will be written.</param>
		/// <param name="value">The value to write.</param>
		public abstract void WriteInt32(string name, int value);

		/// <summary>
		/// Writes a <see cref="T:System.Int64" /> value to the stream.
		/// </summary>
		/// <param name="name">The name of the value. If this is null, no name will be written.</param>
		/// <param name="value">The value to write.</param>
		public abstract void WriteInt64(string name, long value);

		/// <summary>
		/// Writes a <see cref="T:System.Byte" /> value to the stream.
		/// </summary>
		/// <param name="name">The name of the value. If this is null, no name will be written.</param>
		/// <param name="value">The value to write.</param>
		public abstract void WriteByte(string name, byte value);

		/// <summary>
		/// Writes an <see cref="T:System.UInt16" /> value to the stream.
		/// </summary>
		/// <param name="name">The name of the value. If this is null, no name will be written.</param>
		/// <param name="value">The value to write.</param>
		public abstract void WriteUInt16(string name, ushort value);

		/// <summary>
		/// Writes an <see cref="T:System.UInt32" /> value to the stream.
		/// </summary>
		/// <param name="name">The name of the value. If this is null, no name will be written.</param>
		/// <param name="value">The value to write.</param>
		public abstract void WriteUInt32(string name, uint value);

		/// <summary>
		/// Writes an <see cref="T:System.UInt64" /> value to the stream.
		/// </summary>
		/// <param name="name">The name of the value. If this is null, no name will be written.</param>
		/// <param name="value">The value to write.</param>
		public abstract void WriteUInt64(string name, ulong value);

		/// <summary>
		/// Writes a <see cref="T:System.Decimal" /> value to the stream.
		/// </summary>
		/// <param name="name">The name of the value. If this is null, no name will be written.</param>
		/// <param name="value">The value to write.</param>
		public abstract void WriteDecimal(string name, decimal value);

		/// <summary>
		/// Writes a <see cref="T:System.Single" /> value to the stream.
		/// </summary>
		/// <param name="name">The name of the value. If this is null, no name will be written.</param>
		/// <param name="value">The value to write.</param>
		public abstract void WriteSingle(string name, float value);

		/// <summary>
		/// Writes a <see cref="T:System.Double" /> value to the stream.
		/// </summary>
		/// <param name="name">The name of the value. If this is null, no name will be written.</param>
		/// <param name="value">The value to write.</param>
		public abstract void WriteDouble(string name, double value);

		/// <summary>
		/// Writes a <see cref="T:System.Boolean" /> value to the stream.
		/// </summary>
		/// <param name="name">The name of the value. If this is null, no name will be written.</param>
		/// <param name="value">The value to write.</param>
		public abstract void WriteBoolean(string name, bool value);

		/// <summary>
		/// Disposes all resources and streams kept by the data writer.
		/// </summary>
		public abstract void Dispose();

		/// <summary>
		/// Tells the writer that a new serialization session is about to begin, and that it should clear all cached values left over from any prior serialization sessions.
		/// This method is only relevant when the same writer is used to serialize several different, unrelated values.
		/// </summary>
		public virtual void PrepareNewSerializationSession()
		{
			ClearNodes();
		}

		/// <summary>
		/// Gets a dump of the data currently written by the writer. The format of this dump varies, but should be useful for debugging purposes.
		/// </summary>
		public abstract string GetDataDump();
	}
}
