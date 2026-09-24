using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Serialization;
using Sirenix.Serialization.Utilities;
using UnityEngine;

namespace Sirenix.Serialization
{
	/// <summary>
	/// Provides common functionality for serializing and deserializing weakly typed values of a given type, and provides automatic support for the following common serialization conventions:
	/// <para />
	/// <see cref="T:System.Runtime.Serialization.IObjectReference" />, <see cref="!:ISerializationCallbackReceiver" />, <see cref="T:System.Runtime.Serialization.OnSerializingAttribute" />, <see cref="T:System.Runtime.Serialization.OnSerializedAttribute" />, <see cref="T:System.Runtime.Serialization.OnDeserializingAttribute" /> and <see cref="T:System.Runtime.Serialization.OnDeserializedAttribute" />.
	/// </summary>
	/// <seealso cref="T:Sirenix.Serialization.IFormatter" />
	public abstract class WeakBaseFormatter : IFormatter
	{
		protected delegate void SerializationCallback(object value, StreamingContext context);

		protected readonly Type SerializedType;

		protected readonly SerializationCallback[] OnSerializingCallbacks;

		protected readonly SerializationCallback[] OnSerializedCallbacks;

		protected readonly SerializationCallback[] OnDeserializingCallbacks;

		protected readonly SerializationCallback[] OnDeserializedCallbacks;

		protected readonly bool IsValueType;

		protected readonly bool ImplementsISerializationCallbackReceiver;

		protected readonly bool ImplementsIDeserializationCallback;

		protected readonly bool ImplementsIObjectReference;

		Type IFormatter.SerializedType => SerializedType;

		public WeakBaseFormatter(Type serializedType)
		{
			SerializedType = serializedType;
			ImplementsISerializationCallbackReceiver = SerializedType.ImplementsOrInherits(typeof(ISerializationCallbackReceiver));
			ImplementsIDeserializationCallback = SerializedType.ImplementsOrInherits(typeof(IDeserializationCallback));
			ImplementsIObjectReference = SerializedType.ImplementsOrInherits(typeof(IObjectReference));
			if (SerializedType.ImplementsOrInherits(typeof(UnityEngine.Object)))
			{
				DefaultLoggers.DefaultLogger.LogWarning("A formatter has been created for the UnityEngine.Object type " + SerializedType.Name + " - this is *strongly* discouraged. Unity should be allowed to handle serialization and deserialization of its own weird objects. Remember to serialize with a UnityReferenceResolver as the external index reference resolver in the serialization context.\n\n Stacktrace: " + new StackTrace().ToString());
			}
			MethodInfo[] methods = SerializedType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
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
				return delegate(object value, StreamingContext context)
				{
					info.Invoke(value, null);
				};
			}
			if (parameters.Length == 1 && parameters[0].ParameterType == typeof(StreamingContext) && !parameters[0].ParameterType.IsByRef)
			{
				return delegate(object value, StreamingContext context)
				{
					info.Invoke(value, new object[1] { context });
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
		public void Serialize(object value, IDataWriter writer)
		{
			SerializationContext context = writer.Context;
			for (int i = 0; i < OnSerializingCallbacks.Length; i++)
			{
				try
				{
					OnSerializingCallbacks[i](value, context.StreamingContext);
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
					value = v;
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
					OnSerializedCallbacks[j](value, context.StreamingContext);
				}
				catch (Exception exception4)
				{
					context.Config.DebugContext.LogException(exception4);
				}
			}
		}

		/// <summary>
		/// Deserializes a value using a specified <see cref="T:Sirenix.Serialization.IDataReader" />.
		/// </summary>
		/// <param name="reader">The reader to use.</param>
		/// <returns>
		/// The deserialized value.
		/// </returns>
		public object Deserialize(IDataReader reader)
		{
			DeserializationContext context = reader.Context;
			object value = GetUninitializedObject();
			if (IsValueType)
			{
				if (value == null)
				{
					value = Activator.CreateInstance(SerializedType);
				}
				InvokeOnDeserializingCallbacks(value, context);
			}
			else if (value != null)
			{
				RegisterReferenceID(value, reader);
				InvokeOnDeserializingCallbacks(value, context);
				if (ImplementsIObjectReference)
				{
					try
					{
						value = (value as IObjectReference).GetRealObject(context.StreamingContext);
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
						OnDeserializedCallbacks[i](value, context.StreamingContext);
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
					value = v;
				}
				if (ImplementsISerializationCallbackReceiver)
				{
					try
					{
						ISerializationCallbackReceiver v2 = value as ISerializationCallbackReceiver;
						v2.OnAfterDeserialize();
						value = v2;
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
		/// Registers the given object reference in the deserialization context.
		/// <para />
		/// NOTE that this method only does anything if <see cref="!:T" /> is not a value type.
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

		/// <summary>
		/// Invokes all methods on the object with the [OnDeserializing] attribute.
		/// <para />
		/// WARNING: This method will not be called automatically if you override GetUninitializedObject and return null! You will have to call it manually after having created the object instance during deserialization.
		/// </summary>
		/// <param name="value">The value to invoke the callbacks on.</param>
		/// <param name="context">The deserialization context.</param>
		protected void InvokeOnDeserializingCallbacks(object value, DeserializationContext context)
		{
			for (int i = 0; i < OnDeserializingCallbacks.Length; i++)
			{
				try
				{
					OnDeserializingCallbacks[i](value, context.StreamingContext);
				}
				catch (Exception exception)
				{
					context.Config.DebugContext.LogException(exception);
				}
			}
		}

		protected virtual object GetUninitializedObject()
		{
			if (!IsValueType)
			{
				return FormatterServices.GetUninitializedObject(SerializedType);
			}
			return Activator.CreateInstance(SerializedType);
		}

		/// <summary>
		/// Provides the actual implementation for deserializing a value of type <see cref="!:T" />.
		/// </summary>
		/// <param name="value">The uninitialized value to serialize into. This value will have been created earlier using <see cref="M:Sirenix.Serialization.BaseFormatter`1.GetUninitializedObject" />.</param>
		/// <param name="reader">The reader to deserialize with.</param>
		protected abstract void DeserializeImplementation(ref object value, IDataReader reader);

		/// <summary>
		/// Provides the actual implementation for serializing a value of type <see cref="!:T" />.
		/// </summary>
		/// <param name="value">The value to serialize.</param>
		/// <param name="writer">The writer to serialize with.</param>
		protected abstract void SerializeImplementation(ref object value, IDataWriter writer);
	}
}
