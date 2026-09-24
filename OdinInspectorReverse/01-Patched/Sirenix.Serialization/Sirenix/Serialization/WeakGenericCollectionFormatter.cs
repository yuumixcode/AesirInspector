using System;
using System.Collections;
using System.Reflection;

namespace Sirenix.Serialization
{
	public sealed class WeakGenericCollectionFormatter : WeakBaseFormatter
	{
		private readonly Serializer ValueReaderWriter;

		private readonly Type ElementType;

		private readonly PropertyInfo CountProperty;

		private readonly MethodInfo AddMethod;

		public WeakGenericCollectionFormatter(Type collectionType, Type elementType)
			: base(collectionType)
		{
			ElementType = elementType;
			CountProperty = collectionType.GetProperty("Count", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			AddMethod = collectionType.GetMethod("Add", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[1] { elementType }, null);
			if (AddMethod == null)
			{
				throw new ArgumentException("Cannot treat the type " + collectionType.Name + " as a generic collection since it has no accessible Add method.");
			}
			if (CountProperty == null || CountProperty.PropertyType != typeof(int))
			{
				throw new ArgumentException("Cannot treat the type " + collectionType.Name + " as a generic collection since it has no accessible Count property.");
			}
			if (!GenericCollectionFormatter.CanFormat(collectionType, out var e))
			{
				throw new ArgumentException("Cannot treat the type " + collectionType.Name + " as a generic collection.");
			}
			if (e != elementType)
			{
				throw new ArgumentException("Type " + elementType.Name + " is not the element type of the generic collection type " + collectionType.Name + ".");
			}
		}

		/// <summary>
		/// Gets a new object of type <see cref="!:T" />.
		/// </summary>
		/// <returns>
		/// A new object of type <see cref="!:T" />.
		/// </returns>
		protected override object GetUninitializedObject()
		{
			return Activator.CreateInstance(SerializedType);
		}

		/// <summary>
		/// Provides the actual implementation for deserializing a value of type <see cref="!:T" />.
		/// </summary>
		/// <param name="value">The uninitialized value to serialize into. This value will have been created earlier using <see cref="M:Sirenix.Serialization.BaseFormatter`1.GetUninitializedObject" />.</param>
		/// <param name="reader">The reader to deserialize with.</param>
		protected override void DeserializeImplementation(ref object value, IDataReader reader)
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
						object[] addParams = new object[1];
						try
						{
							addParams[0] = ValueReaderWriter.ReadValueWeak(reader);
							AddMethod.Invoke(value, addParams);
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
		protected override void SerializeImplementation(ref object value, IDataWriter writer)
		{
			try
			{
				writer.BeginArrayNode((int)CountProperty.GetValue(value, null));
				foreach (object element in (IEnumerable)value)
				{
					ValueReaderWriter.WriteValueWeak(element, writer);
				}
			}
			finally
			{
				writer.EndArrayNode();
			}
		}
	}
}
