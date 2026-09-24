using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.Serialization.Utilities;

namespace Sirenix.Serialization
{
	public class WeakReflectionFormatter : WeakBaseFormatter
	{
		public WeakReflectionFormatter(Type serializedType)
			: base(serializedType)
		{
		}

		protected override void DeserializeImplementation(ref object value, IDataReader reader)
		{
			Dictionary<string, MemberInfo> members = FormatterUtilities.GetSerializableMembersMap(SerializedType, reader.Context.Config.SerializationPolicy);
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
					reader.Context.Config.DebugContext.LogWarning("Lost serialization data for entry \"" + name + "\" of type \"" + entryType.ToString() + "\" in node \"" + reader.CurrentNodeName + "\" because a serialized member of that name could not be found in type " + SerializedType.GetNiceFullName() + ".");
					reader.SkipEntry();
				}
				else
				{
					Type expectedType = FormatterUtilities.GetContainedType(member);
					try
					{
						Serializer serializer = Serializer.Get(expectedType);
						object entryValue = serializer.ReadValueWeak(reader);
						FormatterUtilities.SetMemberValue(member, value, entryValue);
					}
					catch (Exception exception)
					{
						reader.Context.Config.DebugContext.LogException(exception);
					}
				}
			}
		}

		protected override void SerializeImplementation(ref object value, IDataWriter writer)
		{
			MemberInfo[] members = FormatterUtilities.GetSerializableMembers(SerializedType, writer.Context.Config.SerializationPolicy);
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
