using System;
using System.IO;

namespace Sirenix.Serialization
{
	/// <summary>
	/// Provides basic functionality and overridable abstract methods for implementing a data reader.
	/// <para />
	/// If you inherit this class, it is VERY IMPORTANT that you implement each abstract method to the *exact* specifications the documentation specifies.
	/// </summary>
	/// <seealso cref="T:Sirenix.Serialization.BaseDataReaderWriter" />
	/// <seealso cref="T:Sirenix.Serialization.IDataReader" />
	public abstract class BaseDataReader : BaseDataReaderWriter, IDataReader, IDisposable
	{
		private DeserializationContext context;

		private Stream stream;

		/// <summary>
		/// Gets the current node id. If this is less than zero, the current node has no id.
		/// </summary>
		/// <value>
		/// The current node id.
		/// </value>
		public int CurrentNodeId => base.CurrentNode.Id;

		/// <summary>
		/// Gets the current node depth. In other words, the current count of the node stack.
		/// </summary>
		/// <value>
		/// The current node depth.
		/// </value>
		public int CurrentNodeDepth => base.NodeDepth;

		/// <summary>
		/// Gets the name of the current node.
		/// </summary>
		/// <value>
		/// The name of the current node.
		/// </value>
		public string CurrentNodeName => base.CurrentNode.Name;

		/// <summary>
		/// Gets or sets the base stream of the reader.
		/// </summary>
		/// <value>
		/// The base stream of the reader.
		/// </value>
		/// <exception cref="T:System.ArgumentNullException">value</exception>
		/// <exception cref="T:System.ArgumentException">Cannot read from stream</exception>
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
				if (!value.CanRead)
				{
					throw new ArgumentException("Cannot read from stream");
				}
				stream = value;
			}
		}

		/// <summary>
		/// Gets the deserialization context.
		/// </summary>
		/// <value>
		/// The deserialization context.
		/// </value>
		public DeserializationContext Context
		{
			get
			{
				if (context == null)
				{
					context = new DeserializationContext();
				}
				return context;
			}
			set
			{
				context = value;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Sirenix.Serialization.BaseDataReader" /> class.
		/// </summary>
		/// <param name="stream">The base stream of the reader.</param>
		/// <param name="context">The deserialization context to use.</param>
		/// <exception cref="T:System.ArgumentNullException">The stream or context is null.</exception>
		/// <exception cref="T:System.ArgumentException">Cannot read from stream.</exception>
		protected BaseDataReader(Stream stream, DeserializationContext context)
		{
			this.context = context;
			if (stream != null)
			{
				Stream = stream;
			}
		}

		/// <summary>
		/// Tries to enter a node. This will succeed if the next entry is an <see cref="F:Sirenix.Serialization.EntryType.StartOfNode" />.
		/// <para />
		/// This call MUST (eventually) be followed by a corresponding call to <see cref="!:IDataReader.ExitNode(DeserializationContext)" />
		/// <para />
		/// This call will change the values of the <see cref="P:Sirenix.Serialization.IDataReader.IsInArrayNode" />, <see cref="P:Sirenix.Serialization.IDataReader.CurrentNodeName" />, <see cref="P:Sirenix.Serialization.IDataReader.CurrentNodeId" /> and <see cref="P:Sirenix.Serialization.IDataReader.CurrentNodeDepth" /> properties to the correct values for the current node.
		/// </summary>
		/// <param name="type">The type of the node. This value will be null if there was no metadata, or if the reader's serialization binder failed to resolve the type name.</param>
		/// <returns><c>true</c> if entering a node succeeded, otherwise <c>false</c></returns>
		public abstract bool EnterNode(out Type type);

		/// <summary>
		/// Exits the current node. This method will keep skipping entries using <see cref="!:IDataReader.SkipEntry(DeserializationContext)" /> until an <see cref="F:Sirenix.Serialization.EntryType.EndOfNode" /> is reached, or the end of the stream is reached.
		/// <para />
		/// This call MUST have been preceded by a corresponding call to <see cref="M:Sirenix.Serialization.IDataReader.EnterNode(System.Type@)" />.
		/// <para />
		/// This call will change the values of the <see cref="P:Sirenix.Serialization.IDataReader.IsInArrayNode" />, <see cref="P:Sirenix.Serialization.IDataReader.CurrentNodeName" />, <see cref="P:Sirenix.Serialization.IDataReader.CurrentNodeId" /> and <see cref="P:Sirenix.Serialization.IDataReader.CurrentNodeDepth" /> to the correct values for the node that was prior to the current node.
		/// </summary>
		/// <returns><c>true</c> if the method exited a node, <c>false</c> if it reached the end of the stream.</returns>
		public abstract bool ExitNode();

		/// <summary>
		/// Tries to enters an array node. This will succeed if the next entry is an <see cref="F:Sirenix.Serialization.EntryType.StartOfArray" />.
		/// <para />
		/// This call MUST (eventually) be followed by a corresponding call to <see cref="!:IDataReader.ExitArray(DeserializationContext)" />
		/// <para />
		/// This call will change the values of the <see cref="P:Sirenix.Serialization.IDataReader.IsInArrayNode" />, <see cref="P:Sirenix.Serialization.IDataReader.CurrentNodeName" />, <see cref="P:Sirenix.Serialization.IDataReader.CurrentNodeId" /> and <see cref="P:Sirenix.Serialization.IDataReader.CurrentNodeDepth" /> properties to the correct values for the current array node.
		/// </summary>
		/// <param name="length">The length of the array that was entered.</param>
		/// <returns><c>true</c> if an array was entered, otherwise <c>false</c></returns>
		public abstract bool EnterArray(out long length);

		/// <summary>
		/// Exits the closest array. This method will keep skipping entries using <see cref="!:IDataReader.SkipEntry(DeserializationContext)" /> until an <see cref="F:Sirenix.Serialization.EntryType.EndOfArray" /> is reached, or the end of the stream is reached.
		/// <para />
		/// This call MUST have been preceded by a corresponding call to <see cref="M:Sirenix.Serialization.IDataReader.EnterArray(System.Int64@)" />.
		/// <para />
		/// This call will change the values of the <see cref="P:Sirenix.Serialization.IDataReader.IsInArrayNode" />, <see cref="P:Sirenix.Serialization.IDataReader.CurrentNodeName" />, <see cref="P:Sirenix.Serialization.IDataReader.CurrentNodeId" /> and <see cref="P:Sirenix.Serialization.IDataReader.CurrentNodeDepth" /> to the correct values for the node that was prior to the exited array node.
		/// </summary>
		/// <returns><c>true</c> if the method exited an array, <c>false</c> if it reached the end of the stream.</returns>
		public abstract bool ExitArray();

		/// <summary>
		/// Reads a primitive array value. This call will succeed if the next entry is an <see cref="F:Sirenix.Serialization.EntryType.PrimitiveArray" />.
		/// <para />
		/// If the call fails (and returns <c>false</c>), it will skip the current entry value, unless that entry is an <see cref="F:Sirenix.Serialization.EntryType.EndOfNode" /> or an <see cref="F:Sirenix.Serialization.EntryType.EndOfArray" />.
		/// </summary>
		/// <typeparam name="T">The element type of the primitive array. Valid element types can be determined using <see cref="M:Sirenix.Serialization.FormatterUtilities.IsPrimitiveArrayType(System.Type)" />.</typeparam>
		/// <param name="array">The resulting primitive array.</param>
		/// <returns><c>true</c> if reading a primitive array succeeded, otherwise <c>false</c></returns>
		public abstract bool ReadPrimitiveArray<T>(out T[] array) where T : struct;

		/// <summary>
		/// Peeks ahead and returns the type of the next entry in the stream.
		/// </summary>
		/// <param name="name">The name of the next entry, if it has one.</param>
		/// <returns>The type of the next entry.</returns>
		public abstract EntryType PeekEntry(out string name);

		/// <summary>
		/// Reads an internal reference id. This call will succeed if the next entry is an <see cref="F:Sirenix.Serialization.EntryType.InternalReference" />.
		/// <para />
		/// If the call fails (and returns <c>false</c>), it will skip the current entry value, unless that entry is an <see cref="F:Sirenix.Serialization.EntryType.EndOfNode" /> or an <see cref="F:Sirenix.Serialization.EntryType.EndOfArray" />.
		/// </summary>
		/// <param name="id">The internal reference id.</param>
		/// <returns><c>true</c> if reading the value succeeded, otherwise <c>false</c></returns>
		public abstract bool ReadInternalReference(out int id);

		/// <summary>
		/// Reads an external reference index. This call will succeed if the next entry is an <see cref="F:Sirenix.Serialization.EntryType.ExternalReferenceByIndex" />.
		/// <para />
		/// If the call fails (and returns <c>false</c>), it will skip the current entry value, unless that entry is an <see cref="F:Sirenix.Serialization.EntryType.EndOfNode" /> or an <see cref="F:Sirenix.Serialization.EntryType.EndOfArray" />.
		/// </summary>
		/// <param name="index">The external reference index.</param>
		/// <returns><c>true</c> if reading the value succeeded, otherwise <c>false</c></returns>
		public abstract bool ReadExternalReference(out int index);

		/// <summary>
		/// Reads an external reference guid. This call will succeed if the next entry is an <see cref="F:Sirenix.Serialization.EntryType.ExternalReferenceByGuid" />.
		/// <para />
		/// If the call fails (and returns <c>false</c>), it will skip the current entry value, unless that entry is an <see cref="F:Sirenix.Serialization.EntryType.EndOfNode" /> or an <see cref="F:Sirenix.Serialization.EntryType.EndOfArray" />.
		/// </summary>
		/// <param name="guid">The external reference guid.</param>
		/// <returns><c>true</c> if reading the value succeeded, otherwise <c>false</c></returns>
		public abstract bool ReadExternalReference(out Guid guid);

		/// <summary>
		/// Reads an external reference string. This call will succeed if the next entry is an <see cref="F:Sirenix.Serialization.EntryType.ExternalReferenceByString" />.
		/// <para />
		/// If the call fails (and returns <c>false</c>), it will skip the current entry value, unless that entry is an <see cref="F:Sirenix.Serialization.EntryType.EndOfNode" /> or an <see cref="F:Sirenix.Serialization.EntryType.EndOfArray" />.
		/// </summary>
		/// <param name="id">The external reference string.</param>
		/// <returns>
		///   <c>true</c> if reading the value succeeded, otherwise <c>false</c>
		/// </returns>
		public abstract bool ReadExternalReference(out string id);

		/// <summary>
		/// Reads a <see cref="T:System.Char" /> value. This call will succeed if the next entry is an <see cref="F:Sirenix.Serialization.EntryType.String" />.
		/// <para />
		/// If the string of the entry is longer than 1 character, the first character of the string will be taken as the result.
		/// <para />
		/// If the call fails (and returns <c>false</c>), it will skip the current entry value, unless that entry is an <see cref="F:Sirenix.Serialization.EntryType.EndOfNode" /> or an <see cref="F:Sirenix.Serialization.EntryType.EndOfArray" />.
		/// </summary>
		/// <param name="value">The value that has been read.</param>
		/// <returns><c>true</c> if reading the value succeeded, otherwise <c>false</c></returns>
		public abstract bool ReadChar(out char value);

		/// <summary>
		/// Reads a <see cref="T:System.String" /> value. This call will succeed if the next entry is an <see cref="F:Sirenix.Serialization.EntryType.String" />.
		/// <para />
		/// If the call fails (and returns <c>false</c>), it will skip the current entry value, unless that entry is an <see cref="F:Sirenix.Serialization.EntryType.EndOfNode" /> or an <see cref="F:Sirenix.Serialization.EntryType.EndOfArray" />.
		/// </summary>
		/// <param name="value">The value that has been read.</param>
		/// <returns><c>true</c> if reading the value succeeded, otherwise <c>false</c></returns>
		public abstract bool ReadString(out string value);

		/// <summary>
		/// Reads a <see cref="T:System.Guid" /> value. This call will succeed if the next entry is an <see cref="F:Sirenix.Serialization.EntryType.Guid" />.
		/// <para />
		/// If the call fails (and returns <c>false</c>), it will skip the current entry value, unless that entry is an <see cref="F:Sirenix.Serialization.EntryType.EndOfNode" /> or an <see cref="F:Sirenix.Serialization.EntryType.EndOfArray" />.
		/// </summary>
		/// <param name="value">The value that has been read.</param>
		/// <returns><c>true</c> if reading the value succeeded, otherwise <c>false</c></returns>
		public abstract bool ReadGuid(out Guid value);

		/// <summary>
		/// Reads an <see cref="T:System.SByte" /> value. This call will succeed if the next entry is an <see cref="F:Sirenix.Serialization.EntryType.Integer" />.
		/// <para />
		/// If the value of the stored integer is smaller than <see cref="F:System.SByte.MinValue" /> or larger than <see cref="F:System.SByte.MaxValue" />, the result will be default(<see cref="T:System.SByte" />).
		/// <para />
		/// If the call fails (and returns <c>false</c>), it will skip the current entry value, unless that entry is an <see cref="F:Sirenix.Serialization.EntryType.EndOfNode" /> or an <see cref="F:Sirenix.Serialization.EntryType.EndOfArray" />.
		/// </summary>
		/// <param name="value">The value that has been read.</param>
		/// <returns><c>true</c> if reading the value succeeded, otherwise <c>false</c></returns>
		public abstract bool ReadSByte(out sbyte value);

		/// <summary>
		/// Reads a <see cref="T:System.Int16" /> value. This call will succeed if the next entry is an <see cref="F:Sirenix.Serialization.EntryType.Integer" />.
		/// <para />
		/// If the value of the stored integer is smaller than <see cref="F:System.Int16.MinValue" /> or larger than <see cref="F:System.Int16.MaxValue" />, the result will be default(<see cref="T:System.Int16" />).
		/// <para />
		/// If the call fails (and returns <c>false</c>), it will skip the current entry value, unless that entry is an <see cref="F:Sirenix.Serialization.EntryType.EndOfNode" /> or an <see cref="F:Sirenix.Serialization.EntryType.EndOfArray" />.
		/// </summary>
		/// <param name="value">The value that has been read.</param>
		/// <returns><c>true</c> if reading the value succeeded, otherwise <c>false</c></returns>
		public abstract bool ReadInt16(out short value);

		/// <summary>
		/// Reads an <see cref="T:System.Int32" /> value. This call will succeed if the next entry is an <see cref="F:Sirenix.Serialization.EntryType.Integer" />.
		/// <para />
		/// If the value of the stored integer is smaller than <see cref="F:System.Int32.MinValue" /> or larger than <see cref="F:System.Int32.MaxValue" />, the result will be default(<see cref="T:System.Int32" />).
		/// <para />
		/// If the call fails (and returns <c>false</c>), it will skip the current entry value, unless that entry is an <see cref="F:Sirenix.Serialization.EntryType.EndOfNode" /> or an <see cref="F:Sirenix.Serialization.EntryType.EndOfArray" />.
		/// </summary>
		/// <param name="value">The value that has been read.</param>
		/// <returns><c>true</c> if reading the value succeeded, otherwise <c>false</c></returns>
		public abstract bool ReadInt32(out int value);

		/// <summary>
		/// Reads a <see cref="T:System.Int64" /> value. This call will succeed if the next entry is an <see cref="F:Sirenix.Serialization.EntryType.Integer" />.
		/// <para />
		/// If the value of the stored integer is smaller than <see cref="F:System.Int64.MinValue" /> or larger than <see cref="F:System.Int64.MaxValue" />, the result will be default(<see cref="T:System.Int64" />).
		/// <para />
		/// If the call fails (and returns <c>false</c>), it will skip the current entry value, unless that entry is an <see cref="F:Sirenix.Serialization.EntryType.EndOfNode" /> or an <see cref="F:Sirenix.Serialization.EntryType.EndOfArray" />.
		/// </summary>
		/// <param name="value">The value that has been read.</param>
		/// <returns><c>true</c> if reading the value succeeded, otherwise <c>false</c></returns>
		public abstract bool ReadInt64(out long value);

		/// <summary>
		/// Reads a <see cref="T:System.Byte" /> value. This call will succeed if the next entry is an <see cref="F:Sirenix.Serialization.EntryType.Integer" />.
		/// <para />
		/// If the value of the stored integer is smaller than <see cref="F:System.Byte.MinValue" /> or larger than <see cref="F:System.Byte.MaxValue" />, the result will be default(<see cref="T:System.Byte" />).
		/// <para />
		/// If the call fails (and returns <c>false</c>), it will skip the current entry value, unless that entry is an <see cref="F:Sirenix.Serialization.EntryType.EndOfNode" /> or an <see cref="F:Sirenix.Serialization.EntryType.EndOfArray" />.
		/// </summary>
		/// <param name="value">The value that has been read.</param>
		/// <returns><c>true</c> if reading the value succeeded, otherwise <c>false</c></returns>
		public abstract bool ReadByte(out byte value);

		/// <summary>
		/// Reads an <see cref="T:System.UInt16" /> value. This call will succeed if the next entry is an <see cref="F:Sirenix.Serialization.EntryType.Integer" />.
		/// <para />
		/// If the value of the stored integer is smaller than <see cref="F:System.UInt16.MinValue" /> or larger than <see cref="F:System.UInt16.MaxValue" />, the result will be default(<see cref="T:System.UInt16" />).
		/// <para />
		/// If the call fails (and returns <c>false</c>), it will skip the current entry value, unless that entry is an <see cref="F:Sirenix.Serialization.EntryType.EndOfNode" /> or an <see cref="F:Sirenix.Serialization.EntryType.EndOfArray" />.
		/// </summary>
		/// <param name="value">The value that has been read.</param>
		/// <returns><c>true</c> if reading the value succeeded, otherwise <c>false</c></returns>
		public abstract bool ReadUInt16(out ushort value);

		/// <summary>
		/// Reads an <see cref="T:System.UInt32" /> value. This call will succeed if the next entry is an <see cref="F:Sirenix.Serialization.EntryType.Integer" />.
		/// <para />
		/// If the value of the stored integer is smaller than <see cref="F:System.UInt32.MinValue" /> or larger than <see cref="F:System.UInt32.MaxValue" />, the result will be default(<see cref="T:System.UInt32" />).
		/// <para />
		/// If the call fails (and returns <c>false</c>), it will skip the current entry value, unless that entry is an <see cref="F:Sirenix.Serialization.EntryType.EndOfNode" /> or an <see cref="F:Sirenix.Serialization.EntryType.EndOfArray" />.
		/// </summary>
		/// <param name="value">The value that has been read.</param>
		/// <returns><c>true</c> if reading the value succeeded, otherwise <c>false</c></returns>
		public abstract bool ReadUInt32(out uint value);

		/// <summary>
		/// Reads an <see cref="T:System.UInt64" /> value. This call will succeed if the next entry is an <see cref="F:Sirenix.Serialization.EntryType.Integer" />.
		/// <para />
		/// If the value of the stored integer is smaller than <see cref="F:System.UInt64.MinValue" /> or larger than <see cref="F:System.UInt64.MaxValue" />, the result will be default(<see cref="T:System.UInt64" />).
		/// <para />
		/// If the call fails (and returns <c>false</c>), it will skip the current entry value, unless that entry is an <see cref="F:Sirenix.Serialization.EntryType.EndOfNode" /> or an <see cref="F:Sirenix.Serialization.EntryType.EndOfArray" />.
		/// </summary>
		/// <param name="value">The value that has been read.</param>
		/// <returns><c>true</c> if reading the value succeeded, otherwise <c>false</c></returns>
		public abstract bool ReadUInt64(out ulong value);

		/// <summary>
		/// Reads a <see cref="T:System.Decimal" /> value. This call will succeed if the next entry is an <see cref="F:Sirenix.Serialization.EntryType.FloatingPoint" /> or an <see cref="F:Sirenix.Serialization.EntryType.Integer" />.
		/// <para />
		/// If the stored integer or floating point value is smaller than <see cref="F:System.Decimal.MinValue" /> or larger than <see cref="F:System.Decimal.MaxValue" />, the result will be default(<see cref="T:System.Decimal" />).
		/// <para />
		/// If the call fails (and returns <c>false</c>), it will skip the current entry value, unless that entry is an <see cref="F:Sirenix.Serialization.EntryType.EndOfNode" /> or an <see cref="F:Sirenix.Serialization.EntryType.EndOfArray" />.
		/// </summary>
		/// <param name="value">The value that has been read.</param>
		/// <returns><c>true</c> if reading the value succeeded, otherwise <c>false</c></returns>
		public abstract bool ReadDecimal(out decimal value);

		/// <summary>
		/// Reads a <see cref="T:System.Single" /> value. This call will succeed if the next entry is an <see cref="F:Sirenix.Serialization.EntryType.FloatingPoint" /> or an <see cref="F:Sirenix.Serialization.EntryType.Integer" />.
		/// <para />
		/// If the stored integer or floating point value is smaller than <see cref="F:System.Single.MinValue" /> or larger than <see cref="F:System.Single.MaxValue" />, the result will be default(<see cref="T:System.Single" />).
		/// <para />
		/// If the call fails (and returns <c>false</c>), it will skip the current entry value, unless that entry is an <see cref="F:Sirenix.Serialization.EntryType.EndOfNode" /> or an <see cref="F:Sirenix.Serialization.EntryType.EndOfArray" />.
		/// </summary>
		/// <param name="value">The value that has been read.</param>
		/// <returns><c>true</c> if reading the value succeeded, otherwise <c>false</c></returns>
		public abstract bool ReadSingle(out float value);

		/// <summary>
		/// Reads a <see cref="T:System.Double" /> value. This call will succeed if the next entry is an <see cref="F:Sirenix.Serialization.EntryType.FloatingPoint" /> or an <see cref="F:Sirenix.Serialization.EntryType.Integer" />.
		/// <para />
		/// If the stored integer or floating point value is smaller than <see cref="F:System.Double.MinValue" /> or larger than <see cref="F:System.Double.MaxValue" />, the result will be default(<see cref="T:System.Double" />).
		/// <para />
		/// If the call fails (and returns <c>false</c>), it will skip the current entry value, unless that entry is an <see cref="F:Sirenix.Serialization.EntryType.EndOfNode" /> or an <see cref="F:Sirenix.Serialization.EntryType.EndOfArray" />.
		/// </summary>
		/// <param name="value">The value that has been read.</param>
		/// <returns><c>true</c> if reading the value succeeded, otherwise <c>false</c></returns>
		public abstract bool ReadDouble(out double value);

		/// <summary>
		/// Reads a <see cref="T:System.Boolean" /> value. This call will succeed if the next entry is an <see cref="F:Sirenix.Serialization.EntryType.Boolean" />.
		/// <para />
		/// If the call fails (and returns <c>false</c>), it will skip the current entry value, unless that entry is an <see cref="F:Sirenix.Serialization.EntryType.EndOfNode" /> or an <see cref="F:Sirenix.Serialization.EntryType.EndOfArray" />.
		/// </summary>
		/// <param name="value">The value that has been read.</param>
		/// <returns><c>true</c> if reading the value succeeded, otherwise <c>false</c></returns>
		public abstract bool ReadBoolean(out bool value);

		/// <summary>
		/// Reads a <c>null</c> value. This call will succeed if the next entry is an <see cref="F:Sirenix.Serialization.EntryType.Null" />.
		/// <para />
		/// If the call fails (and returns <c>false</c>), it will skip the current entry value, unless that entry is an <see cref="F:Sirenix.Serialization.EntryType.EndOfNode" /> or an <see cref="F:Sirenix.Serialization.EntryType.EndOfArray" />.
		/// </summary>
		/// <returns><c>true</c> if reading the value succeeded, otherwise <c>false</c></returns>
		public abstract bool ReadNull();

		/// <summary>
		/// Skips the next entry value, unless it is an <see cref="F:Sirenix.Serialization.EntryType.EndOfNode" /> or an <see cref="F:Sirenix.Serialization.EntryType.EndOfArray" />. If the next entry value is an <see cref="F:Sirenix.Serialization.EntryType.StartOfNode" /> or an <see cref="F:Sirenix.Serialization.EntryType.StartOfArray" />, all of its contents will be processed, deserialized and registered in the deserialization context, so that internal reference values are not lost to entries further down the stream.
		/// </summary>
		public virtual void SkipEntry()
		{
			switch (PeekEntry())
			{
			case EntryType.StartOfNode:
			{
				bool exitNode = true;
				EnterNode(out var type);
				try
				{
					if (type != null)
					{
						if (FormatterUtilities.IsPrimitiveType(type))
						{
							Serializer serializer = Serializer.Get(type);
							object value = serializer.ReadValueWeak(this);
							if (CurrentNodeId >= 0)
							{
								Context.RegisterInternalReference(CurrentNodeId, value);
							}
						}
						else
						{
							IFormatter formatter = FormatterLocator.GetFormatter(type, Context.Config.SerializationPolicy);
							object value2 = formatter.Deserialize(this);
							if (CurrentNodeId >= 0)
							{
								Context.RegisterInternalReference(CurrentNodeId, value2);
							}
						}
						break;
					}
					while (true)
					{
						switch (PeekEntry())
						{
						case EntryType.EndOfArray:
							ReadToNextEntry();
							break;
						default:
							SkipEntry();
							break;
						case EntryType.EndOfNode:
						case EntryType.EndOfStream:
							return;
						}
					}
				}
				catch (SerializationAbortException ex)
				{
					exitNode = false;
					throw ex;
				}
				finally
				{
					if (exitNode)
					{
						ExitNode();
					}
				}
			}
			case EntryType.StartOfArray:
				ReadToNextEntry();
				while (true)
				{
					switch (PeekEntry())
					{
					case EntryType.EndOfArray:
						ReadToNextEntry();
						return;
					case EntryType.EndOfNode:
						ReadToNextEntry();
						break;
					default:
						SkipEntry();
						break;
					case EntryType.EndOfStream:
						return;
					}
				}
			default:
				ReadToNextEntry();
				break;
			case EntryType.EndOfNode:
			case EntryType.EndOfArray:
				break;
			}
		}

		/// <summary>
		/// Disposes all resources and streams kept by the data reader.
		/// </summary>
		public abstract void Dispose();

		/// <summary>
		/// Tells the reader that a new serialization session is about to begin, and that it should clear all cached values left over from any prior serialization sessions.
		/// This method is only relevant when the same reader is used to deserialize several different, unrelated values.
		/// </summary>
		public virtual void PrepareNewSerializationSession()
		{
			ClearNodes();
		}

		/// <summary>
		/// Gets a dump of the data being read by the writer. The format of this dump varies, but should be useful for debugging purposes.
		/// </summary>
		public abstract string GetDataDump();

		/// <summary>
		/// Peeks the current entry.
		/// </summary>
		/// <returns>The peeked entry.</returns>
		protected abstract EntryType PeekEntry();

		/// <summary>
		/// Consumes the current entry, and reads to the next one.
		/// </summary>
		/// <returns>The next entry.</returns>
		protected abstract EntryType ReadToNextEntry();
	}
}
