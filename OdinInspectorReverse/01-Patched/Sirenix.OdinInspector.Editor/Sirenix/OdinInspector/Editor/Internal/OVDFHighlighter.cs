using System;
using System.Collections.Generic;
using System.Text;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal static class OVDFHighlighter
	{
		private struct LineRange
		{
			public int Start;

			public int End;

			public int Length => End - Start;
		}

		private struct Span
		{
			public int Start;

			public int Length;

			public string Color;
		}

		private const string Header = "#808080";

		private const string TypeLine = "#89B4FA";

		private const string BlockHash = "#89B4FA";

		private const string BlockRefDollarAndWord = "#89DCEB";

		private const string Keyword = "#CBA6F7";

		private const string Colon = "#808080";

		private const string PositionIndex = "#CBA6F7";

		private const string PositionRefAndWords = "#89DCEB";

		private const string AttrOpAdd = "#A6E3A1";

		private const string AttrOpRemove = "#F38BA8";

		private const string AttrOpModify = "#F9E2AF";

		private const string Bracket = "#808080";

		private const string AttrTypeAdd = "#A6E3A1";

		private const string AttrTypeRemove = "#F38BA8";

		private const string AttrTypeModify = "#F9E2AF";

		private const string MemberName = "#CCCCCC";

		private const string CEquals = "#808080";

		private const string ValueString = "#F9E2AF";

		private const string ValueBoolean = "#B4BEFE";

		private const string ValueIdentifier = "#B4BEFE";

		private const string ValueNull = "#B4BEFE";

		private const string ValueOther = "#B4BEFE";

		private const string ValueNumber = "#F5C2E7";

		private const string ValueColorHex = "#B4BEFE";

		public static string Highlight(string text)
		{
			if (string.IsNullOrEmpty(text))
			{
				return string.Empty;
			}
			List<OVDFTokenizer.Token> tokens = OVDFTokenizer.Tokenize(text);
			List<LineRange> lineRanges = ComputeLineRanges(text, tokens);
			List<Span> spans = new List<Span>(256);
			int tokenIndex = 0;
			for (int lineIdx = 0; lineIdx < lineRanges.Count; lineIdx++)
			{
				LineRange line = lineRanges[lineIdx];
				int firstNonWs = FindFirstNonWhitespace(text, line.Start, line.End);
				if (firstNonWs < 0)
				{
					continue;
				}
				switch (lineIdx)
				{
				case 0:
					AddSpan(spans, line.Start, line.Length, "#808080");
					continue;
				case 1:
					AddSpan(spans, line.Start, line.Length, "#89B4FA");
					continue;
				}
				List<OVDFTokenizer.Token> lineTokens = GatherTokensForLine(tokens, ref tokenIndex, line.Start, line.End);
				if (lineTokens.Count == 0)
				{
					continue;
				}
				if (lineIdx == 2 && lineTokens[0].Kind == OVDFTokenizer.TokenKind.Identifier && IsIdentifierEqual(text, lineTokens[0], "MetaGuid:"))
				{
					AddSpan(spans, line.Start, line.Length, "#808080");
					continue;
				}
				OVDFTokenizer.Token firstTok = lineTokens[0];
				if (firstTok.Kind == OVDFTokenizer.TokenKind.Hash)
				{
					AddSpan(spans, firstTok.Start, firstTok.Length, "#89B4FA");
					if (lineTokens.Count >= 2)
					{
						OVDFTokenizer.Token t1 = lineTokens[1];
						if (t1.Kind == OVDFTokenizer.TokenKind.Reference)
						{
							AddSpan(spans, t1.Start, t1.Length, "#89DCEB");
						}
						else if (t1.Kind == OVDFTokenizer.TokenKind.Identifier)
						{
							AddSpan(spans, t1.Start, t1.Length, "#89DCEB");
						}
						else if (t1.Kind == OVDFTokenizer.TokenKind.StringStart)
						{
							ColorFullString(spans, text, lineTokens, 1, "#89DCEB", line.End);
						}
					}
					continue;
				}
				if (IsIdentifierEqual(text, firstTok, "Position"))
				{
					AddSpan(spans, firstTok.Start, firstTok.Length, "#CBA6F7");
					int i = 1;
					if (!TryColorColon(lineTokens, ref i, spans) || i >= lineTokens.Count)
					{
						continue;
					}
					OVDFTokenizer.Token v = lineTokens[i];
					if (v.Kind == OVDFTokenizer.TokenKind.Reference)
					{
						AddSpan(spans, v.Start, v.Length, "#89DCEB");
						i++;
					}
					else if (v.Kind == OVDFTokenizer.TokenKind.Identifier)
					{
						AddSpan(spans, v.Start, v.Length, "#89DCEB");
						i++;
					}
					else if (v.Kind == OVDFTokenizer.TokenKind.StringStart)
					{
						ColorFullString(spans, text, lineTokens, i, "#89DCEB", line.End);
						i = AdvanceToAfterString(lineTokens, i);
					}
					if (i < lineTokens.Count && lineTokens[i].Kind == OVDFTokenizer.TokenKind.Colon)
					{
						AddSpan(spans, lineTokens[i].Start, lineTokens[i].Length, "#808080");
						i++;
						if (i < lineTokens.Count && lineTokens[i].Kind == OVDFTokenizer.TokenKind.Number)
						{
							AddSpan(spans, lineTokens[i].Start, lineTokens[i].Length, "#CBA6F7");
						}
					}
					continue;
				}
				if (IsIdentifierEqual(text, firstTok, "Visibility"))
				{
					AddSpan(spans, firstTok.Start, firstTok.Length, "#CBA6F7");
					int i2 = 1;
					if (TryColorColon(lineTokens, ref i2, spans) && i2 < lineTokens.Count && lineTokens[i2].Kind == OVDFTokenizer.TokenKind.Identifier)
					{
						AddSpan(spans, lineTokens[i2].Start, lineTokens[i2].Length, "#B4BEFE");
					}
					continue;
				}
				if (firstTok.Kind == OVDFTokenizer.TokenKind.Plus || firstTok.Kind == OVDFTokenizer.TokenKind.Minus || firstTok.Kind == OVDFTokenizer.TokenKind.Star)
				{
					string opColor = ((firstTok.Kind == OVDFTokenizer.TokenKind.Plus) ? "#A6E3A1" : ((firstTok.Kind == OVDFTokenizer.TokenKind.Minus) ? "#F38BA8" : "#F9E2AF"));
					AddSpan(spans, firstTok.Start, firstTok.Length, opColor);
					int lb = FindFirst(lineTokens, 1, OVDFTokenizer.TokenKind.LeftBracket);
					int rb = FindLast(lineTokens, OVDFTokenizer.TokenKind.RightBracket);
					if (lb >= 0)
					{
						AddSpan(spans, lineTokens[lb].Start, lineTokens[lb].Length, "#808080");
					}
					if (rb >= 0)
					{
						AddSpan(spans, lineTokens[rb].Start, lineTokens[rb].Length, "#808080");
					}
					if (lb >= 0 && rb >= 0 && lineTokens[lb].Start + 1 <= lineTokens[rb].Start)
					{
						AddSpan(spans, lineTokens[lb].Start + 1, lineTokens[rb].Start - (lineTokens[lb].Start + 1), (opColor == "#A6E3A1") ? "#A6E3A1" : ((opColor == "#F38BA8") ? "#F38BA8" : "#F9E2AF"));
					}
					continue;
				}
				if (firstTok.Kind != OVDFTokenizer.TokenKind.Identifier)
				{
					continue;
				}
				AddSpan(spans, firstTok.Start, firstTok.Length, "#CCCCCC");
				int eqIdx = FindFirst(lineTokens, 1, OVDFTokenizer.TokenKind.Equal);
				if (eqIdx >= 0)
				{
					OVDFTokenizer.Token eq = lineTokens[eqIdx];
					AddSpan(spans, eq.Start, eq.Length, "#808080");
					int valIdx = eqIdx + 1;
					if (valIdx < lineTokens.Count)
					{
						ColorRhsValue(spans, text, lineTokens, valIdx, line.End);
					}
				}
			}
			return Inject(text, spans);
		}

		private static void ColorRhsValue(List<Span> spans, string text, List<OVDFTokenizer.Token> lineTokens, int startIdx, int lineEnd)
		{
			OVDFTokenizer.Token t = lineTokens[startIdx];
			switch (t.Kind)
			{
			case OVDFTokenizer.TokenKind.StringStart:
				ColorFullString(spans, text, lineTokens, startIdx, "#F9E2AF", lineEnd);
				break;
			case OVDFTokenizer.TokenKind.Color:
				AddSpan(spans, t.Start, t.Length, "#B4BEFE");
				break;
			case OVDFTokenizer.TokenKind.Number:
				AddSpan(spans, t.Start, t.Length, "#F5C2E7");
				break;
			case OVDFTokenizer.TokenKind.Bool:
				AddSpan(spans, t.Start, t.Length, "#B4BEFE");
				break;
			case OVDFTokenizer.TokenKind.Null:
				AddSpan(spans, t.Start, t.Length, "#B4BEFE");
				break;
			case OVDFTokenizer.TokenKind.Identifier:
				AddSpan(spans, t.Start, t.Length, "#B4BEFE");
				break;
			case OVDFTokenizer.TokenKind.Reference:
				AddSpan(spans, t.Start, t.Length, "#B4BEFE");
				break;
			default:
				AddSpan(spans, t.Start, Math.Max(0, lineEnd - t.Start), "#B4BEFE");
				break;
			}
		}

		private static void ColorFullString(List<Span> spans, string text, List<OVDFTokenizer.Token> lineTokens, int stringStartIdx, string color, int lineEnd)
		{
			OVDFTokenizer.Token startTok = lineTokens[stringStartIdx];
			int endIdx = FindMatchingStringEnd(lineTokens, stringStartIdx + 1);
			if (endIdx >= 0)
			{
				OVDFTokenizer.Token endTok = lineTokens[endIdx];
				int length = endTok.Start + endTok.Length - startTok.Start;
				if (length > 0)
				{
					AddSpan(spans, startTok.Start, length, color);
				}
			}
			else
			{
				AddSpan(spans, startTok.Start, Math.Max(0, lineEnd - startTok.Start), color);
			}
		}

		private static int AdvanceToAfterString(List<OVDFTokenizer.Token> lineTokens, int stringStartIdx)
		{
			for (int i = stringStartIdx + 1; i < lineTokens.Count; i++)
			{
				if (lineTokens[i].Kind == OVDFTokenizer.TokenKind.StringEnd)
				{
					return i + 1;
				}
			}
			return lineTokens.Count;
		}

		private static bool TryColorColon(List<OVDFTokenizer.Token> lineTokens, ref int i, List<Span> spans)
		{
			if (i < lineTokens.Count && lineTokens[i].Kind == OVDFTokenizer.TokenKind.Colon)
			{
				AddSpan(spans, lineTokens[i].Start, lineTokens[i].Length, "#808080");
				i++;
				return true;
			}
			return false;
		}

		private static List<LineRange> ComputeLineRanges(string text, List<OVDFTokenizer.Token> tokens)
		{
			List<LineRange> ranges = new List<LineRange>(64);
			int start = 0;
			for (int i = 0; i < tokens.Count; i++)
			{
				OVDFTokenizer.Token t = tokens[i];
				if (t.Kind == OVDFTokenizer.TokenKind.EndOfLine)
				{
					ranges.Add(new LineRange
					{
						Start = start,
						End = t.Start
					});
					start = t.Start + t.Length;
				}
			}
			if (start <= text.Length)
			{
				ranges.Add(new LineRange
				{
					Start = start,
					End = text.Length
				});
			}
			return ranges;
		}

		private static List<OVDFTokenizer.Token> GatherTokensForLine(List<OVDFTokenizer.Token> all, ref int cursor, int lineStart, int lineEnd)
		{
			while (cursor < all.Count && all[cursor].Start + all[cursor].Length <= lineStart)
			{
				cursor++;
			}
			List<OVDFTokenizer.Token> list = new List<OVDFTokenizer.Token>(16);
			for (int i = cursor; i < all.Count; i++)
			{
				OVDFTokenizer.Token t = all[i];
				if (t.Kind == OVDFTokenizer.TokenKind.EndOfLine || t.Kind == OVDFTokenizer.TokenKind.EndOfFile || t.Start >= lineEnd)
				{
					break;
				}
				if (t.Start >= lineStart && t.Start + t.Length <= lineEnd)
				{
					list.Add(t);
				}
			}
			return list;
		}

		private static int FindFirstNonWhitespace(string text, int start, int end)
		{
			for (int i = start; i < end; i++)
			{
				char c = text[i];
				if (!char.IsWhiteSpace(c))
				{
					return i;
				}
			}
			return -1;
		}

		private static bool IsIdentifierEqual(string source, OVDFTokenizer.Token token, string word)
		{
			if (token.Kind != OVDFTokenizer.TokenKind.Identifier)
			{
				return false;
			}
			if (token.Length != word.Length)
			{
				return false;
			}
			return string.Compare(source, token.Start, word, 0, token.Length, StringComparison.OrdinalIgnoreCase) == 0;
		}

		private static int FindFirst(List<OVDFTokenizer.Token> tokens, int startIndex, OVDFTokenizer.TokenKind kind)
		{
			for (int i = startIndex; i < tokens.Count; i++)
			{
				if (tokens[i].Kind == kind)
				{
					return i;
				}
			}
			return -1;
		}

		private static int FindLast(List<OVDFTokenizer.Token> tokens, OVDFTokenizer.TokenKind kind)
		{
			for (int i = tokens.Count - 1; i >= 0; i--)
			{
				if (tokens[i].Kind == kind)
				{
					return i;
				}
			}
			return -1;
		}

		private static int FindMatchingStringEnd(List<OVDFTokenizer.Token> tokens, int fromExclusive)
		{
			for (int i = fromExclusive; i < tokens.Count; i++)
			{
				if (tokens[i].Kind == OVDFTokenizer.TokenKind.StringEnd)
				{
					return i;
				}
			}
			return -1;
		}

		private static void AddSpan(List<Span> spans, int start, int length, string color)
		{
			if (length > 0)
			{
				spans.Add(new Span
				{
					Start = start,
					Length = length,
					Color = color
				});
			}
		}

		private static string Inject(string text, List<Span> spans)
		{
			if (spans == null || spans.Count == 0)
			{
				return text;
			}
			spans.Sort(delegate(Span a, Span b)
			{
				int num = a.Start.CompareTo(b.Start);
				return (num != 0) ? num : b.Length.CompareTo(a.Length);
			});
			List<Span> normalized = new List<Span>(spans.Count);
			int lastEnd = -1;
			for (int i = 0; i < spans.Count; i++)
			{
				Span s = spans[i];
				int sEnd = s.Start + s.Length;
				if (s.Start >= lastEnd)
				{
					normalized.Add(s);
					lastEnd = sEnd;
				}
			}
			StringBuilder sb = new StringBuilder(text.Length + normalized.Count * 32);
			int cursor = 0;
			for (int i2 = 0; i2 < normalized.Count; i2++)
			{
				Span s2 = normalized[i2];
				int start = Math.Max(0, Math.Min(s2.Start, text.Length));
				int end = Math.Max(0, Math.Min(s2.Start + s2.Length, text.Length));
				if (start > cursor)
				{
					sb.Append(text, cursor, start - cursor);
				}
				sb.Append("<color=").Append(s2.Color).Append('>');
				if (end > start)
				{
					sb.Append(text, start, end - start);
				}
				sb.Append("</color>");
				cursor = end;
			}
			if (cursor < text.Length)
			{
				sb.Append(text, cursor, text.Length - cursor);
			}
			return sb.ToString();
		}
	}
}
