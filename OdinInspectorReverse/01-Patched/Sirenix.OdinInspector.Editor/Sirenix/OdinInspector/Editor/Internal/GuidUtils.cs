using System;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal static class GuidUtils
	{
		internal static readonly byte[] ScratchGuidBuffer = new byte[16];

		public static byte[] GetBytesTemp(string guidStr)
		{
			if (string.IsNullOrEmpty(guidStr) || guidStr.Length != 32)
			{
				Array.Clear(ScratchGuidBuffer, 0, ScratchGuidBuffer.Length);
				return ScratchGuidBuffer;
			}
			ScratchGuidBuffer[0] = (byte)((HexCharToByte(guidStr[6]) << 4) | HexCharToByte(guidStr[7]));
			ScratchGuidBuffer[1] = (byte)((HexCharToByte(guidStr[4]) << 4) | HexCharToByte(guidStr[5]));
			ScratchGuidBuffer[2] = (byte)((HexCharToByte(guidStr[2]) << 4) | HexCharToByte(guidStr[3]));
			ScratchGuidBuffer[3] = (byte)((HexCharToByte(guidStr[0]) << 4) | HexCharToByte(guidStr[1]));
			ScratchGuidBuffer[4] = (byte)((HexCharToByte(guidStr[10]) << 4) | HexCharToByte(guidStr[11]));
			ScratchGuidBuffer[5] = (byte)((HexCharToByte(guidStr[8]) << 4) | HexCharToByte(guidStr[9]));
			ScratchGuidBuffer[6] = (byte)((HexCharToByte(guidStr[14]) << 4) | HexCharToByte(guidStr[15]));
			ScratchGuidBuffer[7] = (byte)((HexCharToByte(guidStr[12]) << 4) | HexCharToByte(guidStr[13]));
			ScratchGuidBuffer[8] = (byte)((HexCharToByte(guidStr[16]) << 4) | HexCharToByte(guidStr[17]));
			ScratchGuidBuffer[9] = (byte)((HexCharToByte(guidStr[18]) << 4) | HexCharToByte(guidStr[19]));
			ScratchGuidBuffer[10] = (byte)((HexCharToByte(guidStr[20]) << 4) | HexCharToByte(guidStr[21]));
			ScratchGuidBuffer[11] = (byte)((HexCharToByte(guidStr[22]) << 4) | HexCharToByte(guidStr[23]));
			ScratchGuidBuffer[12] = (byte)((HexCharToByte(guidStr[24]) << 4) | HexCharToByte(guidStr[25]));
			ScratchGuidBuffer[13] = (byte)((HexCharToByte(guidStr[26]) << 4) | HexCharToByte(guidStr[27]));
			ScratchGuidBuffer[14] = (byte)((HexCharToByte(guidStr[28]) << 4) | HexCharToByte(guidStr[29]));
			ScratchGuidBuffer[15] = (byte)((HexCharToByte(guidStr[30]) << 4) | HexCharToByte(guidStr[31]));
			return ScratchGuidBuffer;
		}

		internal static byte HexCharToByte(char c)
		{
			int num;
			if (c < '0' || c > '9')
			{
				if (c < 'a' || c > 'f')
				{
					if (c < 'A' || c > 'F')
					{
						throw new FormatException($"Invalid char: '{c}'.");
					}
					num = c - 65 + 10;
				}
				else
				{
					num = c - 97 + 10;
				}
			}
			else
			{
				num = c - 48;
			}
			return (byte)num;
		}

		public static bool TryGetFromBytes(byte[] bytes, int offset, out Guid result)
		{
			if (offset + 16 > bytes.Length)
			{
				result = default(Guid);
				return false;
			}
			Array.Copy(bytes, offset, ScratchGuidBuffer, 0, 16);
			result = new Guid(ScratchGuidBuffer);
			return true;
		}
	}
}
