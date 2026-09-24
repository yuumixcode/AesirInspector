using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace Sirenix.Serialization
{
	/// <summary>
	/// Corresponds to the .NET <see cref="T:System.BitConverter" /> class, but works only with buffers and so never allocates garbage.
	/// <para />
	/// This class always writes and reads bytes in a little endian format, regardless of system architecture.
	/// </summary>
	public static class ProperBitConverter
	{
		[StructLayout(LayoutKind.Explicit)]
		private struct SingleByteUnion
		{
			[FieldOffset(0)]
			public byte Byte0;

			[FieldOffset(1)]
			public byte Byte1;

			[FieldOffset(2)]
			public byte Byte2;

			[FieldOffset(3)]
			public byte Byte3;

			[FieldOffset(0)]
			public float Value;
		}

		[StructLayout(LayoutKind.Explicit)]
		private struct DoubleByteUnion
		{
			[FieldOffset(0)]
			public byte Byte0;

			[FieldOffset(1)]
			public byte Byte1;

			[FieldOffset(2)]
			public byte Byte2;

			[FieldOffset(3)]
			public byte Byte3;

			[FieldOffset(4)]
			public byte Byte4;

			[FieldOffset(5)]
			public byte Byte5;

			[FieldOffset(6)]
			public byte Byte6;

			[FieldOffset(7)]
			public byte Byte7;

			[FieldOffset(0)]
			public double Value;
		}

		[StructLayout(LayoutKind.Explicit)]
		private struct DecimalByteUnion
		{
			[FieldOffset(0)]
			public byte Byte0;

			[FieldOffset(1)]
			public byte Byte1;

			[FieldOffset(2)]
			public byte Byte2;

			[FieldOffset(3)]
			public byte Byte3;

			[FieldOffset(4)]
			public byte Byte4;

			[FieldOffset(5)]
			public byte Byte5;

			[FieldOffset(6)]
			public byte Byte6;

			[FieldOffset(7)]
			public byte Byte7;

			[FieldOffset(8)]
			public byte Byte8;

			[FieldOffset(9)]
			public byte Byte9;

			[FieldOffset(10)]
			public byte Byte10;

			[FieldOffset(11)]
			public byte Byte11;

			[FieldOffset(12)]
			public byte Byte12;

			[FieldOffset(13)]
			public byte Byte13;

			[FieldOffset(14)]
			public byte Byte14;

			[FieldOffset(15)]
			public byte Byte15;

			[FieldOffset(0)]
			public decimal Value;
		}

		[StructLayout(LayoutKind.Explicit)]
		private struct GuidByteUnion
		{
			[FieldOffset(0)]
			public byte Byte0;

			[FieldOffset(1)]
			public byte Byte1;

			[FieldOffset(2)]
			public byte Byte2;

			[FieldOffset(3)]
			public byte Byte3;

			[FieldOffset(4)]
			public byte Byte4;

			[FieldOffset(5)]
			public byte Byte5;

			[FieldOffset(6)]
			public byte Byte6;

			[FieldOffset(7)]
			public byte Byte7;

			[FieldOffset(8)]
			public byte Byte8;

			[FieldOffset(9)]
			public byte Byte9;

			[FieldOffset(10)]
			public byte Byte10;

			[FieldOffset(11)]
			public byte Byte11;

			[FieldOffset(12)]
			public byte Byte12;

			[FieldOffset(13)]
			public byte Byte13;

			[FieldOffset(14)]
			public byte Byte14;

			[FieldOffset(15)]
			public byte Byte15;

			[FieldOffset(0)]
			public Guid Value;
		}

		private static readonly uint[] ByteToHexCharLookupLowerCase = CreateByteToHexLookup(upperCase: false);

		private static readonly uint[] ByteToHexCharLookupUpperCase = CreateByteToHexLookup(upperCase: true);

		private static readonly byte[] HexToByteLookup = new byte[256]
		{
			255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255, 0, 1,
			2, 3, 4, 5, 6, 7, 8, 9, 255, 255,
			255, 255, 255, 255, 255, 10, 11, 12, 13, 14,
			15, 255, 255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 10, 11, 12,
			13, 14, 15, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255
		};

		private static uint[] CreateByteToHexLookup(bool upperCase)
		{
			uint[] result = new uint[256];
			if (upperCase)
			{
				for (int i = 0; i < 256; i++)
				{
					string s = i.ToString("X2", CultureInfo.InvariantCulture);
					result[i] = s[0] + ((uint)s[1] << 16);
				}
			}
			else
			{
				for (int j = 0; j < 256; j++)
				{
					string s2 = j.ToString("x2", CultureInfo.InvariantCulture);
					result[j] = s2[0] + ((uint)s2[1] << 16);
				}
			}
			return result;
		}

		/// <summary>
		/// Converts a byte array into a hexadecimal string.
		/// </summary>
		public static string BytesToHexString(byte[] bytes, bool lowerCaseHexChars = true)
		{
			uint[] lookup = (lowerCaseHexChars ? ByteToHexCharLookupLowerCase : ByteToHexCharLookupUpperCase);
			char[] result = new char[bytes.Length * 2];
			for (int i = 0; i < bytes.Length; i++)
			{
				int offset = i * 2;
				uint val = lookup[bytes[i]];
				result[offset] = (char)val;
				result[offset + 1] = (char)(val >> 16);
			}
			return new string(result);
		}

		/// <summary>
		/// Converts a hexadecimal string into a byte array.
		/// </summary>
		public static byte[] HexStringToBytes(string hex)
		{
			int length = hex.Length;
			int rLength = length / 2;
			if (length % 2 != 0)
			{
				throw new ArgumentException("Hex string must have an even length.");
			}
			byte[] result = new byte[rLength];
			for (int i = 0; i < rLength; i++)
			{
				int offset = i * 2;
				byte b1;
				try
				{
					b1 = HexToByteLookup[(uint)hex[offset]];
					if (b1 == byte.MaxValue)
					{
						throw new ArgumentException("Expected a hex character, got '" + hex[offset] + "' at string index '" + offset + "'.");
					}
				}
				catch (IndexOutOfRangeException)
				{
					throw new ArgumentException("Expected a hex character, got '" + hex[offset] + "' at string index '" + offset + "'.");
				}
				byte b2;
				try
				{
					b2 = HexToByteLookup[(uint)hex[offset + 1]];
					if (b2 == byte.MaxValue)
					{
						throw new ArgumentException("Expected a hex character, got '" + hex[offset + 1] + "' at string index '" + (offset + 1) + "'.");
					}
				}
				catch (IndexOutOfRangeException)
				{
					throw new ArgumentException("Expected a hex character, got '" + hex[offset + 1] + "' at string index '" + (offset + 1) + "'.");
				}
				result[i] = (byte)((b1 << 4) | b2);
			}
			return result;
		}

		/// <summary>
		/// Reads two bytes from a buffer and converts them into a <see cref="T:System.Int16" /> value.
		/// </summary>
		/// <param name="buffer">The buffer to read from.</param>
		/// <param name="index">The index to start reading at.</param>
		/// <returns>The converted value.</returns>
		public static short ToInt16(byte[] buffer, int index)
		{
			short value = 0;
			value |= buffer[index + 1];
			value <<= 8;
			return (short)(value | buffer[index]);
		}

		/// <summary>
		/// Reads two bytes from a buffer and converts them into a <see cref="T:System.UInt16" /> value.
		/// </summary>
		/// <param name="buffer">The buffer to read from.</param>
		/// <param name="index">The index to start reading at.</param>
		/// <returns>The converted value.</returns>
		public static ushort ToUInt16(byte[] buffer, int index)
		{
			ushort value = 0;
			value |= buffer[index + 1];
			value <<= 8;
			return (ushort)(value | buffer[index]);
		}

		/// <summary>
		/// Reads four bytes from a buffer and converts them into an <see cref="T:System.Int32" /> value.
		/// </summary>
		/// <param name="buffer">The buffer to read from.</param>
		/// <param name="index">The index to start reading at.</param>
		/// <returns>The converted value.</returns>
		public static int ToInt32(byte[] buffer, int index)
		{
			int value = 0;
			value |= buffer[index + 3];
			value <<= 8;
			value |= buffer[index + 2];
			value <<= 8;
			value |= buffer[index + 1];
			value <<= 8;
			return value | buffer[index];
		}

		/// <summary>
		/// Reads four bytes from a buffer and converts them into an <see cref="T:System.UInt32" /> value.
		/// </summary>
		/// <param name="buffer">The buffer to read from.</param>
		/// <param name="index">The index to start reading at.</param>
		/// <returns>The converted value.</returns>
		public static uint ToUInt32(byte[] buffer, int index)
		{
			uint value = 0u;
			value |= buffer[index + 3];
			value <<= 8;
			value |= buffer[index + 2];
			value <<= 8;
			value |= buffer[index + 1];
			value <<= 8;
			return value | buffer[index];
		}

		/// <summary>
		/// Reads eight bytes from a buffer and converts them into a <see cref="T:System.Int64" /> value.
		/// </summary>
		/// <param name="buffer">The buffer to read from.</param>
		/// <param name="index">The index to start reading at.</param>
		/// <returns>The converted value.</returns>
		public static long ToInt64(byte[] buffer, int index)
		{
			long value = 0L;
			value |= buffer[index + 7];
			value <<= 8;
			value |= buffer[index + 6];
			value <<= 8;
			value |= buffer[index + 5];
			value <<= 8;
			value |= buffer[index + 4];
			value <<= 8;
			value |= buffer[index + 3];
			value <<= 8;
			value |= buffer[index + 2];
			value <<= 8;
			value |= buffer[index + 1];
			value <<= 8;
			return value | buffer[index];
		}

		/// <summary>
		/// Reads eight bytes from a buffer and converts them into an <see cref="T:System.UInt64" /> value.
		/// </summary>
		/// <param name="buffer">The buffer to read from.</param>
		/// <param name="index">The index to start reading at.</param>
		/// <returns>The converted value.</returns>
		public static ulong ToUInt64(byte[] buffer, int index)
		{
			ulong value = 0uL;
			value |= buffer[index + 7];
			value <<= 8;
			value |= buffer[index + 6];
			value <<= 8;
			value |= buffer[index + 5];
			value <<= 8;
			value |= buffer[index + 4];
			value <<= 8;
			value |= buffer[index + 3];
			value <<= 8;
			value |= buffer[index + 2];
			value <<= 8;
			value |= buffer[index + 1];
			value <<= 8;
			return value | buffer[index];
		}

		/// <summary>
		/// Reads four bytes from a buffer and converts them into an <see cref="T:System.Single" /> value.
		/// </summary>
		/// <param name="buffer">The buffer to read from.</param>
		/// <param name="index">The index to start reading at.</param>
		/// <returns>The converted value.</returns>
		public static float ToSingle(byte[] buffer, int index)
		{
			SingleByteUnion union = default(SingleByteUnion);
			if (BitConverter.IsLittleEndian)
			{
				union.Byte0 = buffer[index];
				union.Byte1 = buffer[index + 1];
				union.Byte2 = buffer[index + 2];
				union.Byte3 = buffer[index + 3];
			}
			else
			{
				union.Byte3 = buffer[index];
				union.Byte2 = buffer[index + 1];
				union.Byte1 = buffer[index + 2];
				union.Byte0 = buffer[index + 3];
			}
			return union.Value;
		}

		/// <summary>
		/// Reads eight bytes from a buffer and converts them into an <see cref="T:System.Double" /> value.
		/// </summary>
		/// <param name="buffer">The buffer to read from.</param>
		/// <param name="index">The index to start reading at.</param>
		/// <returns>The converted value.</returns>
		public static double ToDouble(byte[] buffer, int index)
		{
			DoubleByteUnion union = default(DoubleByteUnion);
			if (BitConverter.IsLittleEndian)
			{
				union.Byte0 = buffer[index];
				union.Byte1 = buffer[index + 1];
				union.Byte2 = buffer[index + 2];
				union.Byte3 = buffer[index + 3];
				union.Byte4 = buffer[index + 4];
				union.Byte5 = buffer[index + 5];
				union.Byte6 = buffer[index + 6];
				union.Byte7 = buffer[index + 7];
			}
			else
			{
				union.Byte7 = buffer[index];
				union.Byte6 = buffer[index + 1];
				union.Byte5 = buffer[index + 2];
				union.Byte4 = buffer[index + 3];
				union.Byte3 = buffer[index + 4];
				union.Byte2 = buffer[index + 5];
				union.Byte1 = buffer[index + 6];
				union.Byte0 = buffer[index + 7];
			}
			return union.Value;
		}

		/// <summary>
		/// Reads sixteen bytes from a buffer and converts them into a <see cref="T:System.Decimal" /> value.
		/// </summary>
		/// <param name="buffer">The buffer to read from.</param>
		/// <param name="index">The index to start reading at.</param>
		/// <returns>The converted value.</returns>
		public static decimal ToDecimal(byte[] buffer, int index)
		{
			DecimalByteUnion union = default(DecimalByteUnion);
			if (BitConverter.IsLittleEndian)
			{
				union.Byte0 = buffer[index];
				union.Byte1 = buffer[index + 1];
				union.Byte2 = buffer[index + 2];
				union.Byte3 = buffer[index + 3];
				union.Byte4 = buffer[index + 4];
				union.Byte5 = buffer[index + 5];
				union.Byte6 = buffer[index + 6];
				union.Byte7 = buffer[index + 7];
				union.Byte8 = buffer[index + 8];
				union.Byte9 = buffer[index + 9];
				union.Byte10 = buffer[index + 10];
				union.Byte11 = buffer[index + 11];
				union.Byte12 = buffer[index + 12];
				union.Byte13 = buffer[index + 13];
				union.Byte14 = buffer[index + 14];
				union.Byte15 = buffer[index + 15];
			}
			else
			{
				union.Byte15 = buffer[index];
				union.Byte14 = buffer[index + 1];
				union.Byte13 = buffer[index + 2];
				union.Byte12 = buffer[index + 3];
				union.Byte11 = buffer[index + 4];
				union.Byte10 = buffer[index + 5];
				union.Byte9 = buffer[index + 6];
				union.Byte8 = buffer[index + 7];
				union.Byte7 = buffer[index + 8];
				union.Byte6 = buffer[index + 9];
				union.Byte5 = buffer[index + 10];
				union.Byte4 = buffer[index + 11];
				union.Byte3 = buffer[index + 12];
				union.Byte2 = buffer[index + 13];
				union.Byte1 = buffer[index + 14];
				union.Byte0 = buffer[index + 15];
			}
			return union.Value;
		}

		/// <summary>
		/// Reads sixteen bytes from a buffer and converts them into a <see cref="T:System.Guid" /> value.
		/// </summary>
		/// <param name="buffer">The buffer to read from.</param>
		/// <param name="index">The index to start reading at.</param>
		/// <returns>The converted value.</returns>
		public static Guid ToGuid(byte[] buffer, int index)
		{
			GuidByteUnion union = new GuidByteUnion
			{
				Byte0 = buffer[index],
				Byte1 = buffer[index + 1],
				Byte2 = buffer[index + 2],
				Byte3 = buffer[index + 3],
				Byte4 = buffer[index + 4],
				Byte5 = buffer[index + 5],
				Byte6 = buffer[index + 6],
				Byte7 = buffer[index + 7],
				Byte8 = buffer[index + 8],
				Byte9 = buffer[index + 9]
			};
			if (BitConverter.IsLittleEndian)
			{
				union.Byte10 = buffer[index + 10];
				union.Byte11 = buffer[index + 11];
				union.Byte12 = buffer[index + 12];
				union.Byte13 = buffer[index + 13];
				union.Byte14 = buffer[index + 14];
				union.Byte15 = buffer[index + 15];
			}
			else
			{
				union.Byte15 = buffer[index + 10];
				union.Byte14 = buffer[index + 11];
				union.Byte13 = buffer[index + 12];
				union.Byte12 = buffer[index + 13];
				union.Byte11 = buffer[index + 14];
				union.Byte10 = buffer[index + 15];
			}
			return union.Value;
		}

		/// <summary>
		/// Turns a <see cref="T:System.Int16" /> value into two bytes and writes those bytes to a given buffer.
		/// </summary>
		/// <param name="buffer">The buffer to write to.</param>
		/// <param name="index">The index to start writing at.</param>
		/// <param name="value">The value to write.</param>
		public static void GetBytes(byte[] buffer, int index, short value)
		{
			if (BitConverter.IsLittleEndian)
			{
				buffer[index] = (byte)value;
				buffer[index + 1] = (byte)(value >> 8);
			}
			else
			{
				buffer[index] = (byte)(value >> 8);
				buffer[index + 1] = (byte)value;
			}
		}

		/// <summary>
		/// Turns an <see cref="T:System.UInt16" /> value into two bytes and writes those bytes to a given buffer.
		/// </summary>
		/// <param name="buffer">The buffer to write to.</param>
		/// <param name="index">The index to start writing at.</param>
		/// <param name="value">The value to write.</param>
		public static void GetBytes(byte[] buffer, int index, ushort value)
		{
			if (BitConverter.IsLittleEndian)
			{
				buffer[index] = (byte)value;
				buffer[index + 1] = (byte)(value >> 8);
			}
			else
			{
				buffer[index] = (byte)(value >> 8);
				buffer[index + 1] = (byte)value;
			}
		}

		/// <summary>
		/// Turns an <see cref="T:System.Int32" /> value into four bytes and writes those bytes to a given buffer.
		/// </summary>
		/// <param name="buffer">The buffer to write to.</param>
		/// <param name="index">The index to start writing at.</param>
		/// <param name="value">The value to write.</param>
		public static void GetBytes(byte[] buffer, int index, int value)
		{
			if (BitConverter.IsLittleEndian)
			{
				buffer[index] = (byte)value;
				buffer[index + 1] = (byte)(value >> 8);
				buffer[index + 2] = (byte)(value >> 16);
				buffer[index + 3] = (byte)(value >> 24);
			}
			else
			{
				buffer[index] = (byte)(value >> 24);
				buffer[index + 1] = (byte)(value >> 16);
				buffer[index + 2] = (byte)(value >> 8);
				buffer[index + 3] = (byte)value;
			}
		}

		/// <summary>
		/// Turns an <see cref="T:System.UInt32" /> value into four bytes and writes those bytes to a given buffer.
		/// </summary>
		/// <param name="buffer">The buffer to write to.</param>
		/// <param name="index">The index to start writing at.</param>
		/// <param name="value">The value to write.</param>
		public static void GetBytes(byte[] buffer, int index, uint value)
		{
			if (BitConverter.IsLittleEndian)
			{
				buffer[index] = (byte)value;
				buffer[index + 1] = (byte)(value >> 8);
				buffer[index + 2] = (byte)(value >> 16);
				buffer[index + 3] = (byte)(value >> 24);
			}
			else
			{
				buffer[index] = (byte)(value >> 24);
				buffer[index + 1] = (byte)(value >> 16);
				buffer[index + 2] = (byte)(value >> 8);
				buffer[index + 3] = (byte)value;
			}
		}

		/// <summary>
		/// Turns a <see cref="T:System.Int64" /> value into eight bytes and writes those bytes to a given buffer.
		/// </summary>
		/// <param name="buffer">The buffer to write to.</param>
		/// <param name="index">The index to start writing at.</param>
		/// <param name="value">The value to write.</param>
		public static void GetBytes(byte[] buffer, int index, long value)
		{
			if (BitConverter.IsLittleEndian)
			{
				buffer[index] = (byte)value;
				buffer[index + 1] = (byte)(value >> 8);
				buffer[index + 2] = (byte)(value >> 16);
				buffer[index + 3] = (byte)(value >> 24);
				buffer[index + 4] = (byte)(value >> 32);
				buffer[index + 5] = (byte)(value >> 40);
				buffer[index + 6] = (byte)(value >> 48);
				buffer[index + 7] = (byte)(value >> 56);
			}
			else
			{
				buffer[index] = (byte)(value >> 56);
				buffer[index + 1] = (byte)(value >> 48);
				buffer[index + 2] = (byte)(value >> 40);
				buffer[index + 3] = (byte)(value >> 32);
				buffer[index + 4] = (byte)(value >> 24);
				buffer[index + 5] = (byte)(value >> 16);
				buffer[index + 6] = (byte)(value >> 8);
				buffer[index + 7] = (byte)value;
			}
		}

		/// <summary>
		/// Turns an <see cref="T:System.UInt64" /> value into eight bytes and writes those bytes to a given buffer.
		/// </summary>
		/// <param name="buffer">The buffer to write to.</param>
		/// <param name="index">The index to start writing at.</param>
		/// <param name="value">The value to write.</param>
		public static void GetBytes(byte[] buffer, int index, ulong value)
		{
			if (BitConverter.IsLittleEndian)
			{
				buffer[index] = (byte)value;
				buffer[index + 1] = (byte)(value >> 8);
				buffer[index + 2] = (byte)(value >> 16);
				buffer[index + 3] = (byte)(value >> 24);
				buffer[index + 4] = (byte)(value >> 32);
				buffer[index + 5] = (byte)(value >> 40);
				buffer[index + 6] = (byte)(value >> 48);
				buffer[index + 7] = (byte)(value >> 56);
			}
			else
			{
				buffer[index] = (byte)(value >> 56);
				buffer[index + 1] = (byte)(value >> 48);
				buffer[index + 2] = (byte)(value >> 40);
				buffer[index + 3] = (byte)(value >> 32);
				buffer[index + 4] = (byte)(value >> 24);
				buffer[index + 5] = (byte)(value >> 16);
				buffer[index + 6] = (byte)(value >> 8);
				buffer[index + 7] = (byte)value;
			}
		}

		/// <summary>
		/// Turns a <see cref="T:System.Single" /> value into four bytes and writes those bytes to a given buffer.
		/// </summary>
		/// <param name="buffer">The buffer to write to.</param>
		/// <param name="index">The index to start writing at.</param>
		/// <param name="value">The value to write.</param>
		public static void GetBytes(byte[] buffer, int index, float value)
		{
			SingleByteUnion union = new SingleByteUnion
			{
				Value = value
			};
			if (BitConverter.IsLittleEndian)
			{
				buffer[index] = union.Byte0;
				buffer[index + 1] = union.Byte1;
				buffer[index + 2] = union.Byte2;
				buffer[index + 3] = union.Byte3;
			}
			else
			{
				buffer[index] = union.Byte3;
				buffer[index + 1] = union.Byte2;
				buffer[index + 2] = union.Byte1;
				buffer[index + 3] = union.Byte0;
			}
		}

		/// <summary>
		/// Turns a <see cref="T:System.Double" /> value into eight bytes and writes those bytes to a given buffer.
		/// </summary>
		/// <param name="buffer">The buffer to write to.</param>
		/// <param name="index">The index to start writing at.</param>
		/// <param name="value">The value to write.</param>
		public static void GetBytes(byte[] buffer, int index, double value)
		{
			DoubleByteUnion union = new DoubleByteUnion
			{
				Value = value
			};
			if (BitConverter.IsLittleEndian)
			{
				buffer[index] = union.Byte0;
				buffer[index + 1] = union.Byte1;
				buffer[index + 2] = union.Byte2;
				buffer[index + 3] = union.Byte3;
				buffer[index + 4] = union.Byte4;
				buffer[index + 5] = union.Byte5;
				buffer[index + 6] = union.Byte6;
				buffer[index + 7] = union.Byte7;
			}
			else
			{
				buffer[index] = union.Byte7;
				buffer[index + 1] = union.Byte6;
				buffer[index + 2] = union.Byte5;
				buffer[index + 3] = union.Byte4;
				buffer[index + 4] = union.Byte3;
				buffer[index + 5] = union.Byte2;
				buffer[index + 6] = union.Byte1;
				buffer[index + 7] = union.Byte0;
			}
		}

		/// <summary>
		/// Turns a <see cref="T:System.Decimal" /> value into sixteen bytes and writes those bytes to a given buffer.
		/// </summary>
		/// <param name="buffer">The buffer to write to.</param>
		/// <param name="index">The index to start writing at.</param>
		/// <param name="value">The value to write.</param>
		public static void GetBytes(byte[] buffer, int index, decimal value)
		{
			DecimalByteUnion union = new DecimalByteUnion
			{
				Value = value
			};
			if (BitConverter.IsLittleEndian)
			{
				buffer[index] = union.Byte0;
				buffer[index + 1] = union.Byte1;
				buffer[index + 2] = union.Byte2;
				buffer[index + 3] = union.Byte3;
				buffer[index + 4] = union.Byte4;
				buffer[index + 5] = union.Byte5;
				buffer[index + 6] = union.Byte6;
				buffer[index + 7] = union.Byte7;
				buffer[index + 8] = union.Byte8;
				buffer[index + 9] = union.Byte9;
				buffer[index + 10] = union.Byte10;
				buffer[index + 11] = union.Byte11;
				buffer[index + 12] = union.Byte12;
				buffer[index + 13] = union.Byte13;
				buffer[index + 14] = union.Byte14;
				buffer[index + 15] = union.Byte15;
			}
			else
			{
				buffer[index] = union.Byte15;
				buffer[index + 1] = union.Byte14;
				buffer[index + 2] = union.Byte13;
				buffer[index + 3] = union.Byte12;
				buffer[index + 4] = union.Byte11;
				buffer[index + 5] = union.Byte10;
				buffer[index + 6] = union.Byte9;
				buffer[index + 7] = union.Byte8;
				buffer[index + 8] = union.Byte7;
				buffer[index + 9] = union.Byte6;
				buffer[index + 10] = union.Byte5;
				buffer[index + 11] = union.Byte4;
				buffer[index + 12] = union.Byte3;
				buffer[index + 13] = union.Byte2;
				buffer[index + 14] = union.Byte1;
				buffer[index + 15] = union.Byte0;
			}
		}

		/// <summary>
		/// Turns a <see cref="T:System.Guid" /> value into sixteen bytes and writes those bytes to a given buffer.
		/// </summary>
		/// <param name="buffer">The buffer to write to.</param>
		/// <param name="index">The index to start writing at.</param>
		/// <param name="value">The value to write.</param>
		public static void GetBytes(byte[] buffer, int index, Guid value)
		{
			GuidByteUnion union = new GuidByteUnion
			{
				Value = value
			};
			buffer[index] = union.Byte0;
			buffer[index + 1] = union.Byte1;
			buffer[index + 2] = union.Byte2;
			buffer[index + 3] = union.Byte3;
			buffer[index + 4] = union.Byte4;
			buffer[index + 5] = union.Byte5;
			buffer[index + 6] = union.Byte6;
			buffer[index + 7] = union.Byte7;
			buffer[index + 8] = union.Byte8;
			buffer[index + 9] = union.Byte9;
			if (BitConverter.IsLittleEndian)
			{
				buffer[index + 10] = union.Byte10;
				buffer[index + 11] = union.Byte11;
				buffer[index + 12] = union.Byte12;
				buffer[index + 13] = union.Byte13;
				buffer[index + 14] = union.Byte14;
				buffer[index + 15] = union.Byte15;
			}
			else
			{
				buffer[index + 10] = union.Byte15;
				buffer[index + 11] = union.Byte14;
				buffer[index + 12] = union.Byte13;
				buffer[index + 13] = union.Byte12;
				buffer[index + 14] = union.Byte11;
				buffer[index + 15] = union.Byte10;
			}
		}
	}
}
