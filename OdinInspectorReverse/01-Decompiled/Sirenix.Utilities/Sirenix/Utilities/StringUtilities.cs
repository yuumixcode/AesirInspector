using System;
using System.Text;

namespace Sirenix.Utilities
{
	/// <summary>
	/// Not yet documented.
	/// </summary>
	public static class StringUtilities
	{
		/// <summary>
		/// Not yet documented.
		/// </summary>
		/// <param name="bytes">Not yet documented.</param>
		/// <param name="decimals">Not yet documented.</param>
		/// <returns>Not yet documented.</returns>
		public static string NicifyByteSize(int bytes, int decimals = 1)
		{
			StringBuilder builder = new StringBuilder();
			if (bytes < 0)
			{
				builder.Append('-');
				bytes = Math.Abs(bytes);
			}
			int decimalLength = 0;
			string m = null;
			if (bytes > 1000000000)
			{
				builder.Append(bytes / 1000000000);
				bytes -= bytes / 1000000000 * 1000000000;
				decimalLength = 9;
				m = " GB";
			}
			else if (bytes > 1000000)
			{
				builder.Append(bytes / 1000000);
				bytes -= bytes / 1000000 * 1000000;
				decimalLength = 6;
				m = " MB";
			}
			else if (bytes > 1000)
			{
				builder.Append(bytes / 1000);
				bytes -= bytes / 1000 * 1000;
				decimalLength = 3;
				m = " KB";
			}
			else
			{
				builder.Append(bytes);
				decimals = 0;
				decimalLength = 0;
				m = " bytes";
			}
			if (decimals > 0 && decimalLength > 0 && bytes > 0)
			{
				string d = bytes.ToString().PadLeft(decimalLength, '0');
				d = d.Substring(0, (decimals < d.Length) ? decimals : d.Length).TrimEnd(new char[1] { '0' });
				if (d.Length > 0)
				{
					builder.Append('.');
					builder.Append(d);
				}
			}
			builder.Append(m);
			return builder.ToString();
		}

		public static bool FastEndsWith(this string str, string endsWith)
		{
			if (str.Length < endsWith.Length)
			{
				return false;
			}
			for (int i = 0; i < endsWith.Length; i++)
			{
				if (str[str.Length - (1 + i)] != endsWith[endsWith.Length - (1 + i)])
				{
					return false;
				}
			}
			return true;
		}

		/// <summary>
		/// Compares two strings in a number-aware manner, IE, "[2] Foo" is considered to come before "[10] Bar".
		/// </summary>
		public static int NumberAwareStringCompare(string a, string b, bool ignoreLeadingZeroes = true, bool ignoreWhiteSpace = true, bool ignoreCase = false)
		{
			int len1 = a.Length;
			int len2 = b.Length;
			int i1 = 0;
			int j = 0;
			char lc1;
			char lc2;
			while (true)
			{
				bool end1 = i1 == len1;
				bool end2 = j == len2;
				if (end1 && end2)
				{
					if (len1 == len2)
					{
						return 0;
					}
					if (len1 < len2)
					{
						return -1;
					}
					return 1;
				}
				if (end1)
				{
					return -1;
				}
				if (end2)
				{
					return 1;
				}
				if (ignoreWhiteSpace)
				{
					for (; i1 < len1 && char.IsWhiteSpace(a[i1]); i1++)
					{
					}
					for (; j < len2 && char.IsWhiteSpace(b[j]); j++)
					{
					}
				}
				char c1 = a[i1];
				char c2 = b[j];
				if (char.IsDigit(c1) && char.IsDigit(c2))
				{
					if (ignoreLeadingZeroes)
					{
						for (; i1 < len1 && a[i1] == '0'; i1++)
						{
						}
						for (; j < len2 && b[j] == '0'; j++)
						{
						}
					}
					int digEnd1 = i1;
					int k = j;
					for (; digEnd1 < len1 && char.IsDigit(a[digEnd1]); digEnd1++)
					{
					}
					for (; k < len2 && char.IsDigit(b[k]); k++)
					{
					}
					int dig1Length = digEnd1 - i1;
					int dig2Length = k - j;
					if (dig1Length != dig2Length)
					{
						return dig1Length - dig2Length;
					}
					while (i1 < digEnd1)
					{
						if (a[i1] != b[j])
						{
							return a[i1] - b[j];
						}
						i1++;
						j++;
					}
					continue;
				}
				if (ignoreCase)
				{
					if (c1 != c2)
					{
						c1 = char.ToLower(c1);
						c2 = char.ToLower(c2);
						if (c1 != c2)
						{
							return c1 - c2;
						}
					}
				}
				else
				{
					lc1 = char.ToLower(c1);
					lc2 = char.ToLower(c2);
					if (lc1 != lc2)
					{
						break;
					}
					if (c1 != c2)
					{
						return c2 - c1;
					}
				}
				i1++;
				j++;
			}
			return lc1 - lc2;
		}
	}
}
