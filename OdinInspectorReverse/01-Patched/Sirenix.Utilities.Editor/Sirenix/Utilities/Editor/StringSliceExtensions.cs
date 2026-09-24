using System.Text;

namespace Sirenix.Utilities.Editor
{
	public static class StringSliceExtensions
	{
		public static void Append(this StringBuilder sb, ref StringSlice slice)
		{
			sb.Append(slice.String, slice.Index, slice.Length);
		}

		public static StringSlice Slice(this string str, int index, int length)
		{
			return new StringSlice(str, index, length);
		}

		public static StringSlice Slice(this string str, int index)
		{
			return new StringSlice(str, index, str.Length - index);
		}

		public static StringSlice Slice(this string str)
		{
			return new StringSlice(str, 0, str.Length);
		}

		public static bool StartsWith(this string str, ref StringSlice slice)
		{
			int length = slice.Length;
			if (str.Length < length)
			{
				return false;
			}
			for (int i = 0; i < length; i++)
			{
				if (str[i] != slice[i])
				{
					return false;
				}
			}
			return true;
		}

		public static bool EndsWith(this string str, ref StringSlice slice)
		{
			int length = slice.Length;
			if (str.Length < length)
			{
				return false;
			}
			int i = 0;
			int j = length - 1;
			while (i < length)
			{
				if (str[j] != slice[i])
				{
					return false;
				}
				i++;
				j--;
			}
			return true;
		}

		public static bool Contains(this string str, ref StringSlice slice)
		{
			int fsLength = slice.Length;
			int length = str.Length;
			if (length < fsLength)
			{
				return false;
			}
			for (int i = 0; i < length; i++)
			{
				if (str[i] != slice[0])
				{
					continue;
				}
				bool isMatch = true;
				int fsIndex = 1;
				int thisIndex = i + 1;
				while (fsIndex < fsLength && thisIndex < length)
				{
					if (str[thisIndex] != slice[fsIndex])
					{
						isMatch = false;
						break;
					}
					fsIndex++;
					thisIndex++;
				}
				if (isMatch)
				{
					return true;
				}
			}
			return false;
		}
	}
}
