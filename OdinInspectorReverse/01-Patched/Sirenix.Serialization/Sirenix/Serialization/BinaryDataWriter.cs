using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Sirenix.Serialization.Utilities;
using Sirenix.Serialization.Utilities.Unsafe;

namespace Sirenix.Serialization
{
	/// <summary>
	/// Writes data to a stream that can be read by a <see cref="T:Sirenix.Serialization.BinaryDataReader" />.
	/// </summary>
	/// <seealso cref="T:Sirenix.Serialization.BaseDataWriter" />
	public class BinaryDataWriter : BaseDataWriter
	{
		private struct Struct256Bit
		{
			public decimal d1;

			public decimal d2;
		}

		[StructLayout(LayoutKind.Explicit, Size = 8)]
		private struct SixtyFourBitValueToByteUnion
		{
			[FieldOffset(0)]
			public byte b0;

			[FieldOffset(1)]
			public byte b1;

			[FieldOffset(2)]
			public byte b2;

			[FieldOffset(3)]
			public byte b3;

			[FieldOffset(4)]
			public byte b4;

			[FieldOffset(5)]
			public byte b5;

			[FieldOffset(6)]
			public byte b6;

			[FieldOffset(7)]
			public byte b7;

			[FieldOffset(0)]
			public double doubleValue;

			[FieldOffset(0)]
			public ulong ulongValue;

			[FieldOffset(0)]
			public long longValue;
		}

		[StructLayout(LayoutKind.Explicit, Size = 16)]
		private struct OneTwentyEightBitValueToByteUnion
		{
			[FieldOffset(0)]
			public byte b0;

			[FieldOffset(1)]
			public byte b1;

			[FieldOffset(2)]
			public byte b2;

			[FieldOffset(3)]
			public byte b3;

			[FieldOffset(4)]
			public byte b4;

			[FieldOffset(5)]
			public byte b5;

			[FieldOffset(6)]
			public byte b6;

			[FieldOffset(7)]
			public byte b7;

			[FieldOffset(8)]
			public byte b8;

			[FieldOffset(9)]
			public byte b9;

			[FieldOffset(10)]
			public byte b10;

			[FieldOffset(11)]
			public byte b11;

			[FieldOffset(12)]
			public byte b12;

			[FieldOffset(13)]
			public byte b13;

			[FieldOffset(14)]
			public byte b14;

			[FieldOffset(15)]
			public byte b15;

			[FieldOffset(0)]
			public Guid guidValue;

			[FieldOffset(0)]
			public decimal decimalValue;
		}

		private static readonly Dictionary<Type, Delegate> PrimitiveGetBytesMethods = new Dictionary<Type, Delegate>(FastTypeComparer.Instance)
		{
			{
				typeof(char),
				(Action<byte[], int, char>)delegate(byte[] b, int i, char v)
				{
					ProperBitConverter.GetBytes(b, i, v);
				}
			},
			{
				typeof(byte),
				(Action<byte[], int, byte>)delegate(byte[] b, int i, byte v)
				{
					b[i] = v;
				}
			},
			{
				typeof(sbyte),
				(Action<byte[], int, sbyte>)delegate(byte[] b, int i, sbyte v)
				{
					b[i] = (byte)v;
				}
			},
			{
				typeof(bool),
				(Action<byte[], int, bool>)delegate(byte[] b, int i, bool v)
				{
					b[i] = (v ? ((byte)1) : ((byte)0));
				}
			},
			{
				typeof(short),
				new Action<byte[], int, short>(ProperBitConverter.GetBytes)
			},
			{
				typeof(int),
				new Action<byte[], int, int>(ProperBitConverter.GetBytes)
			},
			{
				typeof(long),
				new Action<byte[], int, long>(ProperBitConverter.GetBytes)
			},
			{
				typeof(ushort),
				new Action<byte[], int, ushort>(ProperBitConverter.GetBytes)
			},
			{
				typeof(uint),
				new Action<byte[], int, uint>(ProperBitConverter.GetBytes)
			},
			{
				typeof(ulong),
				new Action<byte[], int, ulong>(ProperBitConverter.GetBytes)
			},
			{
				typeof(decimal),
				new Action<byte[], int, decimal>(ProperBitConverter.GetBytes)
			},
			{
				typeof(float),
				new Action<byte[], int, float>(ProperBitConverter.GetBytes)
			},
			{
				typeof(double),
				new Action<byte[], int, double>(ProperBitConverter.GetBytes)
			},
			{
				typeof(Guid),
				new Action<byte[], int, Guid>(ProperBitConverter.GetBytes)
			}
		};

		private static readonly Dictionary<Type, int> PrimitiveSizes = new Dictionary<Type, int>(FastTypeComparer.Instance)
		{
			{
				typeof(char),
				2
			},
			{
				typeof(byte),
				1
			},
			{
				typeof(sbyte),
				1
			},
			{
				typeof(bool),
				1
			},
			{
				typeof(short),
				2
			},
			{
				typeof(int),
				4
			},
			{
				typeof(long),
				8
			},
			{
				typeof(ushort),
				2
			},
			{
				typeof(uint),
				4
			},
			{
				typeof(ulong),
				8
			},
			{
				typeof(decimal),
				16
			},
			{
				typeof(float),
				4
			},
			{
				typeof(double),
				8
			},
			{
				typeof(Guid),
				16
			}
		};

		private readonly byte[] small_buffer = new byte[16];

		private readonly byte[] buffer = new byte[102400];

		private int bufferIndex;

		private readonly Dictionary<Type, int> types = new Dictionary<Type, int>(16, FastTypeComparer.Instance);

		public bool CompressStringsTo8BitWhenPossible;

		private static readonly Dictionary<Type, Action<BinaryDataWriter, object>> PrimitiveArrayWriters = new Dictionary<Type, Action<BinaryDataWriter, object>>(FastTypeComparer.Instance)
		{
			{
				typeof(char),
				WritePrimitiveArray_char
			},
			{
				typeof(sbyte),
				WritePrimitiveArray_sbyte
			},
			{
				typeof(short),
				WritePrimitiveArray_short
			},
			{
				typeof(int),
				WritePrimitiveArray_int
			},
			{
				typeof(long),
				WritePrimitiveArray_long
			},
			{
				typeof(byte),
				WritePrimitiveArray_byte
			},
			{
				typeof(ushort),
				WritePrimitiveArray_ushort
			},
			{
				typeof(uint),
				WritePrimitiveArray_uint
			},
			{
				typeof(ulong),
				WritePrimitiveArray_ulong
			},
			{
				typeof(decimal),
				WritePrimitiveArray_decimal
			},
			{
				typeof(bool),
				WritePrimitiveArray_bool
			},
			{
				typeof(float),
				WritePrimitiveArray_float
			},
			{
				typeof(double),
				WritePrimitiveArray_double
			},
			{
				typeof(Guid),
				WritePrimitiveArray_Guid
			}
		};

		public BinaryDataWriter()
			: base(null, null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Sirenix.Serialization.BinaryDataWriter" /> class.
		/// </summary>
		/// <param name="stream">The base stream of the writer.</param>
		/// <param name="context">The serialization context to use.</param>
		public BinaryDataWriter(Stream stream, SerializationContext context)
			: base(stream, context)
		{
		}

		/// <summary>
		/// Begins an array node of the given length.
		/// </summary>
		/// <param name="length">The length of the array to come.</param>
		public override void BeginArrayNode(long length)
		{
			EnsureBufferSpace(9);
			buffer[bufferIndex++] = 6;
			UNSAFE_WriteToBuffer_8_Int64(length);
			PushArray();
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
			if (name != null)
			{
				EnsureBufferSpace(1);
				buffer[bufferIndex++] = 1;
				WriteStringFast(name);
			}
			else
			{
				EnsureBufferSpace(1);
				buffer[bufferIndex++] = 2;
			}
			WriteType(type);
			EnsureBufferSpace(4);
			UNSAFE_WriteToBuffer_4_Int32(id);
			PushNode(name, id, type);
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
			if (name != null)
			{
				EnsureBufferSpace(1);
				buffer[bufferIndex++] = 3;
				WriteStringFast(name);
			}
			else
			{
				EnsureBufferSpace(1);
				buffer[bufferIndex++] = 4;
			}
			WriteType(type);
			PushNode(name, -1, type);
		}

		/// <summary>
		/// Disposes all resources kept by the data writer, except the stream, which can be reused later.
		/// </summary>
		public override void Dispose()
		{
			FlushToStream();
		}

		/// <summary>
		/// Ends the current array node, if the current node is an array node.
		/// </summary>
		public override void EndArrayNode()
		{
			PopArray();
			EnsureBufferSpace(1);
			buffer[bufferIndex++] = 7;
		}

		/// <summary>
		/// Ends the current node with the given name. If the current node has another name, an <see cref="T:System.InvalidOperationException" /> is thrown.
		/// </summary>
		/// <param name="name">The name of the node to end. This has to be the name of the current node.</param>
		public override void EndNode(string name)
		{
			PopNode(name);
			EnsureBufferSpace(1);
			buffer[bufferIndex++] = 5;
		}

		private static void WritePrimitiveArray_byte(BinaryDataWriter writer, object o)
		{
			byte[] array = o as byte[];
			writer.EnsureBufferSpace(9);
			writer.buffer[writer.bufferIndex++] = 8;
			writer.UNSAFE_WriteToBuffer_4_Int32(array.Length);
			writer.UNSAFE_WriteToBuffer_4_Int32(1);
			writer.FlushToStream();
			writer.Stream.Write(array, 0, array.Length);
		}

		private unsafe static void WritePrimitiveArray_sbyte(BinaryDataWriter writer, object o)
		{
			sbyte[] array = o as sbyte[];
			int bytesPerElement = 1;
			int byteCount = array.Length * bytesPerElement;
			writer.EnsureBufferSpace(9);
			writer.buffer[writer.bufferIndex++] = 8;
			writer.UNSAFE_WriteToBuffer_4_Int32(array.Length);
			writer.UNSAFE_WriteToBuffer_4_Int32(bytesPerElement);
			if (writer.TryEnsureBufferSpace(byteCount))
			{
				fixed (byte* toBase = writer.buffer)
				{
					fixed (sbyte* ptr = array)
					{
						void* from = ptr;
						void* to = toBase + writer.bufferIndex;
						UnsafeUtilities.MemoryCopy(from, to, byteCount);
					}
				}
				writer.bufferIndex += byteCount;
				return;
			}
			writer.FlushToStream();
			using Buffer<byte> tempBuffer = Buffer<byte>.Claim(byteCount);
			UnsafeUtilities.MemoryCopy(array, tempBuffer.Array, byteCount, 0, 0);
			writer.Stream.Write(tempBuffer.Array, 0, byteCount);
		}

		private unsafe static void WritePrimitiveArray_bool(BinaryDataWriter writer, object o)
		{
			bool[] array = o as bool[];
			int bytesPerElement = 1;
			int byteCount = array.Length * bytesPerElement;
			writer.EnsureBufferSpace(9);
			writer.buffer[writer.bufferIndex++] = 8;
			writer.UNSAFE_WriteToBuffer_4_Int32(array.Length);
			writer.UNSAFE_WriteToBuffer_4_Int32(bytesPerElement);
			if (writer.TryEnsureBufferSpace(byteCount))
			{
				fixed (byte* toBase = writer.buffer)
				{
					fixed (bool* ptr = array)
					{
						void* from = ptr;
						void* to = toBase + writer.bufferIndex;
						UnsafeUtilities.MemoryCopy(from, to, byteCount);
					}
				}
				writer.bufferIndex += byteCount;
				return;
			}
			writer.FlushToStream();
			using Buffer<byte> tempBuffer = Buffer<byte>.Claim(byteCount);
			UnsafeUtilities.MemoryCopy(array, tempBuffer.Array, byteCount, 0, 0);
			writer.Stream.Write(tempBuffer.Array, 0, byteCount);
		}

		private unsafe static void WritePrimitiveArray_char(BinaryDataWriter writer, object o)
		{
			char[] array = o as char[];
			int bytesPerElement = 2;
			int byteCount = array.Length * bytesPerElement;
			writer.EnsureBufferSpace(9);
			writer.buffer[writer.bufferIndex++] = 8;
			writer.UNSAFE_WriteToBuffer_4_Int32(array.Length);
			writer.UNSAFE_WriteToBuffer_4_Int32(bytesPerElement);
			if (writer.TryEnsureBufferSpace(byteCount))
			{
				if (BitConverter.IsLittleEndian)
				{
					fixed (byte* toBase = writer.buffer)
					{
						fixed (char* ptr = array)
						{
							void* from = ptr;
							void* to = toBase + writer.bufferIndex;
							UnsafeUtilities.MemoryCopy(from, to, byteCount);
						}
					}
					writer.bufferIndex += byteCount;
				}
				else
				{
					for (int i = 0; i < array.Length; i++)
					{
						writer.UNSAFE_WriteToBuffer_2_Char(array[i]);
					}
				}
				return;
			}
			writer.FlushToStream();
			using Buffer<byte> tempBuffer = Buffer<byte>.Claim(byteCount);
			if (BitConverter.IsLittleEndian)
			{
				UnsafeUtilities.MemoryCopy(array, tempBuffer.Array, byteCount, 0, 0);
			}
			else
			{
				byte[] b = tempBuffer.Array;
				for (int j = 0; j < array.Length; j++)
				{
					ProperBitConverter.GetBytes(b, j * bytesPerElement, array[j]);
				}
			}
			writer.Stream.Write(tempBuffer.Array, 0, byteCount);
		}

		private unsafe static void WritePrimitiveArray_short(BinaryDataWriter writer, object o)
		{
			short[] array = o as short[];
			int bytesPerElement = 2;
			int byteCount = array.Length * bytesPerElement;
			writer.EnsureBufferSpace(9);
			writer.buffer[writer.bufferIndex++] = 8;
			writer.UNSAFE_WriteToBuffer_4_Int32(array.Length);
			writer.UNSAFE_WriteToBuffer_4_Int32(bytesPerElement);
			if (writer.TryEnsureBufferSpace(byteCount))
			{
				if (BitConverter.IsLittleEndian)
				{
					fixed (byte* toBase = writer.buffer)
					{
						fixed (short* ptr = array)
						{
							void* from = ptr;
							void* to = toBase + writer.bufferIndex;
							UnsafeUtilities.MemoryCopy(from, to, byteCount);
						}
					}
					writer.bufferIndex += byteCount;
				}
				else
				{
					for (int i = 0; i < array.Length; i++)
					{
						writer.UNSAFE_WriteToBuffer_2_Int16(array[i]);
					}
				}
				return;
			}
			writer.FlushToStream();
			using Buffer<byte> tempBuffer = Buffer<byte>.Claim(byteCount);
			if (BitConverter.IsLittleEndian)
			{
				UnsafeUtilities.MemoryCopy(array, tempBuffer.Array, byteCount, 0, 0);
			}
			else
			{
				byte[] b = tempBuffer.Array;
				for (int j = 0; j < array.Length; j++)
				{
					ProperBitConverter.GetBytes(b, j * bytesPerElement, array[j]);
				}
			}
			writer.Stream.Write(tempBuffer.Array, 0, byteCount);
		}

		private unsafe static void WritePrimitiveArray_int(BinaryDataWriter writer, object o)
		{
			int[] array = o as int[];
			int bytesPerElement = 4;
			int byteCount = array.Length * bytesPerElement;
			writer.EnsureBufferSpace(9);
			writer.buffer[writer.bufferIndex++] = 8;
			writer.UNSAFE_WriteToBuffer_4_Int32(array.Length);
			writer.UNSAFE_WriteToBuffer_4_Int32(bytesPerElement);
			if (writer.TryEnsureBufferSpace(byteCount))
			{
				if (BitConverter.IsLittleEndian)
				{
					fixed (byte* toBase = writer.buffer)
					{
						fixed (int* ptr = array)
						{
							void* from = ptr;
							void* to = toBase + writer.bufferIndex;
							UnsafeUtilities.MemoryCopy(from, to, byteCount);
						}
					}
					writer.bufferIndex += byteCount;
				}
				else
				{
					for (int i = 0; i < array.Length; i++)
					{
						writer.UNSAFE_WriteToBuffer_4_Int32(array[i]);
					}
				}
				return;
			}
			writer.FlushToStream();
			using Buffer<byte> tempBuffer = Buffer<byte>.Claim(byteCount);
			if (BitConverter.IsLittleEndian)
			{
				UnsafeUtilities.MemoryCopy(array, tempBuffer.Array, byteCount, 0, 0);
			}
			else
			{
				byte[] b = tempBuffer.Array;
				for (int j = 0; j < array.Length; j++)
				{
					ProperBitConverter.GetBytes(b, j * bytesPerElement, array[j]);
				}
			}
			writer.Stream.Write(tempBuffer.Array, 0, byteCount);
		}

		private unsafe static void WritePrimitiveArray_long(BinaryDataWriter writer, object o)
		{
			long[] array = o as long[];
			int bytesPerElement = 8;
			int byteCount = array.Length * bytesPerElement;
			writer.EnsureBufferSpace(9);
			writer.buffer[writer.bufferIndex++] = 8;
			writer.UNSAFE_WriteToBuffer_4_Int32(array.Length);
			writer.UNSAFE_WriteToBuffer_4_Int32(bytesPerElement);
			if (writer.TryEnsureBufferSpace(byteCount))
			{
				if (BitConverter.IsLittleEndian)
				{
					fixed (byte* toBase = writer.buffer)
					{
						fixed (long* ptr = array)
						{
							void* from = ptr;
							void* to = toBase + writer.bufferIndex;
							UnsafeUtilities.MemoryCopy(from, to, byteCount);
						}
					}
					writer.bufferIndex += byteCount;
				}
				else
				{
					for (int i = 0; i < array.Length; i++)
					{
						writer.UNSAFE_WriteToBuffer_8_Int64(array[i]);
					}
				}
				return;
			}
			writer.FlushToStream();
			using Buffer<byte> tempBuffer = Buffer<byte>.Claim(byteCount);
			if (BitConverter.IsLittleEndian)
			{
				UnsafeUtilities.MemoryCopy(array, tempBuffer.Array, byteCount, 0, 0);
			}
			else
			{
				byte[] b = tempBuffer.Array;
				for (int j = 0; j < array.Length; j++)
				{
					ProperBitConverter.GetBytes(b, j * bytesPerElement, array[j]);
				}
			}
			writer.Stream.Write(tempBuffer.Array, 0, byteCount);
		}

		private unsafe static void WritePrimitiveArray_ushort(BinaryDataWriter writer, object o)
		{
			ushort[] array = o as ushort[];
			int bytesPerElement = 2;
			int byteCount = array.Length * bytesPerElement;
			writer.EnsureBufferSpace(9);
			writer.buffer[writer.bufferIndex++] = 8;
			writer.UNSAFE_WriteToBuffer_4_Int32(array.Length);
			writer.UNSAFE_WriteToBuffer_4_Int32(bytesPerElement);
			if (writer.TryEnsureBufferSpace(byteCount))
			{
				if (BitConverter.IsLittleEndian)
				{
					fixed (byte* toBase = writer.buffer)
					{
						fixed (ushort* ptr = array)
						{
							void* from = ptr;
							void* to = toBase + writer.bufferIndex;
							UnsafeUtilities.MemoryCopy(from, to, byteCount);
						}
					}
					writer.bufferIndex += byteCount;
				}
				else
				{
					for (int i = 0; i < array.Length; i++)
					{
						writer.UNSAFE_WriteToBuffer_2_UInt16(array[i]);
					}
				}
				return;
			}
			writer.FlushToStream();
			using Buffer<byte> tempBuffer = Buffer<byte>.Claim(byteCount);
			if (BitConverter.IsLittleEndian)
			{
				UnsafeUtilities.MemoryCopy(array, tempBuffer.Array, byteCount, 0, 0);
			}
			else
			{
				byte[] b = tempBuffer.Array;
				for (int j = 0; j < array.Length; j++)
				{
					ProperBitConverter.GetBytes(b, j * bytesPerElement, array[j]);
				}
			}
			writer.Stream.Write(tempBuffer.Array, 0, byteCount);
		}

		private unsafe static void WritePrimitiveArray_uint(BinaryDataWriter writer, object o)
		{
			uint[] array = o as uint[];
			int bytesPerElement = 4;
			int byteCount = array.Length * bytesPerElement;
			writer.EnsureBufferSpace(9);
			writer.buffer[writer.bufferIndex++] = 8;
			writer.UNSAFE_WriteToBuffer_4_Int32(array.Length);
			writer.UNSAFE_WriteToBuffer_4_Int32(bytesPerElement);
			if (writer.TryEnsureBufferSpace(byteCount))
			{
				if (BitConverter.IsLittleEndian)
				{
					fixed (byte* toBase = writer.buffer)
					{
						fixed (uint* ptr = array)
						{
							void* from = ptr;
							void* to = toBase + writer.bufferIndex;
							UnsafeUtilities.MemoryCopy(from, to, byteCount);
						}
					}
					writer.bufferIndex += byteCount;
				}
				else
				{
					for (int i = 0; i < array.Length; i++)
					{
						writer.UNSAFE_WriteToBuffer_4_UInt32(array[i]);
					}
				}
				return;
			}
			writer.FlushToStream();
			using Buffer<byte> tempBuffer = Buffer<byte>.Claim(byteCount);
			if (BitConverter.IsLittleEndian)
			{
				UnsafeUtilities.MemoryCopy(array, tempBuffer.Array, byteCount, 0, 0);
			}
			else
			{
				byte[] b = tempBuffer.Array;
				for (int j = 0; j < array.Length; j++)
				{
					ProperBitConverter.GetBytes(b, j * bytesPerElement, array[j]);
				}
			}
			writer.Stream.Write(tempBuffer.Array, 0, byteCount);
		}

		private unsafe static void WritePrimitiveArray_ulong(BinaryDataWriter writer, object o)
		{
			ulong[] array = o as ulong[];
			int bytesPerElement = 8;
			int byteCount = array.Length * bytesPerElement;
			writer.EnsureBufferSpace(9);
			writer.buffer[writer.bufferIndex++] = 8;
			writer.UNSAFE_WriteToBuffer_4_Int32(array.Length);
			writer.UNSAFE_WriteToBuffer_4_Int32(bytesPerElement);
			if (writer.TryEnsureBufferSpace(byteCount))
			{
				if (BitConverter.IsLittleEndian)
				{
					fixed (byte* toBase = writer.buffer)
					{
						fixed (ulong* ptr = array)
						{
							void* from = ptr;
							void* to = toBase + writer.bufferIndex;
							UnsafeUtilities.MemoryCopy(from, to, byteCount);
						}
					}
					writer.bufferIndex += byteCount;
				}
				else
				{
					for (int i = 0; i < array.Length; i++)
					{
						writer.UNSAFE_WriteToBuffer_8_UInt64(array[i]);
					}
				}
				return;
			}
			writer.FlushToStream();
			using Buffer<byte> tempBuffer = Buffer<byte>.Claim(byteCount);
			if (BitConverter.IsLittleEndian)
			{
				UnsafeUtilities.MemoryCopy(array, tempBuffer.Array, byteCount, 0, 0);
			}
			else
			{
				byte[] b = tempBuffer.Array;
				for (int j = 0; j < array.Length; j++)
				{
					ProperBitConverter.GetBytes(b, j * bytesPerElement, array[j]);
				}
			}
			writer.Stream.Write(tempBuffer.Array, 0, byteCount);
		}

		private unsafe static void WritePrimitiveArray_decimal(BinaryDataWriter writer, object o)
		{
			decimal[] array = o as decimal[];
			int bytesPerElement = 16;
			int byteCount = array.Length * bytesPerElement;
			writer.EnsureBufferSpace(9);
			writer.buffer[writer.bufferIndex++] = 8;
			writer.UNSAFE_WriteToBuffer_4_Int32(array.Length);
			writer.UNSAFE_WriteToBuffer_4_Int32(bytesPerElement);
			if (writer.TryEnsureBufferSpace(byteCount))
			{
				if (BitConverter.IsLittleEndian)
				{
					fixed (byte* toBase = writer.buffer)
					{
						fixed (decimal* ptr = array)
						{
							void* from = ptr;
							void* to = toBase + writer.bufferIndex;
							UnsafeUtilities.MemoryCopy(from, to, byteCount);
						}
					}
					writer.bufferIndex += byteCount;
				}
				else
				{
					for (int i = 0; i < array.Length; i++)
					{
						writer.UNSAFE_WriteToBuffer_16_Decimal(array[i]);
					}
				}
				return;
			}
			writer.FlushToStream();
			using Buffer<byte> tempBuffer = Buffer<byte>.Claim(byteCount);
			if (BitConverter.IsLittleEndian)
			{
				UnsafeUtilities.MemoryCopy(array, tempBuffer.Array, byteCount, 0, 0);
			}
			else
			{
				byte[] b = tempBuffer.Array;
				for (int j = 0; j < array.Length; j++)
				{
					ProperBitConverter.GetBytes(b, j * bytesPerElement, array[j]);
				}
			}
			writer.Stream.Write(tempBuffer.Array, 0, byteCount);
		}

		private unsafe static void WritePrimitiveArray_float(BinaryDataWriter writer, object o)
		{
			float[] array = o as float[];
			int bytesPerElement = 4;
			int byteCount = array.Length * bytesPerElement;
			writer.EnsureBufferSpace(9);
			writer.buffer[writer.bufferIndex++] = 8;
			writer.UNSAFE_WriteToBuffer_4_Int32(array.Length);
			writer.UNSAFE_WriteToBuffer_4_Int32(bytesPerElement);
			if (writer.TryEnsureBufferSpace(byteCount))
			{
				if (BitConverter.IsLittleEndian)
				{
					fixed (byte* toBase = writer.buffer)
					{
						fixed (float* ptr = array)
						{
							void* from = ptr;
							void* to = toBase + writer.bufferIndex;
							UnsafeUtilities.MemoryCopy(from, to, byteCount);
						}
					}
					writer.bufferIndex += byteCount;
				}
				else
				{
					for (int i = 0; i < array.Length; i++)
					{
						writer.UNSAFE_WriteToBuffer_4_Float32(array[i]);
					}
				}
				return;
			}
			writer.FlushToStream();
			using Buffer<byte> tempBuffer = Buffer<byte>.Claim(byteCount);
			if (BitConverter.IsLittleEndian)
			{
				UnsafeUtilities.MemoryCopy(array, tempBuffer.Array, byteCount, 0, 0);
			}
			else
			{
				byte[] b = tempBuffer.Array;
				for (int j = 0; j < array.Length; j++)
				{
					ProperBitConverter.GetBytes(b, j * bytesPerElement, array[j]);
				}
			}
			writer.Stream.Write(tempBuffer.Array, 0, byteCount);
		}

		private unsafe static void WritePrimitiveArray_double(BinaryDataWriter writer, object o)
		{
			double[] array = o as double[];
			int bytesPerElement = 8;
			int byteCount = array.Length * bytesPerElement;
			writer.EnsureBufferSpace(9);
			writer.buffer[writer.bufferIndex++] = 8;
			writer.UNSAFE_WriteToBuffer_4_Int32(array.Length);
			writer.UNSAFE_WriteToBuffer_4_Int32(bytesPerElement);
			if (writer.TryEnsureBufferSpace(byteCount))
			{
				if (BitConverter.IsLittleEndian)
				{
					fixed (byte* toBase = writer.buffer)
					{
						fixed (double* ptr = array)
						{
							void* from = ptr;
							void* to = toBase + writer.bufferIndex;
							UnsafeUtilities.MemoryCopy(from, to, byteCount);
						}
					}
					writer.bufferIndex += byteCount;
				}
				else
				{
					for (int i = 0; i < array.Length; i++)
					{
						writer.UNSAFE_WriteToBuffer_8_Float64(array[i]);
					}
				}
				return;
			}
			writer.FlushToStream();
			using Buffer<byte> tempBuffer = Buffer<byte>.Claim(byteCount);
			if (BitConverter.IsLittleEndian)
			{
				UnsafeUtilities.MemoryCopy(array, tempBuffer.Array, byteCount, 0, 0);
			}
			else
			{
				byte[] b = tempBuffer.Array;
				for (int j = 0; j < array.Length; j++)
				{
					ProperBitConverter.GetBytes(b, j * bytesPerElement, array[j]);
				}
			}
			writer.Stream.Write(tempBuffer.Array, 0, byteCount);
		}

		private unsafe static void WritePrimitiveArray_Guid(BinaryDataWriter writer, object o)
		{
			Guid[] array = o as Guid[];
			int bytesPerElement = sizeof(Guid);
			int byteCount = array.Length * bytesPerElement;
			writer.EnsureBufferSpace(9);
			writer.buffer[writer.bufferIndex++] = 8;
			writer.UNSAFE_WriteToBuffer_4_Int32(array.Length);
			writer.UNSAFE_WriteToBuffer_4_Int32(bytesPerElement);
			if (writer.TryEnsureBufferSpace(byteCount))
			{
				if (BitConverter.IsLittleEndian)
				{
					fixed (byte* toBase = writer.buffer)
					{
						fixed (Guid* ptr = array)
						{
							void* from = ptr;
							void* to = toBase + writer.bufferIndex;
							UnsafeUtilities.MemoryCopy(from, to, byteCount);
						}
					}
					writer.bufferIndex += byteCount;
				}
				else
				{
					for (int i = 0; i < array.Length; i++)
					{
						writer.UNSAFE_WriteToBuffer_16_Guid(array[i]);
					}
				}
				return;
			}
			writer.FlushToStream();
			using Buffer<byte> tempBuffer = Buffer<byte>.Claim(byteCount);
			if (BitConverter.IsLittleEndian)
			{
				UnsafeUtilities.MemoryCopy(array, tempBuffer.Array, byteCount, 0, 0);
			}
			else
			{
				byte[] b = tempBuffer.Array;
				for (int j = 0; j < array.Length; j++)
				{
					ProperBitConverter.GetBytes(b, j * bytesPerElement, array[j]);
				}
			}
			writer.Stream.Write(tempBuffer.Array, 0, byteCount);
		}

		/// <summary>
		/// Writes a primitive array to the stream.
		/// </summary>
		/// <typeparam name="T">The element type of the primitive array. Valid element types can be determined using <see cref="M:Sirenix.Serialization.FormatterUtilities.IsPrimitiveArrayType(System.Type)" />.</typeparam>
		/// <param name="array">The primitive array to write.</param>
		/// <exception cref="T:System.ArgumentException">Type  + typeof(T).Name +  is not a valid primitive array type.</exception>
		public override void WritePrimitiveArray<T>(T[] array)
		{
			if (!PrimitiveArrayWriters.TryGetValue(typeof(T), out var writer))
			{
				throw new ArgumentException("Type " + typeof(T).Name + " is not a valid primitive array type.");
			}
			writer(this, array);
		}

		/// <summary>
		/// Writes a <see cref="T:System.Boolean" /> value to the stream.
		/// </summary>
		/// <param name="name">The name of the value. If this is null, no name will be written.</param>
		/// <param name="value">The value to write.</param>
		public override void WriteBoolean(string name, bool value)
		{
			if (name != null)
			{
				EnsureBufferSpace(1);
				buffer[bufferIndex++] = 43;
				WriteStringFast(name);
				EnsureBufferSpace(1);
				buffer[bufferIndex++] = (value ? ((byte)1) : ((byte)0));
			}
			else
			{
				EnsureBufferSpace(2);
				buffer[bufferIndex++] = 44;
				buffer[bufferIndex++] = (value ? ((byte)1) : ((byte)0));
			}
		}

		/// <summary>
		/// Writes a <see cref="T:System.Byte" /> value to the stream.
		/// </summary>
		/// <param name="name">The name of the value. If this is null, no name will be written.</param>
		/// <param name="value">The value to write.</param>
		public override void WriteByte(string name, byte value)
		{
			if (name != null)
			{
				EnsureBufferSpace(1);
				buffer[bufferIndex++] = 17;
				WriteStringFast(name);
				EnsureBufferSpace(1);
				buffer[bufferIndex++] = value;
			}
			else
			{
				EnsureBufferSpace(2);
				buffer[bufferIndex++] = 18;
				buffer[bufferIndex++] = value;
			}
		}

		/// <summary>
		/// Writes a <see cref="T:System.Char" /> value to the stream.
		/// </summary>
		/// <param name="name">The name of the value. If this is null, no name will be written.</param>
		/// <param name="value">The value to write.</param>
		public override void WriteChar(string name, char value)
		{
			if (name != null)
			{
				EnsureBufferSpace(1);
				buffer[bufferIndex++] = 37;
				WriteStringFast(name);
				EnsureBufferSpace(2);
				UNSAFE_WriteToBuffer_2_Char(value);
			}
			else
			{
				EnsureBufferSpace(3);
				buffer[bufferIndex++] = 38;
				UNSAFE_WriteToBuffer_2_Char(value);
			}
		}

		/// <summary>
		/// Writes a <see cref="T:System.Decimal" /> value to the stream.
		/// </summary>
		/// <param name="name">The name of the value. If this is null, no name will be written.</param>
		/// <param name="value">The value to write.</param>
		public override void WriteDecimal(string name, decimal value)
		{
			if (name != null)
			{
				EnsureBufferSpace(1);
				buffer[bufferIndex++] = 35;
				WriteStringFast(name);
				EnsureBufferSpace(16);
				UNSAFE_WriteToBuffer_16_Decimal(value);
			}
			else
			{
				EnsureBufferSpace(17);
				buffer[bufferIndex++] = 36;
				UNSAFE_WriteToBuffer_16_Decimal(value);
			}
		}

		/// <summary>
		/// Writes a <see cref="T:System.Double" /> value to the stream.
		/// </summary>
		/// <param name="name">The name of the value. If this is null, no name will be written.</param>
		/// <param name="value">The value to write.</param>
		public override void WriteDouble(string name, double value)
		{
			if (name != null)
			{
				EnsureBufferSpace(1);
				buffer[bufferIndex++] = 33;
				WriteStringFast(name);
				EnsureBufferSpace(8);
				UNSAFE_WriteToBuffer_8_Float64(value);
			}
			else
			{
				EnsureBufferSpace(9);
				buffer[bufferIndex++] = 34;
				UNSAFE_WriteToBuffer_8_Float64(value);
			}
		}

		/// <summary>
		/// Writes a <see cref="T:System.Guid" /> value to the stream.
		/// </summary>
		/// <param name="name">The name of the value. If this is null, no name will be written.</param>
		/// <param name="value">The value to write.</param>
		public override void WriteGuid(string name, Guid value)
		{
			if (name != null)
			{
				EnsureBufferSpace(1);
				buffer[bufferIndex++] = 41;
				WriteStringFast(name);
				EnsureBufferSpace(16);
				UNSAFE_WriteToBuffer_16_Guid(value);
			}
			else
			{
				EnsureBufferSpace(17);
				buffer[bufferIndex++] = 42;
				UNSAFE_WriteToBuffer_16_Guid(value);
			}
		}

		/// <summary>
		/// Writes an external guid reference to the stream.
		/// </summary>
		/// <param name="name">The name of the value. If this is null, no name will be written.</param>
		/// <param name="guid">The value to write.</param>
		public override void WriteExternalReference(string name, Guid guid)
		{
			if (name != null)
			{
				EnsureBufferSpace(1);
				buffer[bufferIndex++] = 13;
				WriteStringFast(name);
				EnsureBufferSpace(16);
				UNSAFE_WriteToBuffer_16_Guid(guid);
			}
			else
			{
				EnsureBufferSpace(17);
				buffer[bufferIndex++] = 14;
				UNSAFE_WriteToBuffer_16_Guid(guid);
			}
		}

		/// <summary>
		/// Writes an external index reference to the stream.
		/// </summary>
		/// <param name="name">The name of the value. If this is null, no name will be written.</param>
		/// <param name="index">The value to write.</param>
		public override void WriteExternalReference(string name, int index)
		{
			if (name != null)
			{
				EnsureBufferSpace(1);
				buffer[bufferIndex++] = 11;
				WriteStringFast(name);
				EnsureBufferSpace(4);
				UNSAFE_WriteToBuffer_4_Int32(index);
			}
			else
			{
				EnsureBufferSpace(5);
				buffer[bufferIndex++] = 12;
				UNSAFE_WriteToBuffer_4_Int32(index);
			}
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
			if (name != null)
			{
				EnsureBufferSpace(1);
				buffer[bufferIndex++] = 50;
				WriteStringFast(name);
			}
			else
			{
				EnsureBufferSpace(1);
				buffer[bufferIndex++] = 51;
			}
			WriteStringFast(id);
		}

		/// <summary>
		/// Writes an <see cref="T:System.Int32" /> value to the stream.
		/// </summary>
		/// <param name="name">The name of the value. If this is null, no name will be written.</param>
		/// <param name="value">The value to write.</param>
		public override void WriteInt32(string name, int value)
		{
			if (name != null)
			{
				EnsureBufferSpace(1);
				buffer[bufferIndex++] = 23;
				WriteStringFast(name);
				EnsureBufferSpace(4);
				UNSAFE_WriteToBuffer_4_Int32(value);
			}
			else
			{
				EnsureBufferSpace(5);
				buffer[bufferIndex++] = 24;
				UNSAFE_WriteToBuffer_4_Int32(value);
			}
		}

		/// <summary>
		/// Writes a <see cref="T:System.Int64" /> value to the stream.
		/// </summary>
		/// <param name="name">The name of the value. If this is null, no name will be written.</param>
		/// <param name="value">The value to write.</param>
		public override void WriteInt64(string name, long value)
		{
			if (name != null)
			{
				EnsureBufferSpace(1);
				buffer[bufferIndex++] = 27;
				WriteStringFast(name);
				EnsureBufferSpace(8);
				UNSAFE_WriteToBuffer_8_Int64(value);
			}
			else
			{
				EnsureBufferSpace(9);
				buffer[bufferIndex++] = 28;
				UNSAFE_WriteToBuffer_8_Int64(value);
			}
		}

		/// <summary>
		/// Writes a null value to the stream.
		/// </summary>
		/// <param name="name">The name of the value. If this is null, no name will be written.</param>
		public override void WriteNull(string name)
		{
			if (name != null)
			{
				EnsureBufferSpace(1);
				buffer[bufferIndex++] = 45;
				WriteStringFast(name);
			}
			else
			{
				EnsureBufferSpace(1);
				buffer[bufferIndex++] = 46;
			}
		}

		/// <summary>
		/// Writes an internal reference to the stream.
		/// </summary>
		/// <param name="name">The name of the value. If this is null, no name will be written.</param>
		/// <param name="id">The value to write.</param>
		public override void WriteInternalReference(string name, int id)
		{
			if (name != null)
			{
				EnsureBufferSpace(1);
				buffer[bufferIndex++] = 9;
				WriteStringFast(name);
				EnsureBufferSpace(4);
				UNSAFE_WriteToBuffer_4_Int32(id);
			}
			else
			{
				EnsureBufferSpace(5);
				buffer[bufferIndex++] = 10;
				UNSAFE_WriteToBuffer_4_Int32(id);
			}
		}

		/// <summary>
		/// Writes an <see cref="T:System.SByte" /> value to the stream.
		/// </summary>
		/// <param name="name">The name of the value. If this is null, no name will be written.</param>
		/// <param name="value">The value to write.</param>
		public override void WriteSByte(string name, sbyte value)
		{
			if (name != null)
			{
				EnsureBufferSpace(1);
				buffer[bufferIndex++] = 15;
				WriteStringFast(name);
				EnsureBufferSpace(1);
				buffer[bufferIndex++] = (byte)value;
			}
			else
			{
				EnsureBufferSpace(2);
				buffer[bufferIndex++] = 16;
				buffer[bufferIndex++] = (byte)value;
			}
		}

		/// <summary>
		/// Writes a <see cref="T:System.Int16" /> value to the stream.
		/// </summary>
		/// <param name="name">The name of the value. If this is null, no name will be written.</param>
		/// <param name="value">The value to write.</param>
		public override void WriteInt16(string name, short value)
		{
			if (name != null)
			{
				EnsureBufferSpace(1);
				buffer[bufferIndex++] = 19;
				WriteStringFast(name);
				EnsureBufferSpace(2);
				UNSAFE_WriteToBuffer_2_Int16(value);
			}
			else
			{
				EnsureBufferSpace(3);
				buffer[bufferIndex++] = 20;
				UNSAFE_WriteToBuffer_2_Int16(value);
			}
		}

		/// <summary>
		/// Writes a <see cref="T:System.Single" /> value to the stream.
		/// </summary>
		/// <param name="name">The name of the value. If this is null, no name will be written.</param>
		/// <param name="value">The value to write.</param>
		public override void WriteSingle(string name, float value)
		{
			if (name != null)
			{
				EnsureBufferSpace(1);
				buffer[bufferIndex++] = 31;
				WriteStringFast(name);
				EnsureBufferSpace(4);
				UNSAFE_WriteToBuffer_4_Float32(value);
			}
			else
			{
				EnsureBufferSpace(5);
				buffer[bufferIndex++] = 32;
				UNSAFE_WriteToBuffer_4_Float32(value);
			}
		}

		/// <summary>
		/// Writes a <see cref="T:System.String" /> value to the stream.
		/// </summary>
		/// <param name="name">The name of the value. If this is null, no name will be written.</param>
		/// <param name="value">The value to write.</param>
		public override void WriteString(string name, string value)
		{
			if (name != null)
			{
				EnsureBufferSpace(1);
				buffer[bufferIndex++] = 39;
				WriteStringFast(name);
			}
			else
			{
				EnsureBufferSpace(1);
				buffer[bufferIndex++] = 40;
			}
			WriteStringFast(value);
		}

		/// <summary>
		/// Writes an <see cref="T:System.UInt32" /> value to the stream.
		/// </summary>
		/// <param name="name">The name of the value. If this is null, no name will be written.</param>
		/// <param name="value">The value to write.</param>
		public override void WriteUInt32(string name, uint value)
		{
			if (name != null)
			{
				EnsureBufferSpace(1);
				buffer[bufferIndex++] = 25;
				WriteStringFast(name);
				EnsureBufferSpace(4);
				UNSAFE_WriteToBuffer_4_UInt32(value);
			}
			else
			{
				EnsureBufferSpace(5);
				buffer[bufferIndex++] = 26;
				UNSAFE_WriteToBuffer_4_UInt32(value);
			}
		}

		/// <summary>
		/// Writes an <see cref="T:System.UInt64" /> value to the stream.
		/// </summary>
		/// <param name="name">The name of the value. If this is null, no name will be written.</param>
		/// <param name="value">The value to write.</param>
		public override void WriteUInt64(string name, ulong value)
		{
			if (name != null)
			{
				EnsureBufferSpace(1);
				buffer[bufferIndex++] = 29;
				WriteStringFast(name);
				EnsureBufferSpace(8);
				UNSAFE_WriteToBuffer_8_UInt64(value);
			}
			else
			{
				EnsureBufferSpace(9);
				buffer[bufferIndex++] = 30;
				UNSAFE_WriteToBuffer_8_UInt64(value);
			}
		}

		/// <summary>
		/// Writes an <see cref="T:System.UInt16" /> value to the stream.
		/// </summary>
		/// <param name="name">The name of the value. If this is null, no name will be written.</param>
		/// <param name="value">The value to write.</param>
		public override void WriteUInt16(string name, ushort value)
		{
			if (name != null)
			{
				EnsureBufferSpace(1);
				buffer[bufferIndex++] = 21;
				WriteStringFast(name);
				EnsureBufferSpace(2);
				UNSAFE_WriteToBuffer_2_UInt16(value);
			}
			else
			{
				EnsureBufferSpace(3);
				buffer[bufferIndex++] = 22;
				UNSAFE_WriteToBuffer_2_UInt16(value);
			}
		}

		/// <summary>
		/// Tells the writer that a new serialization session is about to begin, and that it should clear all cached values left over from any prior serialization sessions.
		/// This method is only relevant when the same writer is used to serialize several different, unrelated values.
		/// </summary>
		public override void PrepareNewSerializationSession()
		{
			base.PrepareNewSerializationSession();
			types.Clear();
			bufferIndex = 0;
		}

		public override string GetDataDump()
		{
			if (!Stream.CanRead)
			{
				return "Binary data stream for writing cannot be read; cannot dump data.";
			}
			if (!Stream.CanSeek)
			{
				return "Binary data stream cannot seek; cannot dump data.";
			}
			FlushToStream();
			long oldPosition = Stream.Position;
			byte[] bytes = new byte[oldPosition];
			Stream.Position = 0L;
			Stream.Read(bytes, 0, (int)oldPosition);
			Stream.Position = oldPosition;
			return "Binary hex dump: " + ProperBitConverter.BytesToHexString(bytes);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void WriteType(Type type)
		{
			if (type == null)
			{
				EnsureBufferSpace(1);
				buffer[bufferIndex++] = 46;
				return;
			}
			if (types.TryGetValue(type, out var id))
			{
				EnsureBufferSpace(5);
				buffer[bufferIndex++] = 48;
				UNSAFE_WriteToBuffer_4_Int32(id);
				return;
			}
			id = types.Count;
			types.Add(type, id);
			EnsureBufferSpace(5);
			buffer[bufferIndex++] = 47;
			UNSAFE_WriteToBuffer_4_Int32(id);
			WriteStringFast(base.Context.Binder.BindToName(type, base.Context.Config.DebugContext));
		}

		private unsafe void WriteStringFast(string value)
		{
			bool needs16BitsPerChar = true;
			if (CompressStringsTo8BitWhenPossible)
			{
				needs16BitsPerChar = false;
				for (int i = 0; i < value.Length; i++)
				{
					if (value[i] > 'ÿ')
					{
						needs16BitsPerChar = true;
						break;
					}
				}
			}
			int byteCount;
			if (needs16BitsPerChar)
			{
				byteCount = value.Length * 2;
				if (TryEnsureBufferSpace(byteCount + 5))
				{
					buffer[bufferIndex++] = 1;
					UNSAFE_WriteToBuffer_4_Int32(value.Length);
					if (BitConverter.IsLittleEndian)
					{
						fixed (byte* baseToPtr = buffer)
						{
							fixed (char* baseFromPtr = value)
							{
								Struct256Bit* toPtr = (Struct256Bit*)(baseToPtr + bufferIndex);
								Struct256Bit* fromPtr = (Struct256Bit*)baseFromPtr;
								byte* toEnd = (byte*)toPtr + byteCount;
								while (toPtr + 1 <= toEnd)
								{
									*(toPtr++) = *(fromPtr++);
								}
								char* toPtrRest = (char*)toPtr;
								char* fromPtrRest = (char*)fromPtr;
								while (toPtrRest < toEnd)
								{
									*(toPtrRest++) = *(fromPtrRest++);
								}
							}
						}
					}
					else
					{
						fixed (byte* baseToPtr2 = buffer)
						{
							fixed (char* baseFromPtr2 = value)
							{
								byte* toPtr2 = baseToPtr2 + bufferIndex;
								byte* fromPtr2 = (byte*)baseFromPtr2;
								for (int j = 0; j < byteCount; j += 2)
								{
									*toPtr2 = fromPtr2[1];
									toPtr2[1] = *fromPtr2;
									fromPtr2 += 2;
									toPtr2 += 2;
								}
							}
						}
					}
					bufferIndex += byteCount;
					return;
				}
				FlushToStream();
				Stream.WriteByte(1);
				ProperBitConverter.GetBytes(small_buffer, 0, value.Length);
				Stream.Write(small_buffer, 0, 4);
				using Buffer<byte> tempBuffer = Buffer<byte>.Claim(byteCount);
				byte[] array = tempBuffer.Array;
				UnsafeUtilities.StringToBytes(array, value, needs16BitSupport: true);
				Stream.Write(array, 0, byteCount);
				return;
			}
			byteCount = value.Length;
			if (TryEnsureBufferSpace(byteCount + 5))
			{
				buffer[bufferIndex++] = 0;
				UNSAFE_WriteToBuffer_4_Int32(value.Length);
				for (int k = 0; k < byteCount; k++)
				{
					buffer[bufferIndex++] = (byte)value[k];
				}
				return;
			}
			FlushToStream();
			Stream.WriteByte(0);
			ProperBitConverter.GetBytes(small_buffer, 0, value.Length);
			Stream.Write(small_buffer, 0, 4);
			using Buffer<byte> tempBuffer2 = Buffer<byte>.Claim(value.Length);
			byte[] array2 = tempBuffer2.Array;
			for (int l = 0; l < value.Length; l++)
			{
				array2[l] = (byte)value[l];
			}
			Stream.Write(array2, 0, value.Length);
		}

		public override void FlushToStream()
		{
			if (bufferIndex > 0)
			{
				Stream.Write(buffer, 0, bufferIndex);
				bufferIndex = 0;
			}
			base.FlushToStream();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private unsafe void UNSAFE_WriteToBuffer_2_Char(char value)
		{
			fixed (byte* basePtr = buffer)
			{
				if (BitConverter.IsLittleEndian)
				{
					*(char*)(basePtr + bufferIndex) = value;
				}
				else
				{
					byte* ptrTo = basePtr + bufferIndex;
					byte* ptrFrom = (byte*)(&value) + 1;
					*(ptrTo++) = *(ptrFrom--);
					*ptrTo = *ptrFrom;
				}
			}
			bufferIndex += 2;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private unsafe void UNSAFE_WriteToBuffer_2_Int16(short value)
		{
			fixed (byte* basePtr = buffer)
			{
				if (BitConverter.IsLittleEndian)
				{
					*(short*)(basePtr + bufferIndex) = value;
				}
				else
				{
					byte* ptrTo = basePtr + bufferIndex;
					byte* ptrFrom = (byte*)(&value) + 1;
					*(ptrTo++) = *(ptrFrom--);
					*ptrTo = *ptrFrom;
				}
			}
			bufferIndex += 2;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private unsafe void UNSAFE_WriteToBuffer_2_UInt16(ushort value)
		{
			fixed (byte* basePtr = buffer)
			{
				if (BitConverter.IsLittleEndian)
				{
					*(ushort*)(basePtr + bufferIndex) = value;
				}
				else
				{
					byte* ptrTo = basePtr + bufferIndex;
					byte* ptrFrom = (byte*)(&value) + 1;
					*(ptrTo++) = *(ptrFrom--);
					*ptrTo = *ptrFrom;
				}
			}
			bufferIndex += 2;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private unsafe void UNSAFE_WriteToBuffer_4_Int32(int value)
		{
			fixed (byte* basePtr = buffer)
			{
				if (BitConverter.IsLittleEndian)
				{
					*(int*)(basePtr + bufferIndex) = value;
				}
				else
				{
					byte* ptrTo = basePtr + bufferIndex;
					byte* ptrFrom = (byte*)(&value) + 3;
					*(ptrTo++) = *(ptrFrom--);
					*(ptrTo++) = *(ptrFrom--);
					*(ptrTo++) = *(ptrFrom--);
					*ptrTo = *ptrFrom;
				}
			}
			bufferIndex += 4;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private unsafe void UNSAFE_WriteToBuffer_4_UInt32(uint value)
		{
			fixed (byte* basePtr = buffer)
			{
				if (BitConverter.IsLittleEndian)
				{
					*(uint*)(basePtr + bufferIndex) = value;
				}
				else
				{
					byte* ptrTo = basePtr + bufferIndex;
					byte* ptrFrom = (byte*)(&value) + 3;
					*(ptrTo++) = *(ptrFrom--);
					*(ptrTo++) = *(ptrFrom--);
					*(ptrTo++) = *(ptrFrom--);
					*ptrTo = *ptrFrom;
				}
			}
			bufferIndex += 4;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private unsafe void UNSAFE_WriteToBuffer_4_Float32(float value)
		{
			fixed (byte* basePtr = buffer)
			{
				if (BitConverter.IsLittleEndian)
				{
					if (ArchitectureInfo.Architecture_Supports_All_Unaligned_ReadWrites)
					{
						*(float*)(basePtr + bufferIndex) = value;
					}
					else
					{
						byte* from = (byte*)(&value);
						byte* to = basePtr + bufferIndex;
						*(to++) = *(from++);
						*(to++) = *(from++);
						*(to++) = *(from++);
						*to = *from;
					}
				}
				else
				{
					byte* ptrTo = basePtr + bufferIndex;
					byte* ptrFrom = (byte*)(&value) + 3;
					*(ptrTo++) = *(ptrFrom--);
					*(ptrTo++) = *(ptrFrom--);
					*(ptrTo++) = *(ptrFrom--);
					*ptrTo = *ptrFrom;
				}
			}
			bufferIndex += 4;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private unsafe void UNSAFE_WriteToBuffer_8_Int64(long value)
		{
			fixed (byte* basePtr = buffer)
			{
				if (BitConverter.IsLittleEndian)
				{
					if (ArchitectureInfo.Architecture_Supports_All_Unaligned_ReadWrites)
					{
						*(long*)(basePtr + bufferIndex) = value;
					}
					else
					{
						SixtyFourBitValueToByteUnion union = new SixtyFourBitValueToByteUnion
						{
							longValue = value
						};
						buffer[bufferIndex] = union.b0;
						buffer[bufferIndex + 1] = union.b1;
						buffer[bufferIndex + 2] = union.b2;
						buffer[bufferIndex + 3] = union.b3;
						buffer[bufferIndex + 4] = union.b4;
						buffer[bufferIndex + 5] = union.b5;
						buffer[bufferIndex + 6] = union.b6;
						buffer[bufferIndex + 7] = union.b7;
					}
				}
				else
				{
					byte* ptrTo = basePtr + bufferIndex;
					byte* ptrFrom = (byte*)(&value) + 7;
					*(ptrTo++) = *(ptrFrom--);
					*(ptrTo++) = *(ptrFrom--);
					*(ptrTo++) = *(ptrFrom--);
					*(ptrTo++) = *(ptrFrom--);
					*(ptrTo++) = *(ptrFrom--);
					*(ptrTo++) = *(ptrFrom--);
					*(ptrTo++) = *(ptrFrom--);
					*ptrTo = *ptrFrom;
				}
			}
			bufferIndex += 8;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private unsafe void UNSAFE_WriteToBuffer_8_UInt64(ulong value)
		{
			fixed (byte* basePtr = buffer)
			{
				if (BitConverter.IsLittleEndian)
				{
					if (ArchitectureInfo.Architecture_Supports_All_Unaligned_ReadWrites)
					{
						*(ulong*)(basePtr + bufferIndex) = value;
					}
					else
					{
						SixtyFourBitValueToByteUnion union = new SixtyFourBitValueToByteUnion
						{
							ulongValue = value
						};
						buffer[bufferIndex] = union.b0;
						buffer[bufferIndex + 1] = union.b1;
						buffer[bufferIndex + 2] = union.b2;
						buffer[bufferIndex + 3] = union.b3;
						buffer[bufferIndex + 4] = union.b4;
						buffer[bufferIndex + 5] = union.b5;
						buffer[bufferIndex + 6] = union.b6;
						buffer[bufferIndex + 7] = union.b7;
					}
				}
				else
				{
					byte* ptrTo = basePtr + bufferIndex;
					byte* ptrFrom = (byte*)(&value) + 7;
					*(ptrTo++) = *(ptrFrom--);
					*(ptrTo++) = *(ptrFrom--);
					*(ptrTo++) = *(ptrFrom--);
					*(ptrTo++) = *(ptrFrom--);
					*(ptrTo++) = *(ptrFrom--);
					*(ptrTo++) = *(ptrFrom--);
					*(ptrTo++) = *(ptrFrom--);
					*ptrTo = *ptrFrom;
				}
			}
			bufferIndex += 8;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private unsafe void UNSAFE_WriteToBuffer_8_Float64(double value)
		{
			fixed (byte* basePtr = buffer)
			{
				if (BitConverter.IsLittleEndian)
				{
					if (ArchitectureInfo.Architecture_Supports_All_Unaligned_ReadWrites)
					{
						*(double*)(basePtr + bufferIndex) = value;
					}
					else
					{
						SixtyFourBitValueToByteUnion union = new SixtyFourBitValueToByteUnion
						{
							doubleValue = value
						};
						buffer[bufferIndex] = union.b0;
						buffer[bufferIndex + 1] = union.b1;
						buffer[bufferIndex + 2] = union.b2;
						buffer[bufferIndex + 3] = union.b3;
						buffer[bufferIndex + 4] = union.b4;
						buffer[bufferIndex + 5] = union.b5;
						buffer[bufferIndex + 6] = union.b6;
						buffer[bufferIndex + 7] = union.b7;
					}
				}
				else
				{
					byte* ptrTo = basePtr + bufferIndex;
					byte* ptrFrom = (byte*)(&value) + 7;
					*(ptrTo++) = *(ptrFrom--);
					*(ptrTo++) = *(ptrFrom--);
					*(ptrTo++) = *(ptrFrom--);
					*(ptrTo++) = *(ptrFrom--);
					*(ptrTo++) = *(ptrFrom--);
					*(ptrTo++) = *(ptrFrom--);
					*(ptrTo++) = *(ptrFrom--);
					*ptrTo = *ptrFrom;
				}
			}
			bufferIndex += 8;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private unsafe void UNSAFE_WriteToBuffer_16_Decimal(decimal value)
		{
			fixed (byte* basePtr = buffer)
			{
				if (BitConverter.IsLittleEndian)
				{
					if (ArchitectureInfo.Architecture_Supports_All_Unaligned_ReadWrites)
					{
						*(decimal*)(basePtr + bufferIndex) = value;
					}
					else
					{
						OneTwentyEightBitValueToByteUnion union = new OneTwentyEightBitValueToByteUnion
						{
							decimalValue = value
						};
						buffer[bufferIndex] = union.b0;
						buffer[bufferIndex + 1] = union.b1;
						buffer[bufferIndex + 2] = union.b2;
						buffer[bufferIndex + 3] = union.b3;
						buffer[bufferIndex + 4] = union.b4;
						buffer[bufferIndex + 5] = union.b5;
						buffer[bufferIndex + 6] = union.b6;
						buffer[bufferIndex + 7] = union.b7;
						buffer[bufferIndex + 8] = union.b8;
						buffer[bufferIndex + 9] = union.b9;
						buffer[bufferIndex + 10] = union.b10;
						buffer[bufferIndex + 11] = union.b11;
						buffer[bufferIndex + 12] = union.b12;
						buffer[bufferIndex + 13] = union.b13;
						buffer[bufferIndex + 14] = union.b14;
						buffer[bufferIndex + 15] = union.b15;
					}
				}
				else
				{
					byte* ptrTo = basePtr + bufferIndex;
					byte* ptrFrom = (byte*)(&value) + 15;
					*(ptrTo++) = *(ptrFrom--);
					*(ptrTo++) = *(ptrFrom--);
					*(ptrTo++) = *(ptrFrom--);
					*(ptrTo++) = *(ptrFrom--);
					*(ptrTo++) = *(ptrFrom--);
					*(ptrTo++) = *(ptrFrom--);
					*(ptrTo++) = *(ptrFrom--);
					*(ptrTo++) = *(ptrFrom--);
					*(ptrTo++) = *(ptrFrom--);
					*(ptrTo++) = *(ptrFrom--);
					*(ptrTo++) = *(ptrFrom--);
					*(ptrTo++) = *(ptrFrom--);
					*(ptrTo++) = *(ptrFrom--);
					*(ptrTo++) = *(ptrFrom--);
					*(ptrTo++) = *(ptrFrom--);
					*ptrTo = *ptrFrom;
				}
			}
			bufferIndex += 16;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private unsafe void UNSAFE_WriteToBuffer_16_Guid(Guid value)
		{
			fixed (byte* basePtr = buffer)
			{
				if (BitConverter.IsLittleEndian)
				{
					if (ArchitectureInfo.Architecture_Supports_All_Unaligned_ReadWrites)
					{
						*(Guid*)(basePtr + bufferIndex) = value;
					}
					else
					{
						OneTwentyEightBitValueToByteUnion union = new OneTwentyEightBitValueToByteUnion
						{
							guidValue = value
						};
						buffer[bufferIndex] = union.b0;
						buffer[bufferIndex + 1] = union.b1;
						buffer[bufferIndex + 2] = union.b2;
						buffer[bufferIndex + 3] = union.b3;
						buffer[bufferIndex + 4] = union.b4;
						buffer[bufferIndex + 5] = union.b5;
						buffer[bufferIndex + 6] = union.b6;
						buffer[bufferIndex + 7] = union.b7;
						buffer[bufferIndex + 8] = union.b8;
						buffer[bufferIndex + 9] = union.b9;
						buffer[bufferIndex + 10] = union.b10;
						buffer[bufferIndex + 11] = union.b11;
						buffer[bufferIndex + 12] = union.b12;
						buffer[bufferIndex + 13] = union.b13;
						buffer[bufferIndex + 14] = union.b14;
						buffer[bufferIndex + 15] = union.b15;
					}
				}
				else
				{
					byte* ptrTo = basePtr + bufferIndex;
					byte* ptrFrom = (byte*)(&value);
					*(ptrTo++) = *(ptrFrom++);
					*(ptrTo++) = *(ptrFrom++);
					*(ptrTo++) = *(ptrFrom++);
					*(ptrTo++) = *(ptrFrom++);
					*(ptrTo++) = *(ptrFrom++);
					*(ptrTo++) = *(ptrFrom++);
					*(ptrTo++) = *(ptrFrom++);
					*(ptrTo++) = *(ptrFrom++);
					*(ptrTo++) = *(ptrFrom++);
					*(ptrTo++) = *ptrFrom;
					ptrFrom += 6;
					*(ptrTo++) = *(ptrFrom--);
					*(ptrTo++) = *(ptrFrom--);
					*(ptrTo++) = *(ptrFrom--);
					*(ptrTo++) = *(ptrFrom--);
					*(ptrTo++) = *(ptrFrom--);
					*ptrTo = *ptrFrom;
				}
			}
			bufferIndex += 16;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private bool TryEnsureBufferSpace(int space)
		{
			int length = buffer.Length;
			if (space > length)
			{
				return false;
			}
			if (bufferIndex + space > length)
			{
				FlushToStream();
			}
			return true;
		}
	}
}
