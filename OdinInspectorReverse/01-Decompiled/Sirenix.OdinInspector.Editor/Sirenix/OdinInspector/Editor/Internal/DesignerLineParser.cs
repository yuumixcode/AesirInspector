using System;
using System.Text;
using Sirenix.Serialization;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal ref struct DesignerLineParser
	{
		public readonly CharSlice Slice;

		public int Position;

		private static readonly StringBuilder StringBuffer = new StringBuilder(256);

		public bool AtEnd => Position >= Slice.Length - 1;

		public char Current => Slice[Position];

		public DesignerLineParser(CharSlice slice)
		{
			Slice = slice;
			Position = 0;
		}

		public bool TryConsume(string str)
		{
			if (Slice.Length - Position < str.Length)
			{
				return false;
			}
			for (int i = 0; i < str.Length; i++)
			{
				if (Slice[Position + i] != str[i])
				{
					return false;
				}
			}
			Position += str.Length;
			return true;
		}

		public bool TryConsume(char c)
		{
			if (Current != c)
			{
				return false;
			}
			Position++;
			return true;
		}

		public void Consume(char c)
		{
			if (Current != c)
			{
				throw new FormatException($"Expected char '{c}', actual char is '{Current}'.");
			}
			Position++;
		}

		public void ConsumeWhitespace()
		{
			while (Position < Slice.Length && char.IsWhiteSpace(Current))
			{
				Position++;
			}
		}

		public void ConsumeNonWhitespace()
		{
			while (Position < Slice.Length && !char.IsWhiteSpace(Current))
			{
				Position++;
			}
		}

		public void ConsumeDigits()
		{
			while (Position < Slice.Length && char.IsDigit(Current))
			{
				Position++;
			}
		}

		public string ReadRemainder()
		{
			int start = Position;
			Position = Slice.Length;
			return Slice.Slice(start, Position).ToString();
		}

		public string ReadWord()
		{
			int start = Position;
			ConsumeNonWhitespace();
			return Slice.Slice(start, Position).ToString();
		}

		public string ReadWordUntil(char stopAtChar)
		{
			int start = Position;
			while (Position < Slice.Length)
			{
				char c = Current;
				if (c == stopAtChar || char.IsWhiteSpace(c))
				{
					break;
				}
				Position++;
			}
			return Slice.Slice(start, Position).ToString();
		}

		public string ReadNumber(bool digitsOnly = false)
		{
			int start = Position;
			while (Position < Slice.Length)
			{
				char c = Current;
				if ((digitsOnly && !char.IsDigit(c)) || !IsValidNumericFormatChar(c))
				{
					break;
				}
				Position++;
			}
			return Slice.Slice(start, Position).ToString();
		}

		private static bool IsValidNumericFormatChar(char c)
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

		public string ReadUntil(char stopAtChar)
		{
			int start = Position;
			while (Position < Slice.Length)
			{
				char c = Current;
				if (c == stopAtChar)
				{
					break;
				}
				Position++;
			}
			return Slice.Slice(start, Position).ToString();
		}

		public string ReadUntilLast(char stopAtChar)
		{
			int start = Position;
			int length = Slice.Length - start;
			for (int i = Slice.Length - 1; i >= start; i--)
			{
				char c = Slice[i];
				if (c == stopAtChar)
				{
					length = i - start;
					break;
				}
			}
			Position += length;
			return Slice.Slice(start, Position).ToString();
		}

		public string ReadString()
		{
			if (TryConsume("null"))
			{
				return null;
			}
			if (TryConsume("\"\""))
			{
				return string.Empty;
			}
			Consume('"');
			StringBuffer.Clear();
			while (Position < Slice.Length)
			{
				char c = Current;
				Position++;
				switch (c)
				{
				case '"':
					return StringBuffer.ToString();
				case '\\':
					if (Position < Slice.Length)
					{
						char next = Current;
						switch (next)
						{
						case 't':
							StringBuffer.Append('\t');
							break;
						case 'r':
							StringBuffer.Append('\r');
							break;
						case 'n':
							StringBuffer.Append('\n');
							break;
						case '"':
							StringBuffer.Append(next);
							break;
						default:
							StringBuffer.Append(c);
							continue;
						}
						Position++;
						continue;
					}
					break;
				}
				StringBuffer.Append(c);
			}
			return StringBuffer.ToString();
		}

		public int ReadInt32()
		{
			return int.Parse(ReadNumber(digitsOnly: true));
		}

		public object ReadValue(Type memberType)
		{
			if (memberType == typeof(string))
			{
				return ReadString();
			}
			if (memberType == typeof(Type))
			{
				return TwoWaySerializationBinder.Default.BindToType(ReadRemainder());
			}
			if (memberType == typeof(ColumnSize))
			{
				return ColumnSizeParser.Parse(ReadRemainder());
			}
			if (memberType == typeof(Color))
			{
				string text = ReadRemainder();
				if (ColorUtility.TryParseHtmlString(text, out var color))
				{
					return color;
				}
				return Color.clear;
			}
			string valueString = ReadWord();
			if (memberType.IsEnum)
			{
				return Enum.Parse(memberType, valueString);
			}
			if (memberType.IsPrimitive)
			{
				switch (Type.GetTypeCode(memberType))
				{
				case TypeCode.Boolean:
					return string.Equals(valueString, "true", StringComparison.InvariantCultureIgnoreCase);
				case TypeCode.Byte:
					return byte.Parse(valueString);
				case TypeCode.Char:
					return char.Parse(valueString);
				case TypeCode.Decimal:
					return decimal.Parse(valueString);
				case TypeCode.Double:
					return double.Parse(valueString);
				case TypeCode.Int16:
					return short.Parse(valueString);
				case TypeCode.Int32:
					return int.Parse(valueString);
				case TypeCode.Int64:
					return long.Parse(valueString);
				case TypeCode.SByte:
					return sbyte.Parse(valueString);
				case TypeCode.Single:
					return float.Parse(valueString);
				case TypeCode.UInt16:
					return ushort.Parse(valueString);
				case TypeCode.UInt32:
					return uint.Parse(valueString);
				case TypeCode.UInt64:
					return ulong.Parse(valueString);
				}
			}
			return null;
		}
	}
}
