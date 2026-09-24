using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Sirenix.Serialization
{
	/// <summary>
	/// Reads json data from a stream that has been written by a <see cref="T:Sirenix.Serialization.JsonDataWriter" />.
	/// </summary>
	/// <seealso cref="T:Sirenix.Serialization.BaseDataReader" />
	public class JsonDataReader : BaseDataReader
	{
		private JsonTextReader reader;

		private EntryType? peekedEntryType;

		private string peekedEntryName;

		private string peekedEntryContent;

		private Dictionary<int, Type> seenTypes = new Dictionary<int, Type>(16);

		private readonly Dictionary<Type, Delegate> primitiveArrayReaders;

		/// <summary>
		/// Gets or sets the base stream of the reader.
		/// </summary>
		/// <value>
		/// The base stream of the reader.
		/// </value>
		public override Stream Stream
		{
			get
			{
				return base.Stream;
			}
			set
			{
				base.Stream = value;
				reader = new JsonTextReader(base.Stream, base.Context);
			}
		}

		public JsonDataReader()
			: this(null, null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Sirenix.Serialization.JsonDataReader" /> class.
		/// </summary>
		/// <param name="stream">The base stream of the reader.</param>
		/// <param name="context">The deserialization context to use.</param>
		public JsonDataReader(Stream stream, DeserializationContext context)
			: base(stream, context)
		{
			primitiveArrayReaders = new Dictionary<Type, Delegate>
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
		/// Disposes all resources kept by the data reader, except the stream, which can be reused later.
		/// </summary>
		public override void Dispose()
		{
			reader.Dispose();
		}

		/// <summary>
		/// Peeks ahead and returns the type of the next entry in the stream.
		/// </summary>
		/// <param name="name">The name of the next entry, if it has one.</param>
		/// <returns>
		/// The type of the next entry.
		/// </returns>
		public override EntryType PeekEntry(out string name)
		{
			if (peekedEntryType.HasValue)
			{
				name = peekedEntryName;
				return peekedEntryType.Value;
			}
			reader.ReadToNextEntry(out name, out peekedEntryContent, out var entry);
			peekedEntryName = name;
			peekedEntryType = entry;
			return entry;
		}

		/// <summary>
		/// Tries to enter a node. This will succeed if the next entry is an <see cref="F:Sirenix.Serialization.EntryType.StartOfNode" />.
		/// <para />
		/// This call MUST (eventually) be followed by a corresponding call to <see cref="!:IDataReader.ExitNode(DeserializationContext)" /><para />
		/// This call will change the values of the <see cref="P:Sirenix.Serialization.IDataReader.IsInArrayNode" />, <see cref="P:Sirenix.Serialization.IDataReader.CurrentNodeName" />, <see cref="P:Sirenix.Serialization.IDataReader.CurrentNodeId" /> and <see cref="P:Sirenix.Serialization.IDataReader.CurrentNodeDepth" /> properties to the correct values for the current node.
		/// </summary>
		/// <param name="type">The type of the node. This value will be null if there was no metadata, or if the reader's serialization binder failed to resolve the type name.</param>
		/// <returns>
		///   <c>true</c> if entering a node succeeded, otherwise <c>false</c>
		/// </returns>
		public override bool EnterNode(out Type type)
		{
			PeekEntry();
			if (peekedEntryType == EntryType.StartOfNode)
			{
				string nodeName = peekedEntryName;
				int id = -1;
				ReadToNextEntry();
				if (peekedEntryName == "$id")
				{
					if (!int.TryParse(peekedEntryContent, NumberStyles.Any, CultureInfo.InvariantCulture, out id))
					{
						base.Context.Config.DebugContext.LogError("Failed to parse id: " + peekedEntryContent);
						id = -1;
					}
					ReadToNextEntry();
				}
				if (peekedEntryName == "$type" && peekedEntryContent != null && peekedEntryContent.Length > 0)
				{
					if (peekedEntryType == EntryType.Integer)
					{
						if (ReadInt32(out var typeID))
						{
							if (!seenTypes.TryGetValue(typeID, out type))
							{
								base.Context.Config.DebugContext.LogError("Missing type id for node with reference id " + id + ": " + typeID);
							}
						}
						else
						{
							base.Context.Config.DebugContext.LogError("Failed to read type id for node with reference id " + id);
							type = null;
						}
					}
					else
					{
						int typeNameStartIndex = 1;
						int typeID2 = -1;
						int idSplitIndex = peekedEntryContent.IndexOf('|');
						if (idSplitIndex >= 0)
						{
							typeNameStartIndex = idSplitIndex + 1;
							string idStr = peekedEntryContent.Substring(1, idSplitIndex - 1);
							if (!int.TryParse(idStr, NumberStyles.Any, CultureInfo.InvariantCulture, out typeID2))
							{
								typeID2 = -1;
							}
						}
						type = base.Context.Binder.BindToType(peekedEntryContent.Substring(typeNameStartIndex, peekedEntryContent.Length - (1 + typeNameStartIndex)), base.Context.Config.DebugContext);
						if (typeID2 >= 0)
						{
							seenTypes[typeID2] = type;
						}
						peekedEntryType = null;
					}
				}
				else
				{
					type = null;
				}
				PushNode(nodeName, id, type);
				return true;
			}
			SkipEntry();
			type = null;
			return false;
		}

		/// <summary>
		/// Exits the current node. This method will keep skipping entries using <see cref="!:IDataReader.SkipEntry(DeserializationContext)" /> until an <see cref="F:Sirenix.Serialization.EntryType.EndOfNode" /> is reached, or the end of the stream is reached.
		/// <para />
		/// This call MUST have been preceded by a corresponding call to <see cref="M:Sirenix.Serialization.IDataReader.EnterNode(System.Type@)" />.
		/// <para />
		/// This call will change the values of the <see cref="P:Sirenix.Serialization.IDataReader.IsInArrayNode" />, <see cref="P:Sirenix.Serialization.IDataReader.CurrentNodeName" />, <see cref="P:Sirenix.Serialization.IDataReader.CurrentNodeId" /> and <see cref="P:Sirenix.Serialization.IDataReader.CurrentNodeDepth" /> to the correct values for the node that was prior to the current node.
		/// </summary>
		/// <returns>
		///   <c>true</c> if the method exited a node, <c>false</c> if it reached the end of the stream.
		/// </returns>
		public override bool ExitNode()
		{
			PeekEntry();
			while (peekedEntryType != EntryType.EndOfNode && peekedEntryType != EntryType.EndOfStream)
			{
				if (peekedEntryType == EntryType.EndOfArray)
				{
					base.Context.Config.DebugContext.LogError("Data layout mismatch; skipping past array boundary when exiting node.");
					peekedEntryType = null;
				}
				SkipEntry();
			}
			if (peekedEntryType == EntryType.EndOfNode)
			{
				peekedEntryType = null;
				PopNode(base.CurrentNodeName);
				return true;
			}
			return false;
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
				if (peekedEntryName != "$rlength")
				{
					base.Context.Config.DebugContext.LogError("Array entry wasn't preceded by an array length entry!");
					length = 0L;
					return true;
				}
				if (!int.TryParse(peekedEntryContent, NumberStyles.Any, CultureInfo.InvariantCulture, out var intLength))
				{
					base.Context.Config.DebugContext.LogError("Failed to parse array length: " + peekedEntryContent);
					length = 0L;
					return true;
				}
				length = intLength;
				ReadToNextEntry();
				if (peekedEntryName != "$rcontent")
				{
					base.Context.Config.DebugContext.LogError("Failed to find regular array content entry after array length entry!");
					length = 0L;
					return true;
				}
				peekedEntryType = null;
				return true;
			}
			SkipEntry();
			length = 0L;
			return false;
		}

		/// <summary>
		/// Exits the closest array. This method will keep skipping entries using <see cref="!:IDataReader.SkipEntry(DeserializationContext)" /> until an <see cref="F:Sirenix.Serialization.EntryType.EndOfArray" /> is reached, or the end of the stream is reached.
		/// <para />
		/// This call MUST have been preceded by a corresponding call to <see cref="M:Sirenix.Serialization.IDataReader.EnterArray(System.Int64@)" />.
		/// <para />
		/// This call will change the values of the <see cref="P:Sirenix.Serialization.IDataReader.IsInArrayNode" />, <see cref="P:Sirenix.Serialization.IDataReader.CurrentNodeName" />, <see cref="P:Sirenix.Serialization.IDataReader.CurrentNodeId" /> and <see cref="P:Sirenix.Serialization.IDataReader.CurrentNodeDepth" /> to the correct values for the node that was prior to the exited array node.
		/// </summary>
		/// <returns>
		///   <c>true</c> if the method exited an array, <c>false</c> if it reached the end of the stream.
		/// </returns>
		public override bool ExitArray()
		{
			PeekEntry();
			while (peekedEntryType != EntryType.EndOfArray && peekedEntryType != EntryType.EndOfStream)
			{
				if (peekedEntryType == EntryType.EndOfNode)
				{
					base.Context.Config.DebugContext.LogError("Data layout mismatch; skipping past node boundary when exiting array.");
					peekedEntryType = null;
				}
				SkipEntry();
			}
			if (peekedEntryType == EntryType.EndOfArray)
			{
				peekedEntryType = null;
				PopArray();
				return true;
			}
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
			PeekEntry();
			if (peekedEntryType == EntryType.PrimitiveArray)
			{
				PushArray();
				if (peekedEntryName != "$plength")
				{
					base.Context.Config.DebugContext.LogError("Array entry wasn't preceded by an array length entry!");
					array = null;
					return false;
				}
				if (!int.TryParse(peekedEntryContent, NumberStyles.Any, CultureInfo.InvariantCulture, out var intLength))
				{
					base.Context.Config.DebugContext.LogError("Failed to parse array length: " + peekedEntryContent);
					array = null;
					return false;
				}
				ReadToNextEntry();
				if (peekedEntryName != "$pcontent")
				{
					base.Context.Config.DebugContext.LogError("Failed to find primitive array content entry after array length entry!");
					array = null;
					return false;
				}
				peekedEntryType = null;
				Func<T> reader = (Func<T>)primitiveArrayReaders[typeof(T)];
				array = new T[intLength];
				for (int i = 0; i < intLength; i++)
				{
					array[i] = reader();
				}
				ExitArray();
				return true;
			}
			SkipEntry();
			array = null;
			return false;
		}

		/// <summary>
		/// Reads a <see cref="T:System.Boolean" /> value. This call will succeed if the next entry is an <see cref="F:Sirenix.Serialization.EntryType.Boolean" />.
		/// <para />
		/// If the call fails (and returns <c>false</c>), it will skip the current entry value, unless that entry is an <see cref="F:Sirenix.Serialization.EntryType.EndOfNode" /> or an <see cref="F:Sirenix.Serialization.EntryType.EndOfArray" />.
		/// </summary>
		/// <param name="value">The value that has been read.</param>
		/// <returns>
		///   <c>true</c> if reading the value succeeded, otherwise <c>false</c>
		/// </returns>
		public override bool ReadBoolean(out bool value)
		{
			PeekEntry();
			if (peekedEntryType == EntryType.Boolean)
			{
				try
				{
					value = peekedEntryContent == "true";
					return true;
				}
				finally
				{
					MarkEntryConsumed();
				}
			}
			SkipEntry();
			value = false;
			return false;
		}

		/// <summary>
		/// Reads an internal reference id. This call will succeed if the next entry is an <see cref="F:Sirenix.Serialization.EntryType.InternalReference" />.
		/// <para />
		/// If the call fails (and returns <c>false</c>), it will skip the current entry value, unless that entry is an <see cref="F:Sirenix.Serialization.EntryType.EndOfNode" /> or an <see cref="F:Sirenix.Serialization.EntryType.EndOfArray" />.
		/// </summary>
		/// <param name="id">The internal reference id.</param>
		/// <returns>
		///   <c>true</c> if reading the value succeeded, otherwise <c>false</c>
		/// </returns>
		public override bool ReadInternalReference(out int id)
		{
			PeekEntry();
			if (peekedEntryType == EntryType.InternalReference)
			{
				try
				{
					return ReadAnyIntReference(out id);
				}
				finally
				{
					MarkEntryConsumed();
				}
			}
			SkipEntry();
			id = -1;
			return false;
		}

		/// <summary>
		/// Reads an external reference index. This call will succeed if the next entry is an <see cref="F:Sirenix.Serialization.EntryType.ExternalReferenceByIndex" />.
		/// <para />
		/// If the call fails (and returns <c>false</c>), it will skip the current entry value, unless that entry is an <see cref="F:Sirenix.Serialization.EntryType.EndOfNode" /> or an <see cref="F:Sirenix.Serialization.EntryType.EndOfArray" />.
		/// </summary>
		/// <param name="index">The external reference index.</param>
		/// <returns>
		///   <c>true</c> if reading the value succeeded, otherwise <c>false</c>
		/// </returns>
		public override bool ReadExternalReference(out int index)
		{
			PeekEntry();
			if (peekedEntryType == EntryType.ExternalReferenceByIndex)
			{
				try
				{
					return ReadAnyIntReference(out index);
				}
				finally
				{
					MarkEntryConsumed();
				}
			}
			SkipEntry();
			index = -1;
			return false;
		}

		/// <summary>
		/// Reads an external reference guid. This call will succeed if the next entry is an <see cref="F:Sirenix.Serialization.EntryType.ExternalReferenceByGuid" />.
		/// <para />
		/// If the call fails (and returns <c>false</c>), it will skip the current entry value, unless that entry is an <see cref="F:Sirenix.Serialization.EntryType.EndOfNode" /> or an <see cref="F:Sirenix.Serialization.EntryType.EndOfArray" />.
		/// </summary>
		/// <param name="guid">The external reference guid.</param>
		/// <returns>
		///   <c>true</c> if reading the value succeeded, otherwise <c>false</c>
		/// </returns>
		public override bool ReadExternalReference(out Guid guid)
		{
			PeekEntry();
			if (peekedEntryType == EntryType.ExternalReferenceByGuid)
			{
				string guidStr = peekedEntryContent;
				if (guidStr.StartsWith("$guidref"))
				{
					guidStr = guidStr.Substring("$guidref".Length + 1);
				}
				try
				{
					guid = new Guid(guidStr);
					return true;
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
					MarkEntryConsumed();
				}
			}
			SkipEntry();
			guid = Guid.Empty;
			return false;
		}

		/// <summary>
		/// Reads an external reference string. This call will succeed if the next entry is an <see cref="F:Sirenix.Serialization.EntryType.ExternalReferenceByString" />.
		/// <para />
		/// If the call fails (and returns <c>false</c>), it will skip the current entry value, unless that entry is an <see cref="F:Sirenix.Serialization.EntryType.EndOfNode" /> or an <see cref="F:Sirenix.Serialization.EntryType.EndOfArray" />.
		/// </summary>
		/// <param name="id">The external reference string.</param>
		/// <returns>
		///   <c>true</c> if reading the value succeeded, otherwise <c>false</c>
		/// </returns>
		public override bool ReadExternalReference(out string id)
		{
			PeekEntry();
			if (peekedEntryType == EntryType.ExternalReferenceByString)
			{
				id = peekedEntryContent;
				if (id.StartsWith("$strref"))
				{
					id = id.Substring("$strref".Length + 1);
				}
				else if (id.StartsWith("$fstrref"))
				{
					id = id.Substring("$fstrref".Length + 2, id.Length - ("$fstrref".Length + 3));
				}
				MarkEntryConsumed();
				return true;
			}
			SkipEntry();
			id = null;
			return false;
		}

		/// <summary>
		/// Reads a <see cref="T:System.Char" /> value. This call will succeed if the next entry is an <see cref="F:Sirenix.Serialization.EntryType.String" />.
		/// <para />
		/// If the string of the entry is longer than 1 character, the first character of the string will be taken as the result.
		/// <para />
		/// If the call fails (and returns <c>false</c>), it will skip the current entry value, unless that entry is an <see cref="F:Sirenix.Serialization.EntryType.EndOfNode" /> or an <see cref="F:Sirenix.Serialization.EntryType.EndOfArray" />.
		/// </summary>
		/// <param name="value">The value that has been read.</param>
		/// <returns>
		///   <c>true</c> if reading the value succeeded, otherwise <c>false</c>
		/// </returns>
		public override bool ReadChar(out char value)
		{
			PeekEntry();
			if (peekedEntryType == EntryType.String)
			{
				try
				{
					value = peekedEntryContent[1];
					return true;
				}
				finally
				{
					MarkEntryConsumed();
				}
			}
			SkipEntry();
			value = '\0';
			return false;
		}

		/// <summary>
		/// Reads a <see cref="T:System.String" /> value. This call will succeed if the next entry is an <see cref="F:Sirenix.Serialization.EntryType.String" />.
		/// <para />
		/// If the call fails (and returns <c>false</c>), it will skip the current entry value, unless that entry is an <see cref="F:Sirenix.Serialization.EntryType.EndOfNode" /> or an <see cref="F:Sirenix.Serialization.EntryType.EndOfArray" />.
		/// </summary>
		/// <param name="value">The value that has been read.</param>
		/// <returns>
		///   <c>true</c> if reading the value succeeded, otherwise <c>false</c>
		/// </returns>
		public override bool ReadString(out string value)
		{
			PeekEntry();
			if (peekedEntryType == EntryType.String)
			{
				try
				{
					value = peekedEntryContent.Substring(1, peekedEntryContent.Length - 2);
					return true;
				}
				finally
				{
					MarkEntryConsumed();
				}
			}
			SkipEntry();
			value = null;
			return false;
		}

		/// <summary>
		/// Reads a <see cref="T:System.Guid" /> value. This call will succeed if the next entry is an <see cref="F:Sirenix.Serialization.EntryType.Guid" />.
		/// <para />
		/// If the call fails (and returns <c>false</c>), it will skip the current entry value, unless that entry is an <see cref="F:Sirenix.Serialization.EntryType.EndOfNode" /> or an <see cref="F:Sirenix.Serialization.EntryType.EndOfArray" />.
		/// </summary>
		/// <param name="value">The value that has been read.</param>
		/// <returns>
		///   <c>true</c> if reading the value succeeded, otherwise <c>false</c>
		/// </returns>
		public override bool ReadGuid(out Guid value)
		{
			PeekEntry();
			if (peekedEntryType == EntryType.Guid)
			{
				try
				{
					value = new Guid(peekedEntryContent);
					return true;
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
					MarkEntryConsumed();
				}
			}
			SkipEntry();
			value = Guid.Empty;
			return false;
		}

		/// <summary>
		/// Reads an <see cref="T:System.SByte" /> value. This call will succeed if the next entry is an <see cref="F:Sirenix.Serialization.EntryType.Integer" />.
		/// <para />
		/// If the value of the stored integer is smaller than <see cref="F:System.SByte.MinValue" /> or larger than <see cref="F:System.SByte.MaxValue" />, the result will be default(<see cref="T:System.SByte" />).
		/// <para />
		/// If the call fails (and returns <c>false</c>), it will skip the current entry value, unless that entry is an <see cref="F:Sirenix.Serialization.EntryType.EndOfNode" /> or an <see cref="F:Sirenix.Serialization.EntryType.EndOfArray" />.
		/// </summary>
		/// <param name="value">The value that has been read.</param>
		/// <returns>
		///   <c>true</c> if reading the value succeeded, otherwise <c>false</c>
		/// </returns>
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
		/// Reads a <see cref="T:System.Int16" /> value. This call will succeed if the next entry is an <see cref="F:Sirenix.Serialization.EntryType.Integer" />.
		/// <para />
		/// If the value of the stored integer is smaller than <see cref="F:System.Int16.MinValue" /> or larger than <see cref="F:System.Int16.MaxValue" />, the result will be default(<see cref="T:System.Int16" />).
		/// <para />
		/// If the call fails (and returns <c>false</c>), it will skip the current entry value, unless that entry is an <see cref="F:Sirenix.Serialization.EntryType.EndOfNode" /> or an <see cref="F:Sirenix.Serialization.EntryType.EndOfArray" />.
		/// </summary>
		/// <param name="value">The value that has been read.</param>
		/// <returns>
		///   <c>true</c> if reading the value succeeded, otherwise <c>false</c>
		/// </returns>
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
		/// Reads an <see cref="T:System.Int32" /> value. This call will succeed if the next entry is an <see cref="F:Sirenix.Serialization.EntryType.Integer" />.
		/// <para />
		/// If the value of the stored integer is smaller than <see cref="F:System.Int32.MinValue" /> or larger than <see cref="F:System.Int32.MaxValue" />, the result will be default(<see cref="T:System.Int32" />).
		/// <para />
		/// If the call fails (and returns <c>false</c>), it will skip the current entry value, unless that entry is an <see cref="F:Sirenix.Serialization.EntryType.EndOfNode" /> or an <see cref="F:Sirenix.Serialization.EntryType.EndOfArray" />.
		/// </summary>
		/// <param name="value">The value that has been read.</param>
		/// <returns>
		///   <c>true</c> if reading the value succeeded, otherwise <c>false</c>
		/// </returns>
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
		/// Reads a <see cref="T:System.Int64" /> value. This call will succeed if the next entry is an <see cref="F:Sirenix.Serialization.EntryType.Integer" />.
		/// <para />
		/// If the value of the stored integer is smaller than <see cref="F:System.Int64.MinValue" /> or larger than <see cref="F:System.Int64.MaxValue" />, the result will be default(<see cref="T:System.Int64" />).
		/// <para />
		/// If the call fails (and returns <c>false</c>), it will skip the current entry value, unless that entry is an <see cref="F:Sirenix.Serialization.EntryType.EndOfNode" /> or an <see cref="F:Sirenix.Serialization.EntryType.EndOfArray" />.
		/// </summary>
		/// <param name="value">The value that has been read.</param>
		/// <returns>
		///   <c>true</c> if reading the value succeeded, otherwise <c>false</c>
		/// </returns>
		public override bool ReadInt64(out long value)
		{
			PeekEntry();
			if (peekedEntryType == EntryType.Integer)
			{
				try
				{
					if (long.TryParse(peekedEntryContent, NumberStyles.Any, CultureInfo.InvariantCulture, out value))
					{
						return true;
					}
					base.Context.Config.DebugContext.LogError("Failed to parse long from: " + peekedEntryContent);
					return false;
				}
				finally
				{
					MarkEntryConsumed();
				}
			}
			SkipEntry();
			value = 0L;
			return false;
		}

		/// <summary>
		/// Reads a <see cref="T:System.Byte" /> value. This call will succeed if the next entry is an <see cref="F:Sirenix.Serialization.EntryType.Integer" />.
		/// <para />
		/// If the value of the stored integer is smaller than <see cref="F:System.Byte.MinValue" /> or larger than <see cref="F:System.Byte.MaxValue" />, the result will be default(<see cref="T:System.Byte" />).
		/// <para />
		/// If the call fails (and returns <c>false</c>), it will skip the current entry value, unless that entry is an <see cref="F:Sirenix.Serialization.EntryType.EndOfNode" /> or an <see cref="F:Sirenix.Serialization.EntryType.EndOfArray" />.
		/// </summary>
		/// <param name="value">The value that has been read.</param>
		/// <returns>
		///   <c>true</c> if reading the value succeeded, otherwise <c>false</c>
		/// </returns>
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
		/// Reads an <see cref="T:System.UInt16" /> value. This call will succeed if the next entry is an <see cref="F:Sirenix.Serialization.EntryType.Integer" />.
		/// <para />
		/// If the value of the stored integer is smaller than <see cref="F:System.UInt16.MinValue" /> or larger than <see cref="F:System.UInt16.MaxValue" />, the result will be default(<see cref="T:System.UInt16" />).
		/// <para />
		/// If the call fails (and returns <c>false</c>), it will skip the current entry value, unless that entry is an <see cref="F:Sirenix.Serialization.EntryType.EndOfNode" /> or an <see cref="F:Sirenix.Serialization.EntryType.EndOfArray" />.
		/// </summary>
		/// <param name="value">The value that has been read.</param>
		/// <returns>
		///   <c>true</c> if reading the value succeeded, otherwise <c>false</c>
		/// </returns>
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
		/// Reads an <see cref="T:System.UInt32" /> value. This call will succeed if the next entry is an <see cref="F:Sirenix.Serialization.EntryType.Integer" />.
		/// <para />
		/// If the value of the stored integer is smaller than <see cref="F:System.UInt32.MinValue" /> or larger than <see cref="F:System.UInt32.MaxValue" />, the result will be default(<see cref="T:System.UInt32" />).
		/// <para />
		/// If the call fails (and returns <c>false</c>), it will skip the current entry value, unless that entry is an <see cref="F:Sirenix.Serialization.EntryType.EndOfNode" /> or an <see cref="F:Sirenix.Serialization.EntryType.EndOfArray" />.
		/// </summary>
		/// <param name="value">The value that has been read.</param>
		/// <returns>
		///   <c>true</c> if reading the value succeeded, otherwise <c>false</c>
		/// </returns>
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
		/// Reads an <see cref="T:System.UInt64" /> value. This call will succeed if the next entry is an <see cref="F:Sirenix.Serialization.EntryType.Integer" />.
		/// <para />
		/// If the value of the stored integer is smaller than <see cref="F:System.UInt64.MinValue" /> or larger than <see cref="F:System.UInt64.MaxValue" />, the result will be default(<see cref="T:System.UInt64" />).
		/// <para />
		/// If the call fails (and returns <c>false</c>), it will skip the current entry value, unless that entry is an <see cref="F:Sirenix.Serialization.EntryType.EndOfNode" /> or an <see cref="F:Sirenix.Serialization.EntryType.EndOfArray" />.
		/// </summary>
		/// <param name="value">The value that has been read.</param>
		/// <returns>
		///   <c>true</c> if reading the value succeeded, otherwise <c>false</c>
		/// </returns>
		public override bool ReadUInt64(out ulong value)
		{
			PeekEntry();
			if (peekedEntryType == EntryType.Integer)
			{
				try
				{
					if (ulong.TryParse(peekedEntryContent, NumberStyles.Any, CultureInfo.InvariantCulture, out value))
					{
						return true;
					}
					base.Context.Config.DebugContext.LogError("Failed to parse ulong from: " + peekedEntryContent);
					return false;
				}
				finally
				{
					MarkEntryConsumed();
				}
			}
			SkipEntry();
			value = 0uL;
			return false;
		}

		/// <summary>
		/// Reads a <see cref="T:System.Decimal" /> value. This call will succeed if the next entry is an <see cref="F:Sirenix.Serialization.EntryType.FloatingPoint" /> or an <see cref="F:Sirenix.Serialization.EntryType.Integer" />.
		/// <para />
		/// If the stored integer or floating point value is smaller than <see cref="F:System.Decimal.MinValue" /> or larger than <see cref="F:System.Decimal.MaxValue" />, the result will be default(<see cref="T:System.Decimal" />).
		/// <para />
		/// If the call fails (and returns <c>false</c>), it will skip the current entry value, unless that entry is an <see cref="F:Sirenix.Serialization.EntryType.EndOfNode" /> or an <see cref="F:Sirenix.Serialization.EntryType.EndOfArray" />.
		/// </summary>
		/// <param name="value">The value that has been read.</param>
		/// <returns>
		///   <c>true</c> if reading the value succeeded, otherwise <c>false</c>
		/// </returns>
		public override bool ReadDecimal(out decimal value)
		{
			PeekEntry();
			if (peekedEntryType == EntryType.FloatingPoint || peekedEntryType == EntryType.Integer)
			{
				try
				{
					if (decimal.TryParse(peekedEntryContent, NumberStyles.Any, CultureInfo.InvariantCulture, out value))
					{
						return true;
					}
					base.Context.Config.DebugContext.LogError("Failed to parse decimal from: " + peekedEntryContent);
					return false;
				}
				finally
				{
					MarkEntryConsumed();
				}
			}
			SkipEntry();
			value = default(decimal);
			return false;
		}

		/// <summary>
		/// Reads a <see cref="T:System.Single" /> value. This call will succeed if the next entry is an <see cref="F:Sirenix.Serialization.EntryType.FloatingPoint" /> or an <see cref="F:Sirenix.Serialization.EntryType.Integer" />.
		/// <para />
		/// If the stored integer or floating point value is smaller than <see cref="F:System.Single.MinValue" /> or larger than <see cref="F:System.Single.MaxValue" />, the result will be default(<see cref="T:System.Single" />).
		/// <para />
		/// If the call fails (and returns <c>false</c>), it will skip the current entry value, unless that entry is an <see cref="F:Sirenix.Serialization.EntryType.EndOfNode" /> or an <see cref="F:Sirenix.Serialization.EntryType.EndOfArray" />.
		/// </summary>
		/// <param name="value">The value that has been read.</param>
		/// <returns>
		///   <c>true</c> if reading the value succeeded, otherwise <c>false</c>
		/// </returns>
		public override bool ReadSingle(out float value)
		{
			PeekEntry();
			if (peekedEntryType == EntryType.FloatingPoint || peekedEntryType == EntryType.Integer)
			{
				try
				{
					if (float.TryParse(peekedEntryContent, NumberStyles.Any, CultureInfo.InvariantCulture, out value))
					{
						return true;
					}
					base.Context.Config.DebugContext.LogError("Failed to parse float from: " + peekedEntryContent);
					return false;
				}
				finally
				{
					MarkEntryConsumed();
				}
			}
			SkipEntry();
			value = 0f;
			return false;
		}

		/// <summary>
		/// Reads a <see cref="T:System.Double" /> value. This call will succeed if the next entry is an <see cref="F:Sirenix.Serialization.EntryType.FloatingPoint" /> or an <see cref="F:Sirenix.Serialization.EntryType.Integer" />.
		/// <para />
		/// If the stored integer or floating point value is smaller than <see cref="F:System.Double.MinValue" /> or larger than <see cref="F:System.Double.MaxValue" />, the result will be default(<see cref="T:System.Double" />).
		/// <para />
		/// If the call fails (and returns <c>false</c>), it will skip the current entry value, unless that entry is an <see cref="F:Sirenix.Serialization.EntryType.EndOfNode" /> or an <see cref="F:Sirenix.Serialization.EntryType.EndOfArray" />.
		/// </summary>
		/// <param name="value">The value that has been read.</param>
		/// <returns>
		///   <c>true</c> if reading the value succeeded, otherwise <c>false</c>
		/// </returns>
		public override bool ReadDouble(out double value)
		{
			PeekEntry();
			if (peekedEntryType == EntryType.FloatingPoint || peekedEntryType == EntryType.Integer)
			{
				try
				{
					if (double.TryParse(peekedEntryContent, NumberStyles.Any, CultureInfo.InvariantCulture, out value))
					{
						return true;
					}
					base.Context.Config.DebugContext.LogError("Failed to parse double from: " + peekedEntryContent);
					return false;
				}
				finally
				{
					MarkEntryConsumed();
				}
			}
			SkipEntry();
			value = 0.0;
			return false;
		}

		/// <summary>
		/// Reads a <c>null</c> value. This call will succeed if the next entry is an <see cref="F:Sirenix.Serialization.EntryType.Null" />.
		/// <para />
		/// If the call fails (and returns <c>false</c>), it will skip the current entry value, unless that entry is an <see cref="F:Sirenix.Serialization.EntryType.EndOfNode" /> or an <see cref="F:Sirenix.Serialization.EntryType.EndOfArray" />.
		/// </summary>
		/// <returns>
		///   <c>true</c> if reading the value succeeded, otherwise <c>false</c>
		/// </returns>
		public override bool ReadNull()
		{
			PeekEntry();
			if (peekedEntryType == EntryType.Null)
			{
				MarkEntryConsumed();
				return true;
			}
			SkipEntry();
			return false;
		}

		/// <summary>
		/// Tells the reader that a new serialization session is about to begin, and that it should clear all cached values left over from any prior serialization sessions.
		/// This method is only relevant when the same reader is used to deserialize several different, unrelated values.
		/// </summary>
		public override void PrepareNewSerializationSession()
		{
			base.PrepareNewSerializationSession();
			peekedEntryType = null;
			peekedEntryContent = null;
			peekedEntryName = null;
			seenTypes.Clear();
			reader.Reset();
		}

		public override string GetDataDump()
		{
			if (!Stream.CanSeek)
			{
				return "Json data stream cannot seek; cannot dump data.";
			}
			long oldPosition = Stream.Position;
			byte[] bytes = new byte[Stream.Length];
			Stream.Position = 0L;
			Stream.Read(bytes, 0, bytes.Length);
			Stream.Position = oldPosition;
			return "Json: " + Encoding.UTF8.GetString(bytes, 0, bytes.Length);
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
			peekedEntryType = null;
			string name;
			return PeekEntry(out name);
		}

		private void MarkEntryConsumed()
		{
			if (peekedEntryType != EntryType.EndOfArray && peekedEntryType != EntryType.EndOfNode)
			{
				peekedEntryType = null;
			}
		}

		private bool ReadAnyIntReference(out int value)
		{
			int separatorIndex = -1;
			for (int i = 0; i < peekedEntryContent.Length; i++)
			{
				if (peekedEntryContent[i] == ':')
				{
					separatorIndex = i;
					break;
				}
			}
			if (separatorIndex == -1 || separatorIndex == peekedEntryContent.Length - 1)
			{
				base.Context.Config.DebugContext.LogError("Failed to parse id from: " + peekedEntryContent);
			}
			string idStr = peekedEntryContent.Substring(separatorIndex + 1);
			if (int.TryParse(idStr, NumberStyles.Any, CultureInfo.InvariantCulture, out value))
			{
				return true;
			}
			base.Context.Config.DebugContext.LogError("Failed to parse id: " + idStr);
			value = -1;
			return false;
		}
	}
}
