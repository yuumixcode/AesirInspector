using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.Serialization.Utilities;

namespace Sirenix.Serialization
{
	/// <summary>
	/// Final fallback formatter for all types which have no other formatters. This formatter relies on reflection to work, and is thus comparatively slow and creates more garbage than a custom formatter.
	/// </summary>
	/// <typeparam name="T">The type which can be serialized and deserialized by the formatter.</typeparam>
	/// <seealso cref="T:Sirenix.Serialization.BaseFormatter`1" />
	public class ReflectionFormatter<T> : BaseFormatter<T>
	{
		public ISerializationPolicy OverridePolicy { get; private set; }

		public ReflectionFormatter()
		{
		}

		public ReflectionFormatter(ISerializationPolicy overridePolicy)
		{
			OverridePolicy = overridePolicy;
		}

		/// <summary>
		/// Provides the actual implementation for deserializing a value of type <see cref="!:T" />.
		/// </summary>
		/// <param name="value">The uninitialized value to serialize into. This value will have been created earlier using <see cref="M:Sirenix.Serialization.BaseFormatter`1.GetUninitializedObject" />.</param>
		/// <param name="reader">The reader to deserialize with.</param>
		protected override void DeserializeImplementation(ref T value, IDataReader reader)
		{
			object boxedValue = value;
			Dictionary<string, MemberInfo> members = FormatterUtilities.GetSerializableMembersMap(typeof(T), OverridePolicy ?? reader.Context.Config.SerializationPolicy);
			EntryType entryType;
			string name;
			while ((entryType = reader.PeekEntry(out name)) != EntryType.EndOfNode && entryType != EntryType.EndOfArray && entryType != EntryType.EndOfStream)
			{
				MemberInfo member;
				if (string.IsNullOrEmpty(name))
				{
					reader.Context.Config.DebugContext.LogError("Entry of type \"" + entryType.ToString() + "\" in node \"" + reader.CurrentNodeName + "\" is missing a name.");
					reader.SkipEntry();
				}
				else if (!members.TryGetValue(name, out member))
				{
					reader.Context.Config.DebugContext.LogWarning("Lost serialization data for entry \"" + name + "\" of type \"" + entryType.ToString() + "\" in node \"" + reader.CurrentNodeName + "\" because a serialized member of that name could not be found in type " + typeof(T).GetNiceFullName() + ".");
					reader.SkipEntry();
				}
				else
				{
					Type expectedType = FormatterUtilities.GetContainedType(member);
					try
					{
						Serializer serializer = Serializer.Get(expectedType);
						object entryValue = serializer.ReadValueWeak(reader);
						FormatterUtilities.SetMemberValue(member, boxedValue, entryValue);
					}
					catch (Exception exception)
					{
						reader.Context.Config.DebugContext.LogException(exception);
					}
				}
			}
			value = (T)boxedValue;
		}

		/// <summary>
		/// Provides the actual implementation for serializing a value of type <see cref="!:T" />.
		/// </summary>
		/// <param name="value">The value to serialize.</param>
		/// <param name="writer">The writer to serialize with.</param>
		protected override void SerializeImplementation(ref T value, IDataWriter writer)
		{
			MemberInfo[] members = FormatterUtilities.GetSerializableMembers(typeof(T), OverridePolicy ?? writer.Context.Config.SerializationPolicy);
			foreach (MemberInfo member in members)
			{
				object memberValue = FormatterUtilities.GetMemberValue(member, value);
				Type type = FormatterUtilities.GetContainedType(member);
				Serializer serializer = Serializer.Get(type);
				try
				{
					serializer.WriteValueWeak(member.Name, memberValue, writer);
				}
				catch (Exception exception)
				{
					writer.Context.Config.DebugContext.LogException(exception);
				}
			}
		}
	}
}
