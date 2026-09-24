using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Sirenix.Serialization;
using Sirenix.Utilities;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal static class DesignerUtils
	{
		internal class GenericObject
		{
		}

		[StructLayout(LayoutKind.Sequential, Size = 1)]
		internal struct GenericStruct
		{
		}

		private static readonly HashSet<Type> IgnoredTypes = new HashSet<Type>(FastTypeComparer.Instance)
		{
			typeof(void),
			typeof(Type),
			typeof(RuntimeTypeHandle),
			typeof(Attribute),
			typeof(object),
			typeof(ValueType),
			typeof(string),
			typeof(EditorWindow),
			typeof(Behaviour),
			typeof(Component),
			typeof(GameObject),
			typeof(MonoBehaviour),
			typeof(UnityEngine.Object),
			typeof(ScriptableObject),
			typeof(StateMachineBehaviour),
			typeof(OdinEditorWindow),
			typeof(SerializedBehaviour),
			typeof(SerializedComponent),
			typeof(SerializedScriptableObject),
			typeof(SerializedStateMachineBehaviour),
			typeof(SerializedMonoBehaviour),
			typeof(SerializedUnityObject),
			typeof(PropertyTree)
		};

		internal static bool IsRefreshQueued = false;

		public static HashSet<Type> SupportedValueTypesForSerialization = new HashSet<Type>
		{
			typeof(ColumnSize),
			typeof(Color)
		};

		private static readonly StringBuilder FileNameBuffer = new StringBuilder(256);

		private static readonly HashSet<Type> InheritorsFilter = new HashSet<Type>();

		private static readonly List<Type> Inheritors = new List<Type>();

		private static readonly StringBuilder ReusableStringBuilder = new StringBuilder(128);

		public static bool CanTypeBeDesigned(Type type)
		{
			if (type == null)
			{
				return false;
			}
			if (type.IsPrimitive || type.IsEnum || type.IsInterface)
			{
				return false;
			}
			if (IgnoredTypes.Contains(type))
			{
				return false;
			}
			if (!type.IsClass)
			{
				return type.IsValueType;
			}
			return true;
		}

		public static bool IsExcludedFromDesigner(InspectorPropertyInfo info)
		{
			if (info.GetAttribute<ExcludeInOdinDesignerAttribute>() == null)
			{
				return DesignerRegistry.IsTypeOfOwnerExcluded(info);
			}
			return true;
		}

		public static bool IsExcludedFromDesigner(InspectorProperty property)
		{
			if (property.GetAttribute<ExcludeInOdinDesignerAttribute>() == null)
			{
				return DesignerRegistry.IsTypeOfOwnerExcluded(property.Info);
			}
			return true;
		}

		public static int Round4(int x)
		{
			return (x + 3) & -4;
		}

		public static int Round8(int x)
		{
			return (x + 7) & -8;
		}

		public static int Round32(int x)
		{
			return (x + 31) & -32;
		}

		public static int Round64(int x)
		{
			return (x + 63) & -64;
		}

		public static int Round256(int x)
		{
			return (x + 255) & -256;
		}

		public static void RefreshInspectorAndEditors()
		{
			if (IsRefreshQueued)
			{
				return;
			}
			UnityEditorEventUtility.DelayAction(delegate
			{
				try
				{
					List<OdinEditor> activeEditors = OdinEditors.ActiveEditors;
					if (activeEditors != null)
					{
						for (int i = 0; i < activeEditors.Count; i++)
						{
							OdinEditor odinEditor = activeEditors[i];
							if (!(odinEditor == null))
							{
								PropertyTree tree = odinEditor.Tree;
								InspectorProperty inspectorProperty = tree?.RootProperty;
								if (inspectorProperty != null)
								{
									tree.hasSetupSearchFilter = false;
									inspectorProperty.RefreshSetup();
								}
							}
						}
					}
					List<OdinEditorWindow> activeWindows = OdinEditorWindows.ActiveWindows;
					if (activeWindows != null)
					{
						for (int j = 0; j < activeWindows.Count; j++)
						{
							OdinEditorWindow odinEditorWindow = activeWindows[j];
							if (!(odinEditorWindow == null))
							{
								odinEditorWindow.RefreshPropertyTrees();
							}
						}
					}
					InternalEditorUtility.RepaintAllViews();
				}
				finally
				{
					IsRefreshQueued = false;
				}
			});
			IsRefreshQueued = true;
		}

		public static bool CanAttributePropertyBeModified(InspectorProperty property)
		{
			if (!property.Info.HasBackingMembers)
			{
				return false;
			}
			MemberInfo member = property.Info.GetMemberInfo();
			Type fieldType;
			switch (member.MemberType)
			{
			case MemberTypes.Field:
				fieldType = ((FieldInfo)member).FieldType;
				break;
			case MemberTypes.Property:
				if (property.GetAttribute<OdinDesignerBindingAttribute>() == null)
				{
					return false;
				}
				fieldType = ((PropertyInfo)member).PropertyType;
				break;
			default:
				return false;
			}
			if (fieldType == typeof(string) || fieldType == typeof(Type))
			{
				return true;
			}
			if (fieldType.IsEnum)
			{
				return true;
			}
			if (!fieldType.IsValueType)
			{
				return false;
			}
			if (fieldType.IsPrimitive)
			{
				return true;
			}
			return SupportedValueTypesForSerialization.Contains(fieldType);
		}

		public static string CreateFilePathFromType(Type type)
		{
			FileNameBuffer.Clear();
			string root = Path.GetFullPath(GlobalConfig<OdinVisualDesignerConfig>.Instance.SavePath).Replace('\\', '/');
			FileNameBuffer.Append(root);
			char lastChar = root[root.Length - 1];
			if (lastChar != '\\' && lastChar != '/')
			{
				FileNameBuffer.Append('/');
			}
			string ns = type.Namespace;
			string niceName = type.GetNiceName();
			if (!string.IsNullOrEmpty(ns))
			{
				string text = ns;
				foreach (char c in text)
				{
					if (c == '.')
					{
						FileNameBuffer.Append('_');
					}
					else
					{
						FileNameBuffer.Append(c);
					}
				}
				FileNameBuffer.Append('_');
			}
			string text2 = niceName;
			foreach (char c2 in text2)
			{
				switch (c2)
				{
				case '<':
					FileNameBuffer.Append('[');
					break;
				case '>':
					FileNameBuffer.Append(']');
					break;
				default:
					FileNameBuffer.Append(c2);
					break;
				}
			}
			FileNameBuffer.Append('.');
			FileNameBuffer.Append("ovdf");
			return FileNameBuffer.ToString();
		}

		public static bool TryReadMetadataFromFile(string path, out Type type, out string metaGuid)
		{
			type = null;
			metaGuid = null;
			string typeBinding;
			using (StreamReader reader = new StreamReader(path))
			{
				string header = reader.ReadLine();
				if (!DesignerFormat.IsValidHeader(header))
				{
					return false;
				}
				typeBinding = reader.ReadLine();
				if (string.IsNullOrEmpty(typeBinding))
				{
					return false;
				}
				metaGuid = reader.ReadLine();
				if (!string.IsNullOrEmpty(metaGuid))
				{
					if (metaGuid.Length > "MetaGuid:".Length)
					{
						metaGuid = metaGuid.Trim();
						if (metaGuid.StartsWith("MetaGuid:"))
						{
							metaGuid = metaGuid.Remove(0, "MetaGuid:".Length).Trim();
						}
						else
						{
							metaGuid = null;
						}
					}
					else
					{
						metaGuid = null;
					}
				}
			}
			type = ((!string.IsNullOrEmpty(metaGuid)) ? GetTypeFromScriptGuid(metaGuid) : null);
			if (type == null)
			{
				type = TwoWaySerializationBinder.Default.BindToType(typeBinding);
			}
			if (type == null)
			{
				OdinVisualDesignerConfig config = GlobalConfig<OdinVisualDesignerConfig>.Instance;
				if (config.LogLevelTypeMismatches != OdinVisualDesignerLogLevel.None)
				{
					string msg = "Failed to bind type '" + typeBinding + "', from file '" + path + "' on line 2.";
					switch (config.LogLevelTypeMismatches)
					{
					case OdinVisualDesignerLogLevel.Warning:
						DesignerLogger.LogWarning(msg);
						break;
					case OdinVisualDesignerLogLevel.Error:
						DesignerLogger.LogError(msg);
						break;
					}
				}
			}
			return true;
		}

		public static Type GetTypeFromFile(string path)
		{
			TryReadMetadataFromFile(path, out var result, out var _);
			return result;
		}

		public static Type GetBaseType(Type type)
		{
			if (type.IsGenericType)
			{
				Type genericDefinition = type.GetGenericTypeDefinition();
				if (type != genericDefinition)
				{
					return genericDefinition;
				}
			}
			return type.BaseType;
		}

		public static Type GetDeclType(MemberInfo member)
		{
			Type declType = member.DeclaringType;
			if (declType != null && declType.IsGenericType)
			{
				Type genericDef = declType.GetGenericTypeDefinition();
				if (declType != genericDef)
				{
					MemberInfo genericMember = null;
					switch (member.MemberType)
					{
					case MemberTypes.Method:
					{
						MethodInfo method = (MethodInfo)member;
						MethodInfo[] methods = genericDef.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
						MethodInfo methodBaseDefinition = method.GetBaseDefinition();
						ParameterInfo[] methodParams = method.GetParameters();
						Type methodReturnType = method.GetReturnType();
						foreach (MethodInfo methodToCheck in methods)
						{
							if (methodToCheck.MetadataToken == methodBaseDefinition.MetadataToken)
							{
								genericMember = methodToCheck;
								break;
							}
							if (!(methodToCheck.Name != methodBaseDefinition.Name) && !(methodToCheck.GetReturnType().Name != methodReturnType.Name) && AreParametersEqual(methodParams, methodToCheck.GetParameters()))
							{
								genericMember = methodToCheck;
								break;
							}
						}
						break;
					}
					case MemberTypes.Property:
					{
						PropertyInfo prop = (PropertyInfo)member;
						genericMember = genericDef.GetProperty(prop.Name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
						break;
					}
					case MemberTypes.Field:
					{
						FieldInfo field = (FieldInfo)member;
						genericMember = genericDef.GetField(field.Name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
						break;
					}
					}
					if (genericMember != null)
					{
						return genericDef;
					}
				}
			}
			return declType;
		}

		public static Color GetDeclTypeColor(Type editorDeclType, Type declType)
		{
			if (declType == editorDeclType)
			{
				return Color.white;
			}
			int depth = 0;
			Type current = GetBaseType(declType);
			while (current != null && CanTypeBeDesigned(current))
			{
				current = GetBaseType(current);
				depth++;
			}
			return Colors.BreadcrumbColors[depth % Colors.BreadcrumbColors.Length];
		}

		public static Color GetMemberColor(Type editorDeclType, MemberInfo member)
		{
			Type declType = member.DeclaringType;
			if (declType == editorDeclType)
			{
				return Color.white;
			}
			int depth = 0;
			if (declType.IsGenericType && member.MetadataToken != 0)
			{
				Type definition = declType.GetGenericTypeDefinition();
				MemberInfo resolvedMember = definition.Module.ResolveMember(member.MetadataToken);
				if (definition == resolvedMember.DeclaringType)
				{
					depth--;
				}
			}
			Type current = GetBaseType(declType);
			while (current != null && CanTypeBeDesigned(current))
			{
				current = GetBaseType(current);
				depth++;
			}
			return Colors.BreadcrumbColors[depth % Colors.BreadcrumbColors.Length];
		}

		public static void AddGenericMeuItemEditType(GenericMenu menu, string label, Type type)
		{
			if (CanTypeBeDesigned(type))
			{
				menu.AddItem(new GUIContent(label), on: false, delegate
				{
					DesignerEditors.Get(type, null, null)?.OpenWindow();
				});
			}
			else
			{
				menu.AddDisabledItem(new GUIContent(label));
			}
		}

		public static int GetHierarchyDepth(Type type)
		{
			type = GetBaseType(type);
			int depth = 0;
			while (type != null)
			{
				depth++;
				type = GetBaseType(type);
			}
			return depth;
		}

		internal static bool AreParametersEqual(ParameterInfo[] lhs, ParameterInfo[] rhs)
		{
			if (lhs == rhs)
			{
				return true;
			}
			if (lhs == null || rhs == null || lhs.Length != rhs.Length)
			{
				return false;
			}
			for (int i = 0; i < lhs.Length; i++)
			{
				ParameterInfo lhsParam = lhs[i];
				ParameterInfo rhsParam = rhs[i];
				if (lhsParam.ParameterType.Name != rhsParam.ParameterType.Name)
				{
					return false;
				}
			}
			return true;
		}

		public static MethodInfo GetOpenGenericVariantOrSelf(MethodInfo method)
		{
			if (method == null)
			{
				return null;
			}
			Type declType = method.DeclaringType;
			if (declType.IsGenericType && !declType.IsGenericTypeDefinition)
			{
				declType = declType.GetGenericTypeDefinition();
				return MethodBase.GetMethodFromHandle(method.MethodHandle, declType.TypeHandle) as MethodInfo;
			}
			if (method.IsGenericMethod)
			{
				return method.GetGenericMethodDefinition();
			}
			return method;
		}

		public static Type GetClosedVariant(Type type)
		{
			if (type == null)
			{
				return null;
			}
			if (!type.IsGenericType || !type.ContainsGenericParameters)
			{
				return type;
			}
			Type[] genericArgs = type.GetGenericArguments();
			if (HasSelfReferentialConstraint(type, genericArgs))
			{
				throw new InvalidOperationException("The Visual Designer does not currently support generic types with self-referential constraints. (" + type.GetNiceName() + ")");
			}
			for (int i = 0; i < genericArgs.Length; i++)
			{
				Type arg = genericArgs[i];
				if (!arg.IsGenericParameter)
				{
					continue;
				}
				Type[] constraints = arg.GetGenericParameterConstraints();
				GenericParameterAttributes genericAttributes = arg.GenericParameterAttributes;
				if (constraints.Length == 0)
				{
					if (genericAttributes.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint))
					{
						genericArgs[i] = typeof(GenericStruct);
					}
					else
					{
						genericArgs[i] = typeof(GenericObject);
					}
				}
				else
				{
					bool needsDefaultCtor = genericAttributes.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint);
					bool structsOnly = genericAttributes.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint);
					genericArgs[i] = RetrieveValidTypeFromConstraints(constraints, needsDefaultCtor, structsOnly);
				}
			}
			return type.MakeGenericType(genericArgs);
		}

		internal static bool HasSelfReferentialConstraint(Type type, Type[] genericArgs)
		{
			Type typeDef = type.GetGenericTypeDefinition();
			for (int i = 0; i < genericArgs.Length; i++)
			{
				Type[] constraints = genericArgs[i].GetGenericParameterConstraints();
				foreach (Type constraint in constraints)
				{
					if (constraint.IsGenericType && constraint.GetGenericTypeDefinition() == typeDef)
					{
						return true;
					}
				}
			}
			return false;
		}

		internal static Type RetrieveValidTypeFromConstraints(Type[] constraints, bool requireDefaultCtor, bool structsOnly)
		{
			InheritorsFilter.Clear();
			Inheritors.Clear();
			foreach (Type constraint in constraints)
			{
				if (constraint.IsGenericType && constraint.ContainsGenericParameters)
				{
					throw new InvalidOperationException("The Visual Designer does not currently support closing generic types that contain open generic constraints. (" + constraint.GetNiceName() + ")");
				}
				InheritorsFilter.AddRange(TypeRegistry.GetInheritors(constraint));
			}
			Inheritors.AddRange(InheritorsFilter);
			if (structsOnly)
			{
				for (int i2 = Inheritors.Count - 1; i2 >= 0; i2--)
				{
					if (!Inheritors[i2].IsValueType)
					{
						Inheritors.RemoveAt(i2);
					}
				}
			}
			else if (requireDefaultCtor)
			{
				for (int i3 = Inheritors.Count - 1; i3 >= 0; i3--)
				{
					if (!Inheritors[i3].HasDefaultConstructor())
					{
						Inheritors.RemoveAt(i3);
					}
				}
			}
			for (int i4 = Inheritors.Count - 1; i4 >= 0; i4--)
			{
				bool satisfiesConstraints = true;
				for (int j = 0; j < constraints.Length; j++)
				{
					if (!constraints[j].IsAssignableFrom(Inheritors[i4]))
					{
						satisfiesConstraints = false;
						break;
					}
				}
				if (!satisfiesConstraints)
				{
					Inheritors.RemoveAt(i4);
				}
			}
			return FindHighestInHierarchy(Inheritors);
		}

		internal static Type FindHighestInHierarchy(List<Type> types)
		{
			Type highest = null;
			int highestDepth = int.MaxValue;
			foreach (Type current in types)
			{
				int depth = GetInheritanceDepth(current);
				if (depth < highestDepth)
				{
					highestDepth = depth;
					highest = current;
				}
			}
			return highest;
		}

		private static int GetInheritanceDepth(Type type)
		{
			int result = 0;
			Type current = type;
			while (current != null)
			{
				result++;
				current = current.BaseType;
			}
			return result;
		}

		public static string CreateSerializedMethodName(MethodBase method)
		{
			ReusableStringBuilder.Clear();
			ReusableStringBuilder.Append(method.Name);
			if (method.IsGenericMethod)
			{
				Type[] genArgs = method.GetGenericArguments();
				ReusableStringBuilder.Append("<");
				for (int i = 0; i < genArgs.Length; i++)
				{
					if (i != 0)
					{
						ReusableStringBuilder.Append(", ");
					}
					ReusableStringBuilder.Append(genArgs[i].GetNiceName());
				}
				ReusableStringBuilder.Append(">");
			}
			ReusableStringBuilder.Append('(');
			ParameterInfo[] parameters = method.GetParameters();
			for (int j = 0; j < parameters.Length; j++)
			{
				if (j != 0)
				{
					ReusableStringBuilder.Append(", ");
				}
				ParameterInfo param = parameters[j];
				Type paramType = param.ParameterType;
				if (paramType.IsByRef)
				{
					paramType = paramType.GetElementType();
					if (param.IsOut)
					{
						ReusableStringBuilder.Append("out ");
					}
					else if (param.IsIn)
					{
						ReusableStringBuilder.Append("in ");
					}
					else
					{
						ReusableStringBuilder.Append("ref ");
					}
				}
				else if (param.IsIn)
				{
					ReusableStringBuilder.Append("in ");
				}
				ReusableStringBuilder.Append(paramType.GetNiceName());
			}
			ReusableStringBuilder.Append(')');
			return ReusableStringBuilder.ToString();
		}

		public static bool IsPropertyResolverSupported(OdinPropertyResolver resolver)
		{
			if (resolver != null)
			{
				return IsPropertyResolverSupported(resolver.GetType());
			}
			return false;
		}

		public static bool IsPropertyResolverSupported(Type processorType)
		{
			processorType = (processorType.IsGenericType ? processorType.GetGenericTypeDefinition() : processorType);
			if (processorType == typeof(ProcessedMemberPropertyResolver<>))
			{
				return true;
			}
			return processorType == typeof(DesignerEditorPropertyResolver<>);
		}

		public static string TryGetScriptGuid(Type type)
		{
			if (!typeof(MonoBehaviour).IsAssignableFrom(type) && !typeof(ScriptableObject).IsAssignableFrom(type))
			{
				return null;
			}
			string typeName = type.Name;
			string[] guids = AssetDatabase.FindAssets(typeName + " t:MonoScript");
			foreach (string guid in guids)
			{
				string path = AssetDatabase.GUIDToAssetPath(guid);
				MonoScript monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
				if (monoScript != null && monoScript.GetClass() == type)
				{
					return guid;
				}
			}
			return null;
		}

		public static Type GetTypeFromScriptGuid(string guid)
		{
			if (string.IsNullOrEmpty(guid))
			{
				return null;
			}
			string assetPath = AssetDatabase.GUIDToAssetPath(guid);
			return AssetDatabase.LoadAssetAtPath<MonoScript>(assetPath)?.GetClass();
		}
	}
}
