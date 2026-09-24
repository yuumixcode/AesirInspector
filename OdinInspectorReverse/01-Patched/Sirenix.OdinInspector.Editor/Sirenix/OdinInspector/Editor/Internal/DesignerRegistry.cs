using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.Serialization;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal static class DesignerRegistry
	{
		public static readonly Dictionary<Type, HashSet<Type>> RegisteredGenericVariants;

		public static Dictionary<Type, Type> SubGroupMap;

		public static HashSet<Type> SubGroups;

		public static HashSet<Type> ExcludedGroups;

		public static HashSet<Type> ExcludedTypes;

		public static HashSet<Type> ValidAttributes;

		public static bool HasInitialized;

		public static bool IsPropertyDesigned(InspectorProperty property)
		{
			return IsTypeDesigned((property.ValueEntry != null) ? property.ValueEntry.TypeOfValue : property.Info.TypeOfValue);
		}

		public static bool IsTypeDesigned(Type type)
		{
			if (type == null || type.IsPrimitive)
			{
				return false;
			}
			if (EditorTypePatches.Exists(type) || OVDFFileWatcher.DesignerFiles.ContainsKey(type))
			{
				return true;
			}
			Type nextType = DesignerUtils.GetBaseType(type);
			while (nextType != null)
			{
				if (IsTypeDesigned(nextType))
				{
					return true;
				}
				nextType = DesignerUtils.GetBaseType(nextType);
			}
			return false;
		}

		public static bool IsTypeDesignedNoHierarchyCheck(Type type)
		{
			if (type == null || type.IsPrimitive)
			{
				return false;
			}
			if (!EditorTypePatches.Exists(type))
			{
				return OVDFFileWatcher.DesignerFiles.ContainsKey(type);
			}
			return true;
		}

		public static void RegisterGenericVariant(Type type, bool validateTypeArgs)
		{
			Type definition = type.GetGenericTypeDefinition();
			if (type == definition)
			{
				return;
			}
			if (validateTypeArgs)
			{
				Type[] args = type.GetGenericArguments();
				foreach (Type arg in args)
				{
					if (arg == typeof(DesignerUtils.GenericObject) || arg == typeof(DesignerUtils.GenericStruct))
					{
						return;
					}
				}
			}
			if (!RegisteredGenericVariants.TryGetValue(definition, out var set))
			{
				set = (RegisteredGenericVariants[definition] = new HashSet<Type>());
			}
			set.Add(type);
		}

		static DesignerRegistry()
		{
			RegisteredGenericVariants = new Dictionary<Type, HashSet<Type>>(16);
			SubGroupMap = new Dictionary<Type, Type>(FastTypeComparer.Instance)
			{
				{
					typeof(TabGroupAttribute),
					typeof(TabGroupAttribute.TabSubGroupAttribute)
				},
				{
					typeof(ColumnGroupAttribute),
					typeof(ColumnGroupAttribute.ColumnSubGroupAttribute)
				}
			};
			SubGroups = new HashSet<Type>(FastTypeComparer.Instance)
			{
				typeof(TabGroupAttribute.TabSubGroupAttribute),
				typeof(ColumnGroupAttribute.ColumnSubGroupAttribute)
			};
			ExcludedGroups = new HashSet<Type>(FastTypeComparer.Instance)
			{
				typeof(ColumnGroupAttribute),
				typeof(ColumnGroupAttribute.ColumnSubGroupAttribute),
				typeof(TabGroupAttribute.TabSubGroupAttribute)
			};
			ExcludedTypes = new HashSet<Type>(FastTypeComparer.Instance)
			{
				typeof(SerializationData),
				typeof(SerializedBehaviour),
				typeof(SerializedComponent),
				typeof(SerializedMonoBehaviour),
				typeof(SerializedScriptableObject),
				typeof(SerializedStateMachineBehaviour),
				typeof(SerializedUnityObject),
				typeof(EditorWindow),
				typeof(OdinEditorWindow),
				typeof(OdinMenuEditorWindow)
			};
			ValidAttributes = new HashSet<Type>();
			HasInitialized = false;
			Initialize();
		}

		public static void Initialize()
		{
			foreach (Assembly assembly in AssemblyUtilities.GetAllAssemblies())
			{
				foreach (OdinVisualDesignerAttributeItem item in assembly.GetAttributes<OdinVisualDesignerAttributeItem>())
				{
					ValidAttributes.Add(item.AttributeType);
				}
			}
			ValidAttributes.Add(typeof(DelayedAttribute));
			ValidAttributes.Add(typeof(HeaderAttribute));
			ValidAttributes.Add(typeof(InspectorNameAttribute));
			ValidAttributes.Add(typeof(MinAttribute));
			ValidAttributes.Add(typeof(MultilineAttribute));
			ValidAttributes.Add(typeof(RangeAttribute));
			ValidAttributes.Add(typeof(SpaceAttribute));
			ValidAttributes.Add(typeof(TextAreaAttribute));
			ValidAttributes.Add(typeof(TooltipAttribute));
			HasInitialized = true;
		}

		public static bool IsValidAttribute(Attribute attribute)
		{
			if (attribute == null)
			{
				return false;
			}
			if (!HasInitialized)
			{
				Initialize();
			}
			return ValidAttributes.Contains(attribute.GetType());
		}

		public static bool IsTypeOfOwnerExcluded(InspectorPropertyInfo info)
		{
			if (info == null)
			{
				return false;
			}
			Type typeOfOwner = info.TypeOfOwner;
			if (ExcludedTypes.Contains(typeOfOwner))
			{
				return true;
			}
			return typeOfOwner.IsDefined(typeof(ExcludeInOdinDesignerAttribute), inherit: false);
		}
	}
}
