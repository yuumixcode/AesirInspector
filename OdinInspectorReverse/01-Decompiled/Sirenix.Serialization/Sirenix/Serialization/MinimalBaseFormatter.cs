using System;
using System.Runtime.Serialization;

namespace Sirenix.Serialization
{
	/// <summary>
	/// Minimal baseline formatter. Doesn't come with all the bells and whistles of any of the other BaseFormatter classes.
	/// Common serialization conventions aren't automatically supported, and common deserialization callbacks are not automatically invoked.
	/// </summary>
	/// <typeparam name="T">The type which can be serialized and deserialized by the formatter.</typeparam>
	public abstract class MinimalBaseFormatter<T> : IFormatter<T>, IFormatter
	{
		/// <summary>
		/// Whether the serialized value is a value type.
		/// </summary>
		protected static readonly bool IsValueType = typeof(T).IsValueType;

		/// <summary>
		/// Gets the type that the formatter can serialize.
		/// </summary>
		/// <value>
		/// The type that the formatter can serialize.
		/// </value>
		public Type SerializedType => typeof(T);

		/// <summary>
		/// Deserializes a value of type <see cref="!:T" /> using a specified <see cref="T:OdinSerializer.IDataReader" />.
		/// </summary>
		/// <param name="reader">The reader to use.</param>
		/// <returns>
		/// The deserialized value.
		/// </returns>
		public T Deserialize(IDataReader reader)
		{
			T result = GetUninitializedObject();
			if (!IsValueType && result != null)
			{
				RegisterReferenceID(result, reader);
			}
			Read(ref result, reader);
			return result;
		}

		/// <summary>
		/// Serializes a value of type <see cref="!:T" /> using a specified <see cref="T:OdinSerializer.IDataWriter" />.
		/// </summary>
		/// <param name="value">The value to serialize.</param>
		/// <param name="writer">The writer to use.</param>
		public void Serialize(T value, IDataWriter writer)
		{
			Write(ref value, writer);
		}

		/// <summary>
		/// Serializes a value using a specified <see cref="T:Sirenix.Serialization.IDataWriter" />.
		/// </summary>
		/// <param name="value">The value to serialize.</param>
		/// <param name="writer">The writer to use.</param>
		void IFormatter.Serialize(object value, IDataWriter writer)
		{
			if (value is T)
			{
				Serialize((T)value, writer);
			}
		}

		/// <summary>
		/// Deserializes a value using a specified <see cref="T:Sirenix.Serialization.IDataReader" />.
		/// </summary>
		/// <param name="reader">The reader to use.</param>
		/// <returns>
		/// The deserialized value.
		/// </returns>
		object IFormatter.Deserialize(IDataReader reader)
		{
			return Deserialize(reader);
		}

		/// <summary>
		/// Get an uninitialized object of type <see cref="!:T" />. WARNING: If you override this and return null, the object's ID will not be automatically registered.
		/// You will have to call <see cref="!:MinimalBaseFormatter&lt;T&gt;&lt;T&gt;.RegisterReferenceID(T, IDataReader, DeserializationContext)" /> immediately after creating the object yourself during deserialization.
		/// </summary>
		/// <returns>An uninitialized object of type <see cref="!:T" />.</returns>
		protected virtual T GetUninitializedObject()
		{
			if (IsValueType)
			{
				return default(T);
			}
			return (T)FormatterServices.GetUninitializedObject(typeof(T));
		}

		/// <summary>
		/// Reads into the specified value using the specified reader.
		/// </summary>
		/// <param name="value">The value to read into.</param>
		/// <param name="reader">The reader to use.</param>
		protected abstract void Read(ref T value, IDataReader reader);

		/// <summary>
		/// Writes from the specified value using the specified writer.
		/// </summary>
		/// <param name="value">The value to write from.</param>
		/// <param name="writer">The writer to use.</param>
		protected abstract void Write(ref T value, IDataWriter writer);

		/// <summary>
		/// Registers the given object reference in the deserialization context.
		/// <para />
		/// NOTE that this method only does anything if <see cref="!:T" /> is not a value type.
		/// </summary>
		/// <param name="value">The value to register.</param>
		/// <param name="reader">The reader which is currently being used.</param>
		protected void RegisterReferenceID(T value, IDataReader reader)
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
