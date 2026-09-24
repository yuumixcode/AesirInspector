using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Serialization;
using Sirenix.Serialization.Utilities;
using UnityEngine;

namespace Sirenix.Serialization
{
	/// <summary>
	/// Provides common functionality for serializing and deserializing values of type <see cref="!:T" />, and provides automatic support for the following common serialization conventions:
	/// <para />
	/// <see cref="T:System.Runtime.Serialization.IObjectReference" />, <see cref="!:ISerializationCallbackReceiver" />, <see cref="T:System.Runtime.Serialization.OnSerializingAttribute" />, <see cref="T:System.Runtime.Serialization.OnSerializedAttribute" />, <see cref="T:System.Runtime.Serialization.OnDeserializingAttribute" /> and <see cref="T:System.Runtime.Serialization.OnDeserializedAttribute" />.
	/// </summary>
	/// <typeparam name="T">The type which can be serialized and deserialized by the formatter.</typeparam>
	/// <seealso cref="T:Sirenix.Serialization.IFormatter`1" />
	public abstract class BaseFormatter<T> : IFormatter<T>, IFormatter
	{
		protected delegate void SerializationCallback(ref T value, StreamingContext context);

		/// <summary>
		/// The on serializing callbacks for type <see cref="!:T" />.
		/// </summary>
		protected static readonly SerializationCallback[] OnSerializingCallbacks;

		/// <summary>
		/// The on serialized callbacks for type <see cref="!:T" />.
		/// </summary>
		protected static readonly SerializationCallback[] OnSerializedCallbacks;

		/// <summary>
		/// The on deserializing callbacks for type <see cref="!:T" />.
		/// </summary>
		protected static readonly SerializationCallback[] OnDeserializingCallbacks;

		/// <summary>
		/// The on deserialized callbacks for type <see cref="!:T" />.
		/// </summary>
		protected static readonly SerializationCallback[] OnDeserializedCallbacks;

		/// <summary>
		/// Whether the serialized value is a value type.
		/// </summary>
		protected static readonly bool IsValueType;

		protected static readonly bool ImplementsISerializationCallbackReceiver;

		protected static readonly bool ImplementsIDeserializationCallback;

		protected static readonly bool ImplementsIObjectReference;

		/// <summary>
		/// Gets the type that the formatter can serialize.
		/// </summary>
		/// <value>
		/// The type that the formatter can serialize.
		/// </value>
		public Type SerializedType => typeof(T);

		static BaseFormatter()
		{
			IsValueType = typeof(T).IsValueType;
			ImplementsISerializationCallbackReceiver = typeof(T).ImplementsOrInherits(typeof(ISerializationCallbackReceiver));
			ImplementsIDeserializationCallback = typeof(T).ImplementsOrInherits(typeof(IDeserializationCallback));
			ImplementsIObjectReference = typeof(T).ImplementsOrInherits(typeof(IObjectReference));
			if (typeof(T).ImplementsOrInherits(typeof(UnityEngine.Object)))
			{
				DefaultLoggers.DefaultLogger.LogWarning("A formatter has been created for the UnityEngine.Object type " + typeof(T).Name + " - this is *strongly* discouraged. Unity should be allowed to handle serialization and deserialization of its own weird objects. Remember to serialize with a UnityReferenceResolver as the external index reference resolver in the serialization context.\n\n Stacktrace: " + new StackTrace().ToString());
			}
			MethodInfo[] methods = typeof(T).GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			List<SerializationCallback> callbacks = new List<SerializationCallback>();
			OnSerializingCallbacks = GetCallbacks(methods, typeof(OnSerializingAttribute), ref callbacks);
			OnSerializedCallbacks = GetCallbacks(methods, typeof(OnSerializedAttribute), ref callbacks);
			OnDeserializingCallbacks = GetCallbacks(methods, typeof(OnDeserializingAttribute), ref callbacks);
			OnDeserializedCallbacks = GetCallbacks(methods, typeof(OnDeserializedAttribute), ref callbacks);
		}

		private static SerializationCallback[] GetCallbacks(MethodInfo[] methods, Type callbackAttribute, ref List<SerializationCallback> list)
		{
			foreach (MethodInfo method in methods)
			{
				if (method.IsDefined(callbackAttribute, inherit: true))
				{
					SerializationCallback callback = CreateCallback(method);
					if (callback != null)
					{
						list.Add(callback);
					}
				}
			}
			SerializationCallback[] result = list.ToArray();
			list.Clear();
			return result;
		}

		private static SerializationCallback CreateCallback(MethodInfo info)
		{
			ParameterInfo[] parameters = info.GetParameters();
			if (parameters.Length == 0)
			{
				EmitUtilities.InstanceRefMethodCaller<T> action = EmitUtilities.CreateInstanceRefMethodCaller<T>(info);
				return delegate(ref T value, StreamingContext context)
				{
					action(ref value);
				};
			}
			if (parameters.Length == 1 && parameters[0].ParameterType == typeof(StreamingContext) && !parameters[0].ParameterType.IsByRef)
			{
				EmitUtilities.InstanceRefMethodCaller<T, StreamingContext> action2 = EmitUtilities.CreateInstanceRefMethodCaller<T, StreamingContext>(info);
				return delegate(ref T value, StreamingContext context)
				{
					action2(ref value, context);
				};
			}
			DefaultLoggers.DefaultLogger.LogWarning("The method " + info.GetNiceName() + " has an invalid signature and will be ignored by the serialization system.");
			return null;
		}

		/// <summary>
		/// Serializes a value using a specified <see cref="T:Sirenix.Serialization.IDataWriter" />.
		/// </summary>
		/// <param name="value">The value to serialize.</param>
		/// <param name="writer">The writer to use.</param>
		void IFormatter.Serialize(object value, IDataWriter writer)
		{
			Serialize((T)value, writer);
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
		/// Deserializes a value of type <see cref="!:T" /> using a specified <see cref="T:Sirenix.Serialization.IDataReader" />.
		/// </summary>
		/// <param name="reader">The reader to use.</param>
		/// <returns>
		/// The deserialized value.
		/// </returns>
		public T Deserialize(IDataReader reader)
		{
			DeserializationContext context = reader.Context;
			T value = GetUninitializedObject();
			if (IsValueType)
			{
				InvokeOnDeserializingCallbacks(ref value, context);
			}
			else if (value != null)
			{
				RegisterReferenceID(value, reader);
				InvokeOnDeserializingCallbacks(ref value, context);
				if (ImplementsIObjectReference)
				{
					try
					{
						value = (T)(value as IObjectReference).GetRealObject(context.StreamingContext);
						RegisterReferenceID(value, reader);
					}
					catch (Exception exception)
					{
						context.Config.DebugContext.LogException(exception);
					}
				}
			}
			try
			{
				DeserializeImplementation(ref value, reader);
			}
			catch (Exception exception2)
			{
				context.Config.DebugContext.LogException(exception2);
			}
			if (IsValueType || value != null)
			{
				for (int i = 0; i < OnDeserializedCallbacks.Length; i++)
				{
					try
					{
						OnDeserializedCallbacks[i](ref value, context.StreamingContext);
					}
					catch (Exception exception3)
					{
						context.Config.DebugContext.LogException(exception3);
					}
				}
				if (ImplementsIDeserializationCallback)
				{
					IDeserializationCallback v = value as IDeserializationCallback;
					v.OnDeserialization(this);
					value = (T)v;
				}
				if (ImplementsISerializationCallbackReceiver)
				{
					try
					{
						ISerializationCallbackReceiver v2 = value as ISerializationCallbackReceiver;
						v2.OnAfterDeserialize();
						value = (T)v2;
					}
					catch (Exception exception4)
					{
						context.Config.DebugContext.LogException(exception4);
					}
				}
			}
			return value;
		}

		/// <summary>
		/// Serializes a value of type <see cref="!:T" /> using a specified <see cref="T:Sirenix.Serialization.IDataWriter" />.
		/// </summary>
		/// <param name="value">The value to serialize.</param>
		/// <param name="writer">The writer to use.</param>
		public void Serialize(T value, IDataWriter writer)
		{
			SerializationContext context = writer.Context;
			for (int i = 0; i < OnSerializingCallbacks.Length; i++)
			{
				try
				{
					OnSerializingCallbacks[i](ref value, context.StreamingContext);
				}
				catch (Exception exception)
				{
					context.Config.DebugContext.LogException(exception);
				}
			}
			if (ImplementsISerializationCallbackReceiver)
			{
				try
				{
					ISerializationCallbackReceiver v = value as ISerializationCallbackReceiver;
					v.OnBeforeSerialize();
					value = (T)v;
				}
				catch (Exception exception2)
				{
					context.Config.DebugContext.LogException(exception2);
				}
			}
			try
			{
				SerializeImplementation(ref value, writer);
			}
			catch (Exception exception3)
			{
				context.Config.DebugContext.LogException(exception3);
			}
			for (int j = 0; j < OnSerializedCallbacks.Length; j++)
			{
				try
				{
					OnSerializedCallbacks[j](ref value, context.StreamingContext);
				}
				catch (Exception exception4)
				{
					context.Config.DebugContext.LogException(exception4);
				}
			}
		}

		/// <summary>
		/// Get an uninitialized object of type <see cref="!:T" />. WARNING: If you override this and return null, the object's ID will not be automatically registered and its OnDeserializing callbacks will not be automatically called, before deserialization begins.
		/// You will have to call <see cref="M:Sirenix.Serialization.BaseFormatter`1.RegisterReferenceID(`0,Sirenix.Serialization.IDataReader)" /> and <see cref="M:Sirenix.Serialization.BaseFormatter`1.InvokeOnDeserializingCallbacks(`0@,Sirenix.Serialization.DeserializationContext)" /> immediately after creating the object yourself during deserialization.
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

		/// <summary>
		/// Invokes all methods on the object with the [OnDeserializing] attribute.
		/// <para />
		/// WARNING: This method will not be called automatically if you override GetUninitializedObject and return null! You will have to call it manually after having created the object instance during deserialization.
		/// </summary>
		/// <param name="value">The value to invoke the callbacks on.</param>
		/// <param name="context">The deserialization context.</param>
		[Obsolete("Use the InvokeOnDeserializingCallbacks variant that takes a ref T value instead. This is for struct compatibility reasons.", false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		protected void InvokeOnDeserializingCallbacks(T value, DeserializationContext context)
		{
			InvokeOnDeserializingCallbacks(ref value, context);
		}

		/// <summary>
		/// Invokes all methods on the object with the [OnDeserializing] attribute.
		/// <para />
		/// WARNING: This method will not be called automatically if you override GetUninitializedObject and return null! You will have to call it manually after having created the object instance during deserialization.
		/// </summary>
		/// <param name="value">The value to invoke the callbacks on.</param>
		/// <param name="context">The deserialization context.</param>
		protected void InvokeOnDeserializingCallbacks(ref T value, DeserializationContext context)
		{
			for (int i = 0; i < OnDeserializingCallbacks.Length; i++)
			{
				try
				{
					OnDeserializingCallbacks[i](ref value, context.StreamingContext);
				}
				catch (Exception exception)
				{
					context.Config.DebugContext.LogException(exception);
				}
			}
		}

		/// <summary>
		/// Provides the actual implementation for deserializing a value of type <see cref="!:T" />.
		/// </summary>
		/// <param name="value">The uninitialized value to serialize into. This value will have been created earlier using <see cref="M:Sirenix.Serialization.BaseFormatter`1.GetUninitializedObject" />.</param>
		/// <param name="reader">The reader to deserialize with.</param>
		protected abstract void DeserializeImplementation(ref T value, IDataReader reader);

		/// <summary>
		/// Provides the actual implementation for serializing a value of type <see cref="!:T" />.
		/// </summary>
		/// <param name="value">The value to serialize.</param>
		/// <param name="writer">The writer to serialize with.</param>
		protected abstract void SerializeImplementation(ref T value, IDataWriter writer);
	}
}
