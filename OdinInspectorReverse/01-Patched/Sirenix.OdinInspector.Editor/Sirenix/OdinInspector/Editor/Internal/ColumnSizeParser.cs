namespace Sirenix.OdinInspector.Editor.Internal
{
	internal static class ColumnSizeParser
	{
		public static ColumnSize Parse(string source)
		{
			if (!TryParse(source, out var result))
			{
				return ColumnSize.Auto;
			}
			return result;
		}

		public static bool TryParse(string source, out ColumnSize columnSize)
		{
			columnSize = ColumnSize.Auto;
			int index = 0;
			SkipWhitespace(source, ref index);
			if (index == source.Length)
			{
				return true;
			}
			if (IsAt("auto", source, index))
			{
				return true;
			}
			if (!TryParseValue(source, ref index, out columnSize.Value))
			{
				return false;
			}
			SkipWhitespace(source, ref index);
			if (index == source.Length || IsAt("%", source, index) || IsAt("percent", source, index))
			{
				float size = columnSize.Value;
				float percent = ((!float.IsNaN(size) && !float.IsNegativeInfinity(size) && !(size <= 0f)) ? ((!float.IsPositiveInfinity(size) && !(size >= 100f)) ? (size / 100f) : 1f) : 0f);
				columnSize.ColumnType = ColumnType.Percent;
				columnSize.Value = percent;
			}
			else if (IsAt("pixel", source, index) || IsAt("px", source, index))
			{
				columnSize.ColumnType = ColumnType.Pixel;
			}
			return columnSize.ColumnType != ColumnType.Auto;
		}

		private static void SkipWhitespace(string source, ref int index)
		{
			while (index < source.Length && char.IsWhiteSpace(source[index]))
			{
				index++;
			}
		}

		private static bool IsAt(string wordLowerCase, string source, int index)
		{
			if (source.Length - index < wordLowerCase.Length)
			{
				return false;
			}
			for (int i = 0; i < wordLowerCase.Length; i++)
			{
				char cLower = char.ToLower(source[index + i]);
				if (cLower != wordLowerCase[i])
				{
					return false;
				}
			}
			return true;
		}

		private static bool TryParseValue(string source, ref int index, out float value)
		{
			value = 0f;
			if (!IsValidNumberChar(source[index]))
			{
				return false;
			}
			int numStart = index;
			while (index < source.Length && IsValidNumberChar(source[index]))
			{
				index++;
			}
			int numLength = index - numStart;
			return float.TryParse(source.Substring(numStart, numLength), out value);
		}

		private static bool IsValidNumberChar(char c)
		{
			if (char.IsDigit(c))
			{
				return true;
			}
			switch (c)
			{
			case '+':
			case ',':
			case '-':
			case '.':
			case 'E':
			case 'e':
				return true;
			default:
				return false;
			}
		}
	}
}
