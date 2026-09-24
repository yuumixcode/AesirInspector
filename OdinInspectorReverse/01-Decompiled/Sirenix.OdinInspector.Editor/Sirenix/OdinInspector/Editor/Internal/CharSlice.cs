using System;
using System.Globalization;
using System.Text;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal readonly ref struct CharSlice
	{
		public readonly char[] Buffer;

		public readonly int Offset;

		public readonly int Length;

		private static readonly StringBuilder StringBuffer = new StringBuilder(128);

		public char this[int index] => Buffer[Offset + index];

		public CharSlice(char[] buffer, int offset, int length)
		{
			Buffer = buffer;
			Offset = offset;
			Length = length;
		}

		public override string ToString()
		{
			return new string(Buffer, Offset, Length);
		}

		public bool Equals(string str, bool ignoreCase)
		{
			if (string.IsNullOrEmpty(str))
			{
				return Length == 0;
			}
			if (Length != str.Length)
			{
				return false;
			}
			if (ignoreCase)
			{
				for (int i = 0; i < Length; i++)
				{
					char lhs = char.ToLowerInvariant(this[i]);
					char rhs = char.ToLowerInvariant(str[i]);
					if (lhs != rhs)
					{
						return false;
					}
				}
			}
			else
			{
				for (int j = 0; j < Length; j++)
				{
					if (this[j] != str[j])
					{
						return false;
					}
				}
			}
			return true;
		}

		public CharSlice Trim()
		{
			int start = 0;
			int end;
			for (end = Length - 1; start <= end && char.IsWhiteSpace(Buffer[Offset + start]); start++)
			{
			}
			while (end >= start && char.IsWhiteSpace(Buffer[Offset + end]))
			{
				end--;
			}
			return Slice(start, end);
		}

		public bool StartsWith(string str)
		{
			if (string.IsNullOrEmpty(str))
			{
				return Length == 0;
			}
			if (str.Length > Length)
			{
				return false;
			}
			for (int i = 0; i < str.Length; i++)
			{
				if (this[i] != str[i])
				{
					return false;
				}
			}
			return true;
		}

		public int FindNext(char c, int startIndex)
		{
			if (startIndex < 0)
			{
				return -1;
			}
			while (startIndex < Length)
			{
				if (this[startIndex] == c)
				{
					return startIndex;
				}
				startIndex++;
			}
			return -1;
		}

		public int FindNextDigit(int startIndex)
		{
			if (startIndex < 0)
			{
				return -1;
			}
			while (startIndex < Length)
			{
				if (char.IsDigit(this[startIndex]))
				{
					return startIndex;
				}
				startIndex++;
			}
			return -1;
		}

		public int FindNextNonWhitespace(int startIndex)
		{
			if (startIndex < 0)
			{
				return -1;
			}
			while (startIndex < Length)
			{
				if (!char.IsWhiteSpace(this[startIndex]))
				{
					return startIndex;
				}
				startIndex++;
			}
			return -1;
		}

		public int FindNextWhitespace(int startIndex)
		{
			if (startIndex < 0)
			{
				return -1;
			}
			while (startIndex < Length)
			{
				if (char.IsWhiteSpace(this[startIndex]))
				{
					return startIndex;
				}
				startIndex++;
			}
			return -1;
		}

		public int FindNextNumericStart(int startIndex)
		{
			if (startIndex < 0)
			{
				return -1;
			}
			while (startIndex < Length)
			{
				switch (this[startIndex])
				{
				case ',':
				case '-':
				case '.':
				case '0':
				case '1':
				case '2':
				case '3':
				case '4':
				case '5':
				case '6':
				case '7':
				case '8':
				case '9':
					return startIndex;
				}
				startIndex++;
			}
			return -1;
		}

		public int FindNextNumericEnd(int startIndex)
		{
			if (startIndex < 0)
			{
				return -1;
			}
			while (startIndex < Length)
			{
				switch (this[startIndex])
				{
				default:
					return startIndex;
				case '+':
				case ',':
				case '-':
				case '.':
				case '0':
				case '1':
				case '2':
				case '3':
				case '4':
				case '5':
				case '6':
				case '7':
				case '8':
				case '9':
				case 'E':
				case 'e':
					break;
				}
				startIndex++;
			}
			return Length;
		}

		public CharSlice Slice(int start, int end)
		{
			int length = Math.Max(0, end - start);
			if (start < 0 || start + length > Length)
			{
				throw new ArgumentOutOfRangeException();
			}
			int offset = Offset + start;
			return new CharSlice(Buffer, offset, length);
		}

		public CharSlice SliceNumeric(int start, bool isFloatingPoint)
		{
			int end = Offset;
			while (end < Length)
			{
				switch (this[end])
				{
				case '+':
				case ',':
				case '.':
				case 'E':
				case 'e':
					if (isFloatingPoint)
					{
						end++;
						continue;
					}
					break;
				case '-':
				case '0':
				case '1':
				case '2':
				case '3':
				case '4':
				case '5':
				case '6':
				case '7':
				case '8':
				case '9':
					end++;
					continue;
				}
				break;
			}
			return Slice(start, end);
		}

		public sbyte ParseSByte()
		{
			return sbyte.Parse(ToString(), CultureInfo.InvariantCulture);
		}

		public short ParseInt16()
		{
			return short.Parse(ToString(), CultureInfo.InvariantCulture);
		}

		public int ParseInt32()
		{
			return int.Parse(ToString(), CultureInfo.InvariantCulture);
		}

		public long ParseInt64()
		{
			return long.Parse(ToString(), CultureInfo.InvariantCulture);
		}

		public byte ParseByte()
		{
			return byte.Parse(ToString(), CultureInfo.InvariantCulture);
		}

		public ushort ParseUInt16()
		{
			return ushort.Parse(ToString(), CultureInfo.InvariantCulture);
		}

		public uint ParseUInt32()
		{
			return uint.Parse(ToString(), CultureInfo.InvariantCulture);
		}

		public ulong ParseUInt64()
		{
			return ulong.Parse(ToString(), CultureInfo.InvariantCulture);
		}

		public float ParseSingle()
		{
			return float.Parse(ToString(), CultureInfo.InvariantCulture);
		}

		public double ParseDouble()
		{
			return double.Parse(ToString(), CultureInfo.InvariantCulture);
		}

		public decimal ParseDecimal()
		{
			return decimal.Parse(ToString(), CultureInfo.InvariantCulture);
		}

		public bool ParseBoolean()
		{
			if (Equals("true", ignoreCase: true))
			{
				return true;
			}
			if (Equals("false", ignoreCase: true))
			{
				return false;
			}
			throw new FormatException("Could not parse \"" + ToString() + "\" as a boolean.");
		}

		public char ToChar()
		{
			return Buffer[Offset];
		}

		public string ParseString()
		{
			if (Equals("null", ignoreCase: true))
			{
				return null;
			}
			if (Length == 0)
			{
				return string.Empty;
			}
			StringBuffer.Clear();
			for (int i = 0; i < Length; i++)
			{
				char c = this[i];
				if (c == '\\')
				{
					if (++i < Length)
					{
						char next = this[i];
						switch (next)
						{
						case 'n':
							StringBuffer.Append('\n');
							break;
						case 'r':
							StringBuffer.Append('\r');
							break;
						case 't':
							StringBuffer.Append('\t');
							break;
						case '\\':
							StringBuffer.Append('\\');
							break;
						case '"':
							StringBuffer.Append('"');
							break;
						case '\'':
							StringBuffer.Append('\'');
							break;
						case '0':
							StringBuffer.Append('\0');
							break;
						default:
							StringBuffer.Append('\\');
							StringBuffer.Append(next);
							break;
						}
					}
					else
					{
						StringBuffer.Append('\\');
					}
				}
				else
				{
					StringBuffer.Append(c);
				}
			}
			return StringBuffer.ToString();
		}
	}
}
