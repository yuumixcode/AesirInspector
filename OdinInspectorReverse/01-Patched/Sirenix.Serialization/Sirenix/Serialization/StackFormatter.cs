using System;
using System.Collections.Generic;
using Sirenix.Serialization.Utilities;

namespace Sirenix.Serialization
{
	/// <summary>
	/// Custom generic formatter for the generic type definition <see cref="T:System.Collections.Generic.Stack`1" /> and types derived from it.
	/// </summary>
	/// <typeparam name="T">The element type of the formatted stack.</typeparam>
	/// <seealso cref="!:BaseFormatter&lt;System.Collections.Generic.Stack&lt;T&gt;&gt;" />
	public class StackFormatter<TStack, TValue> : BaseFormatter<TStack> where TStack : Stack<TValue>, new()
	{
		private static readonly Serializer<TValue> TSerializer;

		private static readonly bool IsPlainStack;

		static StackFormatter()
		{
			TSerializer = Serializer.Get<TValue>();
			IsPlainStack = typeof(TStack) == typeof(Stack<TValue>);
			new StackFormatter<Stack<int>, int>();
		}

		/// <summary>
		/// Returns null.
		/// </summary>
		/// <returns>
		/// A null value.
		/// </returns>
		protected override TStack GetUninitializedObject()
		{
			return null;
		}

		/// <summary>
		/// Provides the actual implementation for deserializing a value of type <see cref="!:T" />.
		/// </summary>
		/// <param name="value">The uninitialized value to serialize into. This value will have been created earlier using <see cref="M:Sirenix.Serialization.BaseFormatter`1.GetUninitializedObject" />.</param>
		/// <param name="reader">The reader to deserialize with.</param>
		protected override void DeserializeImplementation(ref TStack value, IDataReader reader)
		{
			string name;
			EntryType entry = reader.PeekEntry(out name);
			if (entry == EntryType.StartOfArray)
			{
				try
				{
					reader.EnterArray(out var length);
					if (IsPlainStack)
					{
						value = (TStack)new Stack<TValue>((int)length);
					}
					else
					{
						value = new TStack();
					}
					RegisterReferenceID(value, reader);
					for (int i = 0; i < length; i++)
					{
						if (reader.PeekEntry(out name) == EntryType.EndOfArray)
						{
							reader.Context.Config.DebugContext.LogError("Reached end of array after " + i + " elements, when " + length + " elements were expected.");
							break;
						}
						value.Push(TSerializer.ReadValue(reader));
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
		protected override void SerializeImplementation(ref TStack value, IDataWriter writer)
		{
			try
			{
				writer.BeginArrayNode(value.Count);
				using Cache<List<TValue>> listCache = Cache<List<TValue>>.Claim();
				List<TValue> list = listCache.Value;
				list.Clear();
				foreach (TValue element in value)
				{
					list.Add(element);
				}
				for (int i = list.Count - 1; i >= 0; i--)
				{
					try
					{
						TSerializer.WriteValue(list[i], writer);
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
