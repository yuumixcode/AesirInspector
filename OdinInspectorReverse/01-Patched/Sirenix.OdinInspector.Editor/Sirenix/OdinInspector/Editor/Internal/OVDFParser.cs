using System;
using System.Collections.Generic;
using System.Text;
using Sirenix.Serialization;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal static class OVDFParser
	{
		private sealed class Parser
		{
			private readonly string source;

			private readonly List<OVDFTokenizer.Token> tokens;

			private int current;

			private readonly OVDFFile file;

			public Parser(string source)
			{
				this.source = source;
				tokens = OVDFTokenizer.Tokenize(source);
				current = 0;
				file = new OVDFFile();
			}

			public OVDFFile Parse()
			{
				file.Header = ParseHeader(file);
				while (!IsAtEnd())
				{
					if (Match(OVDFTokenizer.TokenKind.Hash))
					{
						OVDFObject obj = ParseObject();
						if (obj != null)
						{
							file.Objects.Add(obj);
						}
					}
					else
					{
						Advance();
					}
				}
				return file;
			}

			private OVDFHeader ParseHeader(OVDFFile file)
			{
				OVDFHeader header = new OVDFHeader();
				Expect(OVDFTokenizer.TokenKind.Identifier, "OVDF");
				Expect(OVDFTokenizer.TokenKind.Identifier, out var versionToken);
				header.Version = GetText(versionToken).TrimStart('v', 'V');
				Consume(OVDFTokenizer.TokenKind.EndOfLine);
				if (!validOVDFVersions.Contains(header.Version))
				{
					string htmlColor = ColorUtility.ToHtmlStringRGB(SirenixGUIStyles.RedErrorColor);
					file.Diagnostics.Add(new Diagnostic
					{
						Severity = Severity.Error,
						Message = "<color=#" + htmlColor + ">OVDF-P-0002</color>: Invalid OVDF version \"" + header.Version + "\"."
					});
				}
				Expect(OVDFTokenizer.TokenKind.Identifier, out var typeToken);
				header.TargetTypeName = GetText(typeToken);
				Consume(OVDFTokenizer.TokenKind.Comma);
				Expect(OVDFTokenizer.TokenKind.Identifier, out var asmToken);
				header.TargetAssemblyName = GetText(asmToken);
				Match(OVDFTokenizer.TokenKind.EndOfLine);
				string typeName = header.TargetTypeName + ", " + header.TargetAssemblyName;
				Type type = TwoWaySerializationBinder.Default.BindToType(typeName);
				if (type == null)
				{
					string htmlColor2 = ColorUtility.ToHtmlStringRGB(SirenixGUIStyles.YellowWarningColor);
					file.Diagnostics.Add(new Diagnostic
					{
						Severity = Severity.Warning,
						Message = "<b><color=#" + htmlColor2 + ">OVDF-P-0003</color></b>: Targeted type \"" + typeName + "\" can't be found. Have you deleted or renamed the type?"
					});
				}
				else
				{
					header.Type = type;
				}
				if (header.Version == "1.1" && CheckIdentifier("MetaGuid:"))
				{
					Expect(OVDFTokenizer.TokenKind.Identifier, "MetaGuid:");
					Consume(OVDFTokenizer.TokenKind.Colon);
					if (Match(OVDFTokenizer.TokenKind.Identifier) || Match(OVDFTokenizer.TokenKind.Number))
					{
						header.Guid = GetText(Previous());
					}
					else
					{
						header.Guid = string.Empty;
					}
					Match(OVDFTokenizer.TokenKind.EndOfLine);
				}
				return header;
			}

			private OVDFObject ParseObject()
			{
				OVDFObject obj = new OVDFObject();
				if (Match(OVDFTokenizer.TokenKind.Reference))
				{
					obj.Id = GetText(Previous());
				}
				else if (Match(OVDFTokenizer.TokenKind.Identifier))
				{
					obj.Id = GetText(Previous());
				}
				else
				{
					EmitWarning("Expected identifier or reference after '#'");
				}
				while (!IsAtEnd() && !Check(OVDFTokenizer.TokenKind.EndOfLine))
				{
					Advance();
				}
				Match(OVDFTokenizer.TokenKind.EndOfLine);
				ParseObjectBody(obj);
				return obj;
			}

			private void ParseObjectBody(OVDFObject obj)
			{
				while (!IsAtEnd() && !Check(OVDFTokenizer.TokenKind.Hash))
				{
					if (Match(OVDFTokenizer.TokenKind.EndOfLine))
					{
						continue;
					}
					if (CheckIdentifier("Position"))
					{
						ParsePosition(obj);
					}
					else if (CheckIdentifier("Visibility"))
					{
						ParseVisibility(obj);
					}
					else if (Check(OVDFTokenizer.TokenKind.Plus) || Check(OVDFTokenizer.TokenKind.Star) || Check(OVDFTokenizer.TokenKind.Minus))
					{
						OVDFAttribute attr = ParseAttribute();
						if (attr != null)
						{
							obj.Attributes.Add(attr);
						}
					}
					else
					{
						SkipLine();
					}
				}
			}

			private void ParsePosition(OVDFObject obj)
			{
				Expect(OVDFTokenizer.TokenKind.Identifier, "Position");
				Consume(OVDFTokenizer.TokenKind.Colon);
				OVDFPosition position = new OVDFPosition();
				if (Match(OVDFTokenizer.TokenKind.Reference))
				{
					position.ParentId = GetText(Previous());
				}
				else if (Match(OVDFTokenizer.TokenKind.Identifier))
				{
					position.ParentId = GetText(Previous());
				}
				if (Match(OVDFTokenizer.TokenKind.Colon))
				{
					Expect(OVDFTokenizer.TokenKind.Number, out var numToken);
					if (int.TryParse(GetText(numToken), out var index))
					{
						position.Index = index;
					}
				}
				obj.Position = position;
				Match(OVDFTokenizer.TokenKind.EndOfLine);
			}

			private void ParseVisibility(OVDFObject obj)
			{
				Expect(OVDFTokenizer.TokenKind.Identifier, "Visibility");
				Consume(OVDFTokenizer.TokenKind.Colon);
				Expect(OVDFTokenizer.TokenKind.Identifier, out var valueToken);
				obj.Visibility = GetText(valueToken);
				Match(OVDFTokenizer.TokenKind.EndOfLine);
			}

			private OVDFAttribute ParseAttribute()
			{
				char prefix = ((Peek().Kind == OVDFTokenizer.TokenKind.Plus) ? '+' : ((Peek().Kind == OVDFTokenizer.TokenKind.Minus) ? '-' : '*'));
				Advance();
				Consume(OVDFTokenizer.TokenKind.LeftBracket);
				Expect(OVDFTokenizer.TokenKind.Identifier, out var typeToken);
				string typeName = GetText(typeToken);
				while (Check(OVDFTokenizer.TokenKind.Plus))
				{
					Advance();
					Expect(OVDFTokenizer.TokenKind.Identifier, out var nested);
					typeName = typeName + "+" + GetText(nested);
				}
				Consume(OVDFTokenizer.TokenKind.RightBracket);
				Match(OVDFTokenizer.TokenKind.EndOfLine);
				OVDFAttribute attr = new OVDFAttribute();
				attr.Prefix = prefix;
				attr.TypeName = typeName;
				while (Check(OVDFTokenizer.TokenKind.Identifier))
				{
					OVDFProperty prop = ParseProperty();
					if (prop != null)
					{
						attr.Parameters.Add(prop);
					}
				}
				return attr;
			}

			private OVDFProperty ParseProperty()
			{
				Expect(OVDFTokenizer.TokenKind.Identifier, out var keyToken);
				Consume(OVDFTokenizer.TokenKind.Equal);
				OVDFValue value = ParseValue();
				Match(OVDFTokenizer.TokenKind.EndOfLine);
				OVDFProperty prop = new OVDFProperty();
				prop.Name = GetText(keyToken);
				prop.Value = value;
				return prop;
			}

			private OVDFValue ParseValue()
			{
				if (Match(OVDFTokenizer.TokenKind.Reference))
				{
					OVDFValue val = new OVDFValue();
					val.Kind = OVDFValueKind.Reference;
					val.Value = GetText(Previous());
					return val;
				}
				if (Match(OVDFTokenizer.TokenKind.StringStart))
				{
					StringBuilder sb = new StringBuilder();
					while (!IsAtEnd() && !Check(OVDFTokenizer.TokenKind.StringEnd))
					{
						if (Match(OVDFTokenizer.TokenKind.StringContent))
						{
							sb.Append(GetText(Previous()));
						}
						else
						{
							Advance();
						}
					}
					Consume(OVDFTokenizer.TokenKind.StringEnd);
					OVDFValue val2 = new OVDFValue();
					val2.Kind = OVDFValueKind.String;
					val2.Value = sb.ToString();
					return val2;
				}
				if (Match(OVDFTokenizer.TokenKind.Number))
				{
					OVDFValue val3 = new OVDFValue();
					val3.Kind = OVDFValueKind.Number;
					val3.Value = GetText(Previous());
					return val3;
				}
				if (Match(OVDFTokenizer.TokenKind.Bool))
				{
					OVDFValue val4 = new OVDFValue();
					val4.Kind = OVDFValueKind.Bool;
					val4.Value = bool.Parse(GetText(Previous()));
					return val4;
				}
				if (Match(OVDFTokenizer.TokenKind.Null))
				{
					OVDFValue val5 = new OVDFValue();
					val5.Kind = OVDFValueKind.Null;
					val5.Value = null;
					return val5;
				}
				if (Match(OVDFTokenizer.TokenKind.Color))
				{
					OVDFValue val6 = new OVDFValue();
					val6.Kind = OVDFValueKind.Color;
					val6.Value = GetText(Previous());
					return val6;
				}
				if (Match(OVDFTokenizer.TokenKind.Identifier))
				{
					OVDFValue val7 = new OVDFValue();
					val7.Kind = OVDFValueKind.Identifier;
					val7.Value = GetText(Previous());
					return val7;
				}
				OVDFValue unknown = new OVDFValue();
				unknown.Kind = OVDFValueKind.Unknown;
				unknown.Value = null;
				return unknown;
			}

			private bool IsAtEnd()
			{
				return current >= tokens.Count;
			}

			private OVDFTokenizer.Token Peek()
			{
				return tokens[Math.Min(current, tokens.Count - 1)];
			}

			private OVDFTokenizer.Token Previous()
			{
				return tokens[current - 1];
			}

			private OVDFTokenizer.Token Advance()
			{
				OVDFTokenizer.Token token = Peek();
				current++;
				return token;
			}

			private bool Match(OVDFTokenizer.TokenKind kind)
			{
				if (Check(kind))
				{
					Advance();
					return true;
				}
				return false;
			}

			private bool Check(OVDFTokenizer.TokenKind kind)
			{
				if (IsAtEnd())
				{
					return false;
				}
				return Peek().Kind == kind;
			}

			private void Consume(OVDFTokenizer.TokenKind kind)
			{
				if (!Match(kind))
				{
					EmitWarning("Expected " + kind.ToString() + " but found " + Peek().Kind);
				}
			}

			private void Expect(OVDFTokenizer.TokenKind kind, string expectedText)
			{
				if (!Match(kind) || !TokenEquals(Previous(), expectedText, ignoreCase: true))
				{
					EmitWarning("Expected " + expectedText);
				}
			}

			private void Expect(OVDFTokenizer.TokenKind kind, out OVDFTokenizer.Token token)
			{
				if (!Match(kind))
				{
					EmitWarning("Expected " + kind);
				}
				token = Previous();
			}

			private bool CheckIdentifier(string text)
			{
				OVDFTokenizer.Token token = Peek();
				if (token.Kind != OVDFTokenizer.TokenKind.Identifier)
				{
					return false;
				}
				return TokenEquals(token, text, ignoreCase: true);
			}

			private void SkipLine()
			{
				while (!IsAtEnd() && !Check(OVDFTokenizer.TokenKind.EndOfLine))
				{
					Advance();
				}
				Match(OVDFTokenizer.TokenKind.EndOfLine);
			}

			private string GetText(OVDFTokenizer.Token token)
			{
				return source.Substring(token.Start, token.Length);
			}

			private bool TokenEquals(OVDFTokenizer.Token token, string text, bool ignoreCase)
			{
				if (token.Length != text.Length)
				{
					return false;
				}
				int start = token.Start;
				for (int i = 0; i < text.Length; i++)
				{
					char c1 = source[start + i];
					char c2 = text[i];
					if (ignoreCase)
					{
						if (char.ToUpperInvariant(c1) != char.ToUpperInvariant(c2))
						{
							return false;
						}
					}
					else if (c1 != c2)
					{
						return false;
					}
				}
				return true;
			}

			private void EmitWarning(string msg)
			{
			}
		}

		public class OVDFFile
		{
			public OVDFHeader Header = new OVDFHeader();

			public List<OVDFObject> Objects = new List<OVDFObject>();

			public List<Diagnostic> Diagnostics = new List<Diagnostic>();

			public bool HasDiagnostics => Diagnostics.Count > 0;
		}

		public class Diagnostic
		{
			public Severity Severity;

			public string Message;
		}

		public enum Severity
		{
			Info,
			Warning,
			Error
		}

		public class OVDFHeader
		{
			public string Version;

			public string TargetTypeName;

			public string TargetAssemblyName;

			public Type Type;

			public string Guid;
		}

		public class OVDFObject
		{
			public string Id;

			public OVDFPosition Position;

			public string Visibility;

			public List<OVDFAttribute> Attributes = new List<OVDFAttribute>();
		}

		public class OVDFPosition
		{
			public string ParentId;

			public int Index;
		}

		public class OVDFAttribute
		{
			public char Prefix;

			public string TypeName;

			public List<OVDFProperty> Parameters = new List<OVDFProperty>();
		}

		public class OVDFProperty
		{
			public string Name;

			public OVDFValue Value;
		}

		public class OVDFValue
		{
			public OVDFValueKind Kind;

			public object Value;
		}

		public enum OVDFValueKind
		{
			Identifier,
			Reference,
			String,
			Number,
			Bool,
			Null,
			Color,
			Unknown
		}

		private static readonly List<string> validOVDFVersions = new List<string> { "1.0", "1.1" };

		public static OVDFFile Parse(string source)
		{
			Parser parser = new Parser(source);
			return parser.Parse();
		}
	}
}
