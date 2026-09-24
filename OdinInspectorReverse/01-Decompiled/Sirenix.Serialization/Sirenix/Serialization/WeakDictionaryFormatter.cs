using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.Serialization.Utilities;

namespace Sirenix.Serialization
{
	internal sealed class WeakDictionaryFormatter : WeakBaseFormatter
	{
		private readonly bool KeyIsValueType;

		private readonly Serializer EqualityComparerSerializer;

		private readonly Serializer KeyReaderWriter;

		private readonly Serializer ValueReaderWriter;

		private readonly ConstructorInfo ComparerConstructor;

		private readonly PropertyInfo ComparerProperty;

		private readonly PropertyInfo CountProperty;

		private readonly Type KeyType;

		private readonly Type ValueType;

		public WeakDictionaryFormatter(Type serializedType)
			: base(serializedType)
		{
			Type[] args = serializedType.GetArgumentsOfInheritedOpenGenericClass(typeof(Dictionary<, >));
			KeyType = args[0];
			ValueType = args[1];
			KeyIsValueType = KeyType.IsValueType;
			KeyReaderWriter = Serializer.Get(KeyType);
			ValueReaderWriter = Serializer.Get(ValueType);
			CountProperty = serializedType.GetProperty("Count");
			if (CountProperty == null)
			{
				throw new SerializationAbortException("Can't serialize/deserialize the type " + serializedType.GetNiceFullName() + " because it has no accessible Count property.");
			}
			try
			{
				Type equalityComparerType = typeof(IEqualityComparer<>).MakeGenericType(KeyType);
				EqualityComparerSerializer = Serializer.Get(equalityComparerType);
				ComparerConstructor = serializedType.GetConstructor(new Type[1] { equalityComparerType });
				ComparerProperty = serializedType.GetProperty("Comparer");
			}
			catch (Exception)
			{
				EqualityComparerSerializer = Serializer.Get<object>();
				ComparerConstructor = null;
				ComparerProperty = null;
			}
		}

		protected override object GetUninitializedObject()
		{
			return null;
		}

		protected override void DeserializeImplementation(ref object value, IDataReader reader)
		{
			string name;
			EntryType entry = reader.PeekEntry(out name);
			object comparer = null;
			if (name == "comparer" || entry == EntryType.StartOfNode)
			{
				comparer = EqualityComparerSerializer.ReadValueWeak(reader);
				entry = reader.PeekEntry(out name);
			}
			if (entry == EntryType.StartOfArray)
			{
				try
				{
					reader.EnterArray(out var length);
					if (comparer != null && ComparerConstructor != null)
					{
						value = ComparerConstructor.Invoke(new object[1] { comparer });
					}
					else
					{
						value = Activator.CreateInstance(SerializedType);
					}
					IDictionary dict = (IDictionary)value;
					RegisterReferenceID(value, reader);
					for (int i = 0; i < length; i++)
					{
						if (reader.PeekEntry(out name) == EntryType.EndOfArray)
						{
							reader.Context.Config.DebugContext.LogError("Reached end of array after " + i + " elements, when " + length + " elements were expected.");
							break;
						}
						bool exitNode = true;
						try
						{
							reader.EnterNode(out var _);
							object key = KeyReaderWriter.ReadValueWeak(reader);
							object val = ValueReaderWriter.ReadValueWeak(reader);
							if (!KeyIsValueType && key == null)
							{
								reader.Context.Config.DebugContext.LogWarning("Dictionary key of type '" + KeyType.FullName + "' was null upon deserialization. A key has gone missing.");
								continue;
							}
							dict[key] = val;
						}
						catch (SerializationAbortException ex)
						{
							exitNode = false;
							throw ex;
						}
						catch (Exception exception)
						{
							reader.Context.Config.DebugContext.LogException(exception);
						}
						finally
						{
							if (exitNode)
							{
								reader.ExitNode();
							}
						}
						if (!reader.IsInArrayNode)
						{
							reader.Context.Config.DebugContext.LogError("Reading array went wrong. Data dump: " + reader.GetDataDump());
							break;
						}
					}
					return;
				}
				finally
				{
					reader.ExitArray();
				}
			}
			reader.SkipEntry();
		}

		protected override void SerializeImplementation(ref object value, IDataWriter writer)
		{
			try
			{
				IDictionary dict = (IDictionary)value;
				if (ComparerProperty != null)
				{
					object comparer = ComparerProperty.GetValue(value, null);
					if (comparer != null)
					{
						EqualityComparerSerializer.WriteValueWeak("comparer", comparer, writer);
					}
				}
				writer.BeginArrayNode((int)CountProperty.GetValue(value, null));
				IDictionaryEnumerator enumerator = dict.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						bool endNode = true;
						try
						{
							writer.BeginStructNode(null, null);
							KeyReaderWriter.WriteValueWeak("$k", enumerator.Key, writer);
							ValueReaderWriter.WriteValueWeak("$v", enumerator.Value, writer);
						}
						catch (SerializationAbortException ex)
						{
							endNode = false;
							throw ex;
						}
						catch (Exception exception)
						{
							writer.Context.Config.DebugContext.LogException(exception);
						}
						finally
						{
							if (endNode)
							{
								writer.EndNode(null);
							}
						}
					}
				}
				finally
				{
					enumerator.Reset();
					if (enumerator is IDisposable dispose)
					{
						dispose.Dispose();
					}
				}
			}
			finally
			{
				writer.EndArrayNode();
			}
		}
	}
}
