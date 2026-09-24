#define UNITY_EDITOR
using System;
using System.Collections.Generic;
using Sirenix.Serialization.Utilities;

namespace Sirenix.Serialization
{
	public sealed class AnySerializer : Serializer
	{
		private static readonly ISerializationPolicy UnityPolicy = SerializationPolicies.Unity;

		private static readonly ISerializationPolicy StrictPolicy = SerializationPolicies.Strict;

		private static readonly ISerializationPolicy EverythingPolicy = SerializationPolicies.Everything;

		private readonly Type SerializedType;

		private readonly bool IsEnum;

		private readonly bool IsValueType;

		private readonly bool MayBeBoxedValueType;

		private readonly bool IsAbstract;

		private readonly bool IsNullable;

		private readonly bool AllowDeserializeInvalidData;

		private IFormatter UnityPolicyFormatter;

		private IFormatter StrictPolicyFormatter;

		private IFormatter EverythingPolicyFormatter;

		private readonly Dictionary<ISerializationPolicy, IFormatter> FormattersByPolicy = new Dictionary<ISerializationPolicy, IFormatter>(ReferenceEqualityComparer<ISerializationPolicy>.Default);

		private readonly object FormattersByPolicy_LOCK = new object();

		public AnySerializer(Type serializedType)
		{
			SerializedType = serializedType;
			IsEnum = SerializedType.IsEnum;
			IsValueType = SerializedType.IsValueType;
			MayBeBoxedValueType = SerializedType.IsInterface || SerializedType == typeof(object) || SerializedType == typeof(ValueType) || SerializedType == typeof(Enum);
			IsAbstract = SerializedType.IsAbstract || SerializedType.IsInterface;
			IsNullable = SerializedType.IsGenericType && SerializedType.GetGenericTypeDefinition() == typeof(Nullable<>);
			AllowDeserializeInvalidData = SerializedType.IsDefined(typeof(AllowDeserializeInvalidDataAttribute), inherit: true);
		}

		public override object ReadValueWeak(IDataReader reader)
		{
			if (IsEnum)
			{
				string name;
				EntryType entry = reader.PeekEntry(out name);
				if (entry == EntryType.Integer)
				{
					if (!reader.ReadUInt64(out var value))
					{
						reader.Context.Config.DebugContext.LogWarning("Failed to read entry '" + name + "' of type " + entry);
					}
					return Enum.ToObject(SerializedType, value);
				}
				reader.Context.Config.DebugContext.LogWarning("Expected entry of type " + EntryType.Integer.ToString() + ", but got entry '" + name + "' of type " + entry);
				reader.SkipEntry();
				return Activator.CreateInstance(SerializedType);
			}
			DeserializationContext context = reader.Context;
			if (!context.Config.SerializationPolicy.AllowNonSerializableTypes && !SerializedType.IsSerializable)
			{
				context.Config.DebugContext.LogError("The type " + SerializedType.Name + " is not marked as serializable.");
				if (!IsValueType)
				{
					return null;
				}
				return Activator.CreateInstance(SerializedType);
			}
			bool exitNode = true;
			string name2;
			EntryType entry2 = reader.PeekEntry(out name2);
			if (IsValueType)
			{
				switch (entry2)
				{
				case EntryType.Null:
					context.Config.DebugContext.LogWarning("Expecting complex struct of type " + SerializedType.GetNiceFullName() + " but got null value.");
					reader.ReadNull();
					return Activator.CreateInstance(SerializedType);
				default:
					context.Config.DebugContext.LogWarning("Unexpected entry '" + name2 + "' of type " + entry2.ToString() + ", when " + EntryType.StartOfNode.ToString() + " was expected. A value has likely been lost.");
					reader.SkipEntry();
					return Activator.CreateInstance(SerializedType);
				case EntryType.StartOfNode:
					try
					{
						Type expectedType = SerializedType;
						if (reader.EnterNode(out var serializedType))
						{
							if (serializedType != expectedType)
							{
								if (serializedType != null)
								{
									context.Config.DebugContext.LogWarning("Expected complex struct value " + expectedType.Name + " but the serialized value is of type " + serializedType.Name + ".");
									if (serializedType.IsCastableTo(expectedType))
									{
										object value2 = FormatterLocator.GetFormatter(serializedType, context.Config.SerializationPolicy).Deserialize(reader);
										bool serializedTypeIsNullable = serializedType.IsGenericType && serializedType.GetGenericTypeDefinition() == typeof(Nullable<>);
										Func<object, object> castMethod = ((!IsNullable && !serializedTypeIsNullable) ? serializedType.GetCastMethodDelegate(expectedType) : null);
										if (castMethod != null)
										{
											return castMethod(value2);
										}
										return value2;
									}
									if (AllowDeserializeInvalidData || reader.Context.Config.AllowDeserializeInvalidData)
									{
										context.Config.DebugContext.LogWarning("Can't cast serialized type " + serializedType.GetNiceFullName() + " into expected type " + expectedType.GetNiceFullName() + ". Attempting to deserialize with possibly invalid data. Value may be lost or corrupted for node '" + name2 + "'.");
										return GetBaseFormatter(context.Config.SerializationPolicy).Deserialize(reader);
									}
									context.Config.DebugContext.LogWarning("Can't cast serialized type " + serializedType.GetNiceFullName() + " into expected type " + expectedType.GetNiceFullName() + ". Value lost for node '" + name2 + "'.");
									return Activator.CreateInstance(SerializedType);
								}
								if (AllowDeserializeInvalidData || reader.Context.Config.AllowDeserializeInvalidData)
								{
									context.Config.DebugContext.LogWarning("Expected complex struct value " + expectedType.GetNiceFullName() + " but the serialized type could not be resolved. Attempting to deserialize with possibly invalid data. Value may be lost or corrupted for node '" + name2 + "'.");
									return GetBaseFormatter(context.Config.SerializationPolicy).Deserialize(reader);
								}
								context.Config.DebugContext.LogWarning("Expected complex struct value " + expectedType.Name + " but the serialized type could not be resolved. Value lost for node '" + name2 + "'.");
								return Activator.CreateInstance(SerializedType);
							}
							return GetBaseFormatter(context.Config.SerializationPolicy).Deserialize(reader);
						}
						context.Config.DebugContext.LogError("Failed to enter node '" + name2 + "'.");
						return Activator.CreateInstance(SerializedType);
					}
					catch (SerializationAbortException ex)
					{
						exitNode = false;
						throw ex;
					}
					catch (Exception exception)
					{
						context.Config.DebugContext.LogException(exception);
						return Activator.CreateInstance(SerializedType);
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
			switch (entry2)
			{
			case EntryType.Null:
				reader.ReadNull();
				return null;
			case EntryType.ExternalReferenceByIndex:
			{
				reader.ReadExternalReference(out int index);
				object value9 = context.GetExternalObject(index);
				if (value9 != null && !SerializedType.IsAssignableFrom(value9.GetType()))
				{
					context.Config.DebugContext.LogWarning("Can't cast external reference type " + value9.GetType().GetNiceFullName() + " into expected type " + SerializedType.GetNiceFullName() + ". Value lost for node '" + name2 + "'.");
					return null;
				}
				return value9;
			}
			case EntryType.ExternalReferenceByGuid:
			{
				reader.ReadExternalReference(out Guid guid);
				object value7 = context.GetExternalObject(guid);
				if (value7 != null && !SerializedType.IsAssignableFrom(value7.GetType()))
				{
					context.Config.DebugContext.LogWarning("Can't cast external reference type " + value7.GetType().GetNiceFullName() + " into expected type " + SerializedType.GetNiceFullName() + ". Value lost for node '" + name2 + "'.");
					return null;
				}
				return value7;
			}
			case EntryType.ExternalReferenceByString:
			{
				reader.ReadExternalReference(out string id2);
				object value11 = context.GetExternalObject(id2);
				if (value11 != null && !SerializedType.IsAssignableFrom(value11.GetType()))
				{
					context.Config.DebugContext.LogWarning("Can't cast external reference type " + value11.GetType().GetNiceFullName() + " into expected type " + SerializedType.GetNiceFullName() + ". Value lost for node '" + name2 + "'.");
					return null;
				}
				return value11;
			}
			case EntryType.InternalReference:
			{
				reader.ReadInternalReference(out var id);
				object value4 = context.GetInternalReference(id);
				if (value4 != null && !SerializedType.IsAssignableFrom(value4.GetType()))
				{
					context.Config.DebugContext.LogWarning("Can't cast internal reference type " + value4.GetType().GetNiceFullName() + " into expected type " + SerializedType.GetNiceFullName() + ". Value lost for node '" + name2 + "'.");
					return null;
				}
				return value4;
			}
			case EntryType.StartOfNode:
				try
				{
					Type expectedType2 = SerializedType;
					if (reader.EnterNode(out var serializedType2))
					{
						int id3 = reader.CurrentNodeId;
						object result;
						if (!(serializedType2 != null) || !(expectedType2 != serializedType2))
						{
							result = ((!IsAbstract) ? GetBaseFormatter(context.Config.SerializationPolicy).Deserialize(reader) : null);
						}
						else
						{
							bool success = false;
							bool isPrimitive = FormatterUtilities.IsPrimitiveType(serializedType2);
							bool assignableCast;
							if (MayBeBoxedValueType && isPrimitive)
							{
								Serializer serializer = Serializer.Get(serializedType2);
								result = serializer.ReadValueWeak(reader);
								success = true;
							}
							else if ((assignableCast = expectedType2.IsAssignableFrom(serializedType2)) || serializedType2.HasCastDefined(expectedType2, requireImplicitCast: false))
							{
								try
								{
									object value12;
									if (isPrimitive)
									{
										Serializer serializer2 = Serializer.Get(serializedType2);
										value12 = serializer2.ReadValueWeak(reader);
									}
									else
									{
										IFormatter alternateFormatter = FormatterLocator.GetFormatter(serializedType2, context.Config.SerializationPolicy);
										value12 = alternateFormatter.Deserialize(reader);
									}
									if (assignableCast)
									{
										result = value12;
									}
									else
									{
										Func<object, object> castMethod2 = serializedType2.GetCastMethodDelegate(expectedType2);
										result = ((castMethod2 == null) ? value12 : castMethod2(value12));
									}
									success = true;
								}
								catch (SerializationAbortException ex2)
								{
									exitNode = false;
									throw ex2;
								}
								catch (InvalidCastException)
								{
									success = false;
									result = null;
								}
							}
							else if (!IsAbstract && (AllowDeserializeInvalidData || reader.Context.Config.AllowDeserializeInvalidData))
							{
								context.Config.DebugContext.LogWarning("Can't cast serialized type " + serializedType2.GetNiceFullName() + " into expected type " + expectedType2.GetNiceFullName() + ". Attempting to deserialize with invalid data. Value may be lost or corrupted for node '" + name2 + "'.");
								result = GetBaseFormatter(context.Config.SerializationPolicy).Deserialize(reader);
								success = true;
							}
							else
							{
								IFormatter alternateFormatter2 = FormatterLocator.GetFormatter(serializedType2, context.Config.SerializationPolicy);
								object value13 = alternateFormatter2.Deserialize(reader);
								if (id3 >= 0)
								{
									context.RegisterInternalReference(id3, value13);
								}
								result = null;
							}
							if (!success)
							{
								context.Config.DebugContext.LogWarning("Can't cast serialized type " + serializedType2.GetNiceFullName() + " into expected type " + expectedType2.GetNiceFullName() + ". Value lost for node '" + name2 + "'.");
								result = null;
							}
						}
						if (id3 >= 0)
						{
							context.RegisterInternalReference(id3, result);
						}
						return result;
					}
					context.Config.DebugContext.LogError("Failed to enter node '" + name2 + "'.");
					return null;
				}
				catch (SerializationAbortException ex4)
				{
					exitNode = false;
					throw ex4;
				}
				catch (Exception exception2)
				{
					context.Config.DebugContext.LogException(exception2);
					return null;
				}
				finally
				{
					if (exitNode)
					{
						reader.ExitNode();
					}
				}
			case EntryType.Boolean:
				if (MayBeBoxedValueType)
				{
					reader.ReadBoolean(out var value10);
					return value10;
				}
				break;
			case EntryType.FloatingPoint:
				if (MayBeBoxedValueType)
				{
					reader.ReadDouble(out var value8);
					return value8;
				}
				break;
			case EntryType.Integer:
				if (MayBeBoxedValueType)
				{
					reader.ReadInt64(out var value6);
					return value6;
				}
				break;
			case EntryType.String:
				if (MayBeBoxedValueType)
				{
					reader.ReadString(out var value5);
					return value5;
				}
				break;
			case EntryType.Guid:
				if (MayBeBoxedValueType)
				{
					reader.ReadGuid(out var value3);
					return value3;
				}
				break;
			}
			context.Config.DebugContext.LogWarning("Unexpected entry of type " + entry2.ToString() + ", when a reference or node start was expected. A value has been lost.");
			reader.SkipEntry();
			return null;
		}

		public override void WriteValueWeak(string name, object value, IDataWriter writer)
		{
			if (IsEnum)
			{
				Serializer.FireOnSerializedType(SerializedType);
				ulong ul;
				try
				{
					ul = Convert.ToUInt64(value as Enum);
				}
				catch (OverflowException)
				{
					ul = (ulong)Convert.ToInt64(value as Enum);
				}
				writer.WriteUInt64(name, ul);
				return;
			}
			SerializationContext context = writer.Context;
			ISerializationPolicy policy = context.Config.SerializationPolicy;
			if (!policy.AllowNonSerializableTypes && !SerializedType.IsSerializable)
			{
				context.Config.DebugContext.LogError("The type " + SerializedType.Name + " is not marked as serializable.");
				return;
			}
			Serializer.FireOnSerializedType(SerializedType);
			if (IsValueType)
			{
				bool endNode = true;
				try
				{
					writer.BeginStructNode(name, SerializedType);
					GetBaseFormatter(policy).Serialize(value, writer);
					return;
				}
				catch (SerializationAbortException ex2)
				{
					endNode = false;
					throw ex2;
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
			if (context.TryRegisterExternalReference(value, out int index))
			{
				writer.WriteExternalReference(name, index);
				return;
			}
			if (context.TryRegisterExternalReference(value, out Guid guid))
			{
				writer.WriteExternalReference(name, guid);
				return;
			}
			if (context.TryRegisterExternalReference(value, out string strId))
			{
				writer.WriteExternalReference(name, strId);
				return;
			}
			if (context.TryRegisterInternalReference(value, out var id))
			{
				Type type = value.GetType();
				if (MayBeBoxedValueType && FormatterUtilities.IsPrimitiveType(type))
				{
					try
					{
						writer.BeginReferenceNode(name, type, id);
						Serializer serializer = Serializer.Get(type);
						serializer.WriteValueWeak(value, writer);
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
				IFormatter formatter = (((object)type != SerializedType) ? FormatterLocator.GetFormatter(type, policy) : GetBaseFormatter(policy));
				try
				{
					writer.BeginReferenceNode(name, type, id);
					formatter.Serialize(value, writer);
					return;
				}
				catch (SerializationAbortException ex4)
				{
					endNode2 = false;
					throw ex4;
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

		private IFormatter GetBaseFormatter(ISerializationPolicy serializationPolicy)
		{
			if (serializationPolicy == UnityPolicy)
			{
				if (UnityPolicyFormatter == null)
				{
					UnityPolicyFormatter = FormatterLocator.GetFormatter(SerializedType, UnityPolicy);
				}
				return UnityPolicyFormatter;
			}
			if (serializationPolicy == EverythingPolicy)
			{
				if (EverythingPolicyFormatter == null)
				{
					EverythingPolicyFormatter = FormatterLocator.GetFormatter(SerializedType, EverythingPolicy);
				}
				return EverythingPolicyFormatter;
			}
			if (serializationPolicy == StrictPolicy)
			{
				if (StrictPolicyFormatter == null)
				{
					StrictPolicyFormatter = FormatterLocator.GetFormatter(SerializedType, StrictPolicy);
				}
				return StrictPolicyFormatter;
			}
			IFormatter formatter;
			lock (FormattersByPolicy_LOCK)
			{
				if (!FormattersByPolicy.TryGetValue(serializationPolicy, out formatter))
				{
					formatter = FormatterLocator.GetFormatter(SerializedType, serializationPolicy);
					FormattersByPolicy.Add(serializationPolicy, formatter);
				}
			}
			return formatter;
		}
	}
}
