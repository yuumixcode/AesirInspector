using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Sirenix.Config;
using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	public static class TypeRegistry
	{
		internal enum CheckedInstanceResult
		{
			Success = 0,
			NoTypeDefined = -1,
			NoDefaultCtorFound = -2,
			CreateInstanceFuncFailed = -3
		}

		internal readonly struct CheckedInstance
		{
			public readonly object Instance;

			public readonly CheckedInstanceResult Result;

			public CheckedInstance(object instance)
			{
				Instance = instance;
				Result = CheckedInstanceResult.Success;
			}

			public CheckedInstance(CheckedInstanceResult result)
			{
				Instance = null;
				Result = result;
			}
		}

		internal static HashSet<Type> ValidTypes;

		internal static HashSet<Type> HiddenTypes;

		internal static readonly Dictionary<AssemblyCategory, string> CategoryStringMap;

		internal static readonly Dictionary<Type, TypeRegistryItemAttribute> ItemSettings;

		internal static Dictionary<string, string> NamespacePaths;

		internal static Type[] SystemTypes;

		private static readonly Type TypeRegistryItemAttributeType;

		private static readonly Type CompilerGeneratedAttributeType;

		static TypeRegistry()
		{
			HiddenTypes = new HashSet<Type>();
			CategoryStringMap = new Dictionary<AssemblyCategory, string>();
			ItemSettings = new Dictionary<Type, TypeRegistryItemAttribute>();
			NamespacePaths = new Dictionary<string, string>(64);
			SystemTypes = new Type[29]
			{
				typeof(string),
				typeof(bool),
				typeof(sbyte),
				typeof(short),
				typeof(int),
				typeof(long),
				typeof(byte),
				typeof(ushort),
				typeof(uint),
				typeof(ulong),
				typeof(float),
				typeof(double),
				typeof(Array),
				typeof(List<>),
				typeof(Dictionary<, >),
				typeof(HashSet<>),
				typeof(Stack<>),
				typeof(Queue<>),
				typeof(Hashtable),
				typeof(LinkedList<>),
				typeof(LinkedListNode<>),
				typeof(SortedDictionary<, >),
				typeof(SortedList<, >),
				typeof(SortedSet<>),
				typeof(SortedList),
				typeof(ConcurrentBag<>),
				typeof(ConcurrentDictionary<, >),
				typeof(ConcurrentQueue<>),
				typeof(ConcurrentStack<>)
			};
			TypeRegistryItemAttributeType = typeof(TypeRegistryItemAttribute);
			CompilerGeneratedAttributeType = typeof(CompilerGeneratedAttribute);
			ValidTypes = new HashSet<Type>();
			foreach (Type type in AssemblyUtilities.GetTypes(AssemblyCategory.All))
			{
				if (!IsGeneratedType(type))
				{
					ValidTypes.Add(type);
				}
			}
			TypeCache.TypeCollection typesWithSettings = TypeCache.GetTypesWithAttribute(TypeRegistryItemAttributeType);
			for (int i = 0; i < typesWithSettings.Count; i++)
			{
				Type type2 = typesWithSettings[i];
				ItemSettings[type2] = type2.GetAttribute<TypeRegistryItemAttribute>(inherit: false);
			}
			ValidTypes.AddRange(SystemTypes);
		}

		internal static string GetName(Type type)
		{
			TypeSettings userSettings = GlobalConfig<TypeRegistryUserConfig>.Instance.TryGetSettings(type);
			if (userSettings != null && !string.IsNullOrEmpty(userSettings.Name))
			{
				return userSettings.Name;
			}
			if (ItemSettings.TryGetValue(type, out var settings))
			{
				if (string.IsNullOrEmpty(settings.Name))
				{
					return type.Name;
				}
				return settings.Name;
			}
			return type.Name;
		}

		internal static bool HasCustomName(Type type)
		{
			TypeSettings userSettings = GlobalConfig<TypeRegistryUserConfig>.Instance.TryGetSettings(type);
			if (userSettings != null && !string.IsNullOrEmpty(userSettings.Name))
			{
				return true;
			}
			if (ItemSettings.TryGetValue(type, out var settings))
			{
				return !string.IsNullOrEmpty(settings.Name);
			}
			return false;
		}

		internal static string GetNiceName(Type type)
		{
			TypeSettings userSettings = GlobalConfig<TypeRegistryUserConfig>.Instance.TryGetSettings(type);
			if (userSettings != null && !string.IsNullOrEmpty(userSettings.Name))
			{
				return userSettings.Name;
			}
			if (ItemSettings.TryGetValue(type, out var settings))
			{
				if (string.IsNullOrEmpty(settings.Name))
				{
					return type.GetNiceName();
				}
				return settings.Name;
			}
			return type.GetNiceName();
		}

		internal static bool TryGetCustomName(Type type, out string customName)
		{
			TypeSettings userSettings = GlobalConfig<TypeRegistryUserConfig>.Instance.TryGetSettings(type);
			customName = null;
			if (userSettings != null && !string.IsNullOrEmpty(userSettings.Name))
			{
				customName = userSettings.Name;
				return true;
			}
			if (!ItemSettings.TryGetValue(type, out var settings))
			{
				return false;
			}
			if (string.IsNullOrEmpty(settings.Name))
			{
				return false;
			}
			customName = settings.Name;
			return true;
		}

		internal static int GetPriority(Type type)
		{
			int userPriority = GlobalConfig<TypeRegistryUserConfig>.Instance.GetPriority(type);
			if (userPriority != 0)
			{
				return userPriority;
			}
			int priority = 0;
			if (ItemSettings.TryGetValue(type, out var itemSettings))
			{
				priority = itemSettings.Priority;
			}
			return priority;
		}

		internal static bool TryGetIcon(Type type, out SdfIconType icon, out Color? iconColor)
		{
			TypeSettings userSettings = GlobalConfig<TypeRegistryUserConfig>.Instance.TryGetSettings(type);
			icon = SdfIconType.None;
			iconColor = null;
			if (userSettings != null)
			{
				icon = userSettings.Icon;
				if (EditorGUIUtility.isProSkin)
				{
					iconColor = userSettings.DarkIconColor;
				}
				else
				{
					iconColor = userSettings.LightIconColor;
				}
			}
			if (ItemSettings.TryGetValue(type, out var settings))
			{
				if (icon == SdfIconType.None)
				{
					icon = settings.Icon;
				}
				if (!iconColor.HasValue)
				{
					if (EditorGUIUtility.isProSkin)
					{
						iconColor = settings.DarkIconColor;
					}
					else
					{
						iconColor = settings.LightIconColor;
					}
				}
			}
			return icon != SdfIconType.None;
		}

		internal static string GetNamespacePath(Type type)
		{
			string ns = type.Namespace;
			if (string.IsNullOrEmpty(ns))
			{
				return ns;
			}
			if (NamespacePaths.TryGetValue(ns, out var path))
			{
				return path;
			}
			return NamespacePaths[ns] = ns.Replace('.', '/');
		}

		internal static string GetCategoryPath(Type type, bool preferNamespaceOverAssemblyCategory)
		{
			TypeSettings userSettings = GlobalConfig<TypeRegistryUserConfig>.Instance.TryGetSettings(type);
			if (userSettings != null && !string.IsNullOrEmpty(userSettings.Category))
			{
				return userSettings.Category;
			}
			if (ItemSettings.TryGetValue(type, out var itemSettings) && !string.IsNullOrEmpty(itemSettings.CategoryPath))
			{
				return itemSettings.CategoryPath;
			}
			if (preferNamespaceOverAssemblyCategory)
			{
				return GetNamespacePath(type);
			}
			AssemblyCategory assemblyCategory = AssemblyUtilities.GetAssemblyCategory(type.Assembly);
			if (assemblyCategory == AssemblyCategory.None)
			{
				return string.Empty;
			}
			if (!CategoryStringMap.ContainsKey(assemblyCategory))
			{
				CategoryStringMap[assemblyCategory] = assemblyCategory.ToString();
			}
			return CategoryStringMap[assemblyCategory];
		}

		internal static bool IsModifiableType(Type type)
		{
			if (type.IsArray)
			{
				return false;
			}
			if (IsGeneratedType(type))
			{
				return false;
			}
			if (typeof(UnityEngine.Object).IsAssignableFrom(type))
			{
				return false;
			}
			if (type.IsGenericType)
			{
				return false;
			}
			return true;
		}

		public static List<Type> GetValidTypesInCategory(AssemblyCategory category)
		{
			List<Type> items = new List<Type>(64);
			foreach (Type type in AssemblyUtilities.GetTypes(category))
			{
				if (ValidTypes.Contains(type))
				{
					items.Add(type);
				}
			}
			return items;
		}

		public static List<Type> GetInheritors(Type type)
		{
			bool isClosedGeneric = type.IsGenericType && !type.IsGenericTypeDefinition;
			TypeCache.TypeCollection potentialInheritors = ((!isClosedGeneric) ? TypeCache.GetTypesDerivedFrom(type) : TypeCache.GetTypesDerivedFrom(type.GetGenericTypeDefinition()));
			HashSet<Type> result = new HashSet<Type>();
			if (isClosedGeneric)
			{
				Type[] typeGenericArgs = type.GetGenericArguments();
				Type[] systemTypes = SystemTypes;
				foreach (Type systemTypes2 in systemTypes)
				{
					if (!IsGeneratedType(systemTypes2) && TryGetCompatibleClosedGenericType(type, typeGenericArgs, systemTypes2, out var inheritor))
					{
						result.Add(inheritor);
					}
				}
				foreach (Type potentialInheritor in potentialInheritors)
				{
					if (!IsGeneratedType(potentialInheritor) && TryGetCompatibleClosedGenericType(type, typeGenericArgs, potentialInheritor, out var inheritor2))
					{
						result.Add(inheritor2);
					}
				}
			}
			else
			{
				Type[] systemTypes3 = SystemTypes;
				foreach (Type systemType in systemTypes3)
				{
					if (systemType.IsGenericType)
					{
						if (systemType.ImplementsOpenGenericType(type))
						{
							result.Add(systemType);
						}
					}
					else if (type.IsAssignableFrom(systemType))
					{
						result.Add(systemType);
					}
				}
				foreach (Type potentialInheritor2 in potentialInheritors)
				{
					if (!IsGeneratedType(potentialInheritor2))
					{
						result.Add(potentialInheritor2);
					}
				}
			}
			if (!IsGeneratedType(type))
			{
				result.Add(type);
			}
			return result.ToList();
		}

		public static List<Type> GetInstantiableInheritors(Type type, bool includeUnityTypes)
		{
			return GetInstantiableInheritors(type, includeUnityTypes, excludeTypesWithoutDefaultConstructor: false);
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="type"></param>
		/// <param name="includeUnityTypes"></param>
		/// <param name="excludeTypesWithoutDefaultConstructor">This is checked using <see cref="M:Sirenix.Utilities.TypeExtensions.HasDefaultConstructor(System.Type)" />.</param>
		/// <returns></returns>
		public static List<Type> GetInstantiableInheritors(Type type, bool includeUnityTypes, bool excludeTypesWithoutDefaultConstructor)
		{
			if (type.IsGenericType && type.IsGenericTypeDefinition)
			{
				return new List<Type>();
			}
			List<Type> result = GetInheritors(type);
			for (int i = result.Count - 1; i >= 0; i--)
			{
				Type currentInheritor = result[i];
				if (currentInheritor.IsAbstract || currentInheritor.IsInterface || currentInheritor.IsGenericTypeDefinition || (!includeUnityTypes && typeof(UnityEngine.Object).IsAssignableFrom(currentInheritor)) || (excludeTypesWithoutDefaultConstructor && !currentInheritor.HasDefaultConstructor()))
				{
					result.RemoveAt(i);
				}
			}
			return result;
		}

		internal static bool IsGeneratedType(Type type)
		{
			if (type == null)
			{
				return false;
			}
			if (!type.IsDefined(CompilerGeneratedAttributeType, inherit: false) && !type.Name.FastContains('<') && !IsPrivateImplementationDetails(type))
			{
				return type.Assembly.IsDynamic();
			}
			return true;
		}

		internal static bool IsPrivateImplementationDetails(Type type)
		{
			if (type.DeclaringType != null)
			{
				return type.DeclaringType.Name.FastStartsWith("<PrivateImplementationDetails>");
			}
			return false;
		}

		internal static bool TryGetCompatibleClosedGenericType(Type closedGenericType, Type[] closedGenericArgs, Type type, out Type result)
		{
			result = null;
			if (!type.IsGenericType || !type.IsGenericTypeDefinition)
			{
				if (closedGenericType.IsAssignableFrom(type))
				{
					result = type;
					return true;
				}
				return false;
			}
			if (type.TryInferGenericParameters(out var inferredParams, closedGenericArgs))
			{
				Type inferredInheritor = type.MakeGenericType(inferredParams);
				if (closedGenericType.IsAssignableFrom(inferredInheritor))
				{
					result = inferredInheritor;
					return true;
				}
			}
			return false;
		}

		internal static CheckedInstance CreateCheckedInstance(Type type, InspectorProperty property = null)
		{
			if (type == null)
			{
				Debug.LogError("Attempted to create an instance of a NULL type.");
				return new CheckedInstance(CheckedInstanceResult.NoTypeDefined);
			}
			if (type == typeof(TypeSelectorV2.TypeSelectorNoneValue))
			{
				return new CheckedInstance(null);
			}
			NonDefaultConstructorPreference handleNonDefaultCtors;
			if (property != null)
			{
				PolymorphicDrawerSettingsAttribute settings = property.GetAttribute<PolymorphicDrawerSettingsAttribute>();
				if (settings != null)
				{
					if (!string.IsNullOrEmpty(settings.CreateInstanceFunction))
					{
						ValueResolver<object> createInstanceFunc = ValueResolver.Get<object>(property, settings.CreateInstanceFunction, new NamedValue[1]
						{
							new NamedValue("type", typeof(Type), null)
						});
						if (createInstanceFunc.HasError)
						{
							Debug.LogError(createInstanceFunc.ErrorMessage);
							return new CheckedInstance(CheckedInstanceResult.CreateInstanceFuncFailed);
						}
						createInstanceFunc.Context.NamedValues.Set("type", type);
						return new CheckedInstance(createInstanceFunc.GetValue());
					}
					handleNonDefaultCtors = (settings.NonDefaultConstructorPreferenceIsSet ? settings.NonDefaultConstructorPreference : GlobalConfig<GeneralDrawerConfig>.Instance.nonDefaultConstructorPreference);
				}
				else
				{
					handleNonDefaultCtors = GlobalConfig<GeneralDrawerConfig>.Instance.nonDefaultConstructorPreference;
				}
			}
			else
			{
				handleNonDefaultCtors = GlobalConfig<GeneralDrawerConfig>.Instance.nonDefaultConstructorPreference;
			}
			if (type.HasDefaultConstructor() || handleNonDefaultCtors != NonDefaultConstructorPreference.LogWarning)
			{
				return new CheckedInstance(type.InstantiateDefault(handleNonDefaultCtors == NonDefaultConstructorPreference.PreferUninitialized));
			}
			Debug.LogWarning("Failed to instantiate " + type.Name + ", since it has no default constructor.");
			return new CheckedInstance(CheckedInstanceResult.NoDefaultCtorFound);
		}

		private static bool FastStartsWith(this string str, string startsWith)
		{
			if (str.Length < startsWith.Length)
			{
				return false;
			}
			for (int i = 0; i < startsWith.Length; i++)
			{
				if (str[i] != startsWith[i])
				{
					return false;
				}
			}
			return true;
		}

		private static bool FastContains(this string str, char c)
		{
			for (int i = 0; i < str.Length; i++)
			{
				if (str[i] == c)
				{
					return true;
				}
			}
			return false;
		}
	}
}
