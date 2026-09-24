using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using Sirenix.OdinInspector.Editor.Internal;
using Sirenix.OdinInspector.Internal;
using Sirenix.Serialization;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.Events;

namespace Sirenix.OdinInspector.Editor
{
	internal class MemberSerializationInfo
	{
		public readonly string[] Notes;

		public readonly MemberInfo MemberInfo;

		public readonly SerializationFlags Info;

		public readonly SerializationBackendFlags Backend;

		public readonly InfoMessageType OdinMessageType;

		public readonly InfoMessageType UnityMessageType;

		private MemberSerializationInfo(MemberInfo member, string[] notes, SerializationFlags flags, SerializationBackendFlags serializationBackend)
		{
			MemberInfo = member;
			Notes = notes;
			Info = flags;
			Backend = serializationBackend;
			OdinMessageType = InfoMessageType.None;
			UnityMessageType = InfoMessageType.None;
			if (flags.HasAll(SerializationFlags.DefaultSerializationPolicy) && flags.HasNone(SerializationFlags.SerializedByUnity | SerializationFlags.SerializedByOdin | SerializationFlags.NonSerializedAttribute) && flags.HasAny(SerializationFlags.Public | SerializationFlags.SerializeFieldAttribute) && (flags.HasAll(SerializationFlags.Field) || flags.HasAll(SerializationFlags.AutoProperty)))
			{
				if (serializationBackend.HasNone(SerializationBackendFlags.Odin))
				{
					OdinMessageType = InfoMessageType.Info;
				}
				if (flags.HasNone(SerializationFlags.Property) && !UnitySerializationUtility.GuessIfUnityWillSerialize(member.GetReturnType()))
				{
					UnityMessageType = InfoMessageType.Info;
				}
			}
			if (Info.HasAny(SerializationFlags.SerializedByOdin) && Info.HasAny(SerializationFlags.SerializedByUnity))
			{
				OdinMessageType = InfoMessageType.Warning;
				UnityMessageType = InfoMessageType.Warning;
			}
			if (Info.HasAll(SerializationFlags.OdinSerializeAttribute | SerializationFlags.NonSerializedAttribute | SerializationFlags.SerializeReferenceAttribute))
			{
				OdinMessageType = InfoMessageType.Warning;
				UnityMessageType = InfoMessageType.Warning;
			}
			if (Info.HasAll(SerializationFlags.SerializeFieldAttribute | SerializationFlags.NonSerializedAttribute))
			{
				UnityMessageType = InfoMessageType.Warning;
			}
			if (Info.HasAll(SerializationFlags.SerializeFieldAttribute | SerializationFlags.OdinSerializeAttribute))
			{
				if (Info.HasAll(SerializationFlags.SerializedByOdin))
				{
					OdinMessageType = InfoMessageType.Warning;
				}
				if (Info.HasAll(SerializationFlags.SerializedByUnity))
				{
					UnityMessageType = InfoMessageType.Warning;
				}
			}
			if (serializationBackend.HasAll(SerializationBackendFlags.UnityAndOdin) && Info.HasAll(SerializationFlags.SerializedByOdin | SerializationFlags.TypeSupportedByUnity) && Info.HasNone(SerializationFlags.Property | SerializationFlags.SerializedByUnity))
			{
				OdinMessageType = InfoMessageType.Warning;
			}
			if (!serializationBackend.HasAll(SerializationBackendFlags.Odin) && flags.HasAny(SerializationFlags.OdinSerializeAttribute))
			{
				OdinMessageType = InfoMessageType.Error;
			}
			if (Info.HasAll(SerializationFlags.DefaultSerializationPolicy) && Info.HasAny(SerializationFlags.OdinSerializeAttribute) && !Info.HasAny(SerializationFlags.SerializedByOdin))
			{
				OdinMessageType = InfoMessageType.Error;
			}
			if (Info.HasAny(SerializationFlags.SerializeFieldAttribute | SerializationFlags.OdinSerializeAttribute) && !Info.HasAny(SerializationFlags.SerializedByUnity | SerializationFlags.SerializedByOdin | SerializationFlags.NonSerializedAttribute))
			{
				if (serializationBackend.HasAll(SerializationBackendFlags.Odin))
				{
					OdinMessageType = InfoMessageType.Error;
				}
				if (!Info.HasAny(SerializationFlags.Property) && UnitySerializationUtility.GuessIfUnityWillSerialize(member.GetReturnType()))
				{
					UnityMessageType = InfoMessageType.Error;
				}
			}
			if (Info.HasAll(SerializationFlags.Public | SerializationFlags.Field) && !Info.HasAny(SerializationFlags.NonSerializedAttribute) && !Info.HasAny(SerializationFlags.SerializedByUnity | SerializationFlags.SerializedByOdin))
			{
				if (Info.HasAll(SerializationFlags.DefaultSerializationPolicy) && serializationBackend.HasAll(SerializationBackendFlags.Odin))
				{
					OdinMessageType = InfoMessageType.Error;
				}
				if (!Info.HasAny(SerializationFlags.Property) && UnitySerializationUtility.GuessIfUnityWillSerialize(member.GetReturnType()))
				{
					UnityMessageType = InfoMessageType.Error;
				}
			}
			if (Info.HasAll(SerializationFlags.SerializedByUnity | SerializationFlags.SerializeReferenceAttribute))
			{
				Type memberType = member.GetReturnType();
				string errorMessage;
				switch (SerializeReferenceUtility.ValidateBaseType(memberType, out errorMessage))
				{
				case SerializeReferenceValidityResult.Error:
					UnityMessageType = InfoMessageType.Error;
					break;
				case SerializeReferenceValidityResult.Warning:
					UnityMessageType = InfoMessageType.Warning;
					break;
				}
			}
		}

		public static List<MemberSerializationInfo> CreateSerializationOverview(Type type, SerializationBackendFlags serializationBackend, bool includeBaseTypes)
		{
			bool serializeUnityFields;
			ISerializationPolicy serializationPolicy = GetSerializationPolicy(type, out serializeUnityFields);
			return (from x in type.GetAllMembers(includeBaseTypes ? (BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) : (BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
				where x is FieldInfo || x is PropertyInfo
				where !x.Name.StartsWith("<")
				where (AssemblyUtilities.GetAssemblyCategory(x.DeclaringType.Assembly) & AssemblyCategory.UnityEngine) == 0
				where !x.DeclaringType.Assembly.FullName.StartsWith("Sirenix.")
				select CreateInfoFor(x, serializationBackend, serializeUnityFields, serializationPolicy) into x
				orderby x.OdinMessageType == InfoMessageType.Error descending, x.UnityMessageType == InfoMessageType.Error descending, x.OdinMessageType == InfoMessageType.Warning descending, x.UnityMessageType == InfoMessageType.Warning descending, x.OdinMessageType == InfoMessageType.Info descending, x.UnityMessageType == InfoMessageType.Info descending, x.Info.HasAny(SerializationFlags.SerializedByOdin) descending, x.Info.HasAny(SerializationFlags.SerializedByUnity) descending, x.MemberInfo.Name descending
				select x).ToList();
		}

		private static ISerializationPolicy GetSerializationPolicy(Type type, out bool serializeUnityFields)
		{
			serializeUnityFields = false;
			if (!typeof(IOverridesSerializationPolicy).IsAssignableFrom(type))
			{
				return SerializationPolicies.Unity;
			}
			IOverridesSerializationPolicy policyOverride = null;
			UnityEngine.Object[] objects = Resources.FindObjectsOfTypeAll(type);
			foreach (UnityEngine.Object obj in objects)
			{
				if ((object)obj != null)
				{
					policyOverride = obj as IOverridesSerializationPolicy;
					break;
				}
			}
			if (!type.IsAbstract && !type.IsGenericTypeDefinition)
			{
				object inst = FormatterServices.GetUninitializedObject(type);
				policyOverride = inst as IOverridesSerializationPolicy;
			}
			if (policyOverride != null)
			{
				serializeUnityFields = policyOverride.OdinSerializesUnityFields;
				return policyOverride.SerializationPolicy ?? SerializationPolicies.Unity;
			}
			return SerializationPolicies.Unity;
		}

		private static MemberSerializationInfo CreateInfoFor(MemberInfo member, SerializationBackendFlags serializationBackend, bool serializeUnityFields, ISerializationPolicy serializationPolicy)
		{
			SerializationFlags flags = (SerializationFlags)0;
			if (member is FieldInfo)
			{
				FieldInfo f = member as FieldInfo;
				flags |= SerializationFlags.Field;
				if (f.IsPublic)
				{
					flags |= SerializationFlags.Public;
				}
			}
			else if (member is PropertyInfo)
			{
				PropertyInfo p = member as PropertyInfo;
				flags |= SerializationFlags.Property;
				if ((p.GetGetMethod() != null && p.GetGetMethod().IsPublic) || (p.GetSetMethod() != null && p.GetSetMethod().IsPublic))
				{
					flags |= SerializationFlags.Public;
				}
				if (p.IsAutoProperty(allowVirtual: true))
				{
					flags |= SerializationFlags.AutoProperty;
				}
			}
			if (serializationPolicy != null && serializationPolicy.ID == SerializationPolicies.Unity.ID)
			{
				flags |= SerializationFlags.DefaultSerializationPolicy;
			}
			if ((serializationBackend & SerializationBackendFlags.Unity) != SerializationBackendFlags.None && UnitySerializationUtility.GuessIfUnityWillSerialize(member))
			{
				flags |= SerializationFlags.SerializedByUnity;
			}
			if ((serializationBackend & SerializationBackendFlags.Odin) != SerializationBackendFlags.None && UnitySerializationUtility.OdinWillSerialize(member, serializeUnityFields, serializationPolicy))
			{
				flags |= SerializationFlags.SerializedByOdin;
			}
			if (member.IsDefined<SerializeField>())
			{
				flags |= SerializationFlags.SerializeFieldAttribute;
			}
			if (member.IsDefined<SerializeReference>())
			{
				flags |= SerializationFlags.SerializeReferenceAttribute;
			}
			if (member.IsDefined<OdinSerializeAttribute>())
			{
				flags |= SerializationFlags.OdinSerializeAttribute;
			}
			if (member.IsDefined<NonSerializedAttribute>())
			{
				flags |= SerializationFlags.NonSerializedAttribute;
			}
			if (serializationBackend.HasAll(SerializationBackendFlags.Unity) && UnitySerializationUtility.GuessIfUnityWillSerialize(member.GetReturnType()))
			{
				flags |= SerializationFlags.TypeSupportedByUnity;
			}
			return new MemberSerializationInfo(member, CreateNotes(member, flags, serializationBackend, serializationPolicy.ID), flags, serializationBackend);
		}

		private static string[] CreateNotes(MemberInfo member, SerializationFlags flags, SerializationBackendFlags serializationBackend, string serializationPolicyId)
		{
			List<string> notes = new List<string>();
			StringBuilder buffer = new StringBuilder();
			if (serializationBackend.HasNone(SerializationBackendFlags.Odin) || flags.HasAll(SerializationFlags.DefaultSerializationPolicy))
			{
				if (flags.HasAll(SerializationFlags.Property | SerializationFlags.AutoProperty))
				{
					buffer.AppendFormat("The auto property '{0}' ", member.GetNiceName());
				}
				else if (flags.HasAll(SerializationFlags.Property))
				{
					buffer.AppendFormat("The non-auto property '{0}' ", member.GetNiceName());
				}
				else if (flags.HasAll(SerializationFlags.Public))
				{
					buffer.AppendFormat("The public field '{0}' ", member.GetNiceName());
				}
				else
				{
					buffer.AppendFormat("The field '{0}' ", member.GetNiceName());
				}
				if (flags.HasAny(SerializationFlags.SerializedByUnity | SerializationFlags.SerializedByOdin))
				{
					buffer.Append("is serialized by ");
					if (flags.HasAll(SerializationFlags.SerializedByUnity | SerializationFlags.SerializedByOdin))
					{
						buffer.Append("both Unity and Odin ");
					}
					else if (flags.HasAll(SerializationFlags.SerializedByUnity))
					{
						buffer.Append("Unity ");
					}
					else
					{
						buffer.Append("Odin ");
					}
					buffer.Append("because ");
					SerializationFlags relevant = flags & (SerializationFlags.Public | SerializationFlags.SerializeFieldAttribute | SerializationFlags.NonSerializedAttribute | SerializationFlags.SerializeReferenceAttribute);
					if (flags.HasAll(SerializationFlags.OdinSerializeAttribute) && serializationBackend.HasAll(SerializationBackendFlags.Odin))
					{
						relevant |= SerializationFlags.OdinSerializeAttribute;
					}
					if (flags.HasAll(SerializationFlags.Public | SerializationFlags.Property))
					{
						relevant &= ~SerializationFlags.Public;
					}
					switch (relevant)
					{
					case SerializationFlags.Public:
						buffer.Append("it's access modifier is public. ");
						break;
					case SerializationFlags.SerializeFieldAttribute:
						buffer.Append("the [SerializeField] attribute is defined. ");
						break;
					case SerializationFlags.Public | SerializationFlags.SerializeFieldAttribute:
						buffer.Append("the [SerializeField] attribute is defined, and it's public. ");
						break;
					case SerializationFlags.OdinSerializeAttribute:
						buffer.Append("the [OdinSerialize] attribute is defined. ");
						break;
					case SerializationFlags.Public | SerializationFlags.OdinSerializeAttribute:
						buffer.Append("the [OdinSerialize] attribute is defined, and it's public.");
						break;
					case SerializationFlags.SerializeFieldAttribute | SerializationFlags.OdinSerializeAttribute:
						buffer.Append("the [SerializeField] and [OdinSerialize] attributes are defined. ");
						break;
					case SerializationFlags.Public | SerializationFlags.SerializeFieldAttribute | SerializationFlags.OdinSerializeAttribute:
						buffer.Append("it's access modifier is public and the [SerializeField] and [OdinSerialize] attributes are defined. ");
						break;
					case SerializationFlags.SerializeReferenceAttribute:
						buffer.Append("the [SerializeReference] attribute is defined.");
						break;
					case SerializationFlags.Public | SerializationFlags.SerializeReferenceAttribute:
						buffer.Append("the [SerializeReference] attribute is defined");
						if ((flags & SerializationFlags.TypeSupportedByUnity) == SerializationFlags.TypeSupportedByUnity)
						{
							buffer.Append(", and it's public");
						}
						buffer.Append('.');
						break;
					case SerializationFlags.OdinSerializeAttribute | SerializationFlags.SerializeReferenceAttribute:
						buffer.Append("the [SerializeReference] and [OdinSerialize] attributes are defined.");
						break;
					case SerializationFlags.Public | SerializationFlags.OdinSerializeAttribute | SerializationFlags.SerializeReferenceAttribute:
						buffer.Append("the [SerializeReference] and [OdinSerialize] attributes are defined");
						if (flags.HasFlag(SerializationFlags.TypeSupportedByUnity))
						{
							buffer.Append(", and the access modifier is public");
						}
						buffer.Append('.');
						break;
					case SerializationFlags.OdinSerializeAttribute | SerializationFlags.NonSerializedAttribute:
					case SerializationFlags.Public | SerializationFlags.OdinSerializeAttribute | SerializationFlags.NonSerializedAttribute:
					case SerializationFlags.SerializeFieldAttribute | SerializationFlags.OdinSerializeAttribute | SerializationFlags.NonSerializedAttribute:
					case SerializationFlags.Public | SerializationFlags.SerializeFieldAttribute | SerializationFlags.OdinSerializeAttribute | SerializationFlags.NonSerializedAttribute:
					case SerializationFlags.OdinSerializeAttribute | SerializationFlags.NonSerializedAttribute | SerializationFlags.SerializeReferenceAttribute:
					case SerializationFlags.Public | SerializationFlags.OdinSerializeAttribute | SerializationFlags.NonSerializedAttribute | SerializationFlags.SerializeReferenceAttribute:
						buffer.Append("the [OdinSerialize] and [NonSerialized] attributes are defined. ");
						break;
					default:
						buffer.Append("(MISSING CASE: " + relevant.ToString() + ")");
						break;
					}
					if (buffer.Length > 0)
					{
						notes.Add(buffer.ToString());
						buffer.Length = 0;
					}
					if (serializationBackend.HasAll(SerializationBackendFlags.Unity) && flags.HasNone(SerializationFlags.SerializedByUnity))
					{
						buffer.Append("The member is not being serialized by Unity since ");
						if (flags.HasAll(SerializationFlags.Property))
						{
							buffer.Append("Unity does not serialize properties.");
						}
						else if ((flags & SerializationFlags.SerializeReferenceAttribute) != SerializationFlags.SerializeReferenceAttribute && !UnitySerializationUtility.GuessIfUnityWillSerialize(member.GetReturnType()))
						{
							buffer.Append("Unity does not support the type.");
						}
						else if (flags.HasAll(SerializationFlags.NonSerializedAttribute))
						{
							buffer.Append("the [NonSerialized] attribute is defined.");
						}
						else if (!flags.HasAny(SerializationFlags.Public | SerializationFlags.SerializeFieldAttribute | SerializationFlags.SerializeReferenceAttribute))
						{
							buffer.Append("it is neither a public field nor does it have the [SerializeReference] or [SerializeField] attributes defined.");
						}
						else if (UnityVersion.IsVersionOrGreater(6000, 6) && flags.HasNone(SerializationFlags.SerializeFieldAttribute) && member.GetReturnType().IsGenericType && !member.GetReturnType().IsGenericTypeDefinition && member.GetReturnType().GetGenericTypeDefinition() == typeof(Dictionary<, >))
						{
							buffer.Append("it is a dictionary, and Unity only serializes dictionary fields that have the [SerializeField] attribute applied, even when the field is public.");
						}
						else
						{
							buffer.Append("# Missing case, please report: " + flags);
						}
					}
					if (buffer.Length > 0)
					{
						notes.Add(buffer.ToString());
						buffer.Length = 0;
					}
					if (!flags.HasAll(SerializationFlags.SerializedByOdin))
					{
						buffer.Append("Member is not serialized by Odin because ");
						if ((serializationBackend & SerializationBackendFlags.Odin) != SerializationBackendFlags.None)
						{
							if (flags.HasAll(SerializationFlags.SerializedByUnity))
							{
								buffer.Append("the member is already serialized by Unity. ");
							}
						}
						else
						{
							buffer.Append("Odin serialization is not implemented. ");
							if (flags.HasAll(SerializationFlags.OdinSerializeAttribute))
							{
								buffer.Append("The use of [OdinSerialize] attribute is invalid.");
							}
						}
					}
				}
				else
				{
					if (flags.HasAll(SerializationFlags.Property) && serializationBackend.HasAll(SerializationBackendFlags.Odin))
					{
						buffer.Append("is skipped by Odin because ");
						PropertyInfo prop = member as PropertyInfo;
						if (prop.GetGetMethod(nonPublic: true) == null)
						{
							buffer.Append("the property has no getter. ");
						}
						else if (prop.GetSetMethod(nonPublic: true) == null)
						{
							buffer.Append("the property has no setter. ");
						}
						else if (flags.HasNone(SerializationFlags.OdinSerializeAttribute))
						{
							buffer.Append("the [OdinSerialize] attribute has not been applied to it. ");
						}
						else
						{
							buffer.Append("MISSING CASE (please report). ");
						}
						if (flags.HasAll(SerializationFlags.NonSerializedAttribute))
						{
							buffer.Append("( Note: the [NonSerialized] attribute is unnecessary. ) ");
						}
					}
					else if (flags.HasAll(SerializationFlags.Property))
					{
						buffer.Append("is skipped by Unity because Unity does not serialize properties. ");
						if (flags.HasAll(SerializationFlags.SerializeFieldAttribute | SerializationFlags.OdinSerializeAttribute))
						{
							buffer.Append("The use of [SerializeField] and [OdinSerialize] attributes is invalid. ");
						}
						else if (flags.HasAll(SerializationFlags.OdinSerializeAttribute))
						{
							buffer.Append("The use of [OdinSerialize] attribute is invalid. ");
						}
						else if (flags.HasAny(SerializationFlags.SerializeFieldAttribute))
						{
							buffer.Append("The use of [SerializeField] attribute is invalid. ");
						}
						if (flags.HasAny(SerializationFlags.NonSerializedAttribute))
						{
							buffer.Append("The use of [NonSerialized] attribute is unnecessary.");
						}
					}
					else
					{
						buffer.Append("is skipped by ");
						switch (serializationBackend)
						{
						case SerializationBackendFlags.Unity:
							buffer.Append("Unity ");
							break;
						case SerializationBackendFlags.Odin:
							buffer.Append("Odin ");
							break;
						case SerializationBackendFlags.UnityAndOdin:
							buffer.Append("both Unity and Odin ");
							break;
						}
						buffer.Append("because ");
						if (serializationBackend == SerializationBackendFlags.None)
						{
							buffer.Append("there is no serialization backend? ");
						}
						else if (flags.HasAll(SerializationFlags.NonSerializedAttribute))
						{
							buffer.Append("the [NonSerialized] attribute is defined. ");
						}
						else if (serializationBackend.HasAll(SerializationBackendFlags.UnityAndOdin) && flags.HasNone(SerializationFlags.Public | SerializationFlags.SerializeFieldAttribute | SerializationFlags.OdinSerializeAttribute | SerializationFlags.SerializeReferenceAttribute))
						{
							buffer.Append("the field is not public and neither of the [SerializeField], [SerializeReference] or [OdinSerialize] attributes are defined. ");
						}
						else if (flags.HasNone(SerializationFlags.Public | SerializationFlags.SerializeFieldAttribute | SerializationFlags.SerializeReferenceAttribute))
						{
							buffer.Append("the field is neither public nor does it have the [SerializeReference] or [SerializeField] attributes defined.");
						}
						else if (serializationBackend == SerializationBackendFlags.Unity && flags.HasAny(SerializationFlags.Public | SerializationFlags.SerializeFieldAttribute))
						{
							buffer.Append("Unity does not support the type " + member.GetReturnType().GetNiceName());
						}
						if (buffer.Length > 0)
						{
							notes.Add(buffer.ToString());
							buffer.Length = 0;
						}
						if ((serializationBackend & SerializationBackendFlags.Odin) == 0 && flags.HasAll(SerializationFlags.OdinSerializeAttribute))
						{
							notes.Add("Odin serialization is not implemented. The use of [OdinSerialize] attribute is invalid.");
						}
					}
					if (flags.HasAll(SerializationFlags.SerializeFieldAttribute | SerializationFlags.NonSerializedAttribute) && flags.HasNone(SerializationFlags.OdinSerializeAttribute))
					{
						notes.Add("Use of the [SerializeField] attribute along with the [NonSerialized] attribute is weird. Remove either the [SerializeField] or [NonSerialized] attribute.");
					}
					if (flags.HasAll(SerializationFlags.NonSerializedAttribute | SerializationFlags.SerializeReferenceAttribute) && flags.HasNone(SerializationFlags.OdinSerializeAttribute))
					{
						notes.Add("Use of the [SerializeReference] attribute along with the [NonSerialized] attribute is weird. Remove either the [SerializeReference] or [NonSerialized] attribute.");
					}
				}
			}
			else
			{
				if (flags.HasAll(SerializationFlags.AutoProperty))
				{
					buffer.Append(flags.HasAll(SerializationFlags.Public) ? "The public auto property " : "The auto property ");
				}
				else if (flags.HasAll(SerializationFlags.Property))
				{
					buffer.Append(flags.HasAll(SerializationFlags.Public) ? "The public property " : "The property ");
				}
				else
				{
					buffer.Append(flags.HasAll(SerializationFlags.Public) ? "The public field " : "The field ");
				}
				buffer.AppendFormat("'{0}' ", member.GetNiceName());
				if (flags.HasAll(SerializationFlags.SerializedByUnity))
				{
					buffer.Append("is serialized by Unity since ");
					if (flags.HasAll(SerializationFlags.Field))
					{
						switch (flags & (SerializationFlags.Public | SerializationFlags.SerializeFieldAttribute))
						{
						case SerializationFlags.Public:
							buffer.Append("it's access modifier is public. ");
							break;
						case SerializationFlags.SerializeFieldAttribute:
							buffer.Append("the [SerializeField] attribute is defined. ");
							break;
						case SerializationFlags.Public | SerializationFlags.SerializeFieldAttribute:
							buffer.Append("the [SerializeField] attribute is defined, and it's public. ");
							break;
						case SerializationFlags.SerializeReferenceAttribute:
							buffer.Append("the [SerializeReference] attribute is defined.");
							break;
						case SerializationFlags.Public | SerializationFlags.SerializeReferenceAttribute:
							buffer.Append("the [SerializeReference] attribute is defined, and the field is public.");
							break;
						default:
							buffer.Append("(MISSING CASE: " + flags.ToString() + ")");
							break;
						}
					}
					else
					{
						buffer.Append("is serialized by Unity? Unity should not be serializing any properties? ");
					}
				}
				else
				{
					buffer.Append("is skipped by Unity ");
					if (flags.HasAll(SerializationFlags.Field))
					{
						if (flags.HasNone(SerializationFlags.TypeSupportedByUnity | SerializationFlags.SerializeReferenceAttribute))
						{
							buffer.Append("because Unity does not support the type " + member.GetReturnType().GetNiceName());
						}
						else if (flags.HasAll(SerializationFlags.NonSerializedAttribute))
						{
							buffer.Append("because the [NonSerialized] attribute is defined. ");
						}
						else if (flags.HasNone(SerializationFlags.Public | SerializationFlags.SerializeFieldAttribute | SerializationFlags.SerializeReferenceAttribute))
						{
							buffer.Append("because the field is not public and neither of the [SerializeField] or [SerializeReference] attributes defined. ");
						}
						else
						{
							buffer.Append("(MISSING CASE: " + flags.ToString() + ")");
						}
					}
					else
					{
						buffer.Append("because Unity does not serialize properties. ");
					}
				}
				if (buffer.Length > 0)
				{
					notes.Add(buffer.ToString());
					buffer.Length = 0;
				}
				if (flags.HasAll(SerializationFlags.AutoProperty))
				{
					buffer.Append("The auto property ");
				}
				else if (flags.HasAll(SerializationFlags.Property))
				{
					buffer.Append("The property ");
				}
				else
				{
					buffer.Append("The field ");
				}
				if (flags.HasAll(SerializationFlags.SerializedByOdin))
				{
					buffer.Append("is serialized by Odin because of custom serialization policy: " + serializationPolicyId);
				}
				else
				{
					buffer.Append("is skipped by Odin because of custom serialization policy: " + serializationPolicyId);
				}
				if (buffer.Length > 0)
				{
					notes.Add(buffer.ToString());
					buffer.Length = 0;
				}
				if (flags.HasAll(SerializationFlags.SerializedByUnity | SerializationFlags.SerializedByOdin))
				{
					notes.Add("The member is serialized by both Unity and Odin. Consider ensuring that only one serializer is in use.");
				}
			}
			if (buffer.Length > 0)
			{
				notes.Add(buffer.ToString());
				buffer.Length = 0;
			}
			if (serializationBackend.HasAll(SerializationBackendFlags.UnityAndOdin))
			{
				bool isUnitySerializedProperty = flags.HasAll(SerializationFlags.Property | SerializationFlags.SerializedByUnity);
				if (flags.HasAll(SerializationFlags.SerializedByOdin | SerializationFlags.TypeSupportedByUnity | SerializationFlags.SerializeReferenceAttribute) && !isUnitySerializedProperty)
				{
					buffer.Append("The field '" + member.GetNiceName() + "' has the [SerializeReference] attribute defined, and the type '" + member.GetReturnType().GetNiceName() + "' appears to be supported by Unity. Are you certain that you want to use Odin for serializing it?");
				}
				else if (flags.HasAll(SerializationFlags.SerializedByOdin | SerializationFlags.SerializeReferenceAttribute) && !isUnitySerializedProperty)
				{
					buffer.Append("The field '" + member.GetNiceName() + "' has the [SerializeReference] attribute defined. Are you certain that you want to use Odin for serializing it?");
				}
				else if (flags.HasAll(SerializationFlags.SerializedByOdin | SerializationFlags.TypeSupportedByUnity) && !isUnitySerializedProperty)
				{
					buffer.Append("The type '" + member.GetReturnType().GetNiceName() + "' appears to be supported by Unity. Are you certain that you want to use Odin for serializing it?");
				}
				else if (flags.HasAll(SerializationFlags.SerializedByOdin) && flags.HasNone(SerializationFlags.TypeSupportedByUnity))
				{
					buffer.Append("The type '" + member.GetReturnType().GetNiceName() + "' is not supported by Unity" + GuessWhyUnityDoesNotSupport(member.GetReturnType()));
				}
			}
			else if (serializationBackend.HasAll(SerializationBackendFlags.Unity) && flags.HasNone(SerializationFlags.TypeSupportedByUnity))
			{
				buffer.Append("The type '" + member.GetReturnType().GetNiceName() + "' is not supported by Unity" + GuessWhyUnityDoesNotSupport(member.GetReturnType()));
			}
			if (buffer.Length > 0)
			{
				notes.Add(buffer.ToString());
				buffer.Length = 0;
			}
			if (serializationBackend.HasAll(SerializationBackendFlags.Unity) && serializationBackend.HasNone(SerializationBackendFlags.Odin) && flags.HasNone(SerializationFlags.TypeSupportedByUnity) && flags.HasAny(SerializationFlags.Public | SerializationFlags.SerializeFieldAttribute | SerializationFlags.OdinSerializeAttribute) && flags.HasAny(SerializationFlags.Field | SerializationFlags.AutoProperty))
			{
				string inheritFrom = "You could implement Odin serializing by inheriting " + member.DeclaringType.GetNiceName() + " from ";
				if (typeof(MonoBehaviour).IsAssignableFrom(member.DeclaringType))
				{
					buffer.Append(inheritFrom + typeof(SerializedMonoBehaviour).GetNiceName());
				}
				else if (UnityNetworkingUtility.NetworkBehaviourType != null && UnityNetworkingUtility.NetworkBehaviourType.IsAssignableFrom(member.DeclaringType))
				{
					buffer.Append(inheritFrom + " SerializedNetworkBehaviour");
				}
				else if (typeof(Behaviour).IsAssignableFrom(member.DeclaringType))
				{
					buffer.Append(inheritFrom + typeof(SerializedBehaviour).GetNiceName());
				}
				else if (typeof(ScriptableObject).IsAssignableFrom(member.DeclaringType))
				{
					buffer.Append(inheritFrom + typeof(SerializedScriptableObject).GetNiceName());
				}
			}
			if (buffer.Length > 0)
			{
				notes.Add(buffer.ToString());
				buffer.Length = 0;
			}
			if (serializationBackend.HasAll(SerializationBackendFlags.Unity) && flags.HasAll(SerializationFlags.SerializeReferenceAttribute))
			{
				Type memberType = member.GetReturnType();
				if (SerializeReferenceUtility.ValidateBaseType(memberType, out var errorMessage) != SerializeReferenceValidityResult.Valid)
				{
					buffer.Append(errorMessage);
					buffer.Append('\n');
					buffer.Append('\n');
					buffer.Append("Check https://docs.unity3d.com/ScriptReference/SerializeReference.html for more information.");
				}
			}
			if (buffer.Length > 0)
			{
				notes.Add(buffer.ToString());
				buffer.Length = 0;
			}
			if (serializationBackend.HasAll(SerializationBackendFlags.Odin) && flags.HasAll(SerializationFlags.Property | SerializationFlags.SerializedByOdin))
			{
				buffer.Append("It's recommended to use backing fields for serialization instead of properties.");
			}
			if (buffer.Length > 0)
			{
				notes.Add(buffer.ToString());
				buffer.Length = 0;
			}
			return notes.ToArray();
		}

		private static string GuessWhyUnityDoesNotSupport(Type type)
		{
			if (type == typeof(Coroutine))
			{
				return " because Unity will never serialize Coroutines.";
			}
			if (typeof(Delegate).IsAssignableFrom(type))
			{
				return " because Unity does not support delegates.";
			}
			if (type.IsInterface)
			{
				return " because the type is an interface.";
			}
			if (type.IsAbstract)
			{
				return " because the type is abstract.";
			}
			if (type == typeof(object))
			{
				return " because Unity does not support serializing System.Object.";
			}
			if (typeof(Enum).IsAssignableFrom(type))
			{
				Type underlying = Enum.GetUnderlyingType(type);
				if (UnityVersion.IsVersionOrGreater(5, 6) && (underlying == typeof(long) || underlying == typeof(ulong)))
				{
					return " because Unity does not support enums with underlying type of long or ulong.";
				}
				if (UnityVersion.Major <= 5 && UnityVersion.Minor < 6 && underlying != typeof(int) && underlying != typeof(byte))
				{
					return " because prior to Version 5.6 Unity only supports enums with underlying type of int or byte.";
				}
				return ". Was unable to determine why Unity does not support enum with underlying type of: " + underlying.GetNiceName() + ".";
			}
			if (typeof(UnityEventBase).IsAssignableFrom(type) && type.IsGenericType)
			{
				return " because the type is a generic implementation of UnityEventBase.";
			}
			if (type.IsArray)
			{
				if (type.GetArrayRank() > 1 || type.GetElementType().IsArray || type.GetElementType().ImplementsOpenGenericClass(typeof(List<>)))
				{
					return " because Unity does not support multi-dimensional arrays.";
				}
				if (!UnitySerializationUtility.GuessIfUnityWillSerialize(type.GetElementType()))
				{
					return " because Unity does not support the type " + type.GetElementType().GetNiceName() + " as an array element.";
				}
			}
			if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
			{
				Type elementType = type.GetArgumentsOfInheritedOpenGenericClass(typeof(List<>))[0];
				if (elementType.IsArray)
				{
					return " because Unity does not support Lists of arrays.";
				}
				if (elementType.ImplementsOpenGenericClass(typeof(List<>)))
				{
					return " because Unity does not support Lists of Lists.";
				}
				if (!UnitySerializationUtility.GuessIfUnityWillSerialize(elementType))
				{
					return " because Unity does not support the element type of " + elementType.GetNiceName() + ".";
				}
			}
			if (UnityVersion.IsVersionOrGreater(6000, 6) && type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<, >))
			{
				Type[] dictArgs = type.GetArgumentsOfInheritedOpenGenericClass(typeof(Dictionary<, >));
				Type key = dictArgs[0];
				Type value = dictArgs[1];
				if (key != typeof(string) && typeof(IEnumerable).IsAssignableFrom(key))
				{
					return " because the dictionary key type " + key.GetNiceName() + " implements IEnumerable, and Unity does not support collections as dictionary keys (except string).";
				}
				if (value.ImplementsOpenGenericClass(typeof(Dictionary<, >)))
				{
					return " because Unity does not support dictionaries whose value type is itself a dictionary.";
				}
				if (!UnitySerializationUtility.GuessIfUnityWillSerialize(key))
				{
					return " because Unity does not support the dictionary key type " + key.GetNiceName() + ".";
				}
				if (!UnitySerializationUtility.GuessIfUnityWillSerialize(value))
				{
					return " because Unity does not support the dictionary value type " + value.GetNiceName() + ".";
				}
			}
			if (type.IsGenericType || type.GetGenericArguments().Length != 0)
			{
				return " because Unity does not support generic types.";
			}
			if (type.Assembly == typeof(string).Assembly)
			{
				return " because Unity does not serialize [Serializable] structs and classes if they are defined in mscorlib.";
			}
			if (!type.IsDefined<SerializableAttribute>(inherit: false))
			{
				return " because the type is missing a [Serializable] attribute.";
			}
			return ". Was unable to determine reason, please report this to Sirenix.";
		}
	}
}
