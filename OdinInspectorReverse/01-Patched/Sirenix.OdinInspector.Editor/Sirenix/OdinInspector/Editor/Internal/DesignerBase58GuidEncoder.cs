using System;
using System.Text;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal static class DesignerBase58GuidEncoder
	{
		public const string ALPHABET = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";

		public static readonly StringBuilder Buffer = new StringBuilder(24);

		public static readonly byte[] GuidBuffer = new byte[16];

		public static string Encode(Guid guid)
		{
			Buffer.Clear();
			byte[] bytes = guid.ToByteArray();
			int leadingZeroLength = 0;
			for (int i = 0; i < bytes.Length; i++)
			{
				if (bytes[i] != 0)
				{
					leadingZeroLength = i;
					break;
				}
				Buffer.Append("123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz"[0]);
			}
			while (!IsZero(bytes))
			{
				int remainder = DivideRemainder(bytes);
				Buffer.Append("123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz"[remainder]);
			}
			int start = leadingZeroLength;
			int end = Buffer.Length - 1;
			while (start < end)
			{
				StringBuilder buffer = Buffer;
				int index = start;
				StringBuilder buffer2 = Buffer;
				int index2 = end;
				char value = Buffer[end];
				char value2 = Buffer[start];
				buffer[index] = value;
				buffer2[index2] = value2;
				start++;
				end--;
			}
			return Buffer.ToString();
		}

		public static Guid Decode(string base58)
		{
			Array.Clear(GuidBuffer, 0, GuidBuffer.Length);
			foreach (char c in base58)
			{
				int alphabetIndex = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz".IndexOf(c);
				if (alphabetIndex < 0)
				{
					throw new FormatException("Invalid Base58 character");
				}
				int carry = alphabetIndex;
				for (int i2 = GuidBuffer.Length - 1; i2 >= 0; i2--)
				{
					int total = GuidBuffer[i2] * 58 + carry;
					GuidBuffer[i2] = (byte)(total & 0xFF);
					carry = total >> 8;
				}
			}
			return new Guid(GuidBuffer);
		}

		public static int DivideRemainder(byte[] bytes)
		{
			int remainder = 0;
			for (int i = 0; i < bytes.Length; i++)
			{
				int value = bytes[i] + remainder * 256;
				bytes[i] = (byte)(value / 58);
				remainder = value % 58;
			}
			return remainder;
		}

		public static bool IsZero(byte[] bytes)
		{
			for (int i = 0; i < bytes.Length; i++)
			{
				if (bytes[i] != 0)
				{
					return false;
				}
			}
			return true;
		}
	}
}
