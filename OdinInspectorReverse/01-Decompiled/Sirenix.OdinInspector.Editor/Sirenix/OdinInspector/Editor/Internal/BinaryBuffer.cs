using System;
using System.Text;

namespace Sirenix.OdinInspector.Editor.Internal
{
	[Serializable]
	internal class BinaryBuffer
	{
		public int ReadOffset;

		public int Length;

		public byte[] Buffer;

		public int Capacity => Buffer.Length;

		public BinaryBuffer(int capacity)
		{
			ReadOffset = 0;
			Length = 0;
			Buffer = ArrayUtils.CreateOrEmpty<byte>(capacity);
		}

		public void Clear()
		{
			Length = 0;
		}

		public ulong CalcChecksum()
		{
			return CalcChecksum(0, Length);
		}

		public ulong CalcChecksum(int offset, int length)
		{
			ulong hash = 14695981039346656037uL;
			for (int i = offset; i < length; i++)
			{
				hash = (hash ^ Buffer[i]) * 1099511628211L;
			}
			return hash;
		}

		public void Write(byte[] bytes)
		{
			if (bytes != null && bytes.Length != 0)
			{
				EnsureCapacityFor(bytes.Length);
				System.Buffer.BlockCopy(bytes, 0, Buffer, Length, bytes.Length);
				Length += bytes.Length;
			}
		}

		public void RemoveRead()
		{
			if (ReadOffset != 0)
			{
				System.Buffer.BlockCopy(Buffer, ReadOffset, Buffer, 0, Length - ReadOffset);
				Length -= ReadOffset;
				ReadOffset = 0;
			}
		}

		public void WriteStringUTF8UInt16(string str)
		{
			ushort safeSize = EncodingUtils.GetUInt16SafeByteCountUTF8(str);
			if (safeSize == 0)
			{
				WriteUInt16(0);
				return;
			}
			EnsureCapacityFor(2 + safeSize);
			int writtenSize = EncodingUtils.GetBytesUTF8(str, Buffer, Length + 2);
			WriteUInt16((ushort)writtenSize);
			Length += writtenSize;
		}

		public bool TryReadStringUTF8UInt16(out string result)
		{
			if (!TryReadUInt16(out var size))
			{
				result = null;
				return false;
			}
			if (ReadOffset + size > Capacity)
			{
				result = null;
				return false;
			}
			result = ((size > 0) ? Encoding.UTF8.GetString(Buffer, ReadOffset, size) : string.Empty);
			ReadOffset += size;
			return true;
		}

		public void WriteBool(bool value)
		{
			EnsureCapacityFor(1);
			Buffer[Length++] = (value ? ((byte)1) : ((byte)0));
		}

		public bool TryReadBool(out bool value)
		{
			if (ReadOffset + 1 > Capacity)
			{
				value = false;
				return false;
			}
			value = Buffer[ReadOffset++] != 0;
			return true;
		}

		public void WriteInt16(short value)
		{
			EnsureCapacityFor(2);
			Buffer[Length++] = (byte)value;
			Buffer[Length++] = (byte)(value >> 8);
		}

		public void WriteUInt16(ushort value)
		{
			WriteInt16((short)value);
		}

		public void WriteInt32(int value)
		{
			EnsureCapacityFor(4);
			Buffer[Length++] = (byte)value;
			Buffer[Length++] = (byte)(value >> 8);
			Buffer[Length++] = (byte)(value >> 16);
			Buffer[Length++] = (byte)(value >> 24);
		}

		public void WriteUInt32(uint value)
		{
			WriteInt32((int)value);
		}

		public void WriteInt64(long value)
		{
			EnsureCapacityFor(8);
			Buffer[Length++] = (byte)value;
			Buffer[Length++] = (byte)(value >> 8);
			Buffer[Length++] = (byte)(value >> 16);
			Buffer[Length++] = (byte)(value >> 24);
			Buffer[Length++] = (byte)(value >> 32);
			Buffer[Length++] = (byte)(value >> 40);
			Buffer[Length++] = (byte)(value >> 48);
			Buffer[Length++] = (byte)(value >> 56);
		}

		public bool TryReadInt16(out short result)
		{
			if (ReadOffset + 2 > Capacity)
			{
				result = 0;
				return false;
			}
			result = (short)(Buffer[ReadOffset++] | (Buffer[ReadOffset++] << 8));
			return true;
		}

		public bool TryReadUInt16(out ushort result)
		{
			short resultSigned;
			bool couldRead = TryReadInt16(out resultSigned);
			result = (ushort)resultSigned;
			return couldRead;
		}

		public bool TryReadInt32(out int result)
		{
			if (ReadOffset + 4 > Capacity)
			{
				result = 0;
				return false;
			}
			result = Buffer[ReadOffset++] | (Buffer[ReadOffset++] << 8) | (Buffer[ReadOffset++] << 16) | (Buffer[ReadOffset++] << 24);
			return true;
		}

		public bool TryReadUInt32(out uint result)
		{
			int resultSigned;
			bool couldRead = TryReadInt32(out resultSigned);
			result = (uint)resultSigned;
			return couldRead;
		}

		public bool TryReadInt64(out long result)
		{
			if (ReadOffset + 8 > Capacity)
			{
				result = 0L;
				return false;
			}
			result = (long)(Buffer[ReadOffset++] | ((ulong)Buffer[ReadOffset++] << 8) | ((ulong)Buffer[ReadOffset++] << 16) | ((ulong)Buffer[ReadOffset++] << 24) | ((ulong)Buffer[ReadOffset++] << 32) | ((ulong)Buffer[ReadOffset++] << 40) | ((ulong)Buffer[ReadOffset++] << 48) | ((ulong)Buffer[ReadOffset++] << 56));
			return true;
		}

		public bool TryReadUInt64(out ulong result)
		{
			long resultSigned;
			bool couldRead = TryReadInt64(out resultSigned);
			result = (ulong)resultSigned;
			return couldRead;
		}

		public void WriteGuid(string guid)
		{
			byte[] bytes = GuidUtils.GetBytesTemp(guid);
			EnsureCapacityFor(16);
			Array.Copy(bytes, 0, Buffer, Length, 16);
			Length += 16;
		}

		public bool TryReadGuid(out string result)
		{
			if (!GuidUtils.TryGetFromBytes(Buffer, ReadOffset, out var guid))
			{
				result = null;
				return false;
			}
			result = guid.ToString("N");
			ReadOffset += 16;
			return true;
		}

		public void EnsureCapacityFor(int amount)
		{
			ArrayUtils.ResizeIfNeeded(ref Buffer, Length + amount);
		}
	}
}
