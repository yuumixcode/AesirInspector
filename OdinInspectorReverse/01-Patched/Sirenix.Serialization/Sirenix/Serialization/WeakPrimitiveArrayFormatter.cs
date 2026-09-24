using System;
using System.Collections.Generic;
using Sirenix.Serialization.Utilities;

namespace Sirenix.Serialization
{
	public sealed class WeakPrimitiveArrayFormatter : WeakMinimalBaseFormatter
	{
		public enum PrimitiveArrayType
		{
			PrimitiveArray_char,
			PrimitiveArray_sbyte,
			PrimitiveArray_short,
			PrimitiveArray_int,
			PrimitiveArray_long,
			PrimitiveArray_byte,
			PrimitiveArray_ushort,
			PrimitiveArray_uint,
			PrimitiveArray_ulong,
			PrimitiveArray_decimal,
			PrimitiveArray_bool,
			PrimitiveArray_float,
			PrimitiveArray_double,
			PrimitiveArray_Guid
		}

		private static readonly Dictionary<Type, PrimitiveArrayType> PrimitiveTypes = new Dictionary<Type, PrimitiveArrayType>(FastTypeComparer.Instance)
		{
			{
				typeof(char),
				PrimitiveArrayType.PrimitiveArray_char
			},
			{
				typeof(sbyte),
				PrimitiveArrayType.PrimitiveArray_sbyte
			},
			{
				typeof(short),
				PrimitiveArrayType.PrimitiveArray_short
			},
			{
				typeof(int),
				PrimitiveArrayType.PrimitiveArray_int
			},
			{
				typeof(long),
				PrimitiveArrayType.PrimitiveArray_long
			},
			{
				typeof(byte),
				PrimitiveArrayType.PrimitiveArray_byte
			},
			{
				typeof(ushort),
				PrimitiveArrayType.PrimitiveArray_ushort
			},
			{
				typeof(uint),
				PrimitiveArrayType.PrimitiveArray_uint
			},
			{
				typeof(ulong),
				PrimitiveArrayType.PrimitiveArray_ulong
			},
			{
				typeof(decimal),
				PrimitiveArrayType.PrimitiveArray_decimal
			},
			{
				typeof(bool),
				PrimitiveArrayType.PrimitiveArray_bool
			},
			{
				typeof(float),
				PrimitiveArrayType.PrimitiveArray_float
			},
			{
				typeof(double),
				PrimitiveArrayType.PrimitiveArray_double
			},
			{
				typeof(Guid),
				PrimitiveArrayType.PrimitiveArray_Guid
			}
		};

		private readonly Type ElementType;

		private readonly PrimitiveArrayType PrimitiveType;

		public WeakPrimitiveArrayFormatter(Type arrayType, Type elementType)
			: base(arrayType)
		{
			ElementType = elementType;
			if (!PrimitiveTypes.TryGetValue(elementType, out PrimitiveType))
			{
				throw new SerializationAbortException("The type '" + elementType.GetNiceFullName() + "' is not a type that can be written as a primitive array, yet the primitive array formatter is being used for it.");
			}
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
		/// Reads into the specified value using the specified reader.
		/// </summary>
		/// <param name="value">The value to read into.</param>
		/// <param name="reader">The reader to use.</param>
		protected override void Read(ref object value, IDataReader reader)
		{
			if (reader.PeekEntry(out var _) == EntryType.PrimitiveArray)
			{
				switch (PrimitiveType)
				{
				case PrimitiveArrayType.PrimitiveArray_char:
				{
					reader.ReadPrimitiveArray<char>(out var readValue14);
					value = readValue14;
					break;
				}
				case PrimitiveArrayType.PrimitiveArray_sbyte:
				{
					reader.ReadPrimitiveArray<sbyte>(out var readValue13);
					value = readValue13;
					break;
				}
				case PrimitiveArrayType.PrimitiveArray_short:
				{
					reader.ReadPrimitiveArray<short>(out var readValue12);
					value = readValue12;
					break;
				}
				case PrimitiveArrayType.PrimitiveArray_int:
				{
					reader.ReadPrimitiveArray<int>(out var readValue11);
					value = readValue11;
					break;
				}
				case PrimitiveArrayType.PrimitiveArray_long:
				{
					reader.ReadPrimitiveArray<long>(out var readValue10);
					value = readValue10;
					break;
				}
				case PrimitiveArrayType.PrimitiveArray_byte:
				{
					reader.ReadPrimitiveArray<byte>(out var readValue9);
					value = readValue9;
					break;
				}
				case PrimitiveArrayType.PrimitiveArray_ushort:
				{
					reader.ReadPrimitiveArray<ushort>(out var readValue8);
					value = readValue8;
					break;
				}
				case PrimitiveArrayType.PrimitiveArray_uint:
				{
					reader.ReadPrimitiveArray<uint>(out var readValue7);
					value = readValue7;
					break;
				}
				case PrimitiveArrayType.PrimitiveArray_ulong:
				{
					reader.ReadPrimitiveArray<ulong>(out var readValue6);
					value = readValue6;
					break;
				}
				case PrimitiveArrayType.PrimitiveArray_decimal:
				{
					reader.ReadPrimitiveArray<decimal>(out var readValue5);
					value = readValue5;
					break;
				}
				case PrimitiveArrayType.PrimitiveArray_bool:
				{
					reader.ReadPrimitiveArray<bool>(out var readValue4);
					value = readValue4;
					break;
				}
				case PrimitiveArrayType.PrimitiveArray_float:
				{
					reader.ReadPrimitiveArray<float>(out var readValue3);
					value = readValue3;
					break;
				}
				case PrimitiveArrayType.PrimitiveArray_double:
				{
					reader.ReadPrimitiveArray<double>(out var readValue2);
					value = readValue2;
					break;
				}
				case PrimitiveArrayType.PrimitiveArray_Guid:
				{
					reader.ReadPrimitiveArray<Guid>(out var readValue);
					value = readValue;
					break;
				}
				default:
					throw new NotImplementedException();
				}
				RegisterReferenceID(value, reader);
			}
			else
			{
				reader.SkipEntry();
			}
		}

		/// <summary>
		/// Writes from the specified value using the specified writer.
		/// </summary>
		/// <param name="value">The value to write from.</param>
		/// <param name="writer">The writer to use.</param>
		protected override void Write(ref object value, IDataWriter writer)
		{
			switch (PrimitiveType)
			{
			case PrimitiveArrayType.PrimitiveArray_char:
				writer.WritePrimitiveArray((char[])value);
				break;
			case PrimitiveArrayType.PrimitiveArray_sbyte:
				writer.WritePrimitiveArray((sbyte[])value);
				break;
			case PrimitiveArrayType.PrimitiveArray_short:
				writer.WritePrimitiveArray((short[])value);
				break;
			case PrimitiveArrayType.PrimitiveArray_int:
				writer.WritePrimitiveArray((int[])value);
				break;
			case PrimitiveArrayType.PrimitiveArray_long:
				writer.WritePrimitiveArray((long[])value);
				break;
			case PrimitiveArrayType.PrimitiveArray_byte:
				writer.WritePrimitiveArray((byte[])value);
				break;
			case PrimitiveArrayType.PrimitiveArray_ushort:
				writer.WritePrimitiveArray((ushort[])value);
				break;
			case PrimitiveArrayType.PrimitiveArray_uint:
				writer.WritePrimitiveArray((uint[])value);
				break;
			case PrimitiveArrayType.PrimitiveArray_ulong:
				writer.WritePrimitiveArray((ulong[])value);
				break;
			case PrimitiveArrayType.PrimitiveArray_decimal:
				writer.WritePrimitiveArray((decimal[])value);
				break;
			case PrimitiveArrayType.PrimitiveArray_bool:
				writer.WritePrimitiveArray((bool[])value);
				break;
			case PrimitiveArrayType.PrimitiveArray_float:
				writer.WritePrimitiveArray((float[])value);
				break;
			case PrimitiveArrayType.PrimitiveArray_double:
				writer.WritePrimitiveArray((double[])value);
				break;
			case PrimitiveArrayType.PrimitiveArray_Guid:
				writer.WritePrimitiveArray((Guid[])value);
				break;
			}
		}
	}
}
