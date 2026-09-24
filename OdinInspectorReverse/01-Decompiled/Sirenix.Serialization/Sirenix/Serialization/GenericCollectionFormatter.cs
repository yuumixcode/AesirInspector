using System;
using System.Collections.Generic;
using Sirenix.Serialization.Utilities;

namespace Sirenix.Serialization
{
	/// <summary>
	/// Utility class for the <see cref="T:Sirenix.Serialization.GenericCollectionFormatter`2" /> class.
	/// </summary>
	public static class GenericCollectionFormatter
	{
		/// <summary>
		/// Determines whether the specified type can be formatted by a <see cref="T:Sirenix.Serialization.GenericCollectionFormatter`2" />.
		/// <para />
		/// The following criteria are checked: type implements <see cref="T:System.Collections.Generic.ICollection`1" />, type is not abstract, type is not a generic type definition, type is not an interface, type has a public parameterless constructor.
		/// </summary>
		/// <param name="type">The collection type to check.</param>
		/// <param name="elementType">The element type of the collection.</param>
		/// <returns><c>true</c> if the type can be formatted by a <see cref="T:Sirenix.Serialization.GenericCollectionFormatter`2" />, otherwise <c>false</c></returns>
		/// <exception cref="T:System.ArgumentNullException">The type argument is null.</exception>
		public static bool CanFormat(Type type, out Type elementType)
		{
			if (type == null)
			{
				throw new ArgumentNullException();
			}
			if (type.IsAbstract || type.IsGenericTypeDefinition || type.IsInterface || type.GetConstructor(Type.EmptyTypes) == null || !type.ImplementsOpenGenericInterface(typeof(ICollection<>)))
			{
				elementType = null;
				return false;
			}
			elementType = type.GetArgumentsOfInheritedOpenGenericInterface(typeof(ICollection<>))[0];
			return true;
		}
	}
	/// <summary>
	/// Formatter for all eligible types that implement the interface <see cref="T:System.Collections.Generic.ICollection`1" />, and which have no other formatters specified.
	/// <para />
	/// Eligibility for formatting by this class is determined by the <see cref="M:Sirenix.Serialization.GenericCollectionFormatter.CanFormat(System.Type,System.Type@)" /> method.
	/// </summary>
	/// <typeparam name="TCollection">The type of the collection.</typeparam>
	/// <typeparam name="TElement">The type of the element.</typeparam>
	public sealed class GenericCollectionFormatter<TCollection, TElement> : BaseFormatter<TCollection> where TCollection : ICollection<TElement>, new()
	{
		private static Serializer<TElement> valueReaderWriter;

		static GenericCollectionFormatter()
		{
			valueReaderWriter = Serializer.Get<TElement>();
			if (!GenericCollectionFormatter.CanFormat(typeof(TCollection), out var e))
			{
				throw new ArgumentException("Cannot treat the type " + typeof(TCollection).Name + " as a generic collection.");
			}
			if (e != typeof(TElement))
			{
				throw new ArgumentException("Type " + typeof(TElement).Name + " is not the element type of the generic collection type " + typeof(TCollection).Name + ".");
			}
			new GenericCollectionFormatter<List<int>, int>();
		}

		/// <summary>
		/// Creates a new instance of <see cref="T:Sirenix.Serialization.GenericCollectionFormatter`2" />.
		/// </summary>
		public GenericCollectionFormatter()
		{
		}

		/// <summary>
		/// Gets a new object of type <see cref="!:T" />.
		/// </summary>
		/// <returns>
		/// A new object of type <see cref="!:T" />.
		/// </returns>
		protected override TCollection GetUninitializedObject()
		{
			return new TCollection();
		}

		/// <summary>
		/// Provides the actual implementation for deserializing a value of type <see cref="!:T" />.
		/// </summary>
		/// <param name="value">The uninitialized value to serialize into. This value will have been created earlier using <see cref="M:Sirenix.Serialization.BaseFormatter`1.GetUninitializedObject" />.</param>
		/// <param name="reader">The reader to deserialize with.</param>
		protected override void DeserializeImplementation(ref TCollection value, IDataReader reader)
		{
			string name;
			EntryType entry = reader.PeekEntry(out name);
			if (entry == EntryType.StartOfArray)
			{
				try
				{
					reader.EnterArray(out var length);
					for (int i = 0; i < length; i++)
					{
						if (reader.PeekEntry(out name) == EntryType.EndOfArray)
						{
							reader.Context.Config.DebugContext.LogError("Reached end of array after " + i + " elements, when " + length + " elements were expected.");
							break;
						}
						try
						{
							value.Add(valueReaderWriter.ReadValue(reader));
						}
						catch (Exception exception)
						{
							reader.Context.Config.DebugContext.LogException(exception);
						}
						if (!reader.IsInArrayNode)
						{
							reader.Context.Config.DebugContext.LogError("Reading array went wrong. Data dump: " + reader.GetDataDump());
							break;
						}
					}
					return;
				}
				catch (Exception exception2)
				{
					reader.Context.Config.DebugContext.LogException(exception2);
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
		protected override void SerializeImplementation(ref TCollection value, IDataWriter writer)
		{
			try
			{
				writer.BeginArrayNode(value.Count);
				foreach (TElement element in value)
				{
					valueReaderWriter.WriteValue(element, writer);
				}
			}
			finally
			{
				writer.EndArrayNode();
			}
		}
	}
}
