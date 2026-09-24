using System.Text;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal static class EncodingUtils
	{
		public static readonly MutableString ScratchStringBuffer = new MutableString(256);

		public static int GetByteCountUTF8(string str)
		{
			if (string.IsNullOrEmpty(str))
			{
				return 0;
			}
			ScratchStringBuffer.Clear();
			ScratchStringBuffer.Append(str);
			return Encoding.UTF8.GetByteCount(ScratchStringBuffer.Buffer, 0, ScratchStringBuffer.Length);
		}

		public static int GetBytesUTF8(string str, byte[] buffer, int offset)
		{
			if (string.IsNullOrEmpty(str))
			{
				return 0;
			}
			ScratchStringBuffer.Clear();
			ScratchStringBuffer.Append(str);
			return Encoding.UTF8.GetBytes(ScratchStringBuffer.Buffer, 0, ScratchStringBuffer.Length, buffer, offset);
		}

		public static ushort GetUInt16SafeByteCountUTF8(string str)
		{
			if (string.IsNullOrEmpty(str))
			{
				return 0;
			}
			int result = Encoding.UTF8.GetMaxByteCount(str.Length);
			if (result > 65535)
			{
				result = GetByteCountUTF8(str);
				if (result > 65535)
				{
					return 0;
				}
			}
			return (ushort)result;
		}
	}
}
