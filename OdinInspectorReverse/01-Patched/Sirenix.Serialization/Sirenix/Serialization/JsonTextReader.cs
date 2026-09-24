using System;
using System.Collections.Generic;
using System.IO;

namespace Sirenix.Serialization
{
	/// <summary>
	/// Parses json entries from a stream.
	/// </summary>
	/// <seealso cref="T:System.IDisposable" />
	public class JsonTextReader : IDisposable
	{
		private static readonly Dictionary<char, EntryType?> EntryDelineators = new Dictionary<char, EntryType?>
		{
			{
				'{',
				EntryType.StartOfNode
			},
			{
				'}',
				EntryType.EndOfNode
			},
			{ ',', null },
			{
				'[',
				EntryType.PrimitiveArray
			},
			{
				']',
				EntryType.EndOfArray
			}
		};

		private static readonly Dictionary<char, char> UnescapeDictionary = new Dictionary<char, char>
		{
			{ 'a', '\a' },
			{ 'b', '\b' },
			{ 'f', '\f' },
			{ 'n', '\n' },
			{ 'r', '\r' },
			{ 't', '\t' },
			{ '0', '\0' }
		};

		private StreamReader reader;

		private int bufferIndex;

		private char[] buffer = new char[256];

		private char? lastReadChar;

		private char? peekedChar;

		private Queue<char> emergencyPlayback;

		/// <summary>
		/// The current deserialization context used by the text reader.
		/// </summary>
		public DeserializationContext Context { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Sirenix.Serialization.JsonTextReader" /> class.
		/// </summary>
		/// <param name="stream">The stream to parse from.</param>
		/// <param name="context">The deserialization context to use.</param>
		/// <exception cref="T:System.ArgumentNullException">The stream is null.</exception>
		/// <exception cref="T:System.ArgumentException">Cannot read from the stream.</exception>
		public JsonTextReader(Stream stream, DeserializationContext context)
		{
			if (stream == null)
			{
				throw new ArgumentNullException("stream");
			}
			if (context == null)
			{
				throw new ArgumentNullException("context");
			}
			if (!stream.CanRead)
			{
				throw new ArgumentException("Cannot read from stream");
			}
			reader = new StreamReader(stream);
			Context = context;
		}

		/// <summary>
		/// Resets the reader instance's currently peeked char and emergency playback queue.
		/// </summary>
		public void Reset()
		{
			peekedChar = null;
			if (emergencyPlayback != null)
			{
				emergencyPlayback.Clear();
			}
		}

		/// <summary>
		/// Disposes all resources kept by the text reader, except the stream, which can be reused later.
		/// </summary>
		public void Dispose()
		{
		}

		/// <summary>
		/// Reads to (but not past) the beginning of the next json entry, and returns the entry name, contents and type.
		/// </summary>
		/// <param name="name">The name of the entry that was parsed.</param>
		/// <param name="valueContent">The content of the entry that was parsed.</param>
		/// <param name="entry">The type of the entry that was parsed.</param>
		public void ReadToNextEntry(out string name, out string valueContent, out EntryType entry)
		{
			int valueSeparatorIndex = -1;
			bool insideString = false;
			bufferIndex = -1;
			while (!reader.EndOfStream)
			{
				char c = PeekChar();
				if (insideString && lastReadChar == '\\')
				{
					switch (c)
					{
					case '\\':
						lastReadChar = null;
						SkipChar();
						continue;
					case '0':
					case 'a':
					case 'b':
					case 'f':
					case 'n':
					case 'r':
					case 't':
						c = UnescapeDictionary[c];
						lastReadChar = c;
						buffer[bufferIndex] = c;
						SkipChar();
						continue;
					case 'u':
					{
						SkipChar();
						char c2 = ConsumeChar();
						char c3 = ConsumeChar();
						char c4 = ConsumeChar();
						char c5 = ConsumeChar();
						if (IsHex(c2) && IsHex(c3) && IsHex(c4) && IsHex(c5))
						{
							c = ParseHexChar(c2, c3, c4, c5);
							lastReadChar = c;
							buffer[bufferIndex] = c;
							continue;
						}
						Context.Config.DebugContext.LogError("A wild non-hex value appears at position " + reader.BaseStream.Position + "! \\-u-" + c2 + "-" + c3 + "-" + c4 + "-" + c5 + "; current buffer: '" + new string(buffer, 0, bufferIndex + 1) + "'. If the error handling policy is resilient, an attempt will be made to recover from this emergency without a fatal parse error...");
						lastReadChar = null;
						if (emergencyPlayback == null)
						{
							emergencyPlayback = new Queue<char>(5);
						}
						emergencyPlayback.Enqueue('u');
						emergencyPlayback.Enqueue(c2);
						emergencyPlayback.Enqueue(c3);
						emergencyPlayback.Enqueue(c4);
						emergencyPlayback.Enqueue(c5);
						continue;
					}
					}
				}
				if (!insideString && c == ':' && valueSeparatorIndex == -1)
				{
					valueSeparatorIndex = bufferIndex + 1;
				}
				EntryType? foundEntryType;
				if (c == '"')
				{
					if (insideString && lastReadChar == '\\')
					{
						lastReadChar = '"';
						buffer[bufferIndex] = '"';
						SkipChar();
					}
					else
					{
						ReadCharIntoBuffer();
						insideString = !insideString;
					}
				}
				else if (insideString)
				{
					ReadCharIntoBuffer();
				}
				else if (char.IsWhiteSpace(c))
				{
					SkipChar();
				}
				else if (EntryDelineators.TryGetValue(c, out foundEntryType))
				{
					if (foundEntryType.HasValue)
					{
						entry = foundEntryType.Value;
						switch (entry)
						{
						case EntryType.StartOfNode:
						{
							ConsumeChar();
							ParseEntryFromBuffer(out name, out valueContent, out var _, valueSeparatorIndex, EntryType.StartOfNode);
							break;
						}
						case EntryType.PrimitiveArray:
						{
							ConsumeChar();
							ParseEntryFromBuffer(out name, out valueContent, out var _, valueSeparatorIndex, EntryType.PrimitiveArray);
							break;
						}
						case EntryType.EndOfNode:
							if (bufferIndex == -1)
							{
								ConsumeChar();
								name = null;
								valueContent = null;
							}
							else
							{
								ParseEntryFromBuffer(out name, out valueContent, out entry, valueSeparatorIndex, null);
							}
							break;
						case EntryType.EndOfArray:
							if (bufferIndex == -1)
							{
								ConsumeChar();
								name = null;
								valueContent = null;
							}
							else
							{
								ParseEntryFromBuffer(out name, out valueContent, out entry, valueSeparatorIndex, null);
							}
							break;
						default:
							throw new NotImplementedException();
						}
						return;
					}
					SkipChar();
					if (bufferIndex != -1)
					{
						ParseEntryFromBuffer(out name, out valueContent, out entry, valueSeparatorIndex, null);
						return;
					}
				}
				else
				{
					ReadCharIntoBuffer();
				}
			}
			if (bufferIndex == -1)
			{
				name = null;
				valueContent = null;
				entry = EntryType.EndOfStream;
			}
			else
			{
				ParseEntryFromBuffer(out name, out valueContent, out entry, valueSeparatorIndex, EntryType.EndOfStream);
			}
		}

		private void ParseEntryFromBuffer(out string name, out string valueContent, out EntryType entry, int valueSeparatorIndex, EntryType? hintEntry)
		{
			if (bufferIndex >= 0)
			{
				if (valueSeparatorIndex == -1)
				{
					if (hintEntry.HasValue)
					{
						name = null;
						valueContent = new string(buffer, 0, bufferIndex + 1);
						entry = hintEntry.Value;
						return;
					}
					name = null;
					valueContent = new string(buffer, 0, bufferIndex + 1);
					EntryType? guessedPrimitiveType = GuessPrimitiveType(valueContent);
					if (guessedPrimitiveType.HasValue)
					{
						entry = guessedPrimitiveType.Value;
					}
					else
					{
						entry = EntryType.Invalid;
					}
					return;
				}
				if (buffer[0] == '"')
				{
					name = new string(buffer, 1, valueSeparatorIndex - 2);
				}
				else
				{
					name = new string(buffer, 0, valueSeparatorIndex);
				}
				if (StringComparer.Ordinal.Equals(name, "$rcontent") && hintEntry == EntryType.StartOfArray)
				{
					valueContent = null;
					entry = EntryType.StartOfArray;
					return;
				}
				if (StringComparer.Ordinal.Equals(name, "$pcontent") && hintEntry == EntryType.StartOfArray)
				{
					valueContent = null;
					entry = EntryType.PrimitiveArray;
					return;
				}
				if (StringComparer.Ordinal.Equals(name, "$iref"))
				{
					name = null;
					valueContent = new string(buffer, 0, bufferIndex + 1);
					entry = EntryType.InternalReference;
					return;
				}
				if (StringComparer.Ordinal.Equals(name, "$eref"))
				{
					name = null;
					valueContent = new string(buffer, 0, bufferIndex + 1);
					entry = EntryType.ExternalReferenceByIndex;
					return;
				}
				if (StringComparer.Ordinal.Equals(name, "$guidref"))
				{
					name = null;
					valueContent = new string(buffer, 0, bufferIndex + 1);
					entry = EntryType.ExternalReferenceByGuid;
					return;
				}
				if (StringComparer.Ordinal.Equals(name, "$strref"))
				{
					name = null;
					valueContent = new string(buffer, 0, bufferIndex + 1);
					entry = EntryType.ExternalReferenceByString;
					return;
				}
				if (StringComparer.Ordinal.Equals(name, "$fstrref"))
				{
					name = null;
					valueContent = new string(buffer, 0, bufferIndex + 1);
					entry = EntryType.ExternalReferenceByString;
					return;
				}
				if (bufferIndex >= valueSeparatorIndex)
				{
					valueContent = new string(buffer, valueSeparatorIndex + 1, bufferIndex - valueSeparatorIndex);
				}
				else
				{
					valueContent = null;
				}
				if (valueContent != null)
				{
					if (StringComparer.Ordinal.Equals(name, "$rlength"))
					{
						entry = EntryType.StartOfArray;
						return;
					}
					if (StringComparer.Ordinal.Equals(name, "$plength"))
					{
						entry = EntryType.PrimitiveArray;
						return;
					}
					if (valueContent.Length == 0 && hintEntry.HasValue)
					{
						entry = hintEntry.Value;
						return;
					}
					if (StringComparer.OrdinalIgnoreCase.Equals(valueContent, "null"))
					{
						entry = EntryType.Null;
						return;
					}
					if (StringComparer.Ordinal.Equals(valueContent, "{"))
					{
						entry = EntryType.StartOfNode;
						return;
					}
					if (StringComparer.Ordinal.Equals(valueContent, "}"))
					{
						entry = EntryType.EndOfNode;
						return;
					}
					if (StringComparer.Ordinal.Equals(valueContent, "["))
					{
						entry = EntryType.StartOfArray;
						return;
					}
					if (StringComparer.Ordinal.Equals(valueContent, "]"))
					{
						entry = EntryType.EndOfArray;
						return;
					}
					if (valueContent.StartsWith("$iref", StringComparison.Ordinal))
					{
						entry = EntryType.InternalReference;
						return;
					}
					if (valueContent.StartsWith("$eref", StringComparison.Ordinal))
					{
						entry = EntryType.ExternalReferenceByIndex;
						return;
					}
					if (valueContent.StartsWith("$guidref", StringComparison.Ordinal))
					{
						entry = EntryType.ExternalReferenceByGuid;
						return;
					}
					if (valueContent.StartsWith("$strref", StringComparison.Ordinal))
					{
						entry = EntryType.ExternalReferenceByString;
						return;
					}
					if (valueContent.StartsWith("$fstrref", StringComparison.Ordinal))
					{
						entry = EntryType.ExternalReferenceByString;
						return;
					}
					EntryType? guessedPrimitiveType2 = GuessPrimitiveType(valueContent);
					if (guessedPrimitiveType2.HasValue)
					{
						entry = guessedPrimitiveType2.Value;
						return;
					}
				}
			}
			if (hintEntry.HasValue)
			{
				name = null;
				valueContent = null;
				entry = hintEntry.Value;
				return;
			}
			if (bufferIndex == -1)
			{
				Context.Config.DebugContext.LogError("Failed to parse empty entry in the stream.");
			}
			else
			{
				Context.Config.DebugContext.LogError("Tried and failed to parse entry with content '" + new string(buffer, 0, bufferIndex + 1) + "'.");
			}
			if (hintEntry == EntryType.EndOfStream)
			{
				name = null;
				valueContent = null;
				entry = EntryType.EndOfStream;
			}
			else
			{
				name = null;
				valueContent = null;
				entry = EntryType.Invalid;
			}
		}

		private bool IsHex(char c)
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

		private uint ParseSingleChar(char c, uint multiplier)
		{
			uint p = 0u;
			if (c >= '0' && c <= '9')
			{
				p = (uint)(c - 48) * multiplier;
			}
			else if (c >= 'A' && c <= 'F')
			{
				p = (uint)(c - 65 + 10) * multiplier;
			}
			else if (c >= 'a' && c <= 'f')
			{
				p = (uint)(c - 97 + 10) * multiplier;
			}
			return p;
		}

		private char ParseHexChar(char c1, char c2, char c3, char c4)
		{
			uint p1 = ParseSingleChar(c1, 4096u);
			uint p2 = ParseSingleChar(c2, 256u);
			uint p3 = ParseSingleChar(c3, 16u);
			uint p4 = ParseSingleChar(c4, 1u);
			try
			{
				return (char)(p1 + p2 + p3 + p4);
			}
			catch (Exception)
			{
				Context.Config.DebugContext.LogError("Could not parse invalid hex values: " + c1 + c2 + c3 + c4);
				return ' ';
			}
		}

		private char ReadCharIntoBuffer()
		{
			bufferIndex++;
			if (bufferIndex >= buffer.Length - 1)
			{
				char[] newBuffer = new char[buffer.Length * 2];
				Buffer.BlockCopy(buffer, 0, newBuffer, 0, buffer.Length * 2);
				buffer = newBuffer;
			}
			char c = ConsumeChar();
			buffer[bufferIndex] = c;
			lastReadChar = c;
			return c;
		}

		private EntryType? GuessPrimitiveType(string content)
		{
			if (StringComparer.OrdinalIgnoreCase.Equals(content, "null"))
			{
				return EntryType.Null;
			}
			if (content.Length >= 2 && content[0] == '"' && content[content.Length - 1] == '"')
			{
				return EntryType.String;
			}
			if (content.Length == 36 && content.LastIndexOf('-') > 0)
			{
				return EntryType.Guid;
			}
			if (content.Contains(".") || content.Contains(","))
			{
				return EntryType.FloatingPoint;
			}
			if (StringComparer.OrdinalIgnoreCase.Equals(content, "true") || StringComparer.OrdinalIgnoreCase.Equals(content, "false"))
			{
				return EntryType.Boolean;
			}
			if (content.Length >= 1)
			{
				return EntryType.Integer;
			}
			return null;
		}

		private char PeekChar()
		{
			if (!peekedChar.HasValue)
			{
				if (emergencyPlayback != null && emergencyPlayback.Count > 0)
				{
					peekedChar = emergencyPlayback.Dequeue();
				}
				else
				{
					peekedChar = (char)reader.Read();
				}
			}
			return peekedChar.Value;
		}

		private void SkipChar()
		{
			if (!peekedChar.HasValue)
			{
				if (emergencyPlayback != null && emergencyPlayback.Count > 0)
				{
					emergencyPlayback.Dequeue();
				}
				else
				{
					reader.Read();
				}
			}
			else
			{
				peekedChar = null;
			}
		}

		private char ConsumeChar()
		{
			if (!peekedChar.HasValue)
			{
				if (emergencyPlayback != null && emergencyPlayback.Count > 0)
				{
					return emergencyPlayback.Dequeue();
				}
				return (char)reader.Read();
			}
			char? c = peekedChar;
			peekedChar = null;
			return c.Value;
		}
	}
}
