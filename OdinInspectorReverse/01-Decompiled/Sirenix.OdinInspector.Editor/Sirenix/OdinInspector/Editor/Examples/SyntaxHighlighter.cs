using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor.Expressions;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Examples
{
	internal class SyntaxHighlighter
	{
		private struct TokenBuffer
		{
			public Token Token;

			public int StartIndex;

			public int EndIndex;

			public int Length => EndIndex - StartIndex;

			public TokenBuffer(Token token, Tokenizer tokenizer)
			{
				Token = token;
				StartIndex = tokenizer.TokenStartedStringPosition;
				EndIndex = tokenizer.ExpressionStringPosition;
			}

			public string GetString(Tokenizer tokenizer)
			{
				return tokenizer.ExpressionString.Substring(StartIndex, Length);
			}

			public override string ToString()
			{
				return Token.ToString();
			}
		}

		public static Color BackgroundColor = new Color(0.118f, 0.118f, 0.118f, 1f);

		public static Color TextColor = new Color(0.863f, 0.863f, 0.863f, 1f);

		public static Color KeywordColor = new Color(0.337f, 0.612f, 0.839f, 1f);

		public static Color IdentifierColor = new Color(0.306f, 0.788f, 0.69f, 1f);

		public static Color CommentColor = new Color(0.341f, 0.651f, 0.29f, 1f);

		public static Color LiteralColor = new Color(0.71f, 0.808f, 0.659f, 1f);

		public static Color StringLiteralColor = new Color(0.839f, 0.616f, 0.522f, 1f);

		private Tokenizer tokenizer;

		private StringBuilder result = new StringBuilder();

		private List<TokenBuffer> statement = new List<TokenBuffer>();

		private int textPosition;

		public static string Parse(string text)
		{
			return new SyntaxHighlighter().ParseText(text);
		}

		public string ParseText(string text)
		{
			result.Length = 0;
			statement.Clear();
			textPosition = 0;
			tokenizer = new Tokenizer(text)
			{
				TokenizeComments = true,
				TokenizePreprocessors = true
			};
			ReadDeclaration();
			return result.ToString();
		}

		private void ReadDeclaration()
		{
			Token token = Token.UNKNOWN;
			while (token != Token.EOF)
			{
				token = tokenizer.GetNextToken();
				switch (token)
				{
				case Token.COMMENT:
					AppendWhitespace(tokenizer.TokenStartedStringPosition);
					Colorize(tokenizer.TokenStartedStringPosition, tokenizer.ExpressionStringPosition - tokenizer.TokenStartedStringPosition, CommentColor);
					continue;
				case Token.PREPROCESSOR:
					AppendWhitespace(tokenizer.TokenStartedStringPosition);
					Append(tokenizer.TokenStartedStringPosition, tokenizer.ExpressionStringPosition - tokenizer.TokenStartedStringPosition);
					continue;
				default:
					statement.Add(new TokenBuffer(token, tokenizer));
					if (statement[0].Token == Token.LEFT_BRACKET)
					{
						if (token == Token.RIGHT_BRACKET)
						{
							AppendDeclaration(statement, ref textPosition);
							statement.Clear();
						}
						continue;
					}
					switch (token)
					{
					case Token.SCOPE_BEGIN:
						if (statement.Any((TokenBuffer i) => i.Token == Token.LEFT_PARENTHESIS))
						{
							AppendMember(statement);
							statement.Clear();
							ReadImplementation();
						}
						else if (statement.Any((TokenBuffer i) => i.Token == Token.CLASS || i.Token == Token.STRUCT || i.Token == Token.INTERFACE))
						{
							AppendDeclaration(statement, ref textPosition);
							statement.Clear();
						}
						else
						{
							AppendMember(statement);
							statement.Clear();
							ReadImplementation();
						}
						break;
					case Token.SCOPE_END:
						AppendMember(statement);
						statement.Clear();
						break;
					case Token.SEMI_COLON:
						AppendMember(statement);
						statement.Clear();
						break;
					}
					continue;
				case Token.EOF:
					break;
				}
				break;
			}
			if (statement.Count > 0)
			{
				AppendDeclaration(statement, ref textPosition);
				statement.Clear();
			}
		}

		private void ReadImplementation()
		{
			Token token = Token.UNKNOWN;
			while (token != Token.EOF && token != Token.SCOPE_END)
			{
				token = tokenizer.GetNextToken();
				statement.Add(new TokenBuffer(token, tokenizer));
			}
			AppendImplementation(statement);
			statement.Clear();
		}

		private void AppendDeclaration(List<TokenBuffer> statementBuffer, ref int prevIndex)
		{
			for (int i = 0; i < statementBuffer.Count; i++)
			{
				TokenBuffer t = statementBuffer[i];
				AppendWhitespace(t.StartIndex);
				if (t.Token == Token.IDENTIFIER)
				{
					string id = t.GetString(tokenizer);
					if (TypeExtensions.IsCSharpKeyword(id))
					{
						Colorize(id, KeywordColor);
					}
					else
					{
						Colorize(id, IdentifierColor);
					}
					prevIndex = t.EndIndex;
				}
				else
				{
					AppendToken(t);
				}
			}
		}

		private void AppendMember(List<TokenBuffer> statement)
		{
			for (int i = 0; i < statement.Count; i++)
			{
				TokenBuffer t = statement[i];
				AppendWhitespace(t.StartIndex);
				if (t.Token == Token.IDENTIFIER)
				{
					string id = t.GetString(tokenizer);
					if (TypeExtensions.IsCSharpKeyword(id))
					{
						Colorize(id, KeywordColor);
					}
					else
					{
						switch ((i + 1 < statement.Count) ? statement[i + 1].Token : Token.UNKNOWN)
						{
						case Token.LEFT_PARENTHESIS:
						case Token.COMMA:
						case Token.SEMI_COLON:
						case Token.SIMPLE_ASSIGNMENT:
						case Token.SCOPE_BEGIN:
							Append(id);
							break;
						default:
							Colorize(id, IdentifierColor);
							break;
						}
					}
					textPosition = t.EndIndex;
				}
				else
				{
					AppendToken(t);
				}
			}
		}

		private void AppendImplementation(List<TokenBuffer> statement)
		{
			for (int i = 0; i < statement.Count; i++)
			{
				TokenBuffer t = statement[i];
				AppendWhitespace(t.StartIndex);
				if (t.Token == Token.IDENTIFIER)
				{
					string id = t.GetString(tokenizer);
					if (TypeExtensions.IsCSharpKeyword(id))
					{
						Colorize(id, KeywordColor);
					}
					else
					{
						result.Append(id);
					}
					textPosition = t.EndIndex;
				}
				else
				{
					AppendToken(t);
				}
			}
		}

		private void AppendToken(TokenBuffer buffer)
		{
			AppendWhitespace(buffer.StartIndex);
			switch (buffer.Token)
			{
			case Token.IDENTIFIER:
			{
				string id = tokenizer.ExpressionString.Substring(buffer.StartIndex, buffer.Length);
				if (TypeExtensions.IsCSharpKeyword(id))
				{
					Colorize(id, KeywordColor);
				}
				else
				{
					Colorize(id, IdentifierColor);
				}
				break;
			}
			case Token.SIZEOF:
			case Token.TRUE:
			case Token.FALSE:
			case Token.RELATIONAL_IS:
			case Token.RELATIONAL_AS:
			case Token.NEW:
			case Token.THIS:
			case Token.BASE:
			case Token.CHECKED:
			case Token.UNCHECKED:
			case Token.DEFAULT:
			case Token.NULL:
			case Token.TYPEOF:
			case Token.VOID:
			case Token.REF:
			case Token.OUT:
			case Token.IN:
			case Token.CLASS:
			case Token.STRUCT:
			case Token.INTERFACE:
			case Token.RETURN:
				Colorize(buffer.StartIndex, buffer.Length, KeywordColor);
				break;
			case Token.SIGNED_INT32:
			case Token.UNSIGNED_INT32:
			case Token.SIGNED_INT64:
			case Token.UNSIGNED_INT64:
			case Token.FLOAT32:
			case Token.FLOAT64:
			case Token.DECIMAL:
				Colorize(buffer.StartIndex, buffer.Length, LiteralColor);
				break;
			case Token.CHAR_CONSTANT:
			case Token.STRING_CONSTANT:
				Colorize(buffer.StartIndex, buffer.Length, StringLiteralColor);
				break;
			case Token.COMMENT:
				Colorize(buffer.StartIndex, buffer.Length, CommentColor);
				break;
			case Token.EOF:
				return;
			default:
				result.Append(tokenizer.ExpressionString, buffer.StartIndex, buffer.Length);
				break;
			}
			textPosition = buffer.EndIndex;
		}

		private void Colorize(string text, Color color)
		{
			result.Append("<color=#");
			result.Append(ColorUtility.ToHtmlStringRGBA(color));
			result.Append(">");
			Append(text);
			result.Append("</color>");
		}

		private void Colorize(int start, int length, Color color)
		{
			result.Append("<color=#");
			result.Append(ColorUtility.ToHtmlStringRGBA(color));
			result.Append(">");
			Append(start, length);
			result.Append("</color>");
		}

		private void Append(int start, int length)
		{
			result.Append(tokenizer.ExpressionString, start, length);
			textPosition = start + length;
		}

		private void Append(string text)
		{
			result.Append(text);
			textPosition += text.Length;
		}

		private void AppendWhitespace(int position)
		{
			if (position - textPosition > 0)
			{
				result.Append(tokenizer.ExpressionString, textPosition, position - textPosition);
				textPosition = position;
			}
		}
	}
}
