using System.Globalization;
using System.Text;

namespace Sirenix.Serialization.Utilities
{
	/// <summary>
	/// String method extensions.
	/// </summary>
	internal static class StringExtensions
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
	}
}
