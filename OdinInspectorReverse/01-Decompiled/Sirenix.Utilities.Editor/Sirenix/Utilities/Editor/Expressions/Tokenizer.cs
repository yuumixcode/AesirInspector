using System;
using System.Globalization;
using System.Text;

namespace Sirenix.Utilities.Editor.Expressions
{
	public class Tokenizer
	{
		private static readonly char NoMoreCharacters = '\uffff';

		private StringBuilder stringBuilder = new StringBuilder();

		private int expressionStringPosition;

		private string expressionString;

		public bool TokenizeComments;

		public bool TokenizePreprocessors = true;

		public string ExpressionString => expressionString;

		public int ExpressionStringPosition => expressionStringPosition;

		public int TokenStartedStringPosition { get; private set; }

		public string IdentifierValue { get; private set; }

		public int ExpressionArgumentNumber { get; private set; }

		public char CharacterConstantValue { get; private set; }

		public float Float32ConstantValue { get; private set; }

		public double Float64ConstantValue { get; private set; }

		public long SignedIntegerConstantValue { get; private set; }

		public ulong UnsignedIntegerConstantValue { get; private set; }

		public decimal DecimalConstantValue { get; private set; }

		public string StringConstantValue { get; private set; }

		public Tokenizer()
		{
		}

		public Tokenizer(string expressingString)
		{
			SetExpressionString(expressingString);
		}

		public TokenizerState GetState()
		{
			return new TokenizerState
			{
				TokenString = ExpressionString,
				TokenStringPosition = ExpressionStringPosition,
				TokenStartedStringPosition = TokenStartedStringPosition,
				IdentifierValue = IdentifierValue,
				ExpressionArgumentNumber = ExpressionArgumentNumber,
				CharacterConstantValue = CharacterConstantValue,
				Float32ConstantValue = Float32ConstantValue,
				Float64ConstantValue = Float64ConstantValue,
				IntegerConstantValue = SignedIntegerConstantValue,
				UnsignedIntegerConstantValue = UnsignedIntegerConstantValue,
				DecimalConstantValue = DecimalConstantValue,
				StringConstantValue = StringConstantValue
			};
		}

		public void SetState(TokenizerState state)
		{
			expressionStringPosition = state.TokenStringPosition;
			TokenStartedStringPosition = state.TokenStartedStringPosition;
			IdentifierValue = state.IdentifierValue;
			ExpressionArgumentNumber = state.ExpressionArgumentNumber;
			expressionString = state.TokenString;
			CharacterConstantValue = state.CharacterConstantValue;
			Float32ConstantValue = state.Float32ConstantValue;
			Float64ConstantValue = state.Float64ConstantValue;
			SignedIntegerConstantValue = state.IntegerConstantValue;
			UnsignedIntegerConstantValue = state.UnsignedIntegerConstantValue;
			DecimalConstantValue = state.DecimalConstantValue;
			StringConstantValue = state.StringConstantValue;
		}

		public void SetExpressionString(string expressionString)
		{
			this.expressionString = expressionString;
			expressionStringPosition = 0;
			stringBuilder.Length = 0;
		}

		public Token GetNextToken()
		{
			EatWhiteSpace();
			int tempPos = expressionStringPosition;
			char nextCharacter = EatNextCharacter();
			if (nextCharacter == NoMoreCharacters)
			{
				return Token.EOF;
			}
			TokenStartedStringPosition = tempPos;
			switch (nextCharacter)
			{
			case '+':
				if (PeekNextCharacter() == '=')
				{
					EatNextCharacter();
					return Token.ADDITION_ASSIGNMENT;
				}
				if (PeekNextCharacter() == '+')
				{
					EatNextCharacter();
					return Token.INCREMENT;
				}
				if (char.IsDigit(PeekNextCharacter()))
				{
					return ParseNumber(nextCharacter);
				}
				return Token.PLUS;
			case '-':
				if (PeekNextCharacter() == '=')
				{
					EatNextCharacter();
					return Token.SUBTRACTION_ASSIGNMENT;
				}
				if (PeekNextCharacter() == '-')
				{
					EatNextCharacter();
					return Token.DECREMENT;
				}
				if (PeekNextCharacter() == '>')
				{
					EatNextCharacter();
					return Token.MEMBER_ACCESS_POINTER_DEREFERENCE;
				}
				if (char.IsDigit(PeekNextCharacter()))
				{
					return ParseNumber(nextCharacter);
				}
				return Token.MINUS;
			case '*':
				if (PeekNextCharacter() == '=')
				{
					EatNextCharacter();
					return Token.MULTIPLICATION_ASSIGNMENT;
				}
				return Token.MULTIPLY;
			case '/':
				switch (PeekNextCharacter())
				{
				case '=':
					EatNextCharacter();
					return Token.DIVISION_ASSIGNMENT;
				case '/':
				{
					char c2;
					while ((c2 = PeekNextCharacter()) != '\n' && c2 != NoMoreCharacters)
					{
						EatNextCharacter();
					}
					if (TokenizeComments)
					{
						return Token.COMMENT;
					}
					return GetNextToken();
				}
				case '*':
					EatNextCharacter();
					while (true)
					{
						if (PeekNextCharacter() == '*')
						{
							EatNextCharacter();
							if (PeekNextCharacter() == '/')
							{
								EatNextCharacter();
								if (!TokenizeComments)
								{
									break;
								}
								return Token.COMMENT;
							}
						}
						if (PeekNextCharacter() == NoMoreCharacters)
						{
							return GetNextToken();
						}
						EatNextCharacter();
					}
					return GetNextToken();
				default:
					return Token.DIVIDE;
				}
			case '%':
				if (PeekNextCharacter() == '=')
				{
					EatNextCharacter();
					return Token.REMAINDER_ASSIGNMENT;
				}
				return Token.REMAINDER;
			case '?':
				if (PeekNextCharacter() == '?')
				{
					EatNextCharacter();
					return Token.NULL_COALESCE;
				}
				if (PeekNextCharacter() == '.')
				{
					EatNextCharacter();
					return Token.MEMBER_ACCESS_NULL_CONDITIONAL;
				}
				if (PeekNextCharacter() == '[')
				{
					EatNextCharacter();
					return Token.ELEMENT_ACCESS_NULL_CONDITIONAL;
				}
				return Token.QUESTION_MARK;
			case '(':
				return Token.LEFT_PARENTHESIS;
			case ')':
				return Token.RIGHT_PARENTHESIS;
			case '[':
				return Token.LEFT_BRACKET;
			case ']':
				return Token.RIGHT_BRACKET;
			case '.':
				return Token.POINT;
			case ',':
				return Token.COMMA;
			case '~':
				return Token.COMPLEMENT;
			case ';':
				return Token.SEMI_COLON;
			case '{':
				return Token.SCOPE_BEGIN;
			case '}':
				return Token.SCOPE_END;
			case ':':
			{
				char c7 = PeekNextCharacter();
				if (c7 == ':')
				{
					EatNextCharacter();
					return Token.DOUBLE_COLON;
				}
				return Token.COLON;
			}
			case '|':
				switch (PeekNextCharacter())
				{
				case '|':
					EatNextCharacter();
					return Token.LOGICAL_OR;
				case '=':
					EatNextCharacter();
					return Token.BITWISE_OR_ASSIGNMENT;
				default:
					return Token.BITWISE_INCLUSIVE_OR;
				}
			case '^':
				if (PeekNextCharacter() == '=')
				{
					EatNextCharacter();
					return Token.BITWISE_XOR_ASSIGNMENT;
				}
				return Token.BITWISE_EXCLUSIVE_OR;
			case '&':
				switch (PeekNextCharacter())
				{
				case '&':
					EatNextCharacter();
					return Token.LOGICAL_AND;
				case '=':
					EatNextCharacter();
					return Token.BITWISE_AND_ASSIGNMENT;
				default:
					return Token.BITWISE_AND;
				}
			case '=':
				if (PeekNextCharacter() == '=')
				{
					EatNextCharacter();
					return Token.EQUALS;
				}
				return Token.SIMPLE_ASSIGNMENT;
			case '!':
				if (PeekNextCharacter() == '=')
				{
					EatNextCharacter();
					return Token.NOT_EQUALS;
				}
				return Token.NOT;
			case '>':
				switch (PeekNextCharacter())
				{
				case '=':
					EatNextCharacter();
					return Token.GREATER_THAN_OR_EQUAL;
				case '>':
				{
					int positionBackup = expressionStringPosition;
					EatNextCharacter();
					if (PeekNextCharacter() == '=')
					{
						EatNextCharacter();
						return Token.RIGHT_SHIFT_ASSIGNMENT;
					}
					expressionStringPosition = positionBackup;
					break;
				}
				}
				return Token.GREATER_THAN;
			case '<':
				switch (PeekNextCharacter())
				{
				case '=':
					EatNextCharacter();
					return Token.LESS_THAN_OR_EQUAL;
				case '<':
					EatNextCharacter();
					if (PeekNextCharacter() == '=')
					{
						EatNextCharacter();
						return Token.LEFT_SHIFT_ASSIGNMENT;
					}
					return Token.LEFT_SHIFT;
				default:
					return Token.LESS_THAN;
				}
			case '\'':
			{
				char c = PeekNextCharacter();
				if (c == '\\')
				{
					EatNextCharacter();
					CharacterConstantValue = EatNextCharacter();
					if (EatNextCharacter() != '\'')
					{
						throw new SyntaxException(this, "End aposthrope ' expected in character constant");
					}
					return Token.CHAR_CONSTANT;
				}
				if (!char.IsControl(c))
				{
					CharacterConstantValue = EatNextCharacter();
					if (EatNextCharacter() != '\'')
					{
						throw new SyntaxException(this, "End aposthrope ' expected in character constant");
					}
					return Token.CHAR_CONSTANT;
				}
				throw new NotImplementedException("Fancy char escape string parsing");
			}
			case '$':
			{
				char c6 = PeekNextCharacter();
				if (c6 == '"')
				{
					throw new SyntaxException(this, "Interpolated strings ($\"string\" are not supported");
				}
				if (char.IsDigit(c6))
				{
					stringBuilder.Length = 0;
					while (char.IsDigit(PeekNextCharacter()))
					{
						stringBuilder.Append(EatNextCharacter());
					}
					ExpressionArgumentNumber = Convert.ToInt32(stringBuilder.ToString());
					return Token.NUMBERED_EXPRESSION_ARGUMENT;
				}
				if (IsValidIdentifierStartCharacter(c6))
				{
					EatNextCharacter();
					string identifier = ParseIdentifier(c6);
					IdentifierValue = identifier;
					return Token.NAMED_EXPRESSION_ARGUMENT;
				}
				return Token.UNNUMBERED_EXPRESSION_ARGUMENT;
			}
			case '"':
			{
				stringBuilder.Length = 0;
				char c5;
				while ((c5 = EatNextCharacter()) != '"')
				{
					if (c5 == NoMoreCharacters)
					{
						throw new SyntaxException(this, "Expected \" to end string");
					}
					if (c5 == '\\')
					{
						if (PeekNextCharacter() == '"')
						{
							EatNextCharacter();
							stringBuilder.Append('"');
							continue;
						}
						if (PeekNextCharacter() == '\\')
						{
							EatNextCharacter();
							stringBuilder.Append('\\');
							continue;
						}
						if (PeekNextCharacter() == 'n')
						{
							EatNextCharacter();
							stringBuilder.Append('\n');
							continue;
						}
						if (PeekNextCharacter() == 'r')
						{
							EatNextCharacter();
							stringBuilder.Append('\r');
							continue;
						}
						if (PeekNextCharacter() != 't')
						{
							throw new SyntaxException(this, "String escape sequences apart from \\\\, \\\", \\n, \\r and \\t are currently not supported");
						}
						EatNextCharacter();
						stringBuilder.Append('\t');
					}
					else
					{
						stringBuilder.Append(c5);
					}
				}
				StringConstantValue = stringBuilder.ToString();
				return Token.STRING_CONSTANT;
			}
			case '@':
				if (PeekNextCharacter() == '"')
				{
					throw new SyntaxException(this, "Literal strings (@\"string\" are not supported");
				}
				throw new SyntaxException(this, "Invalid token '@'");
			case '#':
			{
				if (PeekNextCharacter() == '(')
				{
					EatNextCharacter();
					stringBuilder.Length = 0;
					char c3;
					while ((c3 = PeekNextCharacter()) != ')' && c3 != NoMoreCharacters)
					{
						EatNextCharacter();
						stringBuilder.Append(c3);
					}
					if (c3 != ')')
					{
						throw new SyntaxException(this, "Expected a ')' to end the property query started by '#('");
					}
					EatNextCharacter();
					IdentifierValue = stringBuilder.ToString();
					return Token.PROPERTY_QUERY;
				}
				char c4;
				while ((c4 = PeekNextCharacter()) != '\n' && c4 != NoMoreCharacters)
				{
					if (c4 == '/')
					{
						c4 = PeekNextCharacter();
						if (c4 == '/' || c4 == '*')
						{
							break;
						}
					}
					EatNextCharacter();
				}
				if (TokenizePreprocessors)
				{
					return Token.PREPROCESSOR;
				}
				return GetNextToken();
			}
			default:
				if (char.IsDigit(nextCharacter))
				{
					return ParseNumber(nextCharacter);
				}
				if (IsValidIdentifierStartCharacter(nextCharacter))
				{
					IdentifierValue = ParseIdentifier(nextCharacter);
					return IdentifierValue switch
					{
						"sizeof" => Token.SIZEOF, 
						"true" => Token.TRUE, 
						"false" => Token.FALSE, 
						"is" => Token.RELATIONAL_IS, 
						"as" => Token.RELATIONAL_AS, 
						"new" => Token.NEW, 
						"this" => Token.THIS, 
						"base" => Token.BASE, 
						"default" => Token.DEFAULT, 
						"checked" => Token.CHECKED, 
						"unchecked" => Token.UNCHECKED, 
						"null" => Token.NULL, 
						"typeof" => Token.TYPEOF, 
						"void" => Token.VOID, 
						"ref" => Token.REF, 
						"out" => Token.OUT, 
						"in" => Token.IN, 
						"class" => Token.CLASS, 
						"struct" => Token.STRUCT, 
						"interface" => Token.INTERFACE, 
						"return" => Token.RETURN, 
						"or" => Token.LOGICAL_OR, 
						"and" => Token.LOGICAL_AND, 
						_ => Token.IDENTIFIER, 
					};
				}
				throw new SyntaxException(this, "Unknown token '" + nextCharacter + "' at position " + (expressionStringPosition - 1));
			}
		}

		private void EatWhiteSpace()
		{
			while (char.IsWhiteSpace(PeekNextCharacter()))
			{
				EatNextCharacter();
			}
		}

		private char EatNextCharacter()
		{
			if (expressionStringPosition == expressionString.Length)
			{
				return NoMoreCharacters;
			}
			return expressionString[expressionStringPosition++];
		}

		private char PeekNextCharacter(int offset = 0)
		{
			if (expressionStringPosition + offset >= expressionString.Length)
			{
				return NoMoreCharacters;
			}
			return expressionString[expressionStringPosition + offset];
		}

		private string ParseIdentifier(char c)
		{
			stringBuilder.Length = 0;
			stringBuilder.Append(c);
			while (IsValidIdentifierPartCharacter(c = PeekNextCharacter()))
			{
				stringBuilder.Append(EatNextCharacter());
			}
			return stringBuilder.ToString();
		}

		private Token ParseNumber(char c)
		{
			bool negativeNumber = c == '-';
			bool floatingNumber = false;
			stringBuilder.Length = 0;
			if (c != '+')
			{
				stringBuilder.Append(c);
			}
			while (char.IsDigit(c = PeekNextCharacter()))
			{
				stringBuilder.Append(EatNextCharacter());
			}
			if (c == '.' && char.IsDigit(PeekNextCharacter(1)))
			{
				floatingNumber = true;
				EatNextCharacter();
				stringBuilder.Append('.');
				while (char.IsDigit(c = PeekNextCharacter()))
				{
					stringBuilder.Append(EatNextCharacter());
				}
			}
			if (c == 'e' || c == 'E')
			{
				throw new SyntaxException(this, "Exponential number literals are not supported");
			}
			if (stringBuilder.Length == 1 && stringBuilder[0] == '0')
			{
				switch (c)
				{
				case 'B':
				case 'b':
					throw new SyntaxException(this, "Binary number literals are not supported");
				case 'X':
				case 'x':
					throw new SyntaxException(this, "Hexadecimal number literals are not supported");
				}
			}
			string numberString = stringBuilder.ToString();
			try
			{
				switch (c)
				{
				case 'M':
				case 'm':
					EatNextCharacter();
					DecimalConstantValue = decimal.Parse(numberString, CultureInfo.InvariantCulture);
					return Token.DECIMAL;
				case 'D':
				case 'F':
				case 'd':
				case 'f':
					floatingNumber = true;
					break;
				}
				if (floatingNumber)
				{
					switch (c)
					{
					case 'F':
					case 'f':
						EatNextCharacter();
						Float32ConstantValue = float.Parse(numberString, NumberStyles.Float, CultureInfo.InvariantCulture);
						return Token.FLOAT32;
					case 'D':
					case 'd':
						EatNextCharacter();
						break;
					}
					Float64ConstantValue = double.Parse(numberString, NumberStyles.Float, CultureInfo.InvariantCulture);
					return Token.FLOAT64;
				}
				switch (c)
				{
				case 'U':
				case 'u':
					EatNextCharacter();
					c = PeekNextCharacter();
					if (c == 'l' || c == 'L')
					{
						if (negativeNumber)
						{
							throw new SyntaxException(this, "An ulong number literal cannot be negative.");
						}
						EatNextCharacter();
						UnsignedIntegerConstantValue = ulong.Parse(numberString, CultureInfo.InvariantCulture);
						return Token.UNSIGNED_INT64;
					}
					if (negativeNumber)
					{
						throw new SyntaxException(this, "An uint number literal cannot be negative.");
					}
					UnsignedIntegerConstantValue = ulong.Parse(numberString, CultureInfo.InvariantCulture);
					if (UnsignedIntegerConstantValue < 0 || UnsignedIntegerConstantValue > uint.MaxValue)
					{
						throw new SyntaxException(this, "uint constant '" + numberString + "' is out of uint number bounds. Use an ulong instead.");
					}
					return Token.UNSIGNED_INT32;
				case 'L':
				case 'l':
					EatNextCharacter();
					SignedIntegerConstantValue = long.Parse(numberString, CultureInfo.InvariantCulture);
					return Token.SIGNED_INT64;
				default:
				{
					if (int.TryParse(numberString, out var intValue))
					{
						SignedIntegerConstantValue = intValue;
						return Token.SIGNED_INT32;
					}
					if (uint.TryParse(numberString, out var uintValue))
					{
						UnsignedIntegerConstantValue = uintValue;
						return Token.UNSIGNED_INT32;
					}
					if (long.TryParse(numberString, out var longValue))
					{
						SignedIntegerConstantValue = longValue;
						return Token.SIGNED_INT64;
					}
					if (ulong.TryParse(numberString, out var ulongValue))
					{
						UnsignedIntegerConstantValue = ulongValue;
						return Token.UNSIGNED_INT64;
					}
					throw new SyntaxException(this, "Could not parse number literal '" + numberString + "'");
				}
				}
			}
			catch (OverflowException)
			{
				throw new SyntaxException(this, "Number literal '" + numberString + "' overflowed bounds");
			}
		}

		private static bool IsValidIdentifierStartCharacter(char c)
		{
			if ((c < 'a' || c > 'z') && (c < 'A' || c > 'Z') && c != '_' && c != '@')
			{
				return char.IsLetter(c);
			}
			return true;
		}

		private static bool IsValidIdentifierPartCharacter(char c)
		{
			if ((c < 'a' || c > 'z') && (c < 'A' || c > 'Z'))
			{
				switch (c)
				{
				default:
					return char.IsLetter(c);
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
				case '_':
					break;
				}
			}
			return true;
		}
	}
}
