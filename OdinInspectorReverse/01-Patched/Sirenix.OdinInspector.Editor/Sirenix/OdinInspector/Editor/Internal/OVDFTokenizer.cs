using System;
using System.Collections.Generic;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal static class OVDFTokenizer
	{
		private sealed class Tokenizer
		{
			private readonly string source;

			private int pointer;

			private int line;

			private int column;

			private readonly List<Token> result;

			public Tokenizer(string source)
			{
				this.source = source;
				pointer = 0;
				line = 1;
				column = 1;
				result = new List<Token>();
			}

			public List<Token> Run()
			{
				while (pointer < source.Length)
				{
					char currentChar = source[pointer];
					switch (currentChar)
					{
					case '\t':
					case ' ':
						pointer++;
						column++;
						continue;
					case '\n':
						EmitToken(TokenKind.EndOfLine, pointer, 1, line, column);
						pointer++;
						line++;
						column = 1;
						continue;
					case '\r':
					{
						char nextChar = ((pointer + 1 < source.Length) ? source[pointer + 1] : '\0');
						if (nextChar == '\n')
						{
							EmitToken(TokenKind.EndOfLine, pointer, 2, line, column);
							pointer += 2;
						}
						else
						{
							EmitToken(TokenKind.EndOfLine, pointer, 1, line, column);
							pointer++;
						}
						line++;
						column = 1;
						continue;
					}
					case ',':
						EmitToken(TokenKind.Comma, pointer, 1, line, column);
						pointer++;
						column++;
						continue;
					case '+':
						EmitToken(TokenKind.Plus, pointer, 1, line, column);
						pointer++;
						column++;
						continue;
					case '-':
						EmitToken(TokenKind.Minus, pointer, 1, line, column);
						pointer++;
						column++;
						continue;
					case '*':
						EmitToken(TokenKind.Star, pointer, 1, line, column);
						pointer++;
						column++;
						continue;
					case '[':
						EmitToken(TokenKind.LeftBracket, pointer, 1, line, column);
						pointer++;
						column++;
						continue;
					case ']':
						EmitToken(TokenKind.RightBracket, pointer, 1, line, column);
						pointer++;
						column++;
						continue;
					case '=':
						EmitToken(TokenKind.Equal, pointer, 1, line, column);
						pointer++;
						column++;
						continue;
					case ':':
						EmitToken(TokenKind.Colon, pointer, 1, line, column);
						pointer++;
						column++;
						continue;
					case '$':
					{
						int start = pointer;
						int col = column;
						pointer++;
						column++;
						while (pointer < source.Length && IsIdentifierPart(source[pointer]))
						{
							pointer++;
							column++;
						}
						int length = pointer - start;
						EmitToken(TokenKind.Reference, start, length, line, col);
						continue;
					}
					case '#':
						if (column == 1)
						{
							EmitToken(TokenKind.Hash, pointer, 1, line, column);
							pointer++;
							column++;
						}
						else
						{
							ReadColor();
						}
						continue;
					}
					if (char.IsDigit(currentChar) || (currentChar == '.' && pointer + 1 < source.Length && char.IsDigit(source[pointer + 1])))
					{
						if (!TryReadGuid32())
						{
							ReadNumber();
						}
					}
					else if (currentChar == '"')
					{
						ReadString();
					}
					else if (IsIdentifierStart(currentChar))
					{
						ReadIdentifier();
					}
					else
					{
						EmitToken(TokenKind.Unknown, pointer, 1, line, column);
						pointer++;
						column++;
					}
				}
				EmitToken(TokenKind.EndOfFile, pointer, 0, line, column);
				return result;
			}

			private void EmitToken(TokenKind kind, int start, int length, int line, int column)
			{
				result.Add(new Token(kind, start, length, line, column));
			}

			private void ReadIdentifier()
			{
				int start = pointer;
				int col = column;
				while (pointer < source.Length && IsIdentifierPart(source[pointer]))
				{
					pointer++;
					column++;
				}
				int length = pointer - start;
				switch (length)
				{
				case 4:
					if (string.Compare(source, start, "true", 0, 4, StringComparison.OrdinalIgnoreCase) == 0)
					{
						EmitToken(TokenKind.Bool, start, length, line, col);
						return;
					}
					if (string.Compare(source, start, "null", 0, 4, StringComparison.OrdinalIgnoreCase) == 0)
					{
						EmitToken(TokenKind.Null, start, length, line, col);
						return;
					}
					break;
				case 5:
					if (string.Compare(source, start, "false", 0, 5, StringComparison.OrdinalIgnoreCase) == 0)
					{
						EmitToken(TokenKind.Bool, start, length, line, col);
						return;
					}
					break;
				}
				EmitToken(TokenKind.Identifier, start, length, line, col);
			}

			private void ReadColor()
			{
				int start = pointer;
				int col = column;
				pointer++;
				column++;
				int digits = 0;
				while (pointer < source.Length && IsHexDigit(source[pointer]) && digits < 8)
				{
					pointer++;
					column++;
					digits++;
				}
				int length = pointer - start;
				if (digits == 6 || digits == 8)
				{
					EmitToken(TokenKind.Color, start, length, line, col);
				}
				else
				{
					EmitToken(TokenKind.InvalidColor, start, length, line, col);
				}
			}

			private void ReadNumber()
			{
				int start = pointer;
				int col = column;
				while (pointer < source.Length && IsNumericChar(source[pointer]))
				{
					pointer++;
					column++;
				}
				int length = pointer - start;
				EmitToken(TokenKind.Number, start, length, line, col);
			}

			private void ReadString()
			{
				int start = pointer;
				int col = column;
				EmitToken(TokenKind.StringStart, pointer, 1, line, column);
				pointer++;
				column++;
				int contentStart = pointer;
				while (pointer < source.Length)
				{
					switch (source[pointer])
					{
					case '"':
						if (contentStart < pointer)
						{
							EmitToken(TokenKind.StringContent, contentStart, pointer - contentStart, line, col + 1);
						}
						EmitToken(TokenKind.StringEnd, pointer, 1, line, column);
						pointer++;
						column++;
						return;
					case '\\':
					{
						if (contentStart < pointer)
						{
							EmitToken(TokenKind.StringContent, contentStart, pointer - contentStart, line, col + 1);
						}
						int escStart = pointer;
						pointer++;
						column++;
						if (pointer < source.Length)
						{
							char escChar = source[pointer];
							pointer++;
							column++;
							if (IsValidEscapeCharacter(escChar))
							{
								EmitToken(TokenKind.EscapeSequence, escStart, 2, line, column - 2);
							}
							else
							{
								EmitToken(TokenKind.InvalidEscapeSequence, escStart, 2, line, column - 2);
							}
						}
						contentStart = pointer;
						break;
					}
					case '\n':
					case '\r':
						EmitToken(TokenKind.UnterminatedString, start, pointer - start, line, col);
						return;
					default:
						pointer++;
						column++;
						break;
					}
				}
				EmitToken(TokenKind.UnterminatedString, start, pointer - start, line, col);
			}

			private bool TryReadGuid32()
			{
				int start = pointer;
				int col = column;
				if (start + 32 > source.Length)
				{
					return false;
				}
				if (!IsHexDigit(source[start]))
				{
					return false;
				}
				for (int i = 0; i < 32; i++)
				{
					if (!IsHexDigit(source[start + i]))
					{
						return false;
					}
				}
				int after = start + 32;
				if (after < source.Length && IsIdentifierPart(source[after]))
				{
					return false;
				}
				EmitToken(TokenKind.Identifier, start, 32, line, col);
				pointer += 32;
				column += 32;
				return true;
			}
		}

		public struct Token
		{
			public TokenKind Kind;

			public int Start;

			public int Length;

			public int Line;

			public int Column;

			public Token(TokenKind kind, int start, int length, int line, int column)
			{
				Kind = kind;
				Start = start;
				Length = length;
				Line = line;
				Column = column;
			}
		}

		public enum TokenKind
		{
			EndOfLine,
			EndOfFile,
			Hash,
			Comma,
			Colon,
			Equal,
			Plus,
			Minus,
			Star,
			LeftBracket,
			RightBracket,
			Identifier,
			Reference,
			Bool,
			Null,
			Number,
			Color,
			StringStart,
			StringContent,
			EscapeSequence,
			StringEnd,
			InvalidColor,
			InvalidEscapeSequence,
			UnterminatedString,
			Unknown
		}

		public static List<Token> Tokenize(string source)
		{
			Tokenizer tokenizer = new Tokenizer(source ?? string.Empty);
			return tokenizer.Run();
		}

		private static bool IsIdentifierStart(char c)
		{
			if (!char.IsLetter(c))
			{
				return c == '_';
			}
			return true;
		}

		private static bool IsIdentifierPart(char c)
		{
			if (!char.IsLetterOrDigit(c) && c != '_' && c != '-')
			{
				return c == '.';
			}
			return true;
		}

		private static bool IsHexDigit(char c)
		{
			if ((c < '0' || c > '9') && (c < 'a' || c > 'f'))
			{
				if (c >= 'A')
				{
					return c <= 'F';
				}
				return false;
			}
			return true;
		}

		private static bool IsNumericChar(char c)
		{
			if (!char.IsDigit(c) && c != '.' && c != ',' && c != 'e' && c != 'E' && c != '+')
			{
				return c == '-';
			}
			return true;
		}

		private static bool IsValidEscapeCharacter(char c)
		{
			if (c != 't' && c != 'r' && c != 'n' && c != '"')
			{
				return c == '\\';
			}
			return true;
		}
	}
}
