using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Sirenix.Serialization.Utilities.Unsafe;

namespace Sirenix.Serialization
{
	/// <summary>
	/// Reads data from a stream that has been written by a <see cref="T:Sirenix.Serialization.BinaryDataWriter" />.
	/// </summary>
	/// <seealso cref="T:Sirenix.Serialization.BaseDataReader" />
	public class BinaryDataReader : BaseDataReader
	{
		private struct Struct256Bit
		{
			public decimal d1;

			public decimal d2;
		}

		private static readonly Dictionary<Type, Delegate> PrimitiveFromByteMethods = new Dictionary<Type, Delegate>
		{
			{
				typeof(char),
				(Func<byte[], int, char>)((byte[] b, int i) => (char)ProperBitConverter.ToUInt16(b, i))
			},
			{
				typeof(byte),
				(Func<byte[], int, byte>)((byte[] b, int i) => b[i])
			},
			{
				typeof(sbyte),
				(Func<byte[], int, sbyte>)((byte[] b, int i) => (sbyte)b[i])
			},
			{
				typeof(bool),
				(Func<byte[], int, bool>)((byte[] b, int i) => b[i] != 0)
			},
			{
				typeof(short),
				new Func<byte[], int, short>(ProperBitConverter.ToInt16)
			},
			{
				typeof(int),
				new Func<byte[], int, int>(ProperBitConverter.ToInt32)
			},
			{
				typeof(long),
				new Func<byte[], int, long>(ProperBitConverter.ToInt64)
			},
			{
				typeof(ushort),
				new Func<byte[], int, ushort>(ProperBitConverter.ToUInt16)
			},
			{
				typeof(uint),
				new Func<byte[], int, uint>(ProperBitConverter.ToUInt32)
			},
			{
				typeof(ulong),
				new Func<byte[], int, ulong>(ProperBitConverter.ToUInt64)
			},
			{
				typeof(decimal),
				new Func<byte[], int, decimal>(ProperBitConverter.ToDecimal)
			},
			{
				typeof(float),
				new Func<byte[], int, float>(ProperBitConverter.ToSingle)
			},
			{
				typeof(double),
				new Func<byte[], int, double>(ProperBitConverter.ToDouble)
			},
			{
				typeof(Guid),
				new Func<byte[], int, Guid>(ProperBitConverter.ToGuid)
			}
		};

		private byte[] internalBufferBackup;

		private byte[] buffer = new byte[102400];

		private int bufferIndex;

		private int bufferEnd;

		private EntryType? peekedEntryType;

		private BinaryEntryType peekedBinaryEntryType;

		private string peekedEntryName;

		private Dictionary<int, Type> types = new Dictionary<int, Type>(16);

		public BinaryDataReader()
			: base(null, null)
		{
			internalBufferBackup = buffer;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Sirenix.Serialization.BinaryDataReader" /> class.
		/// </summary>
		/// <param name="stream">The base stream of the reader.</param>
		/// <param name="context">The deserialization context to use.</param>
		public BinaryDataReader(Stream stream, DeserializationContext context)
			: base(stream, context)
		{
			internalBufferBackup = buffer;
		}

		/// <summary>
		/// Disposes all resources kept by the data reader, except the stream, which can be reused later.
		/// </summary>
		public override void Dispose()
		{
		}

		/// <summary>
		/// Peeks ahead and returns the type of the next entry in the stream.
		/// </summary>
		/// <param name="name">The name of the next entry, if it has one.</param>
		/// <returns>
		/// The type of the next entry.
		/// </returns>
		public override EntryType PeekEntry(out string name)
		{
			if (peekedEntryType.HasValue)
			{
				name = peekedEntryName;
				return peekedEntryType.Value;
			}
			peekedBinaryEntryType = (BinaryEntryType)(HasBufferData(1) ? buffer[bufferIndex++] : 49);
			switch (peekedBinaryEntryType)
			{
			case BinaryEntryType.EndOfStream:
				name = null;
				peekedEntryName = null;
				peekedEntryType = EntryType.EndOfStream;
				break;
			case BinaryEntryType.NamedStartOfReferenceNode:
			case BinaryEntryType.NamedStartOfStructNode:
				name = ReadStringValue();
				peekedEntryType = EntryType.StartOfNode;
				break;
			case BinaryEntryType.UnnamedStartOfReferenceNode:
			case BinaryEntryType.UnnamedStartOfStructNode:
				name = null;
				peekedEntryType = EntryType.StartOfNode;
				break;
			case BinaryEntryType.EndOfNode:
				name = null;
				peekedEntryType = EntryType.EndOfNode;
				break;
			case BinaryEntryType.StartOfArray:
				name = null;
				peekedEntryType = EntryType.StartOfArray;
				break;
			case BinaryEntryType.EndOfArray:
				name = null;
				peekedEntryType = EntryType.EndOfArray;
				break;
			case BinaryEntryType.PrimitiveArray:
				name = null;
				peekedEntryType = EntryType.PrimitiveArray;
				break;
			case BinaryEntryType.NamedInternalReference:
				name = ReadStringValue();
				peekedEntryType = EntryType.InternalReference;
				break;
			case BinaryEntryType.UnnamedInternalReference:
				name = null;
				peekedEntryType = EntryType.InternalReference;
				break;
			case BinaryEntryType.NamedExternalReferenceByIndex:
				name = ReadStringValue();
				peekedEntryType = EntryType.ExternalReferenceByIndex;
				break;
			case BinaryEntryType.UnnamedExternalReferenceByIndex:
				name = null;
				peekedEntryType = EntryType.ExternalReferenceByIndex;
				break;
			case BinaryEntryType.NamedExternalReferenceByGuid:
				name = ReadStringValue();
				peekedEntryType = EntryType.ExternalReferenceByGuid;
				break;
			case BinaryEntryType.UnnamedExternalReferenceByGuid:
				name = null;
				peekedEntryType = EntryType.ExternalReferenceByGuid;
				break;
			case BinaryEntryType.NamedExternalReferenceByString:
				name = ReadStringValue();
				peekedEntryType = EntryType.ExternalReferenceByString;
				break;
			case BinaryEntryType.UnnamedExternalReferenceByString:
				name = null;
				peekedEntryType = EntryType.ExternalReferenceByString;
				break;
			case BinaryEntryType.NamedSByte:
				name = ReadStringValue();
				peekedEntryType = EntryType.Integer;
				break;
			case BinaryEntryType.UnnamedSByte:
				name = null;
				peekedEntryType = EntryType.Integer;
				break;
			case BinaryEntryType.NamedByte:
				name = ReadStringValue();
				peekedEntryType = EntryType.Integer;
				break;
			case BinaryEntryType.UnnamedByte:
				name = null;
				peekedEntryType = EntryType.Integer;
				break;
			case BinaryEntryType.NamedShort:
				name = ReadStringValue();
				peekedEntryType = EntryType.Integer;
				break;
			case BinaryEntryType.UnnamedShort:
				name = null;
				peekedEntryType = EntryType.Integer;
				break;
			case BinaryEntryType.NamedUShort:
				name = ReadStringValue();
				peekedEntryType = EntryType.Integer;
				break;
			case BinaryEntryType.UnnamedUShort:
				name = null;
				peekedEntryType = EntryType.Integer;
				break;
			case BinaryEntryType.NamedInt:
				name = ReadStringValue();
				peekedEntryType = EntryType.Integer;
				break;
			case BinaryEntryType.UnnamedInt:
				name = null;
				peekedEntryType = EntryType.Integer;
				break;
			case BinaryEntryType.NamedUInt:
				name = ReadStringValue();
				peekedEntryType = EntryType.Integer;
				break;
			case BinaryEntryType.UnnamedUInt:
				name = null;
				peekedEntryType = EntryType.Integer;
				break;
			case BinaryEntryType.NamedLong:
				name = ReadStringValue();
				peekedEntryType = EntryType.Integer;
				break;
			case BinaryEntryType.UnnamedLong:
				name = null;
				peekedEntryType = EntryType.Integer;
				break;
			case BinaryEntryType.NamedULong:
				name = ReadStringValue();
				peekedEntryType = EntryType.Integer;
				break;
			case BinaryEntryType.UnnamedULong:
				name = null;
				peekedEntryType = EntryType.Integer;
				break;
			case BinaryEntryType.NamedFloat:
				name = ReadStringValue();
				peekedEntryType = EntryType.FloatingPoint;
				break;
			case BinaryEntryType.UnnamedFloat:
				name = null;
				peekedEntryType = EntryType.FloatingPoint;
				break;
			case BinaryEntryType.NamedDouble:
				name = ReadStringValue();
				peekedEntryType = EntryType.FloatingPoint;
				break;
			case BinaryEntryType.UnnamedDouble:
				name = null;
				peekedEntryType = EntryType.FloatingPoint;
				break;
			case BinaryEntryType.NamedDecimal:
				name = ReadStringValue();
				peekedEntryType = EntryType.FloatingPoint;
				break;
			case BinaryEntryType.UnnamedDecimal:
				name = null;
				peekedEntryType = EntryType.FloatingPoint;
				break;
			case BinaryEntryType.NamedChar:
				name = ReadStringValue();
				peekedEntryType = EntryType.String;
				break;
			case BinaryEntryType.UnnamedChar:
				name = null;
				peekedEntryType = EntryType.String;
				break;
			case BinaryEntryType.NamedString:
				name = ReadStringValue();
				peekedEntryType = EntryType.String;
				break;
			case BinaryEntryType.UnnamedString:
				name = null;
				peekedEntryType = EntryType.String;
				break;
			case BinaryEntryType.NamedGuid:
				name = ReadStringValue();
				peekedEntryType = EntryType.Guid;
				break;
			case BinaryEntryType.UnnamedGuid:
				name = null;
				peekedEntryType = EntryType.Guid;
				break;
			case BinaryEntryType.NamedBoolean:
				name = ReadStringValue();
				peekedEntryType = EntryType.Boolean;
				break;
			case BinaryEntryType.UnnamedBoolean:
				name = null;
				peekedEntryType = EntryType.Boolean;
				break;
			case BinaryEntryType.NamedNull:
				name = ReadStringValue();
				peekedEntryType = EntryType.Null;
				break;
			case BinaryEntryType.UnnamedNull:
				name = null;
				peekedEntryType = EntryType.Null;
				break;
			case BinaryEntryType.TypeName:
			case BinaryEntryType.TypeID:
				peekedBinaryEntryType = BinaryEntryType.Invalid;
				peekedEntryType = EntryType.Invalid;
				throw new InvalidOperationException("Invalid binary data stream: BinaryEntryType.TypeName and BinaryEntryType.TypeID must never be peeked by the binary reader.");
			default:
			{
				name = null;
				peekedBinaryEntryType = BinaryEntryType.Invalid;
				peekedEntryType = EntryType.Invalid;
				byte b = (byte)peekedBinaryEntryType;
				throw new InvalidOperationException("Invalid binary data stream: could not parse peeked BinaryEntryType byte '" + b + "' into a known entry type.");
			}
			}
			peekedEntryName = name;
			return peekedEntryType.Value;
		}

		/// <summary>
		/// Tries to enters an array node. This will succeed if the next entry is an <see cref="F:Sirenix.Serialization.EntryType.StartOfArray" />.
		/// <para />
		/// This call MUST (eventually) be followed by a corresponding call to <see cref="M:Sirenix.Serialization.IDataReader.ExitArray" /><para />
		/// This call will change the values of the <see cref="P:Sirenix.Serialization.IDataReader.IsInArrayNode" />, <see cref="P:Sirenix.Serialization.IDataReader.CurrentNodeName" />, <see cref="P:Sirenix.Serialization.IDataReader.CurrentNodeId" /> and <see cref="P:Sirenix.Serialization.IDataReader.CurrentNodeDepth" /> properties to the correct values for the current array node.
		/// </summary>
		/// <param name="length">The length of the array that was entered.</param>
		/// <returns>
		///   <c>true</c> if an array was entered, otherwise <c>false</c>
		/// </returns>
		public override bool EnterArray(out long length)
		{
			if (!peekedEntryType.HasValue)
			{
				PeekEntry(out var _);
			}
			if (peekedEntryType == EntryType.StartOfArray)
			{
				PushArray();
				MarkEntryContentConsumed();
				if (UNSAFE_Read_8_Int64(out length))
				{
					if (length < 0)
					{
						length = 0L;
						base.Context.Config.DebugContext.LogError("Invalid array length: " + length + ".");
						return false;
					}
					return true;
				}
				return false;
			}
			SkipEntry();
			length = 0L;
			return false;
		}

		/// <summary>
		/// Tries to enter a node. This will succeed if the next entry is an <see cref="F:Sirenix.Serialization.EntryType.StartOfNode" />.
		/// <para />
		/// This call MUST (eventually) be followed by a corresponding call to <see cref="M:Sirenix.Serialization.IDataReader.ExitNode" /><para />
		/// This call will change the values of the <see cref="P:Sirenix.Serialization.IDataReader.IsInArrayNode" />, <see cref="P:Sirenix.Serialization.IDataReader.CurrentNodeName" />, <see cref="P:Sirenix.Serialization.IDataReader.CurrentNodeId" /> and <see cref="P:Sirenix.Serialization.IDataReader.CurrentNodeDepth" /> properties to the correct values for the current node.
		/// </summary>
		/// <param name="type">The type of the node. This value will be null if there was no metadata, or if the reader's serialization binder failed to resolve the type name.</param>
		/// <returns>
		///   <c>true</c> if entering a node succeeded, otherwise <c>false</c>
		/// </returns>
		public override bool EnterNode(out Type type)
		{
			if (!peekedEntryType.HasValue)
			{
				PeekEntry(out var _);
			}
			if (peekedBinaryEntryType == BinaryEntryType.NamedStartOfReferenceNode || peekedBinaryEntryType == BinaryEntryType.UnnamedStartOfReferenceNode)
			{
				MarkEntryContentConsumed();
				type = ReadTypeEntry();
				if (!UNSAFE_Read_4_Int32(out var id))
				{
					type = null;
					return false;
				}
				PushNode(peekedEntryName, id, type);
				return true;
			}
			if (peekedBinaryEntryType == BinaryEntryType.NamedStartOfStructNode || peekedBinaryEntryType == BinaryEntryType.UnnamedStartOfStructNode)
			{
				type = ReadTypeEntry();
				PushNode(peekedEntryName, -1, type);
				MarkEntryContentConsumed();
				return true;
			}
			SkipEntry();
			type = null;
			return false;
		}

		/// <summary>
		/// Exits the closest array. This method will keep skipping entries using <see cref="M:Sirenix.Serialization.IDataReader.SkipEntry" /> until an <see cref="F:Sirenix.Serialization.EntryType.EndOfArray" /> is reached, or the end of the stream is reached.
		/// <para />
		/// This call MUST have been preceded by a corresponding call to <see cref="M:Sirenix.Serialization.IDataReader.EnterArray(System.Int64@)" />.
		/// <para />
		/// This call will change the values of the <see cref="P:Sirenix.Serialization.IDataReader.IsInArrayNode" />, <see cref="P:Sirenix.Serialization.IDataReader.CurrentNodeName" />, <see cref="P:Sirenix.Serialization.IDataReader.CurrentNodeId" /> and <see cref="P:Sirenix.Serialization.IDataReader.CurrentNodeDepth" /> to the correct values for the node that was prior to the exited array node.
		/// </summary>
		/// <returns>
		///   <c>true</c> if the method exited an array, <c>false</c> if it reached the end of the stream.
		/// </returns>
		public override bool ExitArray()
		{
			if (!peekedEntryType.HasValue)
			{
				PeekEntry(out var _);
			}
			while (peekedBinaryEntryType != BinaryEntryType.EndOfArray && peekedBinaryEntryType != BinaryEntryType.EndOfStream)
			{
				if (peekedEntryType == EntryType.EndOfNode)
				{
					base.Context.Config.DebugContext.LogError("Data layout mismatch; skipping past node boundary when exiting array.");
					MarkEntryContentConsumed();
				}
				SkipEntry();
			}
			if (peekedBinaryEntryType == BinaryEntryType.EndOfArray)
			{
				MarkEntryContentConsumed();
				PopArray();
				return true;
			}
			return false;
		}

		/// <summary>
		/// Exits the current node. This method will keep skipping entries using <see cref="M:Sirenix.Serialization.IDataReader.SkipEntry" /> until an <see cref="F:Sirenix.Serialization.EntryType.EndOfNode" /> is reached, or the end of the stream is reached.
		/// <para />
		/// This call MUST have been preceded by a corresponding call to <see cref="M:Sirenix.Serialization.IDataReader.EnterNode(System.Type@)" />.
		/// <para />
		/// This call will change the values of the <see cref="P:Sirenix.Serialization.IDataReader.IsInArrayNode" />, <see cref="P:Sirenix.Serialization.IDataReader.CurrentNodeName" />, <see cref="P:Sirenix.Serialization.IDataReader.CurrentNodeId" /> and <see cref="P:Sirenix.Serialization.IDataReader.CurrentNodeDepth" /> to the correct values for the node that was prior to the current node.
		/// </summary>
		/// <returns>
		///   <c>true</c> if the method exited a node, <c>false</c> if it reached the end of the stream.
		/// </returns>
		public override bool ExitNode()
		{
			if (!peekedEntryType.HasValue)
			{
				PeekEntry(out var _);
			}
			while (peekedBinaryEntryType != BinaryEntryType.EndOfNode && peekedBinaryEntryType != BinaryEntryType.EndOfStream)
			{
				if (peekedEntryType == EntryType.EndOfArray)
				{
					base.Context.Config.DebugContext.LogError("Data layout mismatch; skipping past array boundary when exiting node.");
					MarkEntryContentConsumed();
				}
				SkipEntry();
			}
			if (peekedBinaryEntryType == BinaryEntryType.EndOfNode)
			{
				MarkEntryContentConsumed();
				PopNode(base.CurrentNodeName);
				return true;
			}
			return false;
		}

		/// <summary>
		/// Reads a primitive array value. This call will succeed if the next entry is an <see cref="F:Sirenix.Serialization.EntryType.PrimitiveArray" />.
		/// <para />
		/// If the call fails (and returns <c>false</c>), it will skip the current entry value, unless that entry is an <see cref="F:Sirenix.Serialization.EntryType.EndOfNode" /> or an <see cref="F:Sirenix.Serialization.EntryType.EndOfArray" />.
		/// </summary>
		/// <typeparam name="T">The element type of the primitive array. Valid element types can be determined using <see cref="M:Sirenix.Serialization.FormatterUtilities.IsPrimitiveArrayType(System.Type)" />.</typeparam>
		/// <param name="array">The resulting primitive array.</param>
		/// <returns>
		///   <c>true</c> if reading a primitive array succeeded, otherwise <c>false</c>
		/// </returns>
		/// <exception cref="T:System.ArgumentException">Type  + typeof(T).Name +  is not a valid primitive array type.</exception>
		public unsafe override bool ReadPrimitiveArray<T>(out T[] array)
		{
			if (!FormatterUtilities.IsPrimitiveArrayType(typeof(T)))
			{
				throw new ArgumentException("Type " + typeof(T).Name + " is not a valid primitive array type.");
			}
			if (!peekedEntryType.HasValue)
			{
				PeekEntry(out var _);
			}
			if (peekedEntryType == EntryType.PrimitiveArray)
			{
				MarkEntryContentConsumed();
				if (!UNSAFE_Read_4_Int32(out var elementCount) || !UNSAFE_Read_4_Int32(out var bytesPerElement))
				{
					array = null;
					return false;
				}
				int byteCount = elementCount * bytesPerElement;
				if (!HasBufferData(byteCount))
				{
					bufferIndex = bufferEnd;
					array = null;
					return false;
				}
				if (typeof(T) == typeof(byte))
				{
					byte[] byteArray = new byte[byteCount];
					Buffer.BlockCopy(buffer, bufferIndex, byteArray, 0, byteCount);
					array = (T[])(object)byteArray;
					bufferIndex += byteCount;
					return true;
				}
				array = new T[elementCount];
				if (BitConverter.IsLittleEndian)
				{
					GCHandle toHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
					try
					{
						fixed (byte* fromBase = buffer)
						{
							void* from = fromBase + bufferIndex;
							void* to = toHandle.AddrOfPinnedObject().ToPointer();
							UnsafeUtilities.MemoryCopy(from, to, byteCount);
						}
					}
					finally
					{
						toHandle.Free();
					}
				}
				else
				{
					Func<byte[], int, T> fromBytes = (Func<byte[], int, T>)PrimitiveFromByteMethods[typeof(T)];
					for (int i = 0; i < elementCount; i++)
					{
						array[i] = fromBytes(buffer, bufferIndex + i * bytesPerElement);
					}
				}
				bufferIndex += byteCount;
				return true;
			}
			SkipEntry();
			array = null;
			return false;
		}

		/// <summary>
		/// Reads a <see cref="T:System.Boolean" /> value. This call will succeed if the next entry is an <see cref="F:Sirenix.Serialization.EntryType.Boolean" />.
		/// <para />
		/// If the call fails (and returns <c>false</c>), it will skip the current entry value, unless that entry is an <see cref="F:Sirenix.Serialization.EntryType.EndOfNode" /> or an <see cref="F:Sirenix.Serialization.EntryType.EndOfArray" />.
		/// </summary>
		/// <param name="value">The value that has been read.</param>
		/// <returns>
		///   <c>true</c> if reading the value succeeded, otherwise <c>false</c>
		/// </returns>
		public override bool ReadBoolean(out bool value)
		{
			if (!peekedEntryType.HasValue)
			{
				PeekEntry(out var _);
			}
			if (peekedEntryType == EntryType.Boolean)
			{
				MarkEntryContentConsumed();
				if (HasBufferData(1))
				{
					value = buffer[bufferIndex++] == 1;
					return true;
				}
				value = false;
				return false;
			}
			SkipEntry();
			value = false;
			return false;
		}

		/// <summary>
		/// Reads an <see cref="T:System.SByte" /> value. This call will succeed if the next entry is an <see cref="F:Sirenix.Serialization.EntryType.Integer" />.
		/// <para />
		/// If the value of the stored integer is smaller than <see cref="F:System.SByte.MinValue" /> or larger than <see cref="F:System.SByte.MaxValue" />, the result will be default(<see cref="T:System.SByte" />).
		/// <para />
		/// If the call fails (and returns <c>false</c>), it will skip the current entry value, unless that entry is an <see cref="F:Sirenix.Serialization.EntryType.EndOfNode" /> or an <see cref="F:Sirenix.Serialization.EntryType.EndOfArray" />.
		/// </summary>
		/// <param name="value">The value that has been read.</param>
		/// <returns>
		///   <c>true</c> if reading the value succeeded, otherwise <c>false</c>
		/// </returns>
		public override bool ReadSByte(out sbyte value)
		{
			if (ReadInt64(out var longValue))
			{
				try
				{
					value = checked((sbyte)longValue);
				}
				catch (OverflowException)
				{
					value = 0;
				}
				return true;
			}
			value = 0;
			return false;
		}

		/// <summary>
		/// Reads a <see cref="T:System.Byte" /> value. This call will succeed if the next entry is an <see cref="F:Sirenix.Serialization.EntryType.Integer" />.
		/// <para />
		/// If the value of the stored integer is smaller than <see cref="F:System.Byte.MinValue" /> or larger than <see cref="F:System.Byte.MaxValue" />, the result will be default(<see cref="T:System.Byte" />).
		/// <para />
		/// If the call fails (and returns <c>false</c>), it will skip the current entry value, unless that entry is an <see cref="F:Sirenix.Serialization.EntryType.EndOfNode" /> or an <see cref="F:Sirenix.Serialization.EntryType.EndOfArray" />.
		/// </summary>
		/// <param name="value">The value that has been read.</param>
		/// <returns>
		///   <c>true</c> if reading the value succeeded, otherwise <c>false</c>
		/// </returns>
		public override bool ReadByte(out byte value)
		{
			if (ReadUInt64(out var ulongValue))
			{
				try
				{
					value = checked((byte)ulongValue);
				}
				catch (OverflowException)
				{
					value = 0;
				}
				return true;
			}
			value = 0;
			return false;
		}

		/// <summary>
		/// Reads a <see cref="T:System.Int16" /> value. This call will succeed if the next entry is an <see cref="F:Sirenix.Serialization.EntryType.Integer" />.
		/// <para />
		/// If the value of the stored integer is smaller than <see cref="F:System.Int16.MinValue" /> or larger than <see cref="F:System.Int16.MaxValue" />, the result will be default(<see cref="T:System.Int16" />).
		/// <para />
		/// If the call fails (and returns <c>false</c>), it will skip the current entry value, unless that entry is an <see cref="F:Sirenix.Serialization.EntryType.EndOfNode" /> or an <see cref="F:Sirenix.Serialization.EntryType.EndOfArray" />.
		/// </summary>
		/// <param name="value">The value that has been read.</param>
		/// <returns>
		///   <c>true</c> if reading the value succeeded, otherwise <c>false</c>
		/// </returns>
		public override bool ReadInt16(out short value)
		{
			if (ReadInt64(out var longValue))
			{
				try
				{
					value = checked((short)longValue);
				}
				catch (OverflowException)
				{
					value = 0;
				}
				return true;
			}
			value = 0;
			return false;
		}

		/// <summary>
		/// Reads an <see cref="T:System.UInt16" /> value. This call will succeed if the next entry is an <see cref="F:Sirenix.Serialization.EntryType.Integer" />.
		/// <para />
		/// If the value of the stored integer is smaller than <see cref="F:System.UInt16.MinValue" /> or larger than <see cref="F:System.UInt16.MaxValue" />, the result will be default(<see cref="T:System.UInt16" />).
		/// <para />
		/// If the call fails (and returns <c>false</c>), it will skip the current entry value, unless that entry is an <see cref="F:Sirenix.Serialization.EntryType.EndOfNode" /> or an <see cref="F:Sirenix.Serialization.EntryType.EndOfArray" />.
		/// </summary>
		/// <param name="value">The value that has been read.</param>
		/// <returns>
		///   <c>true</c> if reading the value succeeded, otherwise <c>false</c>
		/// </returns>
		public override bool ReadUInt16(out ushort value)
		{
			if (ReadUInt64(out var ulongValue))
			{
				try
				{
					value = checked((ushort)ulongValue);
				}
				catch (OverflowException)
				{
					value = 0;
				}
				return true;
			}
			value = 0;
			return false;
		}

		/// <summary>
		/// Reads an <see cref="T:System.Int32" /> value. This call will succeed if the next entry is an <see cref="F:Sirenix.Serialization.EntryType.Integer" />.
		/// <para />
		/// If the value of the stored integer is smaller than <see cref="F:System.Int32.MinValue" /> or larger than <see cref="F:System.Int32.MaxValue" />, the result will be default(<see cref="T:System.Int32" />).
		/// <para />
		/// If the call fails (and returns <c>false</c>), it will skip the current entry value, unless that entry is an <see cref="F:Sirenix.Serialization.EntryType.EndOfNode" /> or an <see cref="F:Sirenix.Serialization.EntryType.EndOfArray" />.
		/// </summary>
		/// <param name="value">The value that has been read.</param>
		/// <returns>
		///   <c>true</c> if reading the value succeeded, otherwise <c>false</c>
		/// </returns>
		public override bool ReadInt32(out int value)
		{
			if (ReadInt64(out var longValue))
			{
				try
				{
					value = checked((int)longValue);
				}
				catch (OverflowException)
				{
					value = 0;
				}
				return true;
			}
			value = 0;
			return false;
		}

		/// <summary>
		/// Reads an <see cref="T:System.UInt32" /> value. This call will succeed if the next entry is an <see cref="F:Sirenix.Serialization.EntryType.Integer" />.
		/// <para />
		/// If the value of the stored integer is smaller than <see cref="F:System.UInt32.MinValue" /> or larger than <see cref="F:System.UInt32.MaxValue" />, the result will be default(<see cref="T:System.UInt32" />).
		/// <para />
		/// If the call fails (and returns <c>false</c>), it will skip the current entry value, unless that entry is an <see cref="F:Sirenix.Serialization.EntryType.EndOfNode" /> or an <see cref="F:Sirenix.Serialization.EntryType.EndOfArray" />.
		/// </summary>
		/// <param name="value">The value that has been read.</param>
		/// <returns>
		///   <c>true</c> if reading the value succeeded, otherwise <c>false</c>
		/// </returns>
		public override bool ReadUInt32(out uint value)
		{
			if (ReadUInt64(out var ulongValue))
			{
				try
				{
					value = checked((uint)ulongValue);
				}
				catch (OverflowException)
				{
					value = 0u;
				}
				return true;
			}
			value = 0u;
			return false;
		}

		/// <summary>
		/// Reads a <see cref="T:System.Int64" /> value. This call will succeed if the next entry is an <see cref="F:Sirenix.Serialization.EntryType.Integer" />.
		/// <para />
		/// If the value of the stored integer is smaller than <see cref="F:System.Int64.MinValue" /> or larger than <see cref="F:System.Int64.MaxValue" />, the result will be default(<see cref="T:System.Int64" />).
		/// <para />
		/// If the call fails (and returns <c>false</c>), it will skip the current entry value, unless that entry is an <see cref="F:Sirenix.Serialization.EntryType.EndOfNode" /> or an <see cref="F:Sirenix.Serialization.EntryType.EndOfArray" />.
		/// </summary>
		/// <param name="value">The value that has been read.</param>
		/// <returns>
		///   <c>true</c> if reading the value succeeded, otherwise <c>false</c>
		/// </returns>
		public override bool ReadInt64(out long value)
		{
			if (!peekedEntryType.HasValue)
			{
				PeekEntry(out var _);
			}
			if (peekedEntryType == EntryType.Integer)
			{
				try
				{
					switch (peekedBinaryEntryType)
					{
					case BinaryEntryType.NamedSByte:
					case BinaryEntryType.UnnamedSByte:
					{
						if (!UNSAFE_Read_1_SByte(out var i33))
						{
							value = 0L;
							return false;
						}
						value = i33;
						break;
					}
					case BinaryEntryType.NamedByte:
					case BinaryEntryType.UnnamedByte:
					{
						if (!UNSAFE_Read_1_Byte(out var ui17))
						{
							value = 0L;
							return false;
						}
						value = ui17;
						break;
					}
					case BinaryEntryType.NamedShort:
					case BinaryEntryType.UnnamedShort:
					{
						if (!UNSAFE_Read_2_Int16(out var i34))
						{
							value = 0L;
							return false;
						}
						value = i34;
						break;
					}
					case BinaryEntryType.NamedUShort:
					case BinaryEntryType.UnnamedUShort:
					{
						if (!UNSAFE_Read_2_UInt16(out var ui16))
						{
							value = 0L;
							return false;
						}
						value = ui16;
						break;
					}
					case BinaryEntryType.NamedInt:
					case BinaryEntryType.UnnamedInt:
					{
						if (!UNSAFE_Read_4_Int32(out var i32))
						{
							value = 0L;
							return false;
						}
						value = i32;
						break;
					}
					case BinaryEntryType.NamedUInt:
					case BinaryEntryType.UnnamedUInt:
					{
						if (!UNSAFE_Read_4_UInt32(out var ui32))
						{
							value = 0L;
							return false;
						}
						value = ui32;
						break;
					}
					case BinaryEntryType.NamedLong:
					case BinaryEntryType.UnnamedLong:
						if (!UNSAFE_Read_8_Int64(out value))
						{
							return false;
						}
						break;
					case BinaryEntryType.NamedULong:
					case BinaryEntryType.UnnamedULong:
					{
						if (!UNSAFE_Read_8_UInt64(out var uint64))
						{
							value = 0L;
							return false;
						}
						if (uint64 > long.MaxValue)
						{
							value = 0L;
							return false;
						}
						value = (long)uint64;
						break;
					}
					default:
						throw new InvalidOperationException();
					}
					return true;
				}
				finally
				{
					MarkEntryContentConsumed();
				}
			}
			SkipEntry();
			value = 0L;
			return false;
		}

		/// <summary>
		/// Reads an <see cref="T:System.UInt64" /> value. This call will succeed if the next entry is an <see cref="F:Sirenix.Serialization.EntryType.Integer" />.
		/// <para />
		/// If the value of the stored integer is smaller than <see cref="F:System.UInt64.MinValue" /> or larger than <see cref="F:System.UInt64.MaxValue" />, the result will be default(<see cref="T:System.UInt64" />).
		/// <para />
		/// If the call fails (and returns <c>false</c>), it will skip the current entry value, unless that entry is an <see cref="F:Sirenix.Serialization.EntryType.EndOfNode" /> or an <see cref="F:Sirenix.Serialization.EntryType.EndOfArray" />.
		/// </summary>
		/// <param name="value">The value that has been read.</param>
		/// <returns>
		///   <c>true</c> if reading the value succeeded, otherwise <c>false</c>
		/// </returns>
		public override bool ReadUInt64(out ulong value)
		{
			if (!peekedEntryType.HasValue)
			{
				PeekEntry(out var _);
			}
			if (peekedEntryType == EntryType.Integer)
			{
				try
				{
					switch (peekedBinaryEntryType)
					{
					case BinaryEntryType.NamedSByte:
					case BinaryEntryType.UnnamedSByte:
					case BinaryEntryType.NamedByte:
					case BinaryEntryType.UnnamedByte:
					{
						if (!UNSAFE_Read_1_Byte(out var i67))
						{
							value = 0uL;
							return false;
						}
						value = i67;
						break;
					}
					case BinaryEntryType.NamedShort:
					case BinaryEntryType.UnnamedShort:
					{
						if (!UNSAFE_Read_2_Int16(out var i65))
						{
							value = 0uL;
							return false;
						}
						if (i65 < 0)
						{
							value = 0uL;
							return false;
						}
						value = (ulong)i65;
						break;
					}
					case BinaryEntryType.NamedUShort:
					case BinaryEntryType.UnnamedUShort:
					{
						if (!UNSAFE_Read_2_UInt16(out var ui33))
						{
							value = 0uL;
							return false;
						}
						value = ui33;
						break;
					}
					case BinaryEntryType.NamedInt:
					case BinaryEntryType.UnnamedInt:
					{
						if (!UNSAFE_Read_4_Int32(out var i66))
						{
							value = 0uL;
							return false;
						}
						if (i66 < 0)
						{
							value = 0uL;
							return false;
						}
						value = (ulong)i66;
						break;
					}
					case BinaryEntryType.NamedUInt:
					case BinaryEntryType.UnnamedUInt:
					{
						if (!UNSAFE_Read_4_UInt32(out var ui32))
						{
							value = 0uL;
							return false;
						}
						value = ui32;
						break;
					}
					case BinaryEntryType.NamedLong:
					case BinaryEntryType.UnnamedLong:
					{
						if (!UNSAFE_Read_8_Int64(out var i64))
						{
							value = 0uL;
							return false;
						}
						if (i64 < 0)
						{
							value = 0uL;
							return false;
						}
						value = (ulong)i64;
						break;
					}
					case BinaryEntryType.NamedULong:
					case BinaryEntryType.UnnamedULong:
						if (!UNSAFE_Read_8_UInt64(out value))
						{
							return false;
						}
						break;
					default:
						throw new InvalidOperationException();
					}
					return true;
				}
				finally
				{
					MarkEntryContentConsumed();
				}
			}
			SkipEntry();
			value = 0uL;
			return false;
		}

		/// <summary>
		/// Reads a <see cref="T:System.Char" /> value. This call will succeed if the next entry is an <see cref="F:Sirenix.Serialization.EntryType.String" />.
		/// <para />
		/// If the string of the entry is longer than 1 character, the first character of the string will be taken as the result.
		/// <para />
		/// If the call fails (and returns <c>false</c>), it will skip the current entry value, unless that entry is an <see cref="F:Sirenix.Serialization.EntryType.EndOfNode" /> or an <see cref="F:Sirenix.Serialization.EntryType.EndOfArray" />.
		/// </summary>
		/// <param name="value">The value that has been read.</param>
		/// <returns>
		///   <c>true</c> if reading the value succeeded, otherwise <c>false</c>
		/// </returns>
		public override bool ReadChar(out char value)
		{
			if (!peekedEntryType.HasValue)
			{
				PeekEntry(out var _);
			}
			if (peekedBinaryEntryType == BinaryEntryType.NamedChar || peekedBinaryEntryType == BinaryEntryType.UnnamedChar)
			{
				MarkEntryContentConsumed();
				return UNSAFE_Read_2_Char(out value);
			}
			if (peekedBinaryEntryType == BinaryEntryType.NamedString || peekedBinaryEntryType == BinaryEntryType.UnnamedString)
			{
				MarkEntryContentConsumed();
				string str = ReadStringValue();
				if (str == null || str.Length == 0)
				{
					value = '\0';
					return false;
				}
				value = str[0];
				return true;
			}
			SkipEntry();
			value = '\0';
			return false;
		}

		/// <summary>
		/// Reads a <see cref="T:System.Single" /> value. This call will succeed if the next entry is an <see cref="F:Sirenix.Serialization.EntryType.FloatingPoint" /> or an <see cref="F:Sirenix.Serialization.EntryType.Integer" />.
		/// <para />
		/// If the stored integer or floating point value is smaller than <see cref="F:System.Single.MinValue" /> or larger than <see cref="F:System.Single.MaxValue" />, the result will be default(<see cref="T:System.Single" />).
		/// <para />
		/// If the call fails (and returns <c>false</c>), it will skip the current entry value, unless that entry is an <see cref="F:Sirenix.Serialization.EntryType.EndOfNode" /> or an <see cref="F:Sirenix.Serialization.EntryType.EndOfArray" />.
		/// </summary>
		/// <param name="value">The value that has been read.</param>
		/// <returns>
		///   <c>true</c> if reading the value succeeded, otherwise <c>false</c>
		/// </returns>
		public override bool ReadSingle(out float value)
		{
			if (!peekedEntryType.HasValue)
			{
				PeekEntry(out var _);
			}
			if (peekedBinaryEntryType == BinaryEntryType.NamedFloat || peekedBinaryEntryType == BinaryEntryType.UnnamedFloat)
			{
				MarkEntryContentConsumed();
				return UNSAFE_Read_4_Float32(out value);
			}
			if (peekedBinaryEntryType == BinaryEntryType.NamedDouble || peekedBinaryEntryType == BinaryEntryType.UnnamedDouble)
			{
				MarkEntryContentConsumed();
				if (!UNSAFE_Read_8_Float64(out var d))
				{
					value = 0f;
					return false;
				}
				try
				{
					value = (float)d;
				}
				catch (OverflowException)
				{
					value = 0f;
				}
				return true;
			}
			if (peekedBinaryEntryType == BinaryEntryType.NamedDecimal || peekedBinaryEntryType == BinaryEntryType.UnnamedDecimal)
			{
				MarkEntryContentConsumed();
				if (!UNSAFE_Read_16_Decimal(out var d2))
				{
					value = 0f;
					return false;
				}
				try
				{
					value = (float)d2;
				}
				catch (OverflowException)
				{
					value = 0f;
				}
				return true;
			}
			if (peekedEntryType == EntryType.Integer)
			{
				if (!ReadInt64(out var val))
				{
					value = 0f;
					return false;
				}
				try
				{
					value = val;
				}
				catch (OverflowException)
				{
					value = 0f;
				}
				return true;
			}
			SkipEntry();
			value = 0f;
			return false;
		}

		/// <summary>
		/// Reads a <see cref="T:System.Double" /> value. This call will succeed if the next entry is an <see cref="F:Sirenix.Serialization.EntryType.FloatingPoint" /> or an <see cref="F:Sirenix.Serialization.EntryType.Integer" />.
		/// <para />
		/// If the stored integer or floating point value is smaller than <see cref="F:System.Double.MinValue" /> or larger than <see cref="F:System.Double.MaxValue" />, the result will be default(<see cref="T:System.Double" />).
		/// <para />
		/// If the call fails (and returns <c>false</c>), it will skip the current entry value, unless that entry is an <see cref="F:Sirenix.Serialization.EntryType.EndOfNode" /> or an <see cref="F:Sirenix.Serialization.EntryType.EndOfArray" />.
		/// </summary>
		/// <param name="value">The value that has been read.</param>
		/// <returns>
		///   <c>true</c> if reading the value succeeded, otherwise <c>false</c>
		/// </returns>
		public override bool ReadDouble(out double value)
		{
			if (!peekedEntryType.HasValue)
			{
				PeekEntry(out var _);
			}
			if (peekedBinaryEntryType == BinaryEntryType.NamedDouble || peekedBinaryEntryType == BinaryEntryType.UnnamedDouble)
			{
				MarkEntryContentConsumed();
				return UNSAFE_Read_8_Float64(out value);
			}
			if (peekedBinaryEntryType == BinaryEntryType.NamedFloat || peekedBinaryEntryType == BinaryEntryType.UnnamedFloat)
			{
				MarkEntryContentConsumed();
				if (!UNSAFE_Read_4_Float32(out var s))
				{
					value = 0.0;
					return false;
				}
				value = s;
				return true;
			}
			if (peekedBinaryEntryType == BinaryEntryType.NamedDecimal || peekedBinaryEntryType == BinaryEntryType.UnnamedDecimal)
			{
				MarkEntryContentConsumed();
				if (!UNSAFE_Read_16_Decimal(out var d))
				{
					value = 0.0;
					return false;
				}
				try
				{
					value = (double)d;
				}
				catch (OverflowException)
				{
					value = 0.0;
				}
				return true;
			}
			if (peekedEntryType == EntryType.Integer)
			{
				if (!ReadInt64(out var val))
				{
					value = 0.0;
					return false;
				}
				try
				{
					value = val;
				}
				catch (OverflowException)
				{
					value = 0.0;
				}
				return true;
			}
			SkipEntry();
			value = 0.0;
			return false;
		}

		/// <summary>
		/// Reads a <see cref="T:System.Decimal" /> value. This call will succeed if the next entry is an <see cref="F:Sirenix.Serialization.EntryType.FloatingPoint" /> or an <see cref="F:Sirenix.Serialization.EntryType.Integer" />.
		/// <para />
		/// If the stored integer or floating point value is smaller than <see cref="F:System.Decimal.MinValue" /> or larger than <see cref="F:System.Decimal.MaxValue" />, the result will be default(<see cref="T:System.Decimal" />).
		/// <para />
		/// If the call fails (and returns <c>false</c>), it will skip the current entry value, unless that entry is an <see cref="F:Sirenix.Serialization.EntryType.EndOfNode" /> or an <see cref="F:Sirenix.Serialization.EntryType.EndOfArray" />.
		/// </summary>
		/// <param name="value">The value that has been read.</param>
		/// <returns>
		///   <c>true</c> if reading the value succeeded, otherwise <c>false</c>
		/// </returns>
		public override bool ReadDecimal(out decimal value)
		{
			if (!peekedEntryType.HasValue)
			{
				PeekEntry(out var _);
			}
			if (peekedBinaryEntryType == BinaryEntryType.NamedDecimal || peekedBinaryEntryType == BinaryEntryType.UnnamedDecimal)
			{
				MarkEntryContentConsumed();
				return UNSAFE_Read_16_Decimal(out value);
			}
			if (peekedBinaryEntryType == BinaryEntryType.NamedDouble || peekedBinaryEntryType == BinaryEntryType.UnnamedDouble)
			{
				MarkEntryContentConsumed();
				if (!UNSAFE_Read_8_Float64(out var d))
				{
					value = default(decimal);
					return false;
				}
				try
				{
					value = (decimal)d;
				}
				catch (OverflowException)
				{
					value = default(decimal);
				}
				return true;
			}
			if (peekedBinaryEntryType == BinaryEntryType.NamedFloat || peekedBinaryEntryType == BinaryEntryType.UnnamedFloat)
			{
				MarkEntryContentConsumed();
				if (!UNSAFE_Read_4_Float32(out var f))
				{
					value = default(decimal);
					return false;
				}
				try
				{
					value = (decimal)f;
				}
				catch (OverflowException)
				{
					value = default(decimal);
				}
				return true;
			}
			if (peekedEntryType == EntryType.Integer)
			{
				if (!ReadInt64(out var val))
				{
					value = default(decimal);
					return false;
				}
				try
				{
					value = val;
				}
				catch (OverflowException)
				{
					value = default(decimal);
				}
				return true;
			}
			SkipEntry();
			value = default(decimal);
			return false;
		}

		/// <summary>
		/// Reads an external reference guid. This call will succeed if the next entry is an <see cref="F:Sirenix.Serialization.EntryType.ExternalReferenceByGuid" />.
		/// <para />
		/// If the call fails (and returns <c>false</c>), it will skip the current entry value, unless that entry is an <see cref="F:Sirenix.Serialization.EntryType.EndOfNode" /> or an <see cref="F:Sirenix.Serialization.EntryType.EndOfArray" />.
		/// </summary>
		/// <param name="guid">The external reference guid.</param>
		/// <returns>
		///   <c>true</c> if reading the value succeeded, otherwise <c>false</c>
		/// </returns>
		public override bool ReadExternalReference(out Guid guid)
		{
			if (!peekedEntryType.HasValue)
			{
				PeekEntry(out var _);
			}
			if (peekedBinaryEntryType == BinaryEntryType.NamedExternalReferenceByGuid || peekedBinaryEntryType == BinaryEntryType.UnnamedExternalReferenceByGuid)
			{
				MarkEntryContentConsumed();
				return UNSAFE_Read_16_Guid(out guid);
			}
			SkipEntry();
			guid = default(Guid);
			return false;
		}

		/// <summary>
		/// Reads a <see cref="T:System.Guid" /> value. This call will succeed if the next entry is an <see cref="F:Sirenix.Serialization.EntryType.Guid" />.
		/// <para />
		/// If the call fails (and returns <c>false</c>), it will skip the current entry value, unless that entry is an <see cref="F:Sirenix.Serialization.EntryType.EndOfNode" /> or an <see cref="F:Sirenix.Serialization.EntryType.EndOfArray" />.
		/// </summary>
		/// <param name="value">The value that has been read.</param>
		/// <returns>
		///   <c>true</c> if reading the value succeeded, otherwise <c>false</c>
		/// </returns>
		public override bool ReadGuid(out Guid value)
		{
			if (!peekedEntryType.HasValue)
			{
				PeekEntry(out var _);
			}
			if (peekedBinaryEntryType == BinaryEntryType.NamedGuid || peekedBinaryEntryType == BinaryEntryType.UnnamedGuid)
			{
				MarkEntryContentConsumed();
				return UNSAFE_Read_16_Guid(out value);
			}
			SkipEntry();
			value = default(Guid);
			return false;
		}

		/// <summary>
		/// Reads an external reference index. This call will succeed if the next entry is an <see cref="F:Sirenix.Serialization.EntryType.ExternalReferenceByIndex" />.
		/// <para />
		/// If the call fails (and returns <c>false</c>), it will skip the current entry value, unless that entry is an <see cref="F:Sirenix.Serialization.EntryType.EndOfNode" /> or an <see cref="F:Sirenix.Serialization.EntryType.EndOfArray" />.
		/// </summary>
		/// <param name="index">The external reference index.</param>
		/// <returns>
		///   <c>true</c> if reading the value succeeded, otherwise <c>false</c>
		/// </returns>
		public override bool ReadExternalReference(out int index)
		{
			if (!peekedEntryType.HasValue)
			{
				PeekEntry(out var _);
			}
			if (peekedBinaryEntryType == BinaryEntryType.NamedExternalReferenceByIndex || peekedBinaryEntryType == BinaryEntryType.UnnamedExternalReferenceByIndex)
			{
				MarkEntryContentConsumed();
				return UNSAFE_Read_4_Int32(out index);
			}
			SkipEntry();
			index = -1;
			return false;
		}

		/// <summary>
		/// Reads an external reference string. This call will succeed if the next entry is an <see cref="F:Sirenix.Serialization.EntryType.ExternalReferenceByString" />.
		/// <para />
		/// If the call fails (and returns <c>false</c>), it will skip the current entry value, unless that entry is an <see cref="F:Sirenix.Serialization.EntryType.EndOfNode" /> or an <see cref="F:Sirenix.Serialization.EntryType.EndOfArray" />.
		/// </summary>
		/// <param name="id">The external reference string.</param>
		/// <returns>
		///   <c>true</c> if reading the value succeeded, otherwise <c>false</c>
		/// </returns>
		public override bool ReadExternalReference(out string id)
		{
			if (!peekedEntryType.HasValue)
			{
				PeekEntry(out var _);
			}
			if (peekedBinaryEntryType == BinaryEntryType.NamedExternalReferenceByString || peekedBinaryEntryType == BinaryEntryType.UnnamedExternalReferenceByString)
			{
				id = ReadStringValue();
				MarkEntryContentConsumed();
				return id != null;
			}
			SkipEntry();
			id = null;
			return false;
		}

		/// <summary>
		/// Reads a <c>null</c> value. This call will succeed if the next entry is an <see cref="F:Sirenix.Serialization.EntryType.Null" />.
		/// <para />
		/// If the call fails (and returns <c>false</c>), it will skip the current entry value, unless that entry is an <see cref="F:Sirenix.Serialization.EntryType.EndOfNode" /> or an <see cref="F:Sirenix.Serialization.EntryType.EndOfArray" />.
		/// </summary>
		/// <returns>
		///   <c>true</c> if reading the value succeeded, otherwise <c>false</c>
		/// </returns>
		public override bool ReadNull()
		{
			if (!peekedEntryType.HasValue)
			{
				PeekEntry(out var _);
			}
			if (peekedBinaryEntryType == BinaryEntryType.NamedNull || peekedBinaryEntryType == BinaryEntryType.UnnamedNull)
			{
				MarkEntryContentConsumed();
				return true;
			}
			SkipEntry();
			return false;
		}

		/// <summary>
		/// Reads an internal reference id. This call will succeed if the next entry is an <see cref="F:Sirenix.Serialization.EntryType.InternalReference" />.
		/// <para />
		/// If the call fails (and returns <c>false</c>), it will skip the current entry value, unless that entry is an <see cref="F:Sirenix.Serialization.EntryType.EndOfNode" /> or an <see cref="F:Sirenix.Serialization.EntryType.EndOfArray" />.
		/// </summary>
		/// <param name="id">The internal reference id.</param>
		/// <returns>
		///   <c>true</c> if reading the value succeeded, otherwise <c>false</c>
		/// </returns>
		public override bool ReadInternalReference(out int id)
		{
			if (!peekedEntryType.HasValue)
			{
				PeekEntry(out var _);
			}
			if (peekedBinaryEntryType == BinaryEntryType.NamedInternalReference || peekedBinaryEntryType == BinaryEntryType.UnnamedInternalReference)
			{
				MarkEntryContentConsumed();
				return UNSAFE_Read_4_Int32(out id);
			}
			SkipEntry();
			id = -1;
			return false;
		}

		/// <summary>
		/// Reads a <see cref="T:System.String" /> value. This call will succeed if the next entry is an <see cref="F:Sirenix.Serialization.EntryType.String" />.
		/// <para />
		/// If the call fails (and returns <c>false</c>), it will skip the current entry value, unless that entry is an <see cref="F:Sirenix.Serialization.EntryType.EndOfNode" /> or an <see cref="F:Sirenix.Serialization.EntryType.EndOfArray" />.
		/// </summary>
		/// <param name="value">The value that has been read.</param>
		/// <returns>
		///   <c>true</c> if reading the value succeeded, otherwise <c>false</c>
		/// </returns>
		public override bool ReadString(out string value)
		{
			if (!peekedEntryType.HasValue)
			{
				PeekEntry(out var _);
			}
			if (peekedBinaryEntryType == BinaryEntryType.NamedString || peekedBinaryEntryType == BinaryEntryType.UnnamedString)
			{
				value = ReadStringValue();
				MarkEntryContentConsumed();
				return value != null;
			}
			SkipEntry();
			value = null;
			return false;
		}

		/// <summary>
		/// Tells the reader that a new serialization session is about to begin, and that it should clear all cached values left over from any prior serialization sessions.
		/// This method is only relevant when the same reader is used to deserialize several different, unrelated values.
		/// </summary>
		public override void PrepareNewSerializationSession()
		{
			base.PrepareNewSerializationSession();
			peekedEntryType = null;
			peekedEntryName = null;
			peekedBinaryEntryType = BinaryEntryType.Invalid;
			types.Clear();
			bufferIndex = 0;
			bufferEnd = 0;
			buffer = internalBufferBackup;
		}

		public unsafe override string GetDataDump()
		{
			byte[] bytes;
			if (bufferEnd == buffer.Length)
			{
				bytes = buffer;
			}
			else
			{
				bytes = new byte[bufferEnd];
				fixed (byte* ptr = buffer)
				{
					void* from = ptr;
					fixed (byte* ptr2 = bytes)
					{
						void* to = ptr2;
						UnsafeUtilities.MemoryCopy(from, to, bytes.Length);
					}
				}
			}
			return "Binary hex dump: " + ProperBitConverter.BytesToHexString(bytes);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private unsafe string ReadStringValue()
		{
			if (!UNSAFE_Read_1_Byte(out var charSizeFlag))
			{
				return null;
			}
			if (!UNSAFE_Read_4_Int32(out var length))
			{
				return null;
			}
			string str = new string(' ', length);
			if (charSizeFlag == 0)
			{
				fixed (byte* baseFromPtr = buffer)
				{
					fixed (char* baseToPtr = str)
					{
						byte* fromPtr = baseFromPtr + bufferIndex;
						byte* toPtr = (byte*)baseToPtr;
						if (BitConverter.IsLittleEndian)
						{
							for (int i = 0; i < length; i++)
							{
								*(toPtr++) = *(fromPtr++);
								toPtr++;
							}
						}
						else
						{
							for (int j = 0; j < length; j++)
							{
								toPtr++;
								*(toPtr++) = *(fromPtr++);
							}
						}
					}
				}
				bufferIndex += length;
				return str;
			}
			int bytes = length * 2;
			fixed (byte* baseFromPtr2 = buffer)
			{
				fixed (char* baseToPtr2 = str)
				{
					if (BitConverter.IsLittleEndian)
					{
						Struct256Bit* fromLargePtr = (Struct256Bit*)(baseFromPtr2 + bufferIndex);
						Struct256Bit* toLargePtr = (Struct256Bit*)baseToPtr2;
						byte* end = (byte*)baseToPtr2 + bytes;
						while (toLargePtr + 1 < end)
						{
							*(toLargePtr++) = *(fromLargePtr++);
						}
						byte* fromSmallPtr = (byte*)fromLargePtr;
						byte* toSmallPtr = (byte*)toLargePtr;
						while (toSmallPtr < end)
						{
							*(toSmallPtr++) = *(fromSmallPtr++);
						}
					}
					else
					{
						byte* fromPtr2 = baseFromPtr2 + bufferIndex;
						byte* toPtr2 = (byte*)baseToPtr2;
						for (int k = 0; k < length; k++)
						{
							*toPtr2 = fromPtr2[1];
							toPtr2[1] = *fromPtr2;
							fromPtr2 += 2;
							toPtr2 += 2;
						}
					}
				}
			}
			bufferIndex += bytes;
			return str;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void SkipStringValue()
		{
			if (UNSAFE_Read_1_Byte(out var charSizeFlag) && UNSAFE_Read_4_Int32(out var skipBytes))
			{
				if (charSizeFlag != 0)
				{
					skipBytes *= 2;
				}
				if (HasBufferData(skipBytes))
				{
					bufferIndex += skipBytes;
				}
				else
				{
					bufferIndex = bufferEnd;
				}
			}
		}

		private void SkipPeekedEntryContent()
		{
			if (!peekedEntryType.HasValue)
			{
				return;
			}
			try
			{
				switch (peekedBinaryEntryType)
				{
				case BinaryEntryType.NamedStartOfReferenceNode:
				case BinaryEntryType.UnnamedStartOfReferenceNode:
					ReadTypeEntry();
					if (SkipBuffer(4))
					{
					}
					break;
				case BinaryEntryType.NamedStartOfStructNode:
				case BinaryEntryType.UnnamedStartOfStructNode:
					ReadTypeEntry();
					break;
				case BinaryEntryType.StartOfArray:
					SkipBuffer(8);
					break;
				case BinaryEntryType.PrimitiveArray:
				{
					if (UNSAFE_Read_4_Int32(out var elements) && UNSAFE_Read_4_Int32(out var bytesPerElement))
					{
						SkipBuffer(elements * bytesPerElement);
					}
					break;
				}
				case BinaryEntryType.NamedSByte:
				case BinaryEntryType.UnnamedSByte:
				case BinaryEntryType.NamedByte:
				case BinaryEntryType.UnnamedByte:
				case BinaryEntryType.NamedBoolean:
				case BinaryEntryType.UnnamedBoolean:
					SkipBuffer(1);
					break;
				case BinaryEntryType.NamedShort:
				case BinaryEntryType.UnnamedShort:
				case BinaryEntryType.NamedUShort:
				case BinaryEntryType.UnnamedUShort:
				case BinaryEntryType.NamedChar:
				case BinaryEntryType.UnnamedChar:
					SkipBuffer(2);
					break;
				case BinaryEntryType.NamedInternalReference:
				case BinaryEntryType.UnnamedInternalReference:
				case BinaryEntryType.NamedExternalReferenceByIndex:
				case BinaryEntryType.UnnamedExternalReferenceByIndex:
				case BinaryEntryType.NamedInt:
				case BinaryEntryType.UnnamedInt:
				case BinaryEntryType.NamedUInt:
				case BinaryEntryType.UnnamedUInt:
				case BinaryEntryType.NamedFloat:
				case BinaryEntryType.UnnamedFloat:
					SkipBuffer(4);
					break;
				case BinaryEntryType.NamedLong:
				case BinaryEntryType.UnnamedLong:
				case BinaryEntryType.NamedULong:
				case BinaryEntryType.UnnamedULong:
				case BinaryEntryType.NamedDouble:
				case BinaryEntryType.UnnamedDouble:
					SkipBuffer(8);
					break;
				case BinaryEntryType.NamedExternalReferenceByGuid:
				case BinaryEntryType.UnnamedExternalReferenceByGuid:
				case BinaryEntryType.NamedDecimal:
				case BinaryEntryType.UnnamedDecimal:
				case BinaryEntryType.NamedGuid:
				case BinaryEntryType.UnnamedGuid:
					SkipBuffer(8);
					break;
				case BinaryEntryType.NamedString:
				case BinaryEntryType.UnnamedString:
				case BinaryEntryType.NamedExternalReferenceByString:
				case BinaryEntryType.UnnamedExternalReferenceByString:
					SkipStringValue();
					break;
				case BinaryEntryType.TypeName:
					base.Context.Config.DebugContext.LogError("Parsing error in binary data reader: should not be able to peek a TypeName entry.");
					SkipBuffer(4);
					ReadStringValue();
					break;
				case BinaryEntryType.TypeID:
					base.Context.Config.DebugContext.LogError("Parsing error in binary data reader: should not be able to peek a TypeID entry.");
					SkipBuffer(4);
					break;
				case BinaryEntryType.Invalid:
				case BinaryEntryType.EndOfNode:
				case BinaryEntryType.EndOfArray:
				case BinaryEntryType.NamedNull:
				case BinaryEntryType.UnnamedNull:
				case BinaryEntryType.EndOfStream:
					break;
				}
			}
			finally
			{
				MarkEntryContentConsumed();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private bool SkipBuffer(int amount)
		{
			int newIndex = bufferIndex + amount;
			if (newIndex > bufferEnd)
			{
				bufferIndex = bufferEnd;
				return false;
			}
			bufferIndex = newIndex;
			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private Type ReadTypeEntry()
		{
			if (!HasBufferData(1))
			{
				return null;
			}
			BinaryEntryType entryType = (BinaryEntryType)buffer[bufferIndex++];
			int id;
			Type type;
			switch (entryType)
			{
			case BinaryEntryType.TypeID:
				if (!UNSAFE_Read_4_Int32(out id))
				{
					return null;
				}
				if (!types.TryGetValue(id, out type))
				{
					base.Context.Config.DebugContext.LogError("Missing type ID during deserialization: " + id + " at node " + base.CurrentNodeName + " and depth " + base.CurrentNodeDepth + " and id " + base.CurrentNodeId);
				}
				break;
			case BinaryEntryType.TypeName:
			{
				if (!UNSAFE_Read_4_Int32(out id))
				{
					return null;
				}
				string name = ReadStringValue();
				type = ((name == null) ? null : base.Context.Binder.BindToType(name, base.Context.Config.DebugContext));
				types.Add(id, type);
				break;
			}
			case BinaryEntryType.UnnamedNull:
				type = null;
				break;
			default:
				type = null;
				base.Context.Config.DebugContext.LogError("Expected TypeName, TypeID or UnnamedNull entry flag for reading type data, but instead got the entry flag: " + entryType.ToString() + ".");
				break;
			}
			return type;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void MarkEntryContentConsumed()
		{
			peekedEntryType = null;
			peekedEntryName = null;
			peekedBinaryEntryType = BinaryEntryType.Invalid;
		}

		/// <summary>
		/// Peeks the current entry.
		/// </summary>
		/// <returns>The peeked entry.</returns>
		protected override EntryType PeekEntry()
		{
			string name;
			return PeekEntry(out name);
		}

		/// <summary>
		/// Consumes the current entry, and reads to the next one.
		/// </summary>
		/// <returns>The next entry.</returns>
		protected override EntryType ReadToNextEntry()
		{
			SkipPeekedEntryContent();
			string name;
			return PeekEntry(out name);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private bool UNSAFE_Read_1_Byte(out byte value)
		{
			if (HasBufferData(1))
			{
				value = buffer[bufferIndex++];
				return true;
			}
			value = 0;
			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private bool UNSAFE_Read_1_SByte(out sbyte value)
		{
			if (HasBufferData(1))
			{
				value = (sbyte)buffer[bufferIndex++];
				return true;
			}
			value = 0;
			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private unsafe bool UNSAFE_Read_2_Int16(out short value)
		{
			if (HasBufferData(2))
			{
				fixed (byte* basePtr = buffer)
				{
					if (BitConverter.IsLittleEndian)
					{
						value = *(short*)(basePtr + bufferIndex);
					}
					else
					{
						short val = 0;
						byte* toPtr = (byte*)(&val) + 1;
						byte* fromPtr = basePtr + bufferIndex;
						*(toPtr--) = *(fromPtr++);
						*toPtr = *fromPtr;
						value = val;
					}
				}
				bufferIndex += 2;
				return true;
			}
			bufferIndex = bufferEnd;
			value = 0;
			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private unsafe bool UNSAFE_Read_2_UInt16(out ushort value)
		{
			if (HasBufferData(2))
			{
				fixed (byte* basePtr = buffer)
				{
					if (BitConverter.IsLittleEndian)
					{
						value = *(ushort*)(basePtr + bufferIndex);
					}
					else
					{
						ushort val = 0;
						byte* toPtr = (byte*)(&val) + 1;
						byte* fromPtr = basePtr + bufferIndex;
						*(toPtr--) = *(fromPtr++);
						*toPtr = *fromPtr;
						value = val;
					}
				}
				bufferIndex += 2;
				return true;
			}
			bufferIndex = bufferEnd;
			value = 0;
			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private unsafe bool UNSAFE_Read_2_Char(out char value)
		{
			if (HasBufferData(2))
			{
				fixed (byte* basePtr = buffer)
				{
					if (BitConverter.IsLittleEndian)
					{
						value = *(char*)(basePtr + bufferIndex);
					}
					else
					{
						char val = '\0';
						byte* toPtr = (byte*)(&val) + 1;
						byte* fromPtr = basePtr + bufferIndex;
						*(toPtr--) = *(fromPtr++);
						*toPtr = *fromPtr;
						value = val;
					}
				}
				bufferIndex += 2;
				return true;
			}
			bufferIndex = bufferEnd;
			value = '\0';
			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private unsafe bool UNSAFE_Read_4_Int32(out int value)
		{
			if (HasBufferData(4))
			{
				fixed (byte* basePtr = buffer)
				{
					if (BitConverter.IsLittleEndian)
					{
						value = *(int*)(basePtr + bufferIndex);
					}
					else
					{
						int val = 0;
						byte* toPtr = (byte*)(&val) + 3;
						byte* fromPtr = basePtr + bufferIndex;
						*(toPtr--) = *(fromPtr++);
						*(toPtr--) = *(fromPtr++);
						*(toPtr--) = *(fromPtr++);
						*toPtr = *fromPtr;
						value = val;
					}
				}
				bufferIndex += 4;
				return true;
			}
			bufferIndex = bufferEnd;
			value = 0;
			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private unsafe bool UNSAFE_Read_4_UInt32(out uint value)
		{
			if (HasBufferData(4))
			{
				fixed (byte* basePtr = buffer)
				{
					if (BitConverter.IsLittleEndian)
					{
						value = *(uint*)(basePtr + bufferIndex);
					}
					else
					{
						uint val = 0u;
						byte* toPtr = (byte*)(&val) + 3;
						byte* fromPtr = basePtr + bufferIndex;
						*(toPtr--) = *(fromPtr++);
						*(toPtr--) = *(fromPtr++);
						*(toPtr--) = *(fromPtr++);
						*toPtr = *fromPtr;
						value = val;
					}
				}
				bufferIndex += 4;
				return true;
			}
			bufferIndex = bufferEnd;
			value = 0u;
			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private unsafe bool UNSAFE_Read_4_Float32(out float value)
		{
			if (HasBufferData(4))
			{
				fixed (byte* basePtr = buffer)
				{
					if (BitConverter.IsLittleEndian)
					{
						if (ArchitectureInfo.Architecture_Supports_Unaligned_Float32_Reads)
						{
							value = *(float*)(basePtr + bufferIndex);
						}
						else
						{
							float result = 0f;
							*(int*)(&result) = *(int*)(basePtr + bufferIndex);
							value = result;
						}
					}
					else
					{
						float val = 0f;
						byte* toPtr = (byte*)(&val) + 3;
						byte* fromPtr = basePtr + bufferIndex;
						*(toPtr--) = *(fromPtr++);
						*(toPtr--) = *(fromPtr++);
						*(toPtr--) = *(fromPtr++);
						*toPtr = *fromPtr;
						value = val;
					}
				}
				bufferIndex += 4;
				return true;
			}
			bufferIndex = bufferEnd;
			value = 0f;
			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private unsafe bool UNSAFE_Read_8_Int64(out long value)
		{
			if (HasBufferData(8))
			{
				fixed (byte* basePtr = buffer)
				{
					if (BitConverter.IsLittleEndian)
					{
						if (ArchitectureInfo.Architecture_Supports_All_Unaligned_ReadWrites)
						{
							value = *(long*)(basePtr + bufferIndex);
						}
						else
						{
							long result = 0L;
							int* toPtr = (int*)(&result);
							int* fromPtr = (int*)(basePtr + bufferIndex);
							*(toPtr++) = *(fromPtr++);
							*toPtr = *fromPtr;
							value = result;
						}
					}
					else
					{
						long val = 0L;
						byte* toPtr2 = (byte*)(&val) + 7;
						byte* fromPtr2 = basePtr + bufferIndex;
						*(toPtr2--) = *(fromPtr2++);
						*(toPtr2--) = *(fromPtr2++);
						*(toPtr2--) = *(fromPtr2++);
						*(toPtr2--) = *(fromPtr2++);
						*(toPtr2--) = *(fromPtr2++);
						*(toPtr2--) = *(fromPtr2++);
						*(toPtr2--) = *(fromPtr2++);
						*toPtr2 = *fromPtr2;
						value = val;
					}
				}
				bufferIndex += 8;
				return true;
			}
			bufferIndex = bufferEnd;
			value = 0L;
			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private unsafe bool UNSAFE_Read_8_UInt64(out ulong value)
		{
			if (HasBufferData(8))
			{
				fixed (byte* basePtr = buffer)
				{
					if (BitConverter.IsLittleEndian)
					{
						if (ArchitectureInfo.Architecture_Supports_All_Unaligned_ReadWrites)
						{
							value = *(ulong*)(basePtr + bufferIndex);
						}
						else
						{
							ulong result = 0uL;
							int* toPtr = (int*)(&result);
							int* fromPtr = (int*)(basePtr + bufferIndex);
							*(toPtr++) = *(fromPtr++);
							*toPtr = *fromPtr;
							value = result;
						}
					}
					else
					{
						ulong val = 0uL;
						byte* toPtr2 = (byte*)(&val) + 7;
						byte* fromPtr2 = basePtr + bufferIndex;
						*(toPtr2--) = *(fromPtr2++);
						*(toPtr2--) = *(fromPtr2++);
						*(toPtr2--) = *(fromPtr2++);
						*(toPtr2--) = *(fromPtr2++);
						*(toPtr2--) = *(fromPtr2++);
						*(toPtr2--) = *(fromPtr2++);
						*(toPtr2--) = *(fromPtr2++);
						*toPtr2 = *fromPtr2;
						value = val;
					}
				}
				bufferIndex += 8;
				return true;
			}
			bufferIndex = bufferEnd;
			value = 0uL;
			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private unsafe bool UNSAFE_Read_8_Float64(out double value)
		{
			if (HasBufferData(8))
			{
				fixed (byte* basePtr = buffer)
				{
					if (BitConverter.IsLittleEndian)
					{
						if (ArchitectureInfo.Architecture_Supports_All_Unaligned_ReadWrites)
						{
							value = *(double*)(basePtr + bufferIndex);
						}
						else
						{
							double result = 0.0;
							int* toPtr = (int*)(&result);
							int* fromPtr = (int*)(basePtr + bufferIndex);
							*(toPtr++) = *(fromPtr++);
							*toPtr = *fromPtr;
							value = result;
						}
					}
					else
					{
						double val = 0.0;
						byte* toPtr2 = (byte*)(&val) + 7;
						byte* fromPtr2 = basePtr + bufferIndex;
						*(toPtr2--) = *(fromPtr2++);
						*(toPtr2--) = *(fromPtr2++);
						*(toPtr2--) = *(fromPtr2++);
						*(toPtr2--) = *(fromPtr2++);
						*(toPtr2--) = *(fromPtr2++);
						*(toPtr2--) = *(fromPtr2++);
						*(toPtr2--) = *(fromPtr2++);
						*toPtr2 = *fromPtr2;
						value = val;
					}
				}
				bufferIndex += 8;
				return true;
			}
			bufferIndex = bufferEnd;
			value = 0.0;
			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private unsafe bool UNSAFE_Read_16_Decimal(out decimal value)
		{
			if (HasBufferData(16))
			{
				fixed (byte* basePtr = buffer)
				{
					if (BitConverter.IsLittleEndian)
					{
						if (ArchitectureInfo.Architecture_Supports_All_Unaligned_ReadWrites)
						{
							value = *(decimal*)(basePtr + bufferIndex);
						}
						else
						{
							decimal result = default(decimal);
							int* toPtr = (int*)(&result);
							int* fromPtr = (int*)(basePtr + bufferIndex);
							*(toPtr++) = *(fromPtr++);
							*(toPtr++) = *(fromPtr++);
							*(toPtr++) = *(fromPtr++);
							*toPtr = *fromPtr;
							value = result;
						}
					}
					else
					{
						decimal val = default(decimal);
						byte* toPtr2 = (byte*)(&val) + 15;
						byte* fromPtr2 = basePtr + bufferIndex;
						*(toPtr2--) = *(fromPtr2++);
						*(toPtr2--) = *(fromPtr2++);
						*(toPtr2--) = *(fromPtr2++);
						*(toPtr2--) = *(fromPtr2++);
						*(toPtr2--) = *(fromPtr2++);
						*(toPtr2--) = *(fromPtr2++);
						*(toPtr2--) = *(fromPtr2++);
						*(toPtr2--) = *(fromPtr2++);
						*(toPtr2--) = *(fromPtr2++);
						*(toPtr2--) = *(fromPtr2++);
						*(toPtr2--) = *(fromPtr2++);
						*(toPtr2--) = *(fromPtr2++);
						*(toPtr2--) = *(fromPtr2++);
						*(toPtr2--) = *(fromPtr2++);
						*(toPtr2--) = *(fromPtr2++);
						*toPtr2 = *fromPtr2;
						value = val;
					}
				}
				bufferIndex += 16;
				return true;
			}
			bufferIndex = bufferEnd;
			value = default(decimal);
			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private unsafe bool UNSAFE_Read_16_Guid(out Guid value)
		{
			if (HasBufferData(16))
			{
				fixed (byte* basePtr = buffer)
				{
					if (BitConverter.IsLittleEndian)
					{
						if (ArchitectureInfo.Architecture_Supports_All_Unaligned_ReadWrites)
						{
							value = *(Guid*)(basePtr + bufferIndex);
						}
						else
						{
							Guid result = default(Guid);
							int* toPtr = (int*)(&result);
							int* fromPtr = (int*)(basePtr + bufferIndex);
							*(toPtr++) = *(fromPtr++);
							*(toPtr++) = *(fromPtr++);
							*(toPtr++) = *(fromPtr++);
							*toPtr = *fromPtr;
							value = result;
						}
					}
					else
					{
						Guid val = default(Guid);
						byte* toPtr2 = (byte*)(&val);
						byte* fromPtr2 = basePtr + bufferIndex;
						*(toPtr2++) = *(fromPtr2++);
						*(toPtr2++) = *(fromPtr2++);
						*(toPtr2++) = *(fromPtr2++);
						*(toPtr2++) = *(fromPtr2++);
						*(toPtr2++) = *(fromPtr2++);
						*(toPtr2++) = *(fromPtr2++);
						*(toPtr2++) = *(fromPtr2++);
						*(toPtr2++) = *(fromPtr2++);
						*(toPtr2++) = *(fromPtr2++);
						*toPtr2 = *(fromPtr2++);
						toPtr2 += 6;
						*(toPtr2--) = *(fromPtr2++);
						*(toPtr2--) = *(fromPtr2++);
						*(toPtr2--) = *(fromPtr2++);
						*(toPtr2--) = *(fromPtr2++);
						*(toPtr2--) = *(fromPtr2++);
						*toPtr2 = *fromPtr2;
						value = val;
					}
				}
				bufferIndex += 16;
				return true;
			}
			bufferIndex = bufferEnd;
			value = default(Guid);
			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private bool HasBufferData(int amount)
		{
			if (bufferEnd == 0)
			{
				ReadEntireStreamToBuffer();
			}
			return bufferIndex + amount <= bufferEnd;
		}

		private void ReadEntireStreamToBuffer()
		{
			bufferIndex = 0;
			if (Stream is MemoryStream)
			{
				try
				{
					buffer = (Stream as MemoryStream).GetBuffer();
					bufferEnd = (int)Stream.Length;
					bufferIndex = (int)Stream.Position;
					return;
				}
				catch (UnauthorizedAccessException)
				{
				}
			}
			buffer = internalBufferBackup;
			int remainder = (int)(Stream.Length - Stream.Position);
			if (buffer.Length >= remainder)
			{
				Stream.Read(buffer, 0, remainder);
			}
			else
			{
				buffer = new byte[remainder];
				Stream.Read(buffer, 0, remainder);
				if (remainder <= 10485760)
				{
					internalBufferBackup = buffer;
				}
			}
			bufferIndex = 0;
			bufferEnd = remainder;
		}
	}
}
