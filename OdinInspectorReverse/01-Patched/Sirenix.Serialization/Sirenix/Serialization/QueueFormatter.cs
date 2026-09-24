using System;
using System.Collections.Generic;

namespace Sirenix.Serialization
{
	/// <summary>
	/// Custom generic formatter for the generic type definition <see cref="T:System.Collections.Generic.Queue`1" />.
	/// </summary>
	/// <typeparam name="T">The element type of the formatted queue.</typeparam>
	/// <seealso cref="!:BaseFormatter&lt;System.Collections.Generic.Queue&lt;T&gt;&gt;" />
	public class QueueFormatter<TQueue, TValue> : BaseFormatter<TQueue> where TQueue : Queue<TValue>, new()
	{
		private static readonly Serializer<TValue> TSerializer;

		private static readonly bool IsPlainQueue;

		static QueueFormatter()
		{
			TSerializer = Serializer.Get<TValue>();
			IsPlainQueue = typeof(TQueue) == typeof(Queue<TValue>);
			new QueueFormatter<Queue<int>, int>();
		}

		/// <summary>
		/// Returns null.
		/// </summary>
		/// <returns>
		/// A null value.
		/// </returns>
		protected override TQueue GetUninitializedObject()
		{
			return null;
		}

		/// <summary>
		/// Provides the actual implementation for deserializing a value of type <see cref="!:T" />.
		/// </summary>
		/// <param name="value">The uninitialized value to serialize into. This value will have been created earlier using <see cref="M:Sirenix.Serialization.BaseFormatter`1.GetUninitializedObject" />.</param>
		/// <param name="reader">The reader to deserialize with.</param>
		protected override void DeserializeImplementation(ref TQueue value, IDataReader reader)
		{
			string name;
			EntryType entry = reader.PeekEntry(out name);
			if (entry == EntryType.StartOfArray)
			{
				try
				{
					reader.EnterArray(out var length);
					if (IsPlainQueue)
					{
						value = (TQueue)new Queue<TValue>((int)length);
					}
					else
					{
						value = new TQueue();
					}
					RegisterReferenceID(value, reader);
					for (int i = 0; i < length; i++)
					{
						if (reader.PeekEntry(out name) == EntryType.EndOfArray)
						{
							reader.Context.Config.DebugContext.LogError("Reached end of array after " + i + " elements, when " + length + " elements were expected.");
							break;
						}
						value.Enqueue(TSerializer.ReadValue(reader));
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

		/// <summary>
		/// Provides the actual implementation for serializing a value of type <see cref="!:T" />.
		/// </summary>
		/// <param name="value">The value to serialize.</param>
		/// <param name="writer">The writer to serialize with.</param>
		protected override void SerializeImplementation(ref TQueue value, IDataWriter writer)
		{
			try
			{
				writer.BeginArrayNode(value.Count);
				foreach (TValue element in value)
				{
					try
					{
						TSerializer.WriteValue(element, writer);
					}
					catch (Exception exception)
					{
						writer.Context.Config.DebugContext.LogException(exception);
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
