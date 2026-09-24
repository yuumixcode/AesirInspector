using System;

namespace Sirenix.Utilities.Editor
{
	public struct StringSlice : IEquatable<string>, IEquatable<StringSlice>
	{
		public struct PreHashed : IEquatable<PreHashed>
		{
			public StringSlice Slice;

			public int Hash;

			public PreHashed(StringSlice slice)
			{
				Slice = slice;
				Hash = Slice.GetHashCode();
			}

			public static implicit operator PreHashed(string str)
			{
				return new PreHashed(new StringSlice(str));
			}

			public static implicit operator PreHashed(StringSlice slice)
			{
				return new PreHashed(slice);
			}

			public bool Equals(PreHashed other)
			{
				return other.Slice == Slice;
			}

			public override int GetHashCode()
			{
				if (Hash == 0)
				{
					Hash = Slice.GetHashCode();
				}
				return Hash;
			}
		}

		public string String;

		public int Index;

		public int Length;

		public char this[int index]
		{
			get
			{
				if (index < 0 || index >= Length)
				{
					throw new IndexOutOfRangeException();
				}
				return String[Index + index];
			}
		}

		public StringSlice(string str)
		{
			String = str;
			Index = 0;
			Length = str.Length;
		}

		public StringSlice(string str, int index, int length)
		{
			int strLength = str.Length;
			if (index < 0 || length < 0 || index + length > strLength)
			{
				throw new IndexOutOfRangeException();
			}
			String = str;
			Index = index;
			Length = length;
		}

		public static implicit operator StringSlice(string str)
		{
			return new StringSlice
			{
				String = str,
				Index = 0,
				Length = str.Length
			};
		}

		public static explicit operator string(StringSlice slice)
		{
			return slice.ToString();
		}

		public override string ToString()
		{
			if (String == null)
			{
				return string.Empty;
			}
			if (Index == 0 && Length == String.Length)
			{
				return String;
			}
			return String.Substring(Index, Length);
		}

		public unsafe override int GetHashCode()
		{
			if (String == null)
			{
				return 0;
			}
			int length = Length;
			fixed (char* basePtr = String)
			{
				char* ptr = basePtr + Index;
				int num = 352654597;
				int num2 = num;
				int* ptr2 = (int*)ptr;
				for (int index = length; index > 0; index -= 4)
				{
					if (index <= 1)
					{
						num = ((num << 5) + num + (num >> 27)) ^ *(ushort*)ptr2;
						break;
					}
					num = ((num << 5) + num + (num >> 27)) ^ *ptr2;
					if (index <= 2)
					{
						break;
					}
					if (index <= 3)
					{
						num2 = ((num2 << 5) + num2 + (num2 >> 27)) ^ ((ushort*)ptr2)[2];
						break;
					}
					num2 = ((num2 << 5) + num2 + (num2 >> 27)) ^ ptr2[1];
					ptr2 += 2;
				}
				return num + num2 * 1566083941;
			}
		}

		public override bool Equals(object obj)
		{
			if (obj is string str)
			{
				return this == str;
			}
			if (obj is StringSlice)
			{
				return this == (StringSlice)obj;
			}
			return false;
		}

		public bool Equals(StringSlice other)
		{
			return this == other;
		}

		public bool Equals(string other)
		{
			return this == other;
		}

		public static bool operator ==(StringSlice a, StringSlice b)
		{
			int length = a.Length;
			if (length != b.Length)
			{
				return false;
			}
			if (a.Index == b.Index && a.String != null && (object)a.String == b.String)
			{
				return true;
			}
			int end = a.Index + length;
			int a_i = a.Index;
			int b_i = b.Index;
			while (a_i < end)
			{
				if (a.String[a_i] != b.String[b_i])
				{
					return false;
				}
				a_i++;
				b_i++;
			}
			return true;
		}

		public static bool operator ==(StringSlice slice, string str)
		{
			if (str == null)
			{
				return slice.String == null;
			}
			if (slice.String == null)
			{
				return false;
			}
			if (slice.Length != str.Length)
			{
				return false;
			}
			if (slice.Index == 0)
			{
				return slice.String == str;
			}
			int i = 0;
			int j = slice.Index;
			while (i < str.Length)
			{
				if (str[i] != slice.String[j])
				{
					return false;
				}
				i++;
				j++;
			}
			return true;
		}

		public static bool operator !=(StringSlice a, StringSlice b)
		{
			return !(a == b);
		}

		public static bool operator !=(StringSlice fs, string str)
		{
			return !(fs == str);
		}

		public StringSlice Slice(int index, int length)
		{
			if (index < 0 || length < 0 || index + length > Length)
			{
				throw new IndexOutOfRangeException();
			}
			return new StringSlice(String, Index + index, length);
		}

		public StringSlice Slice(int index)
		{
			if (index < 0 || index > Length)
			{
				throw new IndexOutOfRangeException();
			}
			return new StringSlice(String, Index + index, Length - index);
		}

		public bool StartsWith(string str)
		{
			if (String == null)
			{
				return false;
			}
			int length = str.Length;
			if (Length < length)
			{
				return false;
			}
			for (int i = 0; i < length; i++)
			{
				if (this[i] != str[i])
				{
					return false;
				}
			}
			return true;
		}

		public bool StartsWith(ref StringSlice fs)
		{
			if (String == null)
			{
				return false;
			}
			int length = fs.Length;
			if (Length < length)
			{
				return false;
			}
			for (int i = 0; i < length; i++)
			{
				if (this[i] != fs[i])
				{
					return false;
				}
			}
			return true;
		}

		public bool EndsWith(ref string str)
		{
			if (String == null)
			{
				return false;
			}
			int length = str.Length;
			if (Length < length)
			{
				return false;
			}
			int i = 0;
			int j = Length - 1;
			while (i < length)
			{
				if (this[j] != str[i])
				{
					return false;
				}
				i++;
				j--;
			}
			return true;
		}

		public bool EndsWith(ref StringSlice fs)
		{
			if (String == null)
			{
				return false;
			}
			int length = fs.Length;
			if (Length < length)
			{
				return false;
			}
			int i = 0;
			int j = Length - 1;
			while (i < length)
			{
				if (this[j] != fs[i])
				{
					return false;
				}
				i++;
				j--;
			}
			return true;
		}

		public bool Contains(string str)
		{
			if (String == null)
			{
				return false;
			}
			int strLength = str.Length;
			int length = Length;
			if (length < strLength)
			{
				return false;
			}
			for (int i = 0; i < length; i++)
			{
				if (this[i] != str[0])
				{
					continue;
				}
				bool isMatch = true;
				int fsIndex = 1;
				int thisIndex = i + 1;
				while (fsIndex < strLength && thisIndex < length)
				{
					if (this[thisIndex] != str[fsIndex])
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

		public bool Contains(ref StringSlice fs)
		{
			if (String == null)
			{
				return false;
			}
			int fsLength = fs.Length;
			int length = Length;
			if (length < fsLength)
			{
				return false;
			}
			for (int i = 0; i < length; i++)
			{
				if (this[i] != fs[0])
				{
					continue;
				}
				bool isMatch = true;
				int fsIndex = 1;
				int thisIndex = i + 1;
				while (fsIndex < fsLength && thisIndex < length)
				{
					if (this[thisIndex] != fs[fsIndex])
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

		public int FirstIndexOf(char c)
		{
			int i = Index;
			for (int end = i + Length; i < end; i++)
			{
				if (String[i] == c)
				{
					return i - Index;
				}
			}
			return -1;
		}

		public int LastIndexOf(char c)
		{
			for (int i = Index + Length - 1; i >= Index; i--)
			{
				if (String[i] == c)
				{
					return i - Index;
				}
			}
			return -1;
		}

		public StringSlice TrimStart()
		{
			StringSlice result = this;
			while (result.Length > 0 && char.IsWhiteSpace(result[0]))
			{
				result.Index++;
				result.Length--;
			}
			return result;
		}

		public StringSlice TrimEnd()
		{
			StringSlice result = this;
			while (result.Length > 0 && char.IsWhiteSpace(result[result.Length - 1]))
			{
				result.Length--;
			}
			return result;
		}

		public StringSlice Trim()
		{
			StringSlice result = this;
			while (result.Length > 0 && char.IsWhiteSpace(result[0]))
			{
				result.Index++;
				result.Length--;
			}
			while (result.Length > 0 && char.IsWhiteSpace(result[result.Length - 1]))
			{
				result.Length--;
			}
			return result;
		}

		public bool TryParseToInt(out int result)
		{
			int length = Length;
			bool isNegative = false;
			result = 0;
			for (int i = 0; i < length; i++)
			{
				char c = this[i];
				if (c == '-' && i == 0)
				{
					isNegative = true;
					continue;
				}
				c = (char)(c - 48);
				if (c < '\0' || c > '\t')
				{
					result = 0;
					return false;
				}
				result = result * 10 + c;
			}
			if (isNegative)
			{
				result = -result;
			}
			return true;
		}
	}
}
