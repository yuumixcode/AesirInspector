using System;
using System.Globalization;
using System.Text;

namespace Sirenix.Serialization
{
	public sealed class WeakMultiDimensionalArrayFormatter : WeakBaseFormatter
	{
		private const string RANKS_NAME = "ranks";

		private const char RANKS_SEPARATOR = '|';

		private readonly int ArrayRank;

		private readonly Type ElementType;

		private readonly Serializer ValueReaderWriter;

		public WeakMultiDimensionalArrayFormatter(Type arrayType, Type elementType)
			: base(arrayType)
		{
			ArrayRank = arrayType.GetArrayRank();
			ElementType = elementType;
			ValueReaderWriter = Serializer.Get(elementType);
		}

		/// <summary>
		/// Returns null.
		/// </summary>
		/// <returns>
		/// A null value.
		/// </returns>
		protected override object GetUninitializedObject()
		{
			return null;
		}

		/// <summary>
		/// Provides the actual implementation for deserializing a value of type <see cref="!:T" />.
		/// </summary>
		/// <param name="value">The uninitialized value to serialize into. This value will have been created earlier using <see cref="M:Sirenix.Serialization.BaseFormatter`1.GetUninitializedObject" />.</param>
		/// <param name="reader">The reader to deserialize with.</param>
		protected override void DeserializeImplementation(ref object value, IDataReader reader)
		{
			EntryType entry = reader.PeekEntry(out var name);
			if (entry == EntryType.StartOfArray)
			{
				reader.EnterArray(out var length);
				entry = reader.PeekEntry(out name);
				if (entry != EntryType.String || name != "ranks")
				{
					value = null;
					reader.SkipEntry();
					return;
				}
				reader.ReadString(out var lengthStr);
				string[] lengthsStrs = lengthStr.Split(new char[1] { '|' });
				if (lengthsStrs.Length != ArrayRank)
				{
					value = null;
					reader.SkipEntry();
					return;
				}
				int[] lengths = new int[lengthsStrs.Length];
				for (int i = 0; i < lengthsStrs.Length; i++)
				{
					if (int.TryParse(lengthsStrs[i], out var rankVal))
					{
						lengths[i] = rankVal;
						continue;
					}
					value = null;
					reader.SkipEntry();
					return;
				}
				long rankTotal = lengths[0];
				for (int j = 1; j < lengths.Length; j++)
				{
					rankTotal *= lengths[j];
				}
				if (rankTotal != length)
				{
					value = null;
					reader.SkipEntry();
					return;
				}
				value = Array.CreateInstance(ElementType, lengths);
				RegisterReferenceID(value, reader);
				int elements = 0;
				try
				{
					IterateArrayWrite((Array)value, delegate
					{
						if (reader.PeekEntry(out name) == EntryType.EndOfArray)
						{
							reader.Context.Config.DebugContext.LogError("Reached end of array after " + elements + " elements, when " + length + " elements were expected.");
							throw new InvalidOperationException();
						}
						object result = ValueReaderWriter.ReadValueWeak(reader);
						if (!reader.IsInArrayNode)
						{
							reader.Context.Config.DebugContext.LogError("Reading array went wrong. Data dump: " + reader.GetDataDump());
							throw new InvalidOperationException();
						}
						elements++;
						return result;
					});
				}
				catch (InvalidOperationException)
				{
				}
				catch (Exception exception)
				{
					reader.Context.Config.DebugContext.LogException(exception);
				}
				reader.ExitArray();
			}
			else
			{
				value = null;
				reader.SkipEntry();
			}
		}

		/// <summary>
		/// Provides the actual implementation for serializing a value of type <see cref="!:T" />.
		/// </summary>
		/// <param name="value">The value to serialize.</param>
		/// <param name="writer">The writer to serialize with.</param>
		protected override void SerializeImplementation(ref object value, IDataWriter writer)
		{
			Array array = value as Array;
			try
			{
				writer.BeginArrayNode(array.LongLength);
				int[] lengths = new int[ArrayRank];
				for (int i = 0; i < ArrayRank; i++)
				{
					lengths[i] = array.GetLength(i);
				}
				StringBuilder sb = new StringBuilder();
				for (int j = 0; j < ArrayRank; j++)
				{
					if (j > 0)
					{
						sb.Append('|');
					}
					sb.Append(lengths[j].ToString(CultureInfo.InvariantCulture));
				}
				string lengthStr = sb.ToString();
				writer.WriteString("ranks", lengthStr);
				IterateArrayRead((Array)value, delegate(object v)
				{
					ValueReaderWriter.WriteValueWeak(v, writer);
				});
			}
			finally
			{
				writer.EndArrayNode();
			}
		}

		private void IterateArrayWrite(Array a, Func<object> write)
		{
			int[] indices = new int[ArrayRank];
			IterateArrayWrite(a, 0, indices, write);
		}

		private void IterateArrayWrite(Array a, int rank, int[] indices, Func<object> write)
		{
			for (int i = 0; i < a.GetLength(rank); i++)
			{
				indices[rank] = i;
				if (rank + 1 < a.Rank)
				{
					IterateArrayWrite(a, rank + 1, indices, write);
				}
				else
				{
					a.SetValue(write(), indices);
				}
			}
		}

		private void IterateArrayRead(Array a, Action<object> read)
		{
			int[] indices = new int[ArrayRank];
			IterateArrayRead(a, 0, indices, read);
		}

		private void IterateArrayRead(Array a, int rank, int[] indices, Action<object> read)
		{
			for (int i = 0; i < a.GetLength(rank); i++)
			{
				indices[rank] = i;
				if (rank + 1 < a.Rank)
				{
					IterateArrayRead(a, rank + 1, indices, read);
				}
				else
				{
					read(a.GetValue(indices));
				}
			}
		}
	}
}
