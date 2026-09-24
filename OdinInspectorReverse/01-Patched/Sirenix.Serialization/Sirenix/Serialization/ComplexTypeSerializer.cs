#define UNITY_EDITOR
using System;
using System.Collections.Generic;
using Sirenix.Serialization.Utilities;

namespace Sirenix.Serialization
{
	/// <summary>
	/// Serializer for all complex types; IE, types which are not primitives as determined by the <see cref="M:Sirenix.Serialization.FormatterUtilities.IsPrimitiveType(System.Type)" /> method.
	/// </summary>
	/// <typeparam name="T">The type which the <see cref="T:Sirenix.Serialization.ComplexTypeSerializer`1" /> can serialize and deserialize.</typeparam>
	/// <seealso cref="T:Sirenix.Serialization.Serializer`1" />
	public class ComplexTypeSerializer<T> : Serializer<T>
	{
		private static readonly bool ComplexTypeMayBeBoxedValueType = typeof(T).IsInterface || typeof(T) == typeof(object) || typeof(T) == typeof(ValueType) || typeof(T) == typeof(Enum);

		private static readonly bool ComplexTypeIsAbstract = typeof(T).IsAbstract || typeof(T).IsInterface;

		private static readonly bool ComplexTypeIsNullable = typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(Nullable<>);

		private static readonly bool ComplexTypeIsValueType = typeof(T).IsValueType;

		private static readonly Type TypeOf_T = typeof(T);

		private static readonly bool AllowDeserializeInvalidDataForT = typeof(T).IsDefined(typeof(AllowDeserializeInvalidDataAttribute), inherit: true);

		private static readonly Dictionary<ISerializationPolicy, IFormatter<T>> FormattersByPolicy = new Dictionary<ISerializationPolicy, IFormatter<T>>(ReferenceEqualityComparer<ISerializationPolicy>.Default);

		private static readonly object FormattersByPolicy_LOCK = new object();

		private static readonly ISerializationPolicy UnityPolicy = SerializationPolicies.Unity;

		private static readonly ISerializationPolicy StrictPolicy = SerializationPolicies.Strict;

		private static readonly ISerializationPolicy EverythingPolicy = SerializationPolicies.Everything;

		private static IFormatter<T> UnityPolicyFormatter;

		private static IFormatter<T> StrictPolicyFormatter;

		private static IFormatter<T> EverythingPolicyFormatter;

		/// <summary>
		/// Reads a value of type <see cref="!:T" />.
		/// </summary>
		/// <param name="reader">The reader to use.</param>
		/// <returns>
		/// The value which has been read.
		/// </returns>
		public override T ReadValue(IDataReader reader)
		{
			DeserializationContext context = reader.Context;
			if (!context.Config.SerializationPolicy.AllowNonSerializableTypes && !TypeOf_T.IsSerializable)
			{
				context.Config.DebugContext.LogError("The type " + TypeOf_T.GetNiceFullName() + " is not marked as serializable.");
				return default(T);
			}
			bool exitNode = true;
			string name;
			EntryType entry = reader.PeekEntry(out name);
			if (ComplexTypeIsValueType)
			{
				switch (entry)
				{
				case EntryType.Null:
					context.Config.DebugContext.LogWarning("Expecting complex struct of type " + TypeOf_T.GetNiceFullName() + " but got null value.");
					reader.ReadNull();
					return default(T);
				default:
					context.Config.DebugContext.LogWarning("Unexpected entry '" + name + "' of type " + entry.ToString() + ", when " + EntryType.StartOfNode.ToString() + " was expected. A value has likely been lost.");
					reader.SkipEntry();
					return default(T);
				case EntryType.StartOfNode:
					try
					{
						Type expectedType = TypeOf_T;
						if (reader.EnterNode(out var serializedType))
						{
							if (serializedType != expectedType)
							{
								if (serializedType != null)
								{
									context.Config.DebugContext.LogWarning("Expected complex struct value " + expectedType.GetNiceFullName() + " but the serialized value is of type " + serializedType.GetNiceFullName() + ".");
									if (serializedType.IsCastableTo(expectedType))
									{
										object value = FormatterLocator.GetFormatter(serializedType, context.Config.SerializationPolicy).Deserialize(reader);
										bool serializedTypeIsNullable = serializedType.IsGenericType && serializedType.GetGenericTypeDefinition() == typeof(Nullable<>);
										Func<object, object> castMethod = ((!ComplexTypeIsNullable && !serializedTypeIsNullable) ? serializedType.GetCastMethodDelegate(expectedType) : null);
										if (castMethod != null)
										{
											return (T)castMethod(value);
										}
										return (T)value;
									}
									if (AllowDeserializeInvalidDataForT || reader.Context.Config.AllowDeserializeInvalidData)
									{
										context.Config.DebugContext.LogWarning("Can't cast serialized type " + serializedType.GetNiceFullName() + " into expected type " + expectedType.GetNiceFullName() + ". Attempting to deserialize with possibly invalid data. Value may be lost or corrupted for node '" + name + "'.");
										return GetBaseFormatter(context.Config.SerializationPolicy).Deserialize(reader);
									}
									context.Config.DebugContext.LogWarning("Can't cast serialized type " + serializedType.GetNiceFullName() + " into expected type " + expectedType.GetNiceFullName() + ". Value lost for node '" + name + "'.");
									return default(T);
								}
								if (AllowDeserializeInvalidDataForT || reader.Context.Config.AllowDeserializeInvalidData)
								{
									context.Config.DebugContext.LogWarning("Expected complex struct value " + expectedType.GetNiceFullName() + " but the serialized type could not be resolved. Attempting to deserialize with possibly invalid data. Value may be lost or corrupted for node '" + name + "'.");
									return GetBaseFormatter(context.Config.SerializationPolicy).Deserialize(reader);
								}
								context.Config.DebugContext.LogWarning("Expected complex struct value " + expectedType.GetNiceFullName() + " but the serialized type could not be resolved. Value lost for node '" + name + "'.");
								return default(T);
							}
							return GetBaseFormatter(context.Config.SerializationPolicy).Deserialize(reader);
						}
						context.Config.DebugContext.LogError("Failed to enter node '" + name + "'.");
						return default(T);
					}
					catch (SerializationAbortException ex)
					{
						exitNode = false;
						throw ex;
					}
					catch (Exception exception)
					{
						context.Config.DebugContext.LogException(exception);
						return default(T);
					}
					finally
					{
						if (exitNode)
						{
							reader.ExitNode();
						}
					}
				}
			}
			switch (entry)
			{
			case EntryType.Null:
				reader.ReadNull();
				return default(T);
			case EntryType.ExternalReferenceByIndex:
			{
				reader.ReadExternalReference(out int index);
				object value9 = context.GetExternalObject(index);
				try
				{
					return (T)value9;
				}
				catch (InvalidCastException)
				{
					context.Config.DebugContext.LogWarning("Can't cast external reference type " + value9.GetType().GetNiceFullName() + " into expected type " + TypeOf_T.GetNiceFullName() + ". Value lost for node '" + name + "'.");
					return default(T);
				}
			}
			case EntryType.ExternalReferenceByGuid:
			{
				reader.ReadExternalReference(out Guid guid);
				object value7 = context.GetExternalObject(guid);
				try
				{
					return (T)value7;
				}
				catch (InvalidCastException)
				{
					context.Config.DebugContext.LogWarning("Can't cast external reference type " + value7.GetType().GetNiceFullName() + " into expected type " + TypeOf_T.GetNiceFullName() + ". Value lost for node '" + name + "'.");
					return default(T);
				}
			}
			case EntryType.ExternalReferenceByString:
			{
				reader.ReadExternalReference(out string id2);
				object value5 = context.GetExternalObject(id2);
				try
				{
					return (T)value5;
				}
				catch (InvalidCastException)
				{
					context.Config.DebugContext.LogWarning("Can't cast external reference type " + value5.GetType().GetNiceFullName() + " into expected type " + TypeOf_T.GetNiceFullName() + ". Value lost for node '" + name + "'.");
					return default(T);
				}
			}
			case EntryType.InternalReference:
			{
				reader.ReadInternalReference(out var id);
				object value3 = context.GetInternalReference(id);
				try
				{
					return (T)value3;
				}
				catch (InvalidCastException)
				{
					context.Config.DebugContext.LogWarning("Can't cast internal reference type " + value3.GetType().GetNiceFullName() + " into expected type " + TypeOf_T.GetNiceFullName() + ". Value lost for node '" + name + "'.");
					return default(T);
				}
			}
			case EntryType.StartOfNode:
				try
				{
					Type expectedType2 = TypeOf_T;
					if (reader.EnterNode(out var serializedType2))
					{
						int id3 = reader.CurrentNodeId;
						T result;
						if (!(serializedType2 != null) || !(expectedType2 != serializedType2))
						{
							result = ((!ComplexTypeIsAbstract) ? GetBaseFormatter(context.Config.SerializationPolicy).Deserialize(reader) : default(T));
						}
						else
						{
							bool success = false;
							bool isPrimitive = FormatterUtilities.IsPrimitiveType(serializedType2);
							bool assignableCast;
							if (ComplexTypeMayBeBoxedValueType && isPrimitive)
							{
								Serializer serializer = Serializer.Get(serializedType2);
								result = (T)serializer.ReadValueWeak(reader);
								success = true;
							}
							else if ((assignableCast = expectedType2.IsAssignableFrom(serializedType2)) || serializedType2.HasCastDefined(expectedType2, requireImplicitCast: false))
							{
								try
								{
									object value11;
									if (isPrimitive)
									{
										Serializer serializer2 = Serializer.Get(serializedType2);
										value11 = serializer2.ReadValueWeak(reader);
									}
									else
									{
										IFormatter alternateFormatter = FormatterLocator.GetFormatter(serializedType2, context.Config.SerializationPolicy);
										value11 = alternateFormatter.Deserialize(reader);
									}
									if (assignableCast)
									{
										result = (T)value11;
									}
									else
									{
										Func<object, object> castMethod2 = serializedType2.GetCastMethodDelegate(expectedType2);
										result = ((castMethod2 == null) ? ((T)value11) : ((T)castMethod2(value11)));
									}
									success = true;
								}
								catch (SerializationAbortException ex6)
								{
									exitNode = false;
									throw ex6;
								}
								catch (InvalidCastException)
								{
									success = false;
									result = default(T);
								}
							}
							else if (!ComplexTypeIsAbstract && (AllowDeserializeInvalidDataForT || reader.Context.Config.AllowDeserializeInvalidData))
							{
								context.Config.DebugContext.LogWarning("Can't cast serialized type " + serializedType2.GetNiceFullName() + " into expected type " + expectedType2.GetNiceFullName() + ". Attempting to deserialize with invalid data. Value may be lost or corrupted for node '" + name + "'.");
								result = GetBaseFormatter(context.Config.SerializationPolicy).Deserialize(reader);
								success = true;
							}
							else
							{
								IFormatter alternateFormatter2 = FormatterLocator.GetFormatter(serializedType2, context.Config.SerializationPolicy);
								object value12 = alternateFormatter2.Deserialize(reader);
								if (id3 >= 0)
								{
									context.RegisterInternalReference(id3, value12);
								}
								result = default(T);
							}
							if (!success)
							{
								context.Config.DebugContext.LogWarning("Can't cast serialized type " + serializedType2.GetNiceFullName() + " into expected type " + expectedType2.GetNiceFullName() + ". Value lost for node '" + name + "'.");
								result = default(T);
							}
						}
						if (id3 >= 0)
						{
							context.RegisterInternalReference(id3, result);
						}
						return result;
					}
					context.Config.DebugContext.LogError("Failed to enter node '" + name + "'.");
					return default(T);
				}
				catch (SerializationAbortException ex8)
				{
					exitNode = false;
					throw ex8;
				}
				catch (Exception exception2)
				{
					context.Config.DebugContext.LogException(exception2);
					return default(T);
				}
				finally
				{
					if (exitNode)
					{
						reader.ExitNode();
					}
				}
			case EntryType.Boolean:
				if (ComplexTypeMayBeBoxedValueType)
				{
					reader.ReadBoolean(out var value10);
					return (T)(object)value10;
				}
				break;
			case EntryType.FloatingPoint:
				if (ComplexTypeMayBeBoxedValueType)
				{
					reader.ReadDouble(out var value8);
					return (T)(object)value8;
				}
				break;
			case EntryType.Integer:
				if (ComplexTypeMayBeBoxedValueType)
				{
					reader.ReadInt64(out var value6);
					return (T)(object)value6;
				}
				break;
			case EntryType.String:
				if (ComplexTypeMayBeBoxedValueType)
				{
					reader.ReadString(out var value4);
					return (T)(object)value4;
				}
				break;
			case EntryType.Guid:
				if (ComplexTypeMayBeBoxedValueType)
				{
					reader.ReadGuid(out var value2);
					return (T)(object)value2;
				}
				break;
			}
			context.Config.DebugContext.LogWarning("Unexpected entry of type " + entry.ToString() + ", when a reference or node start was expected. A value has been lost.");
			reader.SkipEntry();
			return default(T);
		}

		private static IFormatter<T> GetBaseFormatter(ISerializationPolicy serializationPolicy)
		{
			if (serializationPolicy == UnityPolicy)
			{
				if (UnityPolicyFormatter == null)
				{
					UnityPolicyFormatter = FormatterLocator.GetFormatter<T>(UnityPolicy);
				}
				return UnityPolicyFormatter;
			}
			if (serializationPolicy == EverythingPolicy)
			{
				if (EverythingPolicyFormatter == null)
				{
					EverythingPolicyFormatter = FormatterLocator.GetFormatter<T>(EverythingPolicy);
				}
				return EverythingPolicyFormatter;
			}
			if (serializationPolicy == StrictPolicy)
			{
				if (StrictPolicyFormatter == null)
				{
					StrictPolicyFormatter = FormatterLocator.GetFormatter<T>(StrictPolicy);
				}
				return StrictPolicyFormatter;
			}
			IFormatter<T> formatter;
			lock (FormattersByPolicy_LOCK)
			{
				if (!FormattersByPolicy.TryGetValue(serializationPolicy, out formatter))
				{
					formatter = FormatterLocator.GetFormatter<T>(serializationPolicy);
					FormattersByPolicy.Add(serializationPolicy, formatter);
				}
			}
			return formatter;
		}

		/// <summary>
		/// Writes a value of type <see cref="!:T" />.
		/// </summary>
		/// <param name="name">The name of the value to write.</param>
		/// <param name="value">The value to write.</param>
		/// <param name="writer">The writer to use.</param>
		public override void WriteValue(string name, T value, IDataWriter writer)
		{
			SerializationContext context = writer.Context;
			ISerializationPolicy policy = context.Config.SerializationPolicy;
			if (!policy.AllowNonSerializableTypes && !TypeOf_T.IsSerializable)
			{
				context.Config.DebugContext.LogError("The type " + TypeOf_T.GetNiceFullName() + " is not marked as serializable.");
				return;
			}
			Serializer<T>.FireOnSerializedType();
			if (ComplexTypeIsValueType)
			{
				bool endNode = true;
				try
				{
					writer.BeginStructNode(name, TypeOf_T);
					GetBaseFormatter(policy).Serialize(value, writer);
					return;
				}
				catch (SerializationAbortException ex)
				{
					endNode = false;
					throw ex;
				}
				finally
				{
					if (endNode)
					{
						writer.EndNode(name);
					}
				}
			}
			bool endNode2 = true;
			if (value == null)
			{
				writer.WriteNull(name);
				return;
			}
			if (context.TryRegisterExternalReference((object)value, out int index))
			{
				writer.WriteExternalReference(name, index);
				return;
			}
			if (context.TryRegisterExternalReference((object)value, out Guid guid))
			{
				writer.WriteExternalReference(name, guid);
				return;
			}
			if (context.TryRegisterExternalReference((object)value, out string strId))
			{
				writer.WriteExternalReference(name, strId);
				return;
			}
			if (context.TryRegisterInternalReference(value, out var id))
			{
				Type type = value.GetType();
				if (ComplexTypeMayBeBoxedValueType && FormatterUtilities.IsPrimitiveType(type))
				{
					try
					{
						writer.BeginReferenceNode(name, type, id);
						Serializer serializer = Serializer.Get(type);
						serializer.WriteValueWeak(value, writer);
						return;
					}
					catch (SerializationAbortException ex2)
					{
						endNode2 = false;
						throw ex2;
					}
					finally
					{
						if (endNode2)
						{
							writer.EndNode(name);
						}
					}
				}
				IFormatter formatter = (((object)type != TypeOf_T) ? FormatterLocator.GetFormatter(type, policy) : GetBaseFormatter(policy));
				try
				{
					writer.BeginReferenceNode(name, type, id);
					formatter.Serialize(value, writer);
					return;
				}
				catch (SerializationAbortException ex3)
				{
					endNode2 = false;
					throw ex3;
				}
				finally
				{
					if (endNode2)
					{
						writer.EndNode(name);
					}
				}
			}
			writer.WriteInternalReference(name, id);
		}
	}
}
