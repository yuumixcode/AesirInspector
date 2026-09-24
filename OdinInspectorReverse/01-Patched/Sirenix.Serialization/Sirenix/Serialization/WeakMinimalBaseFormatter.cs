using System;
using System.Runtime.Serialization;

namespace Sirenix.Serialization
{
	public abstract class WeakMinimalBaseFormatter : IFormatter
	{
		protected readonly Type SerializedType;

		/// <summary>
		/// Whether the serialized value is a value type.
		/// </summary>
		protected readonly bool IsValueType;

		/// <summary>
		/// Gets the type that the formatter can serialize.
		/// </summary>
		/// <value>
		/// The type that the formatter can serialize.
		/// </value>
		Type IFormatter.SerializedType => SerializedType;

		public WeakMinimalBaseFormatter(Type serializedType)
		{
			SerializedType = serializedType;
			IsValueType = SerializedType.IsValueType;
		}

		public object Deserialize(IDataReader reader)
		{
			object result = GetUninitializedObject();
			if (!IsValueType && result != null)
			{
				RegisterReferenceID(result, reader);
			}
			Read(ref result, reader);
			return result;
		}

		public void Serialize(object value, IDataWriter writer)
		{
			Write(ref value, writer);
		}

		/// <summary>
		/// Get an uninitialized object of type <see cref="!:T" />. WARNING: If you override this and return null, the object's ID will not be automatically registered.
		/// You will have to call <see cref="!:MinimalBaseFormatter&lt;T&gt;&lt;T&gt;.RegisterReferenceID(T, IDataReader, DeserializationContext)" /> immediately after creating the object yourself during deserialization.
		/// </summary>
		/// <returns>An uninitialized object of type <see cref="!:T" />.</returns>
		protected virtual object GetUninitializedObject()
		{
			if (IsValueType)
			{
				return Activator.CreateInstance(SerializedType);
			}
			return FormatterServices.GetUninitializedObject(SerializedType);
		}

		/// <summary>
		/// Reads into the specified value using the specified reader.
		/// </summary>
		/// <param name="value">The value to read into.</param>
		/// <param name="reader">The reader to use.</param>
		protected abstract void Read(ref object value, IDataReader reader);

		/// <summary>
		/// Writes from the specified value using the specified writer.
		/// </summary>
		/// <param name="value">The value to write from.</param>
		/// <param name="writer">The writer to use.</param>
		protected abstract void Write(ref object value, IDataWriter writer);

		/// <summary>
		/// Registers the given object reference in the deserialization context.
		/// <para />
		/// NOTE that this method only does anything if the serialized type is not a value type.
		/// </summary>
		/// <param name="value">The value to register.</param>
		/// <param name="reader">The reader which is currently being used.</param>
		protected void RegisterReferenceID(object value, IDataReader reader)
		{
			if (!IsValueType)
			{
				int id = reader.CurrentNodeId;
				if (id < 0)
				{
					reader.Context.Config.DebugContext.LogWarning("Reference type node is missing id upon deserialization. Some references may be broken. This tends to happen if a value type has changed to a reference type (IE, struct to class) since serialization took place.");
				}
				else
				{
					reader.Context.RegisterInternalReference(id, value);
				}
			}
		}
	}
}
