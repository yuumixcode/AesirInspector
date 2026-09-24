using System;
using System.Reflection;
using System.Runtime.Serialization;

namespace Sirenix.Serialization
{
	public sealed class WeakSerializableFormatter : WeakBaseFormatter
	{
		private readonly Func<SerializationInfo, StreamingContext, ISerializable> ISerializableConstructor;

		private readonly WeakReflectionFormatter ReflectionFormatter;

		public WeakSerializableFormatter(Type serializedType)
			: base(serializedType)
		{
			WeakSerializableFormatter weakSerializableFormatter = this;
			Type current = serializedType;
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
					ISerializable serializable = (ISerializable)FormatterServices.GetUninitializedObject(weakSerializableFormatter.SerializedType);
					constructor.Invoke(serializable, new object[2] { info, context });
					return serializable;
				};
			}
			else
			{
				DefaultLoggers.DefaultLogger.LogWarning("Type " + SerializedType.Name + " implements the interface ISerializable but does not implement the required constructor with signature " + SerializedType.Name + "(SerializationInfo info, StreamingContext context). The interface declaration will be ignored, and the formatter fallbacks to reflection.");
				ReflectionFormatter = new WeakReflectionFormatter(SerializedType);
			}
		}

		/// <summary>
		/// Get an uninitialized object of type <see cref="!:T" />. WARNING: If you override this and return null, the object's ID will not be automatically registered and its OnDeserializing callbacks will not be automatically called, before deserialization begins.
		/// You will have to call <see cref="!:BaseFormatter&lt;T&gt;.RegisterReferenceID(T, IDataReader, DeserializationContext)" /> and <see cref="!:BaseFormatter&lt;T&gt;.InvokeOnDeserializingCallbacks(T, IDataReader, DeserializationContext)" /> immediately after creating the object yourself during deserialization.
		/// </summary>
		/// <returns>
		/// An uninitialized object of type <see cref="!:T" />.
		/// </returns>
		protected override object GetUninitializedObject()
		{
			return null;
		}

		/// <summary>
		/// Provides the actual implementation for deserializing a value of type <see cref="!:T" />.
		/// </summary>
		/// <param name="value">The uninitialized value to serialize into. This value will have been created earlier using <see cref="M:Sirenix.Serialization.BaseFormatter`1.GetUninitializedObject" />.</param>
		/// <param name="reader">The reader to deserialize with.</param>
		protected override void DeserializeImplementation(ref object value, IDataReader reader)
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
					InvokeOnDeserializingCallbacks(value, reader.Context);
					if (!IsValueType)
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
			InvokeOnDeserializingCallbacks(value, reader.Context);
			if (!IsValueType)
			{
				RegisterReferenceID(value, reader);
			}
		}

		/// <summary>
		/// Provides the actual implementation for serializing a value of type <see cref="!:T" />.
		/// </summary>
		/// <param name="value">The value to serialize.</param>
		/// <param name="writer">The writer to serialize with.</param>
		protected override void SerializeImplementation(ref object value, IDataWriter writer)
		{
			if (ISerializableConstructor != null)
			{
				ISerializable serializable = value as ISerializable;
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

		private SerializationInfo ReadSerializationInfo(IDataReader reader)
		{
			EntryType entry = reader.PeekEntry(out var name);
			if (entry == EntryType.StartOfArray)
			{
				try
				{
					reader.EnterArray(out var length);
					SerializationInfo info = new SerializationInfo(SerializedType, reader.Context.FormatterConverter);
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
