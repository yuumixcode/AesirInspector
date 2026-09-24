using System;
using System.Runtime.InteropServices;

namespace Sirenix.Serialization.Utilities.Unsafe
{
	/// <summary>
	/// Contains utilities for performing common unsafe operations.
	/// </summary>
	internal static class UnsafeUtilities
	{
		private struct Struct256Bit
		{
			public decimal d1;

			public decimal d2;
		}

		/// <summary>
		/// Blindly creates an array of structs from an array of bytes via direct memory copy/blit.
		/// </summary>
		public static T[] StructArrayFromBytes<T>(byte[] bytes, int byteLength) where T : struct
		{
			return StructArrayFromBytes<T>(bytes, 0, 0);
		}

		/// <summary>
		/// Blindly creates an array of structs from an array of bytes via direct memory copy/blit.
		/// </summary>
		public static T[] StructArrayFromBytes<T>(byte[] bytes, int byteLength, int byteOffset) where T : struct
		{
			if (bytes == null)
			{
				throw new ArgumentNullException("bytes");
			}
			if (byteLength <= 0)
			{
				throw new ArgumentException("Byte length must be larger than zero.");
			}
			if (byteOffset < 0)
			{
				throw new ArgumentException("Byte offset must be larger than or equal to zero.");
			}
			int typeSize = Marshal.SizeOf(typeof(T));
			if (byteOffset % 8 != 0)
			{
				throw new ArgumentException("Byte offset must be divisible by " + 8 + " (IE, sizeof(ulong))");
			}
			if (byteLength + byteOffset >= bytes.Length)
			{
				throw new ArgumentException("Given byte array of size " + bytes.Length + " is not large enough to copy requested number of bytes " + byteLength + ".");
			}
			if ((byteLength - byteOffset) % typeSize != 0)
			{
				throw new ArgumentException("The length in the given byte array (" + bytes.Length + ", and " + (bytes.Length - byteOffset) + " minus byteOffset " + byteOffset + ") to convert to type " + typeof(T).Name + " is not divisible by the size of " + typeof(T).Name + " (" + typeSize + ").");
			}
			int elementCount = (bytes.Length - byteOffset) / typeSize;
			T[] array = new T[elementCount];
			MemoryCopy(bytes, array, byteLength, byteOffset, 0);
			return array;
		}

		/// <summary>
		/// Blindly copies an array of structs into an array of bytes via direct memory copy/blit.
		/// </summary>
		public static byte[] StructArrayToBytes<T>(T[] array) where T : struct
		{
			byte[] bytes = null;
			return StructArrayToBytes(array, ref bytes, 0);
		}

		/// <summary>
		/// Blindly copies an array of structs into an array of bytes via direct memory copy/blit.
		/// </summary>
		public static byte[] StructArrayToBytes<T>(T[] array, ref byte[] bytes, int byteOffset) where T : struct
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			if (byteOffset < 0)
			{
				throw new ArgumentException("Byte offset must be larger than or equal to zero.");
			}
			int typeSize = Marshal.SizeOf(typeof(T));
			int byteCount = typeSize * array.Length;
			if (bytes == null)
			{
				bytes = new byte[byteCount + byteOffset];
			}
			else if (bytes.Length + byteOffset > byteCount)
			{
				throw new ArgumentException("Byte array must be at least " + (bytes.Length + byteOffset) + " long with the given byteOffset.");
			}
			MemoryCopy(array, bytes, byteCount, 0, byteOffset);
			return bytes;
		}

		/// <summary>
		/// Creates a new string from the contents of a given byte buffer.
		/// </summary>
		public unsafe static string StringFromBytes(byte[] buffer, int charLength, bool needs16BitSupport)
		{
			int byteCount = (needs16BitSupport ? (charLength * 2) : charLength);
			if (buffer.Length < byteCount)
			{
				throw new ArgumentException("Buffer is not large enough to contain the given string; a size of at least " + byteCount + " is required.");
			}
			GCHandle toHandle = default(GCHandle);
			string result = new string(' ', charLength);
			try
			{
				toHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
				if (needs16BitSupport)
				{
					if (BitConverter.IsLittleEndian)
					{
						fixed (char* charPtr1 = result)
						{
							ushort* fromPtr1 = (ushort*)toHandle.AddrOfPinnedObject().ToPointer();
							ushort* toPtr1 = (ushort*)charPtr1;
							for (int i = 0; i < byteCount; i += 2)
							{
								*(toPtr1++) = *(fromPtr1++);
							}
						}
					}
					else
					{
						fixed (char* charPtr2 = result)
						{
							byte* fromPtr2 = (byte*)toHandle.AddrOfPinnedObject().ToPointer();
							byte* toPtr2 = (byte*)charPtr2;
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
				else if (BitConverter.IsLittleEndian)
				{
					fixed (char* charPtr3 = result)
					{
						byte* fromPtr3 = (byte*)toHandle.AddrOfPinnedObject().ToPointer();
						byte* toPtr3 = (byte*)charPtr3;
						for (int k = 0; k < byteCount; k++)
						{
							*(toPtr3++) = *(fromPtr3++);
							toPtr3++;
						}
					}
				}
				else
				{
					fixed (char* charPtr4 = result)
					{
						byte* fromPtr4 = (byte*)toHandle.AddrOfPinnedObject().ToPointer();
						byte* toPtr4 = (byte*)charPtr4;
						for (int l = 0; l < byteCount; l++)
						{
							toPtr4++;
							*(toPtr4++) = *(fromPtr4++);
						}
					}
				}
			}
			finally
			{
				if (toHandle.IsAllocated)
				{
					toHandle.Free();
				}
			}
			return result;
		}

		/// <summary>
		/// Writes the contents of a string into a given byte buffer.
		/// </summary>
		public unsafe static int StringToBytes(byte[] buffer, string value, bool needs16BitSupport)
		{
			int byteCount = (needs16BitSupport ? (value.Length * 2) : value.Length);
			if (buffer.Length < byteCount)
			{
				throw new ArgumentException("Buffer is not large enough to contain the given string; a size of at least " + byteCount + " is required.");
			}
			GCHandle toHandle = default(GCHandle);
			try
			{
				toHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
				if (needs16BitSupport)
				{
					if (BitConverter.IsLittleEndian)
					{
						fixed (char* charPtr1 = value)
						{
							ushort* fromPtr1 = (ushort*)charPtr1;
							ushort* toPtr1 = (ushort*)toHandle.AddrOfPinnedObject().ToPointer();
							for (int i = 0; i < byteCount; i += 2)
							{
								*(toPtr1++) = *(fromPtr1++);
							}
						}
					}
					else
					{
						fixed (char* charPtr2 = value)
						{
							byte* fromPtr2 = (byte*)charPtr2;
							byte* toPtr2 = (byte*)toHandle.AddrOfPinnedObject().ToPointer();
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
				else if (BitConverter.IsLittleEndian)
				{
					fixed (char* charPtr3 = value)
					{
						byte* fromPtr3 = (byte*)charPtr3;
						byte* toPtr3 = (byte*)toHandle.AddrOfPinnedObject().ToPointer();
						for (int k = 0; k < byteCount; k++)
						{
							fromPtr3++;
							*(toPtr3++) = *(fromPtr3++);
						}
					}
				}
				else
				{
					fixed (char* charPtr4 = value)
					{
						byte* fromPtr4 = (byte*)charPtr4;
						byte* toPtr4 = (byte*)toHandle.AddrOfPinnedObject().ToPointer();
						for (int l = 0; l < byteCount; l++)
						{
							*(toPtr4++) = *(fromPtr4++);
							fromPtr4++;
						}
					}
				}
			}
			finally
			{
				if (toHandle.IsAllocated)
				{
					toHandle.Free();
				}
			}
			return byteCount;
		}

		public unsafe static void MemoryCopy(void* from, void* to, int bytes)
		{
			byte* end = (byte*)to + bytes;
			Struct256Bit* fromBigPtr = (Struct256Bit*)from;
			Struct256Bit* toBigPtr = (Struct256Bit*)to;
			while (toBigPtr + 1 <= end)
			{
				*(toBigPtr++) = *(fromBigPtr++);
			}
			byte* fromSmallPtr = (byte*)fromBigPtr;
			byte* toSmallPtr = (byte*)toBigPtr;
			while (toSmallPtr < end)
			{
				*(toSmallPtr++) = *(fromSmallPtr++);
			}
		}

		/// <summary>
		/// Blindly mem-copies a given number of bytes from the memory location of one object to another. WARNING: This method is ridiculously dangerous. Only use if you know what you're doing.
		/// </summary>
		public unsafe static void MemoryCopy(object from, object to, int byteCount, int fromByteOffset, int toByteOffset)
		{
			GCHandle fromHandle = default(GCHandle);
			GCHandle toHandle = default(GCHandle);
			if (fromByteOffset % 8 != 0 || toByteOffset % 8 != 0)
			{
				throw new ArgumentException("Byte offset must be divisible by " + 8 + " (IE, sizeof(ulong))");
			}
			try
			{
				int restBytes = byteCount % 8;
				int ulongCount = (byteCount - restBytes) / 8;
				int fromOffsetCount = fromByteOffset / 8;
				int toOffsetCount = toByteOffset / 8;
				fromHandle = GCHandle.Alloc(from, GCHandleType.Pinned);
				toHandle = GCHandle.Alloc(to, GCHandleType.Pinned);
				ulong* fromUlongPtr = (ulong*)fromHandle.AddrOfPinnedObject().ToPointer();
				ulong* toUlongPtr = (ulong*)toHandle.AddrOfPinnedObject().ToPointer();
				if (fromOffsetCount > 0)
				{
					fromUlongPtr += fromOffsetCount;
				}
				if (toOffsetCount > 0)
				{
					toUlongPtr += toOffsetCount;
				}
				for (int i = 0; i < ulongCount; i++)
				{
					*(toUlongPtr++) = *(fromUlongPtr++);
				}
				if (restBytes > 0)
				{
					byte* fromBytePtr = (byte*)fromUlongPtr;
					byte* toBytePtr = (byte*)toUlongPtr;
					for (int j = 0; j < restBytes; j++)
					{
						*(toBytePtr++) = *(fromBytePtr++);
					}
				}
			}
			finally
			{
				if (fromHandle.IsAllocated)
				{
					fromHandle.Free();
				}
				if (toHandle.IsAllocated)
				{
					toHandle.Free();
				}
			}
		}
	}
}
