using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Sirenix.Serialization
{
	/// <summary>
	/// Not yet documented.
	/// </summary>
	public class SerializationNodeDataReader : BaseDataReader
	{
		private string peekedEntryName;

		private EntryType? peekedEntryType;

		private string peekedEntryData;

		private int currentIndex = -1;

		private List<SerializationNode> nodes;

		private Dictionary<Type, Delegate> primitiveTypeReaders;

		private bool IndexIsValid
		{
			get
			{
				if (nodes != null && currentIndex >= 0)
				{
					return currentIndex < nodes.Count;
				}
				return false;
			}
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public List<SerializationNode> Nodes
		{
			get
			{
				if (nodes == null)
				{
					nodes = new List<SerializationNode>();
				}
				return nodes;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException();
				}
				nodes = value;
			}
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public override Stream Stream
		{
			get
			{
				throw new NotSupportedException("This data reader has no stream.");
			}
			set
			{
				throw new NotSupportedException("This data reader has no stream.");
			}
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public SerializationNodeDataReader(DeserializationContext context)
			: base(null, context)
		{
			primitiveTypeReaders = new Dictionary<Type, Delegate>
			{
				{
					typeof(char),
					(Func<char>)delegate
					{
						ReadChar(out var value);
						return value;
					}
				},
				{
					typeof(sbyte),
					(Func<sbyte>)delegate
					{
						ReadSByte(out var value);
						return value;
					}
				},
				{
					typeof(short),
					(Func<short>)delegate
					{
						ReadInt16(out var value);
						return value;
					}
				},
				{
					typeof(int),
					(Func<int>)delegate
					{
						ReadInt32(out var value);
						return value;
					}
				},
				{
					typeof(long),
					(Func<long>)delegate
					{
						ReadInt64(out var value);
						return value;
					}
				},
				{
					typeof(byte),
					(Func<byte>)delegate
					{
						ReadByte(out var value);
						return value;
					}
				},
				{
					typeof(ushort),
					(Func<ushort>)delegate
					{
						ReadUInt16(out var value);
						return value;
					}
				},
				{
					typeof(uint),
					(Func<uint>)delegate
					{
						ReadUInt32(out var value);
						return value;
					}
				},
				{
					typeof(ulong),
					(Func<ulong>)delegate
					{
						ReadUInt64(out var value);
						return value;
					}
				},
				{
					typeof(decimal),
					(Func<decimal>)delegate
					{
						ReadDecimal(out var value);
						return value;
					}
				},
				{
					typeof(bool),
					(Func<bool>)delegate
					{
						ReadBoolean(out var value);
						return value;
					}
				},
				{
					typeof(float),
					(Func<float>)delegate
					{
						ReadSingle(out var value);
						return value;
					}
				},
				{
					typeof(double),
					(Func<double>)delegate
					{
						ReadDouble(out var value);
						return value;
					}
				},
				{
					typeof(Guid),
					(Func<Guid>)delegate
					{
						ReadGuid(out var value);
						return value;
					}
				}
			};
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public override void Dispose()
		{
			nodes = null;
			currentIndex = -1;
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public override void PrepareNewSerializationSession()
		{
			base.PrepareNewSerializationSession();
			currentIndex = -1;
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public override EntryType PeekEntry(out string name)
		{
			if (peekedEntryType.HasValue)
			{
				name = peekedEntryName;
				return peekedEntryType.Value;
			}
			currentIndex++;
			if (IndexIsValid)
			{
				SerializationNode node = nodes[currentIndex];
				peekedEntryName = node.Name;
				peekedEntryType = node.Entry;
				peekedEntryData = node.Data;
			}
			else
			{
				peekedEntryName = null;
				peekedEntryType = EntryType.EndOfStream;
				peekedEntryData = null;
			}
			name = peekedEntryName;
			return peekedEntryType.Value;
		}

		/// <summary>
		/// Tries to enters an array node. This will succeed if the next entry is an <see cref="F:Sirenix.Serialization.EntryType.StartOfArray" />.
		/// <para />
		/// This call MUST (eventually) be followed by a corresponding call to <see cref="!:IDataReader.ExitArray(DeserializationContext)" /><para />
		/// This call will change the values of the <see cref="P:Sirenix.Serialization.IDataReader.IsInArrayNode" />, <see cref="P:Sirenix.Serialization.IDataReader.CurrentNodeName" />, <see cref="P:Sirenix.Serialization.IDataReader.CurrentNodeId" /> and <see cref="P:Sirenix.Serialization.IDataReader.CurrentNodeDepth" /> properties to the correct values for the current array node.
		/// </summary>
		/// <param name="length">The length of the array that was entered.</param>
		/// <returns>
		///   <c>true</c> if an array was entered, otherwise <c>false</c>
		/// </returns>
		public override bool EnterArray(out long length)
		{
			PeekEntry();
			if (peekedEntryType == EntryType.StartOfArray)
			{
				PushArray();
				if (!long.TryParse(peekedEntryData, NumberStyles.Any, CultureInfo.InvariantCulture, out length))
				{
					length = 0L;
					base.Context.Config.DebugContext.LogError("Failed to parse array length from data '" + peekedEntryData + "'.");
				}
				ConsumeCurrentEntry();
				return true;
			}
			SkipEntry();
			length = 0L;
			return false;
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public override bool EnterNode(out Type type)
		{
			PeekEntry();
			if (peekedEntryType == EntryType.StartOfNode)
			{
				string data = peekedEntryData;
				int id = -1;
				type = null;
				if (!string.IsNullOrEmpty(data))
				{
					string typeName = null;
					int separator = data.IndexOf("|", StringComparison.InvariantCulture);
					int parsedId;
					if (separator >= 0)
					{
						typeName = data.Substring(separator + 1);
						string idStr = data.Substring(0, separator);
						if (int.TryParse(idStr, NumberStyles.Any, CultureInfo.InvariantCulture, out parsedId))
						{
							id = parsedId;
						}
						else
						{
							base.Context.Config.DebugContext.LogError("Failed to parse id string '" + idStr + "' from data '" + data + "'.");
						}
					}
					else if (int.TryParse(data, out parsedId))
					{
						id = parsedId;
					}
					else
					{
						typeName = data;
					}
					if (typeName != null)
					{
						type = base.Context.Binder.BindToType(typeName, base.Context.Config.DebugContext);
					}
				}
				ConsumeCurrentEntry();
				PushNode(peekedEntryName, id, type);
				return true;
			}
			SkipEntry();
			type = null;
			return false;
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public override bool ExitArray()
		{
			PeekEntry();
			while (peekedEntryType != EntryType.EndOfArray && peekedEntryType != EntryType.EndOfStream)
			{
				if (peekedEntryType == EntryType.EndOfNode)
				{
					base.Context.Config.DebugContext.LogError("Data layout mismatch; skipping past node boundary when exiting array.");
					ConsumeCurrentEntry();
				}
				SkipEntry();
			}
			if (peekedEntryType == EntryType.EndOfArray)
			{
				ConsumeCurrentEntry();
				PopArray();
				return true;
			}
			return false;
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public override bool ExitNode()
		{
			PeekEntry();
			while (peekedEntryType != EntryType.EndOfNode && peekedEntryType != EntryType.EndOfStream)
			{
				if (peekedEntryType == EntryType.EndOfArray)
				{
					base.Context.Config.DebugContext.LogError("Data layout mismatch; skipping past array boundary when exiting node.");
					ConsumeCurrentEntry();
				}
				SkipEntry();
			}
			if (peekedEntryType == EntryType.EndOfNode)
			{
				ConsumeCurrentEntry();
				PopNode(base.CurrentNodeName);
				return true;
			}
			return false;
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public override bool ReadBoolean(out bool value)
		{
			PeekEntry();
			try
			{
				if (peekedEntryType == EntryType.Boolean)
				{
					value = peekedEntryData == "true";
					return true;
				}
				value = false;
				return false;
			}
			finally
			{
				ConsumeCurrentEntry();
			}
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public override bool ReadByte(out byte value)
		{
			if (ReadUInt64(out var ulongValue))
			{
				try
				{
					value = checked((byte)ulongValue);
				}
				catch (OverflowException)
				{
					value = 0;
				}
				return true;
			}
			value = 0;
			return false;
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public override bool ReadChar(out char value)
		{
			PeekEntry();
			if (peekedEntryType == EntryType.String)
			{
				try
				{
					if (peekedEntryData.Length == 1)
					{
						value = peekedEntryData[0];
						return true;
					}
					base.Context.Config.DebugContext.LogWarning("Expected string of length 1 for char entry.");
					value = '\0';
					return false;
				}
				finally
				{
					ConsumeCurrentEntry();
				}
			}
			SkipEntry();
			value = '\0';
			return false;
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public override bool ReadDecimal(out decimal value)
		{
			PeekEntry();
			if (peekedEntryType == EntryType.FloatingPoint || peekedEntryType == EntryType.Integer)
			{
				try
				{
					if (!decimal.TryParse(peekedEntryData, NumberStyles.Any, CultureInfo.InvariantCulture, out value))
					{
						base.Context.Config.DebugContext.LogError("Failed to parse decimal value from entry data '" + peekedEntryData + "'.");
						return false;
					}
					return true;
				}
				finally
				{
					ConsumeCurrentEntry();
				}
			}
			SkipEntry();
			value = default(decimal);
			return false;
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public override bool ReadDouble(out double value)
		{
			PeekEntry();
			if (peekedEntryType == EntryType.FloatingPoint || peekedEntryType == EntryType.Integer)
			{
				try
				{
					if (!double.TryParse(peekedEntryData, NumberStyles.Any, CultureInfo.InvariantCulture, out value))
					{
						base.Context.Config.DebugContext.LogError("Failed to parse double value from entry data '" + peekedEntryData + "'.");
						return false;
					}
					return true;
				}
				finally
				{
					ConsumeCurrentEntry();
				}
			}
			SkipEntry();
			value = 0.0;
			return false;
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public override bool ReadExternalReference(out Guid guid)
		{
			PeekEntry();
			if (peekedEntryType == EntryType.ExternalReferenceByGuid)
			{
				try
				{
					if ((guid = new Guid(peekedEntryData)) != Guid.Empty)
					{
						return true;
					}
					guid = Guid.Empty;
					return false;
				}
				catch (FormatException)
				{
					guid = Guid.Empty;
					return false;
				}
				catch (OverflowException)
				{
					guid = Guid.Empty;
					return false;
				}
				finally
				{
					ConsumeCurrentEntry();
				}
			}
			SkipEntry();
			guid = Guid.Empty;
			return false;
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public override bool ReadExternalReference(out string id)
		{
			PeekEntry();
			if (peekedEntryType == EntryType.ExternalReferenceByString)
			{
				id = peekedEntryData;
				ConsumeCurrentEntry();
				return true;
			}
			SkipEntry();
			id = null;
			return false;
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public override bool ReadExternalReference(out int index)
		{
			PeekEntry();
			if (peekedEntryType == EntryType.ExternalReferenceByIndex)
			{
				try
				{
					if (!int.TryParse(peekedEntryData, NumberStyles.Any, CultureInfo.InvariantCulture, out index))
					{
						base.Context.Config.DebugContext.LogError("Failed to parse external index reference integer value from entry data '" + peekedEntryData + "'.");
						return false;
					}
					return true;
				}
				finally
				{
					ConsumeCurrentEntry();
				}
			}
			SkipEntry();
			index = 0;
			return false;
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public override bool ReadGuid(out Guid value)
		{
			PeekEntry();
			if (peekedEntryType == EntryType.Guid)
			{
				try
				{
					if ((value = new Guid(peekedEntryData)) != Guid.Empty)
					{
						return true;
					}
					value = Guid.Empty;
					return false;
				}
				catch (FormatException)
				{
					value = Guid.Empty;
					return false;
				}
				catch (OverflowException)
				{
					value = Guid.Empty;
					return false;
				}
				finally
				{
					ConsumeCurrentEntry();
				}
			}
			SkipEntry();
			value = Guid.Empty;
			return false;
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public override bool ReadInt16(out short value)
		{
			if (ReadInt64(out var longValue))
			{
				try
				{
					value = checked((short)longValue);
				}
				catch (OverflowException)
				{
					value = 0;
				}
				return true;
			}
			value = 0;
			return false;
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public override bool ReadInt32(out int value)
		{
			if (ReadInt64(out var longValue))
			{
				try
				{
					value = checked((int)longValue);
				}
				catch (OverflowException)
				{
					value = 0;
				}
				return true;
			}
			value = 0;
			return false;
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public override bool ReadInt64(out long value)
		{
			PeekEntry();
			if (peekedEntryType == EntryType.Integer)
			{
				try
				{
					if (!long.TryParse(peekedEntryData, NumberStyles.Any, CultureInfo.InvariantCulture, out value))
					{
						base.Context.Config.DebugContext.LogError("Failed to parse integer value from entry data '" + peekedEntryData + "'.");
						return false;
					}
					return true;
				}
				finally
				{
					ConsumeCurrentEntry();
				}
			}
			SkipEntry();
			value = 0L;
			return false;
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public override bool ReadInternalReference(out int id)
		{
			PeekEntry();
			if (peekedEntryType == EntryType.InternalReference)
			{
				try
				{
					if (!int.TryParse(peekedEntryData, NumberStyles.Any, CultureInfo.InvariantCulture, out id))
					{
						base.Context.Config.DebugContext.LogError("Failed to parse internal reference id integer value from entry data '" + peekedEntryData + "'.");
						return false;
					}
					return true;
				}
				finally
				{
					ConsumeCurrentEntry();
				}
			}
			SkipEntry();
			id = 0;
			return false;
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public override bool ReadNull()
		{
			PeekEntry();
			if (peekedEntryType == EntryType.Null)
			{
				ConsumeCurrentEntry();
				return true;
			}
			SkipEntry();
			return false;
		}

		/// <summary>
		/// Reads a primitive array value. This call will succeed if the next entry is an <see cref="F:Sirenix.Serialization.EntryType.PrimitiveArray" />.
		/// <para />
		/// If the call fails (and returns <c>false</c>), it will skip the current entry value, unless that entry is an <see cref="F:Sirenix.Serialization.EntryType.EndOfNode" /> or an <see cref="F:Sirenix.Serialization.EntryType.EndOfArray" />.
		/// </summary>
		/// <typeparam name="T">The element type of the primitive array. Valid element types can be determined using <see cref="M:Sirenix.Serialization.FormatterUtilities.IsPrimitiveArrayType(System.Type)" />.</typeparam>
		/// <param name="array">The resulting primitive array.</param>
		/// <returns>
		///   <c>true</c> if reading a primitive array succeeded, otherwise <c>false</c>
		/// </returns>
		/// <exception cref="T:System.ArgumentException">Type  + typeof(T).Name +  is not a valid primitive array type.</exception>
		public override bool ReadPrimitiveArray<T>(out T[] array)
		{
			if (!FormatterUtilities.IsPrimitiveArrayType(typeof(T)))
			{
				throw new ArgumentException("Type " + typeof(T).Name + " is not a valid primitive array type.");
			}
			if (peekedEntryType != EntryType.PrimitiveArray)
			{
				SkipEntry();
				array = null;
				return false;
			}
			if (typeof(T) == typeof(byte))
			{
				array = (T[])(object)ProperBitConverter.HexStringToBytes(peekedEntryData);
				return true;
			}
			PeekEntry();
			if (peekedEntryType != EntryType.PrimitiveArray)
			{
				DebugContext debugContext = base.Context.Config.DebugContext;
				string[] obj = new string[5]
				{
					"Expected entry of type '",
					EntryType.StartOfArray.ToString(),
					"' when reading primitive array but got entry of type '",
					null,
					null
				};
				EntryType? entryType = peekedEntryType;
				obj[3] = entryType.ToString();
				obj[4] = "'.";
				debugContext.LogError(string.Concat(obj));
				SkipEntry();
				array = new T[0];
				return false;
			}
			if (!long.TryParse(peekedEntryData, NumberStyles.Any, CultureInfo.InvariantCulture, out var length))
			{
				base.Context.Config.DebugContext.LogError("Failed to parse primitive array length from entry data '" + peekedEntryData + "'.");
				SkipEntry();
				array = new T[0];
				return false;
			}
			ConsumeCurrentEntry();
			PushArray();
			array = new T[length];
			Func<T> reader = (Func<T>)primitiveTypeReaders[typeof(T)];
			for (int i = 0; i < length; i++)
			{
				array[i] = reader();
			}
			ExitArray();
			return true;
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public override bool ReadSByte(out sbyte value)
		{
			if (ReadInt64(out var longValue))
			{
				try
				{
					value = checked((sbyte)longValue);
				}
				catch (OverflowException)
				{
					value = 0;
				}
				return true;
			}
			value = 0;
			return false;
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public override bool ReadSingle(out float value)
		{
			PeekEntry();
			if (peekedEntryType == EntryType.FloatingPoint || peekedEntryType == EntryType.Integer)
			{
				try
				{
					if (!float.TryParse(peekedEntryData, NumberStyles.Any, CultureInfo.InvariantCulture, out value))
					{
						base.Context.Config.DebugContext.LogError("Failed to parse float value from entry data '" + peekedEntryData + "'.");
						return false;
					}
					return true;
				}
				finally
				{
					ConsumeCurrentEntry();
				}
			}
			SkipEntry();
			value = 0f;
			return false;
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public override bool ReadString(out string value)
		{
			PeekEntry();
			if (peekedEntryType == EntryType.String)
			{
				value = peekedEntryData;
				ConsumeCurrentEntry();
				return true;
			}
			SkipEntry();
			value = null;
			return false;
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public override bool ReadUInt16(out ushort value)
		{
			if (ReadUInt64(out var ulongValue))
			{
				try
				{
					value = checked((ushort)ulongValue);
				}
				catch (OverflowException)
				{
					value = 0;
				}
				return true;
			}
			value = 0;
			return false;
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public override bool ReadUInt32(out uint value)
		{
			if (ReadUInt64(out var ulongValue))
			{
				try
				{
					value = checked((uint)ulongValue);
				}
				catch (OverflowException)
				{
					value = 0u;
				}
				return true;
			}
			value = 0u;
			return false;
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public override bool ReadUInt64(out ulong value)
		{
			PeekEntry();
			if (peekedEntryType == EntryType.Integer)
			{
				try
				{
					if (!ulong.TryParse(peekedEntryData, NumberStyles.Any, CultureInfo.InvariantCulture, out value))
					{
						base.Context.Config.DebugContext.LogError("Failed to parse integer value from entry data '" + peekedEntryData + "'.");
						return false;
					}
					return true;
				}
				finally
				{
					ConsumeCurrentEntry();
				}
			}
			SkipEntry();
			value = 0uL;
			return false;
		}

		public override string GetDataDump()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("Nodes: \n\n");
			for (int i = 0; i < nodes.Count; i++)
			{
				SerializationNode node = nodes[i];
				sb.Append("    - Name: " + node.Name);
				if (i == currentIndex)
				{
					sb.AppendLine("    <<<< READ POSITION");
				}
				else
				{
					sb.AppendLine();
				}
				int entry = (int)node.Entry;
				sb.AppendLine("      Entry: " + entry);
				sb.AppendLine("      Data: " + node.Data);
			}
			return sb.ToString();
		}

		private void ConsumeCurrentEntry()
		{
			if (peekedEntryType.HasValue && peekedEntryType != EntryType.EndOfStream)
			{
				peekedEntryType = null;
			}
		}

		/// <summary>
		/// Peeks the current entry.
		/// </summary>
		/// <returns>The peeked entry.</returns>
		protected override EntryType PeekEntry()
		{
			string name;
			return PeekEntry(out name);
		}

		/// <summary>
		/// Consumes the current entry, and reads to the next one.
		/// </summary>
		/// <returns>The next entry.</returns>
		protected override EntryType ReadToNextEntry()
		{
			ConsumeCurrentEntry();
			string name;
			return PeekEntry(out name);
		}
	}
}
