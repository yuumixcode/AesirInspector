using System;

namespace Sirenix.Utilities.Editor
{
	/// <summary>
	/// Compare strings and produce a distance score between them.
	/// </summary>
	public static class FuzzySearch
	{
		public static bool Contains(string searchTerm, string text)
		{
			if (searchTerm == null || searchTerm.Length == 0 || text == null || text.Length == 0)
			{
				return false;
			}
			int i = 0;
			int j = 0;
			int stLength = searchTerm.Length;
			int txtLength = text.Length;
			char a = searchTerm[i];
			while (a == ' ' && ++i < stLength)
			{
				a = searchTerm[i];
			}
			bool doAbbriviation = char.IsUpper(a);
			if (!doAbbriviation && a >= 'A' && a <= 'Z')
			{
				a = (char)(a + 32);
			}
			bool prevWasLetter = false;
			do
			{
				char b = text[j++];
				if (doAbbriviation)
				{
					if (!prevWasLetter)
					{
						b = char.ToUpper(b);
					}
				}
				else if (b >= 'A' && b <= 'Z')
				{
					b = (char)(b + 32);
				}
				if (a == b)
				{
					i++;
					if (i >= stLength)
					{
						break;
					}
					a = searchTerm[i];
					while (a == ' ' && ++i < stLength)
					{
						a = searchTerm[i];
					}
					doAbbriviation = char.IsUpper(a);
					if (!doAbbriviation && a >= 'A' && a <= 'Z')
					{
						a = (char)(a + 32);
					}
				}
				prevWasLetter = char.IsLetter(b);
			}
			while (j < txtLength);
			return i >= stLength;
		}

		public static bool Contains(string searchTerm, string text, out int score)
		{
			score = 0;
			if (searchTerm == null || searchTerm.Length == 0 || text == null || text.Length == 0)
			{
				return false;
			}
			int i = 0;
			int j = 0;
			int stLength = searchTerm.Length;
			int txtLength = text.Length;
			char a = searchTerm[i];
			while (a == ' ' && ++i < stLength)
			{
				a = searchTerm[i];
			}
			bool doAbbriviation = char.IsUpper(a);
			if (!doAbbriviation && a >= 'A' && a <= 'Z')
			{
				a = (char)(a + 32);
			}
			int bonus = 50;
			bool prevWasLetter = false;
			do
			{
				char b = text[j++];
				if (doAbbriviation)
				{
					if (!prevWasLetter)
					{
						bonus = 50;
						b = char.ToUpper(b);
					}
				}
				else if (b >= 'A' && b <= 'Z')
				{
					b = (char)(b + 32);
				}
				if (a == b)
				{
					score += bonus;
					bonus += 20;
					i++;
					if (i >= stLength)
					{
						break;
					}
					a = searchTerm[i];
					while (a == ' ' && ++i < stLength)
					{
						a = searchTerm[i];
					}
					doAbbriviation = char.IsUpper(a);
					if (!doAbbriviation && a >= 'A' && a <= 'Z')
					{
						a = (char)(a + 32);
					}
				}
				else if (!doAbbriviation || char.IsUpper(b))
				{
					bonus = (prevWasLetter ? 20 : 50);
				}
				prevWasLetter = char.IsLetter(b);
			}
			while (j < txtLength);
			score -= txtLength - i;
			return i >= stLength;
		}

		/// <summary>
		/// Determines whether if the source is within the search.
		/// </summary>
		/// <param name="source">The source string.</param>
		/// <param name="target">The target string.</param>
		/// <param name="ignoreCase">Should the algorithm ignore letter case?.</param>
		/// <param name="abbreviation">Should the algorithm attempt to search on an abbreviation of the source?.</param>
		/// <param name="threshold">Threshold for what is considered to be within the search. 0 will return everything and 1 will only return exact matches.</param>
		/// <returns>True if the source is within the search. Otherwise false.</returns>
		[Obsolete("Use FuzzySearch.Contains(searchTerm, text, out score) instead.")]
		public static bool Contains(ref string source, ref string target, float threshold = 0.8f, bool ignoreCase = true, bool abbreviation = true)
		{
			return Compare(ref source, ref target, ignoreCase, abbreviation) >= threshold;
		}

		/// <summary>
		/// Compares the target to the source and returns a distance score.
		/// </summary>
		/// <param name="source">The source string.</param>
		/// <param name="target">The target string.</param>
		/// <param name="ignoreCase"></param>
		/// <param name="abbreviation"></param>
		/// <returns>Distance score. 0 is no match, and 1 is exact match.</returns>
		[Obsolete("Use FuzzySearch.Contains(searchTerm, text, out score) instead.")]
		public static float Compare(ref string source, ref string target, bool ignoreCase = true, bool abbreviation = true)
		{
			int tLength = ((target != null) ? target.Length : 0);
			int sLength = ((source != null) ? source.Length : 0);
			if (tLength == 0)
			{
				return 1f;
			}
			if (sLength == 0)
			{
				return 0f;
			}
			int t = 0;
			int s = 0;
			char tChar = target[0];
			if (tChar >= 'A' && tChar <= 'Z')
			{
				tChar = (char)(tChar + 32);
			}
			while (t < tLength && s < sLength)
			{
				if (tChar != ' ')
				{
					char sChar = source[s];
					s++;
					if (sChar == ' ')
					{
						continue;
					}
					if (sChar >= 'A' && sChar <= 'Z')
					{
						sChar = (char)(sChar + 32);
					}
					if (sChar == tChar)
					{
						t++;
						if (t == tLength)
						{
							break;
						}
						tChar = target[t];
						if (tChar >= 'A' && tChar <= 'Z')
						{
							tChar = (char)(tChar + 32);
						}
					}
				}
				else
				{
					t++;
				}
			}
			return (float)t / (float)tLength;
		}
	}
}
