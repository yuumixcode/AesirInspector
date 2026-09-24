using System;
using System.Globalization;
using System.Text;

namespace Sirenix.Utilities
{
	/// <summary>
	/// String method extensions.
	/// </summary>
	public static class StringExtensions
	{
		/// <summary>
		/// Eg MY_INT_VALUE =&gt; MyIntValue
		/// </summary>
		public static string ToTitleCase(this string input)
		{
			StringBuilder builder = new StringBuilder();
			for (int i = 0; i < input.Length; i++)
			{
				char current = input[i];
				if (current == '_' && i + 1 < input.Length)
				{
					char next = input[i + 1];
					if (char.IsLower(next))
					{
						next = char.ToUpper(next, CultureInfo.InvariantCulture);
					}
					builder.Append(next);
					i++;
				}
				else
				{
					builder.Append(current);
				}
			}
			return builder.ToString();
		}

		/// <summary>
		/// Returns whether or not the specified string is contained with this string
		/// </summary>
		public static bool Contains(this string source, string toCheck, StringComparison comparisonType)
		{
			return source.IndexOf(toCheck, comparisonType) >= 0;
		}

		/// <summary>
		/// Ex: "thisIsCamelCase" -&gt; "This Is Camel Case"
		/// </summary>
		public static string SplitPascalCase(this string input)
		{
			if (input == null || input.Length == 0)
			{
				return input;
			}
			StringBuilder sb = new StringBuilder(input.Length);
			if (char.IsLetter(input[0]))
			{
				sb.Append(char.ToUpper(input[0]));
			}
			else
			{
				sb.Append(input[0]);
			}
			for (int i = 1; i < input.Length; i++)
			{
				char c = input[i];
				if (char.IsUpper(c) && !char.IsUpper(input[i - 1]))
				{
					sb.Append(' ');
				}
				sb.Append(c);
			}
			return sb.ToString();
		}

		/// <summary>
		/// Returns true if this string is null, empty, or contains only whitespace.
		/// </summary>
		/// <param name="str">The string to check.</param>
		/// <returns><c>true</c> if this string is null, empty, or contains only whitespace; otherwise, <c>false</c>.</returns>
		public static bool IsNullOrWhitespace(this string str)
		{
			if (!string.IsNullOrEmpty(str))
			{
				for (int i = 0; i < str.Length; i++)
				{
					if (!char.IsWhiteSpace(str[i]))
					{
						return false;
					}
				}
			}
			return true;
		}

		/// <summary>
		/// O(n*m) - Use with care.
		/// </summary>
		public static int CalculateLevenshteinDistance(string source1, string source2)
		{
			int source1Length = source1.Length;
			int source2Length = source2.Length;
			int[,] matrix = new int[source1Length + 1, source2Length + 1];
			if (source1Length == 0)
			{
				return source2Length;
			}
			if (source2Length == 0)
			{
				return source1Length;
			}
			int i = 0;
			while (i <= source1Length)
			{
				matrix[i, 0] = i++;
			}
			int j = 0;
			while (j <= source2Length)
			{
				matrix[0, j] = j++;
			}
			for (int k = 1; k <= source1Length; k++)
			{
				for (int l = 1; l <= source2Length; l++)
				{
					int cost = ((source2[l - 1] != source1[k - 1]) ? 1 : 0);
					matrix[k, l] = Math.Min(Math.Min(matrix[k - 1, l] + 1, matrix[k, l - 1] + 1), matrix[k - 1, l - 1] + cost);
				}
			}
			return matrix[source1Length, source2Length];
		}
	}
}
