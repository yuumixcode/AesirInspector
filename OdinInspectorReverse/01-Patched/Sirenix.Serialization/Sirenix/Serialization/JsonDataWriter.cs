using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Sirenix.Serialization
{
	/// <summary>
	/// Writes json data to a stream that can be read by a <see cref="T:Sirenix.Serialization.JsonDataReader" />.
	/// </summary>
	/// <seealso cref="T:Sirenix.Serialization.BaseDataWriter" />
	public class JsonDataWriter : BaseDataWriter
	{
		private static readonly uint[] ByteToHexCharLookup = CreateByteToHexLookup();

		private static readonly string NEW_LINE = Environment.NewLine;

		private bool justStarted;

		private bool forceNoSeparatorNextLine;

		private Dictionary<Type, Delegate> primitiveTypeWriters;

		private Dictionary<Type, int> seenTypes = new Dictionary<Type, int>(16);

		private byte[] buffer = new byte[102400];

		private int bufferIndex;

		/// <summary>
		/// Gets or sets a value indicating whether the json should be packed, or formatted as human-readable.
		/// </summary>
		/// <value>
		///   <c>true</c> if the json should be formatted as human-readable; otherwise, <c>false</c>.
		/// </value>
		public bool FormatAsReadable;

		/// <summary>
		/// Whether to enable an optimization that ensures any given type name is only written once into the json stream, and thereafter kept track of by ID.
		/// </summary>
		public bool EnableTypeOptimization;

		public JsonDataWriter()
			: this(null, null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Sirenix.Serialization.JsonDataWriter" /> class.
		/// </summary>
		/// <param name="stream">The base stream of the writer.</param>
		/// <param name="context">The serialization context to use.</param>&gt;
		/// <param name="formatAsReadable">Whether the json should be packed, or formatted as human-readable.</param>
		public JsonDataWriter(Stream stream, SerializationContext context, bool formatAsReadable = true)
			: base(stream, context)
		{
			FormatAsReadable = formatAsReadable;
			justStarted = true;
			EnableTypeOptimization = true;
			primitiveTypeWriters = new Dictionary<Type, Delegate>
			{
				{
					typeof(char),
					new Action<string, char>(WriteChar)
				},
				{
					typeof(sbyte),
					new Action<string, sbyte>(WriteSByte)
				},
				{
					typeof(short),
					new Action<string, short>(WriteInt16)
				},
				{
					typeof(int),
					new Action<string, int>(WriteInt32)
				},
				{
					typeof(long),
					new Action<string, long>(WriteInt64)
				},
				{
					typeof(byte),
					new Action<string, byte>(WriteByte)
				},
				{
					typeof(ushort),
					new Action<string, ushort>(WriteUInt16)
				},
				{
					typeof(uint),
					new Action<string, uint>(WriteUInt32)
				},
				{
					typeof(ulong),
					new Action<string, ulong>(WriteUInt64)
				},
				{
					typeof(decimal),
					new Action<string, decimal>(WriteDecimal)
				},
				{
					typeof(bool),
					new Action<string, bool>(WriteBoolean)
				},
				{
					typeof(float),
					new Action<string, float>(WriteSingle)
				},
				{
					typeof(double),
					new Action<string, double>(WriteDouble)
				},
				{
					typeof(Guid),
					new Action<string, Guid>(WriteGuid)
				}
			};
		}

		/// <summary>
		/// Enable the "just started" flag, causing the writer to start a new "base" json object container.
		/// </summary>
		public void MarkJustStarted()
		{
			justStarted = true;
		}

		/// <summary>
		/// Flushes everything that has been written so far to the writer's base stream.
		/// </summary>
		public override void FlushToStream()
		{
			if (bufferIndex > 0)
			{
				Stream.Write(buffer, 0, bufferIndex);
				bufferIndex = 0;
			}
			base.FlushToStream();
		}

		/// <summary>
		/// Writes the beginning of a reference node.
		/// <para />
		/// This call MUST eventually be followed by a corresponding call to <see cref="M:Sirenix.Serialization.IDataWriter.EndNode(System.String)" />, with the same name.
		/// </summary>
		/// <param name="name">The name of the reference node.</param>
		/// <param name="type">The type of the reference node. If null, no type metadata will be written.</param>
		/// <param name="id">The id of the reference node. This id is acquired by calling <see cref="M:Sirenix.Serialization.SerializationContext.TryRegisterInternalReference(System.Object,System.Int32@)" />.</param>
		public override void BeginReferenceNode(string name, Type type, int id)
		{
			WriteEntry(name, "{");
			PushNode(name, id, type);
			forceNoSeparatorNextLine = true;
			WriteInt32("$id", id);
			if (type != null)
			{
				WriteTypeEntry(type);
			}
		}

		/// <summary>
		/// Begins a struct/value type node. This is essentially the same as a reference node, except it has no internal reference id.
		/// <para />
		/// This call MUST eventually be followed by a corresponding call to <see cref="M:Sirenix.Serialization.IDataWriter.EndNode(System.String)" />, with the same name.
		/// </summary>
		/// <param name="name">The name of the struct node.</param>
		/// <param name="type">The type of the struct node. If null, no type metadata will be written.</param>
		public override void BeginStructNode(string name, Type type)
		{
			WriteEntry(name, "{");
			PushNode(name, -1, type);
			forceNoSeparatorNextLine = true;
			if (type != null)
			{
				WriteTypeEntry(type);
			}
		}

		/// <summary>
		/// Ends the current node with the given name. If the current node has another name, an <see cref="T:System.InvalidOperationException" /> is thrown.
		/// </summary>
		/// <param name="name">The name of the node to end. This has to be the name of the current node.</param>
		public override void EndNode(string name)
		{
			PopNode(name);
			StartNewLine(noSeparator: true);
			EnsureBufferSpace(1);
			buffer[bufferIndex++] = 125;
		}

		/// <summary>
		/// Begins an array node of the given length.
		/// </summary>
		/// <param name="length">The length of the array to come.</param>
		public override void BeginArrayNode(long length)
		{
			WriteInt64("$rlength", length);
			WriteEntry("$rcontent", "[");
			forceNoSeparatorNextLine = true;
			PushArray();
		}

		/// <summary>
		/// Ends the current array node, if the current node is an array node.
		/// </summary>
		public override void EndArrayNode()
		{
			PopArray();
			StartNewLine(noSeparator: true);
			EnsureBufferSpace(1);
			buffer[bufferIndex++] = 93;
		}

		/// <summary>
		/// Writes a primitive array to the stream.
		/// </summary>
		/// <typeparam name="T">The element type of the primitive array. Valid element types can be determined using <see cref="M:Sirenix.Serialization.FormatterUtilities.IsPrimitiveArrayType(System.Type)" />.</typeparam>
		/// <param name="array">The primitive array to write.</param>
		/// <exception cref="T:System.ArgumentException">Type  + typeof(T).Name +  is not a valid primitive array type.</exception>
		/// <exception cref="T:System.ArgumentNullException">array</exception>
		public override void WritePrimitiveArray<T>(T[] array)
		{
			if (!FormatterUtilities.IsPrimitiveArrayType(typeof(T)))
			{
				throw new ArgumentException("Type " + typeof(T).Name + " is not a valid primitive array type.");
			}
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			Action<string, T> writer = (Action<string, T>)primitiveTypeWriters[typeof(T)];
			WriteInt64("$plength", array.Length);
			WriteEntry("$pcontent", "[");
			forceNoSeparatorNextLine = true;
			PushArray();
			for (int i = 0; i < array.Length; i++)
			{
				writer(null, array[i]);
			}
			PopArray();
			StartNewLine(noSeparator: true);
			EnsureBufferSpace(1);
			buffer[bufferIndex++] = 93;
		}

		/// <summary>
		/// Writes a <see cref="T:System.Boolean" /> value to the stream.
		/// </summary>
		/// <param name="name">The name of the value. If this is null, no name will be written.</param>
		/// <param name="value">The value to write.</param>
		public override void WriteBoolean(string name, bool value)
		{
			WriteEntry(name, value ? "true" : "false");
		}

		/// <summary>
		/// Writes a <see cref="T:System.Byte" /> value to the stream.
		/// </summary>
		/// <param name="name">The name of the value. If this is null, no name will be written.</param>
		/// <param name="value">The value to write.</param>
		public override void WriteByte(string name, byte value)
		{
			WriteUInt64(name, value);
		}

		/// <summary>
		/// Writes a <see cref="T:System.Char" /> value to the stream.
		/// </summary>
		/// <param name="name">The name of the value. If this is null, no name will be written.</param>
		/// <param name="value">The value to write.</param>
		public override void WriteChar(string name, char value)
		{
			WriteString(name, value.ToString(CultureInfo.InvariantCulture));
		}

		/// <summary>
		/// Writes a <see cref="T:System.Decimal" /> value to the stream.
		/// </summary>
		/// <param name="name">The name of the value. If this is null, no name will be written.</param>
		/// <param name="value">The value to write.</param>
		public override void WriteDecimal(string name, decimal value)
		{
			WriteEntry(name, value.ToString("G", CultureInfo.InvariantCulture));
		}

		/// <summary>
		/// Writes a <see cref="T:System.Double" /> value to the stream.
		/// </summary>
		/// <param name="name">The name of the value. If this is null, no name will be written.</param>
		/// <param name="value">The value to write.</param>
		public override void WriteDouble(string name, double value)
		{
			WriteEntry(name, value.ToString("R", CultureInfo.InvariantCulture));
		}

		/// <summary>
		/// Writes an <see cref="T:System.Int32" /> value to the stream.
		/// </summary>
		/// <param name="name">The name of the value. If this is null, no name will be written.</param>
		/// <param name="value">The value to write.</param>
		public override void WriteInt32(string name, int value)
		{
			WriteInt64(name, value);
		}

		/// <summary>
		/// Writes a <see cref="T:System.Int64" /> value to the stream.
		/// </summary>
		/// <param name="name">The name of the value. If this is null, no name will be written.</param>
		/// <param name="value">The value to write.</param>
		public override void WriteInt64(string name, long value)
		{
			WriteEntry(name, value.ToString("D", CultureInfo.InvariantCulture));
		}

		/// <summary>
		/// Writes a null value to the stream.
		/// </summary>
		/// <param name="name">The name of the value. If this is null, no name will be written.</param>
		public override void WriteNull(string name)
		{
			WriteEntry(name, "null");
		}

		/// <summary>
		/// Writes an internal reference to the stream.
		/// </summary>
		/// <param name="name">The name of the value. If this is null, no name will be written.</param>
		/// <param name="id">The value to write.</param>
		public override void WriteInternalReference(string name, int id)
		{
			WriteEntry(name, "$iref:" + id.ToString("D", CultureInfo.InvariantCulture));
		}

		/// <summary>
		/// Writes an <see cref="T:System.SByte" /> value to the stream.
		/// </summary>
		/// <param name="name">The name of the value. If this is null, no name will be written.</param>
		/// <param name="value">The value to write.</param>
		public override void WriteSByte(string name, sbyte value)
		{
			WriteInt64(name, value);
		}

		/// <summary>
		/// Writes a <see cref="T:System.Int16" /> value to the stream.
		/// </summary>
		/// <param name="name">The name of the value. If this is null, no name will be written.</param>
		/// <param name="value">The value to write.</param>
		public override void WriteInt16(string name, short value)
		{
			WriteInt64(name, value);
		}

		/// <summary>
		/// Writes a <see cref="T:System.Single" /> value to the stream.
		/// </summary>
		/// <param name="name">The name of the value. If this is null, no name will be written.</param>
		/// <param name="value">The value to write.</param>
		public override void WriteSingle(string name, float value)
		{
			WriteEntry(name, value.ToString("R", CultureInfo.InvariantCulture));
		}

		/// <summary>
		/// Writes a <see cref="T:System.String" /> value to the stream.
		/// </summary>
		/// <param name="name">The name of the value. If this is null, no name will be written.</param>
		/// <param name="value">The value to write.</param>
		public override void WriteString(string name, string value)
		{
			StartNewLine();
			if (name != null)
			{
				EnsureBufferSpace(name.Length + value.Length + 6);
				buffer[bufferIndex++] = 34;
				for (int i = 0; i < name.Length; i++)
				{
					buffer[bufferIndex++] = (byte)name[i];
				}
				buffer[bufferIndex++] = 34;
				buffer[bufferIndex++] = 58;
				if (FormatAsReadable)
				{
					buffer[bufferIndex++] = 32;
				}
			}
			else
			{
				EnsureBufferSpace(value.Length + 2);
			}
			buffer[bufferIndex++] = 34;
			Buffer_WriteString_WithEscape(value);
			buffer[bufferIndex++] = 34;
		}

		/// <summary>
		/// Writes a <see cref="T:System.Guid" /> value to the stream.
		/// </summary>
		/// <param name="name">The name of the value. If this is null, no name will be written.</param>
		/// <param name="value">The value to write.</param>
		public override void WriteGuid(string name, Guid value)
		{
			WriteEntry(name, value.ToString("D", CultureInfo.InvariantCulture));
		}

		/// <summary>
		/// Writes an <see cref="T:System.UInt32" /> value to the stream.
		/// </summary>
		/// <param name="name">The name of the value. If this is null, no name will be written.</param>
		/// <param name="value">The value to write.</param>
		public override void WriteUInt32(string name, uint value)
		{
			WriteUInt64(name, value);
		}

		/// <summary>
		/// Writes an <see cref="T:System.UInt64" /> value to the stream.
		/// </summary>
		/// <param name="name">The name of the value. If this is null, no name will be written.</param>
		/// <param name="value">The value to write.</param>
		public override void WriteUInt64(string name, ulong value)
		{
			WriteEntry(name, value.ToString("D", CultureInfo.InvariantCulture));
		}

		/// <summary>
		/// Writes an external index reference to the stream.
		/// </summary>
		/// <param name="name">The name of the value. If this is null, no name will be written.</param>
		/// <param name="index">The value to write.</param>
		public override void WriteExternalReference(string name, int index)
		{
			WriteEntry(name, "$eref:" + index.ToString("D", CultureInfo.InvariantCulture));
		}

		/// <summary>
		/// Writes an external guid reference to the stream.
		/// </summary>
		/// <param name="name">The name of the value. If this is null, no name will be written.</param>
		/// <param name="guid">The value to write.</param>
		public override void WriteExternalReference(string name, Guid guid)
		{
			WriteEntry(name, "$guidref:" + guid.ToString("D", CultureInfo.InvariantCulture));
		}

		/// <summary>
		/// Writes an external string reference to the stream.
		/// </summary>
		/// <param name="name">The name of the value. If this is null, no name will be written.</param>
		/// <param name="id">The value to write.</param>
		public override void WriteExternalReference(string name, string id)
		{
			if (id == null)
			{
				throw new ArgumentNullException("id");
			}
			WriteEntry(name, "$fstrref");
			EnsureBufferSpace(id.Length + 3);
			buffer[bufferIndex++] = 58;
			buffer[bufferIndex++] = 34;
			Buffer_WriteString_WithEscape(id);
			buffer[bufferIndex++] = 34;
		}

		/// <summary>
		/// Writes an <see cref="T:System.UInt16" /> value to the stream.
		/// </summary>
		/// <param name="name">The name of the value. If this is null, no name will be written.</param>
		/// <param name="value">The value to write.</param>
		public override void WriteUInt16(string name, ushort value)
		{
			WriteUInt64(name, value);
		}

		/// <summary>
		/// Disposes all resources kept by the data writer, except the stream, which can be reused later.
		/// </summary>
		public override void Dispose()
		{
		}

		/// <summary>
		/// Tells the writer that a new serialization session is about to begin, and that it should clear all cached values left over from any prior serialization sessions.
		/// This method is only relevant when the same writer is used to serialize several different, unrelated values.
		/// </summary>
		public override void PrepareNewSerializationSession()
		{
			base.PrepareNewSerializationSession();
			seenTypes.Clear();
			justStarted = true;
		}

		public override string GetDataDump()
		{
			if (!Stream.CanRead)
			{
				return "Json data stream for writing cannot be read; cannot dump data.";
			}
			if (!Stream.CanSeek)
			{
				return "Json data stream cannot seek; cannot dump data.";
			}
			long oldPosition = Stream.Position;
			byte[] bytes = new byte[oldPosition];
			Stream.Position = 0L;
			Stream.Read(bytes, 0, (int)oldPosition);
			Stream.Position = oldPosition;
			return "Json: " + Encoding.UTF8.GetString(bytes, 0, bytes.Length);
		}

		private void WriteEntry(string name, string contents)
		{
			StartNewLine();
			if (name != null)
			{
				EnsureBufferSpace(name.Length + contents.Length + 4);
				buffer[bufferIndex++] = 34;
				for (int i = 0; i < name.Length; i++)
				{
					buffer[bufferIndex++] = (byte)name[i];
				}
				buffer[bufferIndex++] = 34;
				buffer[bufferIndex++] = 58;
				if (FormatAsReadable)
				{
					buffer[bufferIndex++] = 32;
				}
			}
			else
			{
				EnsureBufferSpace(contents.Length);
			}
			for (int j = 0; j < contents.Length; j++)
			{
				buffer[bufferIndex++] = (byte)contents[j];
			}
		}

		private void WriteEntry(string name, string contents, char surroundContentsWith)
		{
			StartNewLine();
			if (name != null)
			{
				EnsureBufferSpace(name.Length + contents.Length + 6);
				buffer[bufferIndex++] = 34;
				for (int i = 0; i < name.Length; i++)
				{
					buffer[bufferIndex++] = (byte)name[i];
				}
				buffer[bufferIndex++] = 34;
				buffer[bufferIndex++] = 58;
				if (FormatAsReadable)
				{
					buffer[bufferIndex++] = 32;
				}
			}
			else
			{
				EnsureBufferSpace(contents.Length + 2);
			}
			buffer[bufferIndex++] = (byte)surroundContentsWith;
			for (int j = 0; j < contents.Length; j++)
			{
				buffer[bufferIndex++] = (byte)contents[j];
			}
			buffer[bufferIndex++] = (byte)surroundContentsWith;
		}

		private void WriteTypeEntry(Type type)
		{
			if (EnableTypeOptimization)
			{
				if (seenTypes.TryGetValue(type, out var id))
				{
					WriteInt32("$type", id);
					return;
				}
				id = seenTypes.Count;
				seenTypes.Add(type, id);
				WriteString("$type", id + "|" + base.Context.Binder.BindToName(type, base.Context.Config.DebugContext));
			}
			else
			{
				WriteString("$type", base.Context.Binder.BindToName(type, base.Context.Config.DebugContext));
			}
		}

		private void StartNewLine(bool noSeparator = false)
		{
			if (justStarted)
			{
				justStarted = false;
				return;
			}
			if (!noSeparator && !forceNoSeparatorNextLine)
			{
				EnsureBufferSpace(1);
				buffer[bufferIndex++] = 44;
			}
			forceNoSeparatorNextLine = false;
			if (FormatAsReadable)
			{
				int count = base.NodeDepth * 4;
				EnsureBufferSpace(NEW_LINE.Length + count);
				for (int i = 0; i < NEW_LINE.Length; i++)
				{
					buffer[bufferIndex++] = (byte)NEW_LINE[i];
				}
				for (int j = 0; j < count; j++)
				{
					buffer[bufferIndex++] = 32;
				}
			}
		}

		private void EnsureBufferSpace(int space)
		{
			int length = buffer.Length;
			if (space > length)
			{
				throw new Exception("Insufficient buffer capacity");
			}
			if (bufferIndex + space > length)
			{
				FlushToStream();
			}
		}

		private void Buffer_WriteString_WithEscape(string str)
		{
			EnsureBufferSpace(str.Length);
			for (int i = 0; i < str.Length; i++)
			{
				char c = str[i];
				if (c < '\0' || c > '\u007f')
				{
					EnsureBufferSpace(str.Length - i + 6);
					buffer[bufferIndex++] = 92;
					buffer[bufferIndex++] = 117;
					int byte1 = (int)c >> 8;
					byte byte2 = (byte)c;
					uint lookup = ByteToHexCharLookup[byte1];
					buffer[bufferIndex++] = (byte)lookup;
					buffer[bufferIndex++] = (byte)(lookup >> 16);
					lookup = ByteToHexCharLookup[byte2];
					buffer[bufferIndex++] = (byte)lookup;
					buffer[bufferIndex++] = (byte)(lookup >> 16);
					continue;
				}
				EnsureBufferSpace(2);
				switch (c)
				{
				case '"':
					buffer[bufferIndex++] = 92;
					buffer[bufferIndex++] = 34;
					break;
				case '\\':
					buffer[bufferIndex++] = 92;
					buffer[bufferIndex++] = 92;
					break;
				case '\a':
					buffer[bufferIndex++] = 92;
					buffer[bufferIndex++] = 97;
					break;
				case '\b':
					buffer[bufferIndex++] = 92;
					buffer[bufferIndex++] = 98;
					break;
				case '\f':
					buffer[bufferIndex++] = 92;
					buffer[bufferIndex++] = 102;
					break;
				case '\n':
					buffer[bufferIndex++] = 92;
					buffer[bufferIndex++] = 110;
					break;
				case '\r':
					buffer[bufferIndex++] = 92;
					buffer[bufferIndex++] = 114;
					break;
				case '\t':
					buffer[bufferIndex++] = 92;
					buffer[bufferIndex++] = 116;
					break;
				case '\0':
					buffer[bufferIndex++] = 92;
					buffer[bufferIndex++] = 48;
					break;
				default:
					buffer[bufferIndex++] = (byte)c;
					break;
				}
			}
		}

		private static uint[] CreateByteToHexLookup()
		{
			uint[] result = new uint[256];
			for (int i = 0; i < 256; i++)
			{
				string s = i.ToString("x2", CultureInfo.InvariantCulture);
				result[i] = s[0] + ((uint)s[1] << 16);
			}
			return result;
		}
	}
}
