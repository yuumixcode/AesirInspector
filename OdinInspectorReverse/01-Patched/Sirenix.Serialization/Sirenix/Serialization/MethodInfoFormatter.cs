using System;
using System.Linq;
using System.Reflection;
using Sirenix.Serialization.Utilities;

namespace Sirenix.Serialization
{
	/// <summary>
	/// Custom formatter for MethodInfo, since Unity Mono's MethodInfo ISerializable implementation will often crash if the method no longer exists upon deserialization.
	/// </summary>
	/// <seealso cref="T:Sirenix.Serialization.BaseFormatter`1" />
	public class MethodInfoFormatter<T> : BaseFormatter<T> where T : MethodInfo
	{
		private static readonly Serializer<string> StringSerializer = Serializer.Get<string>();

		private static readonly Serializer<Type> TypeSerializer = Serializer.Get<Type>();

		private static readonly Serializer<Type[]> TypeArraySerializer = Serializer.Get<Type[]>();

		/// <summary>
		/// Provides the actual implementation for deserializing a value of type <see cref="!:T" />.
		/// </summary>
		/// <param name="value">The uninitialized value to serialize into. This value will have been created earlier using <see cref="M:OdinSerializer.BaseFormatter`1.GetUninitializedObject" />.</param>
		/// <param name="reader">The reader to deserialize with.</param>
		protected override void DeserializeImplementation(ref T value, IDataReader reader)
		{
			EntryType entry = reader.PeekEntry(out var name);
			if (entry == EntryType.StartOfArray)
			{
				IFormatter serializableFormatter = new WeakSerializableFormatter(typeof(T));
				value = (T)serializableFormatter.Deserialize(reader);
				return;
			}
			Type declaringType = null;
			string methodName = null;
			Type[] signature = null;
			Type[] genericArguments = null;
			while ((entry = reader.PeekEntry(out name)) != EntryType.EndOfNode && entry != EntryType.EndOfArray && entry != EntryType.EndOfStream)
			{
				switch (name)
				{
				case "declaringType":
				{
					Type t = TypeSerializer.ReadValue(reader);
					if (t != null)
					{
						declaringType = t;
					}
					break;
				}
				case "methodName":
					methodName = StringSerializer.ReadValue(reader);
					break;
				case "signature":
					signature = TypeArraySerializer.ReadValue(reader);
					break;
				case "genericArguments":
					genericArguments = TypeArraySerializer.ReadValue(reader);
					break;
				default:
					reader.SkipEntry();
					break;
				}
			}
			if (declaringType == null)
			{
				reader.Context.Config.DebugContext.LogWarning("Missing declaring type of MethodInfo on deserialize.");
				return;
			}
			if (methodName == null)
			{
				reader.Context.Config.DebugContext.LogError("Missing method name of MethodInfo on deserialize.");
				return;
			}
			bool useSignature = false;
			bool wasAmbiguous = false;
			if (signature != null)
			{
				useSignature = true;
				for (int i = 0; i < signature.Length; i++)
				{
					if (signature[i] == null)
					{
						useSignature = false;
						break;
					}
				}
			}
			MethodInfo methodInfo;
			if (useSignature)
			{
				try
				{
					methodInfo = declaringType.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy, null, signature, null);
				}
				catch (AmbiguousMatchException)
				{
					methodInfo = null;
					wasAmbiguous = true;
				}
			}
			else
			{
				try
				{
					methodInfo = declaringType.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
				}
				catch (AmbiguousMatchException)
				{
					methodInfo = null;
					wasAmbiguous = true;
				}
			}
			if (methodInfo == null)
			{
				if (useSignature)
				{
					reader.Context.Config.DebugContext.LogWarning("Could not find method with signature " + name + "(" + string.Join(", ", signature.Select((Type p) => p.GetNiceFullName()).ToArray()) + ") on type '" + declaringType.FullName + (wasAmbiguous ? "; resolution was ambiguous between multiple methods" : string.Empty) + ".");
				}
				else
				{
					reader.Context.Config.DebugContext.LogWarning("Could not find method with name " + name + " on type '" + declaringType.GetNiceFullName() + (wasAmbiguous ? "; resolution was ambiguous between multiple methods" : string.Empty) + ".");
				}
				return;
			}
			if (methodInfo.IsGenericMethodDefinition)
			{
				if (genericArguments == null)
				{
					reader.Context.Config.DebugContext.LogWarning("Method '" + declaringType.GetNiceFullName() + "." + methodInfo.GetNiceName() + "' to deserialize is a generic method definition, but no generic arguments were in the serialization data.");
					return;
				}
				int argCount = methodInfo.GetGenericArguments().Length;
				if (genericArguments.Length != argCount)
				{
					reader.Context.Config.DebugContext.LogWarning("Method '" + declaringType.GetNiceFullName() + "." + methodInfo.GetNiceName() + "' to deserialize is a generic method definition, but there is the wrong number of generic arguments in the serialization data.");
					return;
				}
				for (int i2 = 0; i2 < genericArguments.Length; i2++)
				{
					if (genericArguments[i2] == null)
					{
						reader.Context.Config.DebugContext.LogWarning("Method '" + declaringType.GetNiceFullName() + "." + methodInfo.GetNiceName() + "' to deserialize is a generic method definition, but one of the serialized generic argument types failed to bind on deserialization.");
						return;
					}
				}
				try
				{
					methodInfo = methodInfo.MakeGenericMethod(genericArguments);
				}
				catch (Exception ex3)
				{
					reader.Context.Config.DebugContext.LogWarning("Method '" + declaringType.GetNiceFullName() + "." + methodInfo.GetNiceName() + "' to deserialize is a generic method definition, but failed to create generic method from definition, using generic arguments '" + string.Join(", ", genericArguments.Select((Type p) => p.GetNiceFullName()).ToArray()) + "'. Method creation failed with an exception of type " + ex3.GetType().GetNiceFullName() + ", with the message: " + ex3.Message);
					return;
				}
			}
			try
			{
				value = (T)methodInfo;
			}
			catch (InvalidCastException)
			{
				reader.Context.Config.DebugContext.LogWarning("The serialized method '" + declaringType.GetNiceFullName() + "." + methodInfo.GetNiceName() + "' was successfully resolved into a MethodInfo reference of the runtime type '" + methodInfo.GetType().GetNiceFullName() + "', but failed to be cast to expected type '" + typeof(T).GetNiceFullName() + "'.");
				return;
			}
			RegisterReferenceID(value, reader);
		}

		/// <summary>
		/// Provides the actual implementation for serializing a value of type <see cref="!:T" />.
		/// </summary>
		/// <param name="value">The value to serialize.</param>
		/// <param name="writer">The writer to serialize with.</param>
		protected override void SerializeImplementation(ref T value, IDataWriter writer)
		{
			MethodInfo methodInfo = value;
			if (methodInfo.GetType().Name.Contains("DynamicMethod"))
			{
				writer.Context.Config.DebugContext.LogWarning("Cannot serialize a dynamically emitted method " + methodInfo?.ToString() + ".");
				return;
			}
			if (methodInfo.IsGenericMethodDefinition)
			{
				writer.Context.Config.DebugContext.LogWarning("Serializing a MethodInfo for a generic method definition '" + methodInfo.GetNiceName() + "' is not currently supported.");
				return;
			}
			TypeSerializer.WriteValue("declaringType", methodInfo.DeclaringType, writer);
			StringSerializer.WriteValue("methodName", methodInfo.Name, writer);
			ParameterInfo[] parameters = ((!methodInfo.IsGenericMethod) ? methodInfo.GetParameters() : methodInfo.GetGenericMethodDefinition().GetParameters());
			Type[] signature = new Type[parameters.Length];
			for (int i = 0; i < signature.Length; i++)
			{
				signature[i] = parameters[i].ParameterType;
			}
			TypeArraySerializer.WriteValue("signature", signature, writer);
			if (methodInfo.IsGenericMethod)
			{
				Type[] genericArguments = methodInfo.GetGenericArguments();
				TypeArraySerializer.WriteValue("genericArguments", genericArguments, writer);
			}
		}

		/// <summary>
		/// Get an uninitialized object of type <see cref="!:T" />. WARNING: If you override this and return null, the object's ID will not be automatically registered and its OnDeserializing callbacks will not be automatically called, before deserialization begins.
		/// You will have to call <see cref="M:OdinSerializer.BaseFormatter`1.RegisterReferenceID(`0,OdinSerializer.IDataReader)" /> and <see cref="M:OdinSerializer.BaseFormatter`1.InvokeOnDeserializingCallbacks(`0,OdinSerializer.DeserializationContext)" /> immediately after creating the object yourself during deserialization.
		/// </summary>
		/// <returns>
		/// An uninitialized object of type <see cref="!:T" />.
		/// </returns>
		protected override T GetUninitializedObject()
		{
			return null;
		}
	}
}
