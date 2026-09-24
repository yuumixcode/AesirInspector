using System;
using System.Reflection;
using System.Runtime.Serialization;

namespace Sirenix.Serialization
{
	/// <summary>
	/// Formatter for all types that implement the ISerializable interface.
	/// </summary>
	/// <typeparam name="T">The type which can be serialized and deserialized by the formatter.</typeparam>
	/// <seealso cref="T:Sirenix.Serialization.BaseFormatter`1" />
	public sealed class SerializableFormatter<T> : BaseFormatter<T> where T : ISerializable
	{
		private static readonly Func<SerializationInfo, StreamingContext, T> ISerializableConstructor;

		private static readonly ReflectionFormatter<T> ReflectionFormatter;

		static SerializableFormatter()
		{
			Type current = typeof(T);
			ConstructorInfo constructor = null;
			do
			{
				constructor = current.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[2]
				{
					typeof(SerializationInfo),
					typeof(StreamingContext)
				}, null);
				current = current.BaseType;
			}
			while (constructor == null && current != typeof(object) && current != null);
			if (constructor != null)
			{
				ISerializableConstructor = delegate(SerializationInfo info, StreamingContext context)
				{
					T val = (T)FormatterServices.GetUninitializedObject(typeof(T));
					constructor.Invoke(val, new object[2] { info, context });
					return val;
				};
			}
			else
			{
				DefaultLoggers.DefaultLogger.LogWarning("Type " + typeof(T).Name + " implements the interface ISerializable but does not implement the required constructor with signature " + typeof(T).Name + "(SerializationInfo info, StreamingContext context). The interface declaration will be ignored, and the formatter fallbacks to reflection.");
				ReflectionFormatter = new ReflectionFormatter<T>();
			}
		}

		/// <summary>
		/// Get an uninitialized object of type <see cref="!:T" />. WARNING: If you override this and return null, the object's ID will not be automatically registered and its OnDeserializing callbacks will not be automatically called, before deserialization begins.
		/// You will have to call <see cref="!:BaseFormatter&lt;T&gt;.RegisterReferenceID(T, IDataReader, DeserializationContext)" /> and <see cref="!:BaseFormatter&lt;T&gt;.InvokeOnDeserializingCallbacks(T, IDataReader, DeserializationContext)" /> immediately after creating the object yourself during deserialization.
		/// </summary>
		/// <returns>
		/// An uninitialized object of type <see cref="!:T" />.
		/// </returns>
		protected override T GetUninitializedObject()
		{
			return default(T);
		}

		/// <summary>
		/// Provides the actual implementation for deserializing a value of type <see cref="!:T" />.
		/// </summary>
		/// <param name="value">The uninitialized value to serialize into. This value will have been created earlier using <see cref="M:Sirenix.Serialization.BaseFormatter`1.GetUninitializedObject" />.</param>
		/// <param name="reader">The reader to deserialize with.</param>
		protected override void DeserializeImplementation(ref T value, IDataReader reader)
		{
			if (ISerializableConstructor != null)
			{
				SerializationInfo info = ReadSerializationInfo(reader);
				if (info == null)
				{
					return;
				}
				try
				{
					value = ISerializableConstructor(info, reader.Context.StreamingContext);
					InvokeOnDeserializingCallbacks(ref value, reader.Context);
					if (!BaseFormatter<T>.IsValueType)
					{
						RegisterReferenceID(value, reader);
					}
					return;
				}
				catch (Exception exception)
				{
					reader.Context.Config.DebugContext.LogException(exception);
					return;
				}
			}
			value = ReflectionFormatter.Deserialize(reader);
			InvokeOnDeserializingCallbacks(ref value, reader.Context);
			if (!BaseFormatter<T>.IsValueType)
			{
				RegisterReferenceID(value, reader);
			}
		}

		/// <summary>
		/// Provides the actual implementation for serializing a value of type <see cref="!:T" />.
		/// </summary>
		/// <param name="value">The value to serialize.</param>
		/// <param name="writer">The writer to serialize with.</param>
		protected override void SerializeImplementation(ref T value, IDataWriter writer)
		{
			if (ISerializableConstructor != null)
			{
				ISerializable serializable = value;
				SerializationInfo info = new SerializationInfo(value.GetType(), writer.Context.FormatterConverter);
				try
				{
					serializable.GetObjectData(info, writer.Context.StreamingContext);
				}
				catch (Exception exception)
				{
					writer.Context.Config.DebugContext.LogException(exception);
				}
				WriteSerializationInfo(info, writer);
			}
			else
			{
				ReflectionFormatter.Serialize(value, writer);
			}
		}

		/// <summary>
		/// Creates and reads into a <see cref="T:System.Runtime.Serialization.SerializationInfo" /> instance using a given reader and context.
		/// </summary>
		/// <param name="reader">The reader to use.</param>
		/// <returns>
		/// The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> which was read.
		/// </returns>
		private SerializationInfo ReadSerializationInfo(IDataReader reader)
		{
			EntryType entry = reader.PeekEntry(out var name);
			if (entry == EntryType.StartOfArray)
			{
				try
				{
					reader.EnterArray(out var length);
					SerializationInfo info = new SerializationInfo(typeof(T), reader.Context.FormatterConverter);
					for (int i = 0; i < length; i++)
					{
						Type type = null;
						entry = reader.PeekEntry(out name);
						if (entry == EntryType.String && name == "type")
						{
							reader.ReadString(out var typeName);
							type = reader.Context.Binder.BindToType(typeName, reader.Context.Config.DebugContext);
						}
						if (type == null)
						{
							reader.SkipEntry();
						}
						else
						{
							entry = reader.PeekEntry(out name);
							Serializer readerWriter = Serializer.Get(type);
							object value = readerWriter.ReadValueWeak(reader);
							info.AddValue(name, value);
						}
					}
					return info;
				}
				finally
				{
					reader.ExitArray();
				}
			}
			return null;
		}

		/// <summary>
		/// Writes the given <see cref="T:System.Runtime.Serialization.SerializationInfo" /> using the given writer.
		/// </summary>
		/// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> to write.</param>
		/// <param name="writer">The writer to use.</param>
		private void WriteSerializationInfo(SerializationInfo info, IDataWriter writer)
		{
			try
			{
				writer.BeginArrayNode(info.MemberCount);
				SerializationInfoEnumerator enumerator = info.GetEnumerator();
				while (enumerator.MoveNext())
				{
					SerializationEntry entry = enumerator.Current;
					try
					{
						writer.WriteString("type", writer.Context.Binder.BindToName(entry.ObjectType, writer.Context.Config.DebugContext));
						Serializer readerWriter = Serializer.Get(entry.ObjectType);
						readerWriter.WriteValueWeak(entry.Name, entry.Value, writer);
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
