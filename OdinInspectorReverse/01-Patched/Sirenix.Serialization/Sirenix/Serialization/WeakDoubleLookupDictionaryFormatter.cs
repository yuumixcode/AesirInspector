using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.Serialization.Utilities;

namespace Sirenix.Serialization
{
	internal sealed class WeakDoubleLookupDictionaryFormatter : WeakBaseFormatter
	{
		private readonly Serializer PrimaryReaderWriter;

		private readonly Serializer InnerReaderWriter;

		public WeakDoubleLookupDictionaryFormatter(Type serializedType)
			: base(serializedType)
		{
			Type[] args = serializedType.GetArgumentsOfInheritedOpenGenericClass(typeof(Dictionary<, >));
			PrimaryReaderWriter = Serializer.Get(args[0]);
			InnerReaderWriter = Serializer.Get(args[1]);
		}

		protected override object GetUninitializedObject()
		{
			return null;
		}

		protected override void SerializeImplementation(ref object value, IDataWriter writer)
		{
			try
			{
				IDictionary dict = (IDictionary)value;
				writer.BeginArrayNode(dict.Count);
				bool endNode = true;
				IDictionaryEnumerator enumerator = dict.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						try
						{
							writer.BeginStructNode(null, null);
							PrimaryReaderWriter.WriteValueWeak("$k", enumerator.Key, writer);
							InnerReaderWriter.WriteValueWeak("$v", enumerator.Value, writer);
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

		protected override void DeserializeImplementation(ref object value, IDataReader reader)
		{
			string name;
			EntryType entry = reader.PeekEntry(out name);
			if (entry == EntryType.StartOfArray)
			{
				try
				{
					reader.EnterArray(out var length);
					value = Activator.CreateInstance(SerializedType);
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
							object key = PrimaryReaderWriter.ReadValueWeak(reader);
							object inner = InnerReaderWriter.ReadValueWeak(reader);
							dict.Add(key, inner);
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
	}
}
