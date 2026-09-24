using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Sirenix.OdinInspector.Editor.Drawers;
using Sirenix.OdinInspector.Editor.TypeSearch;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	public static class DrawerUtilities
	{
		private static class Null
		{
		}

		private struct DrawerAndPriority
		{
			public Type Drawer;

			public DrawerPriority Priority;

			public string Name;
		}

		public static class InvalidAttributeTargetUtility
		{
			private static readonly Dictionary<Type, List<Type>> ConcreteAttributeTargets = new Dictionary<Type, List<Type>>(FastTypeComparer.Instance);

			private static readonly Dictionary<Type, List<Type>> GenericParameterAttributeTargets = new Dictionary<Type, List<Type>>(FastTypeComparer.Instance);

			private static readonly DoubleLookupDictionary<Type, Type, bool> ShowErrorCache = new DoubleLookupDictionary<Type, Type, bool>(FastTypeComparer.Instance, FastTypeComparer.Instance);

			private static readonly List<Type> EmptyList = new List<Type>();

			public static void RegisterValidAttributeTarget(Type attribute, Type target)
			{
				List<Type> list;
				if (attribute.IsGenericParameter)
				{
					if (!GenericParameterAttributeTargets.TryGetValue(attribute, out list))
					{
						list = new List<Type>();
						GenericParameterAttributeTargets[attribute] = list;
					}
				}
				else if (!ConcreteAttributeTargets.TryGetValue(attribute, out list))
				{
					list = new List<Type>();
					ConcreteAttributeTargets[attribute] = list;
				}
				list.Add(target);
			}

			public static List<Type> GetValidTargets(Type attribute)
			{
				if (!ConcreteAttributeTargets.TryGetValue(attribute, out var result) && !GenericParameterAttributeTargets.TryGetValue(attribute, out result))
				{
					bool foundAnyValids = false;
					foreach (KeyValuePair<Type, List<Type>> entry in GenericParameterAttributeTargets)
					{
						Type param = entry.Key;
						List<Type> targets = entry.Value;
						if (param.GenericParameterIsFulfilledBy(attribute))
						{
							result = targets.ToList();
							ConcreteAttributeTargets[attribute] = result;
							foundAnyValids = true;
						}
					}
					if (!foundAnyValids)
					{
						ConcreteAttributeTargets[attribute] = null;
					}
				}
				return result ?? EmptyList;
			}

			public static bool ShowInvalidAttributeErrorFor(InspectorProperty property, Type attribute)
			{
				if (property.ValueEntry == null)
				{
					return false;
				}
				if (property.ValueEntry.BaseValueType == typeof(object))
				{
					return false;
				}
				if (property.Parent != null && property.Parent.ChildResolver is ICollectionResolver)
				{
					return false;
				}
				if (property.GetAttribute<SuppressInvalidAttributeErrorAttribute>() != null)
				{
					return false;
				}
				if (property.Info.TypeOfValue.IsInterface)
				{
					return false;
				}
				if (property.ChildResolver is ICollectionResolver collectionResolver)
				{
					if (collectionResolver.ElementType == typeof(object))
					{
						return false;
					}
					if (collectionResolver.ElementType.IsInterface)
					{
						return false;
					}
					if (ShowInvalidAttributeErrorFor(attribute, property.ValueEntry.BaseValueType))
					{
						return ShowInvalidAttributeErrorFor(attribute, collectionResolver.ElementType);
					}
					return false;
				}
				return ShowInvalidAttributeErrorFor(attribute, property.ValueEntry.BaseValueType);
			}

			public static bool ShowInvalidAttributeErrorFor(Type attribute, Type value)
			{
				if (!ShowErrorCache.TryGetInnerValue(attribute, value, out var result))
				{
					result = CalculateShowInvalidAttributeErrorFor(attribute, value);
					ShowErrorCache[attribute][value] = result;
				}
				return result;
			}

			private static bool CalculateShowInvalidAttributeErrorFor(Type attribute, Type value)
			{
				if (attribute == typeof(DelayedAttribute) || attribute == typeof(DelayedPropertyAttribute))
				{
					return false;
				}
				List<Type> validTargets = GetValidTargets(attribute);
				if (validTargets.Count == 0)
				{
					return false;
				}
				if (value == typeof(object))
				{
					return false;
				}
				for (int i = 0; i < validTargets.Count; i++)
				{
					Type valid = validTargets[i];
					if (valid == value)
					{
						return false;
					}
					if (valid.IsGenericParameter && valid.GenericParameterIsFulfilledBy(value))
					{
						return false;
					}
				}
				return true;
			}
		}

		public static readonly TypeSearchIndex SearchIndex;

		private static List<DrawerAndPriority> AllDrawerTypes;

		internal static readonly MethodInfo DecoratorDrawerCreatePropertyGUIMethod;

		private static readonly FieldInfo CustomPropertyDrawerTypeField;

		private static readonly FieldInfo CustomPropertyDrawerUseForChildrenField;

		private static readonly bool SupportsUnityDrawers;

		private static readonly Dictionary<Type, DrawerPriority> DrawerTypePriorityLookup;

		private static readonly Dictionary<Type, OdinDrawer> UninitializedDrawers;

		private static TypeSearchResult[][] CachedQueryResultArray;

		private static readonly Type AbstractTypeUnityPropertyDrawer_TArg2;

		private static readonly Type UnityPropertyAttributeDrawer_TArg1;

		private static readonly Type UnityDecoratorAttributeDrawer_TArg1;

		private static readonly TypeMatchRule InvalidAttributeRule;

		/// <summary>
		/// Odin has its own implementations for these attribute drawers; never use Unity's.
		/// </summary>
		private static readonly HashSet<string> ExcludeUnityDrawers;

		private static readonly Dictionary<Type, TypeSearchResult[]> InvalidAttributeTypeSearchResults;

		static DrawerUtilities()
		{
			SearchIndex = new TypeSearchIndex
			{
				MatchedTypeLogName = "drawer"
			};
			CustomPropertyDrawerTypeField = typeof(CustomPropertyDrawer).GetField("m_Type", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			CustomPropertyDrawerUseForChildrenField = typeof(CustomPropertyDrawer).GetField("m_UseForChildren", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			SupportsUnityDrawers = CustomPropertyDrawerTypeField != null && CustomPropertyDrawerUseForChildrenField != null;
			DrawerTypePriorityLookup = new Dictionary<Type, DrawerPriority>(FastTypeComparer.Instance);
			UninitializedDrawers = new Dictionary<Type, OdinDrawer>(FastTypeComparer.Instance);
			CachedQueryResultArray = new TypeSearchResult[16][];
			AbstractTypeUnityPropertyDrawer_TArg2 = typeof(AbstractTypeUnityPropertyDrawer<, , >).GetGenericArguments()[2];
			UnityPropertyAttributeDrawer_TArg1 = typeof(UnityPropertyAttributeDrawer<, , >).GetGenericArguments()[1];
			UnityDecoratorAttributeDrawer_TArg1 = typeof(UnityDecoratorAttributeDrawer<, , >).GetGenericArguments()[1];
			InvalidAttributeRule = new TypeMatchRule("Invalid Attribute Notification Dummy Rule (This is never matched against, but only serves to be a result rule for invalid attribute type search results)", (TypeSearchInfo typeSearchInfo, Type[] target) => (Type)null);
			ExcludeUnityDrawers = new HashSet<string> { "HeaderDrawer", "DelayedDrawer", "MultilineDrawer", "RangeDrawer", "SpaceDrawer", "TextAreaDrawer", "ColorUsageDrawer" };
			InvalidAttributeTypeSearchResults = new Dictionary<Type, TypeSearchResult[]>(FastTypeComparer.Instance);
			DecoratorDrawerCreatePropertyGUIMethod = typeof(DecoratorDrawer).GetMethod("CreatePropertyGUI", BindingFlags.Instance | BindingFlags.Public);
			using (SimpleProfiler.Section("DrawerUtilities"))
			{
				if (!SupportsUnityDrawers)
				{
					Debug.LogWarning("Could not find internal fields 'm_Type' and/or 'm_UseForChildren' in type CustomPropertyDrawer in this version of Unity; support for legacy Unity PropertyDrawers and DecoratorDrawers has been disabled in Odin's inspector. Please report this on Odin's issue tracker.");
				}
				IList<Type> odinTypes = TypeCache.GetTypesDerivedFrom(typeof(OdinDrawer));
				object obj;
				if (!SupportsUnityDrawers)
				{
					obj = null;
				}
				else
				{
					IList<Type> list = TypeCache.GetTypesDerivedFrom(typeof(GUIDrawer));
					obj = list;
				}
				IList<Type> unityTypes = (IList<Type>)obj;
				int allDrawerCountGuess = odinTypes.Count;
				if (unityTypes != null)
				{
					allDrawerCountGuess += unityTypes.Count;
					allDrawerCountGuess += 50;
				}
				AllDrawerTypes = new List<DrawerAndPriority>(allDrawerCountGuess);
				foreach (Type type in odinTypes)
				{
					if (!type.IsAbstract)
					{
						ProcessDrawerType(type, isOdin: true, isUnity: false);
					}
				}
				if (SupportsUnityDrawers)
				{
					foreach (Type type2 in unityTypes)
					{
						if (!type2.IsAbstract)
						{
							ProcessDrawerType(type2, isOdin: false, isUnity: true);
						}
					}
				}
				SearchIndex.MatchRules.Add(new TypeMatchRule("Unity Drawer Generic Target Matcher", delegate(TypeSearchInfo typeSearchInfo, Type[] targets)
				{
					if (targets.Length != 1)
					{
						return (Type)null;
					}
					if (!typeSearchInfo.Targets[0].IsGenericTypeDefinition)
					{
						return (Type)null;
					}
					Type genericTypeDefinition = typeSearchInfo.MatchType.GetGenericTypeDefinition();
					bool flag = genericTypeDefinition == typeof(AbstractTypeUnityPropertyDrawer<, , >);
					bool flag2 = genericTypeDefinition == typeof(UnityPropertyDrawer<, >);
					if (!(flag || flag2))
					{
						return (Type)null;
					}
					if (flag)
					{
						if (targets[0].ImplementsOpenGenericType(typeSearchInfo.Targets[0]))
						{
							Type[] genericArguments = typeSearchInfo.MatchType.GetGenericArguments();
							return typeSearchInfo.MatchType.GetGenericTypeDefinition().MakeGenericType(genericArguments[0], targets[0], targets[0]);
						}
					}
					else
					{
						if (!targets[0].IsGenericType)
						{
							return (Type)null;
						}
						if (targets[0].GetGenericTypeDefinition() == typeSearchInfo.Targets[0])
						{
							Type[] genericArguments2 = typeSearchInfo.MatchType.GetGenericArguments();
							return typeSearchInfo.MatchType.GetGenericTypeDefinition().MakeGenericType(genericArguments2[0], targets[0]);
						}
					}
					return (Type)null;
				}));
				for (int i = 0; i < AllDrawerTypes.Count; i++)
				{
					Type type3 = AllDrawerTypes[i].Drawer;
					TypeSearchInfo info = new TypeSearchInfo
					{
						MatchType = type3,
						Priority = AllDrawerTypes.Count - i,
						Targets = null,
						TargetCategories = null
					};
					Type[] args;
					if ((args = type3.GetArgumentsOfInheritedOpenGenericClass(typeof(OdinValueDrawer<>))) != null)
					{
						info.Targets = args;
						info.TargetCategories = TypeSearchIndex.ValueMatchCategoryArray;
					}
					else if (type3.ImplementsOpenGenericClass(typeof(OdinAttributeDrawer<>)))
					{
						if ((args = type3.GetArgumentsOfInheritedOpenGenericClass(typeof(OdinAttributeDrawer<, >))) != null)
						{
							info.Targets = args;
							info.TargetCategories = TypeSearchIndex.AttributeValueMatchCategoryArray;
							InvalidAttributeTargetUtility.RegisterValidAttributeTarget(info.Targets[0], info.Targets[1]);
						}
						else
						{
							info.Targets = type3.GetArgumentsOfInheritedOpenGenericClass(typeof(OdinAttributeDrawer<>));
							info.TargetCategories = TypeSearchIndex.AttributeMatchCategoryArray;
						}
					}
					else if ((args = type3.GetArgumentsOfInheritedOpenGenericClass(typeof(OdinGroupDrawer<>))) != null)
					{
						info.Targets = args;
						info.TargetCategories = TypeSearchIndex.AttributeMatchCategoryArray;
					}
					else if (!type3.IsFullyConstructedGenericType())
					{
						info.Targets = type3.GetGenericArguments();
					}
					info.Targets = info.Targets ?? Type.EmptyTypes;
					SearchIndex.AddIndexedTypeUnsorted(info);
				}
				Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
				foreach (Assembly assembly in assemblies)
				{
					object[] array = assembly.SafeGetCustomAttributes(typeof(StaticInitializeBeforeDrawingAttribute), inherit: false);
					foreach (object attr in array)
					{
						StaticInitializeBeforeDrawingAttribute castAttr = attr as StaticInitializeBeforeDrawingAttribute;
						if (castAttr.Types == null)
						{
							continue;
						}
						Type[] types = castAttr.Types;
						foreach (Type type4 in types)
						{
							if (!(type4 == null))
							{
								RuntimeHelpers.RunClassConstructor(type4.TypeHandle);
							}
						}
					}
				}
			}
		}

		private static bool FastStartsWith(string str, string startsWith)
		{
			if (startsWith.Length > str.Length)
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

		private static void InsertSortedIntoAllDrawerTypes(Type drawer)
		{
			string name = drawer.Name;
			DrawerPriority priority = GetDrawerPriority(drawer);
			int left = 0;
			int right = AllDrawerTypes.Count - 1;
			int current = 0;
			int compare = 0;
			while (left <= right)
			{
				current = (left + right) / 2;
				DrawerAndPriority middleDrawer = AllDrawerTypes[current];
				compare = priority.CompareTo(middleDrawer.Priority);
				if (compare == 0)
				{
					compare = -name.CompareTo(middleDrawer.Name);
				}
				if (compare < 0)
				{
					left = current + 1;
					continue;
				}
				if (compare <= 0)
				{
					break;
				}
				right = current - 1;
			}
			if (compare == 0)
			{
				for (int count = AllDrawerTypes.Count; current + 1 < count; current++)
				{
					DrawerAndPriority next = AllDrawerTypes[current + 1];
					if (priority > next.Priority)
					{
						current++;
						break;
					}
				}
			}
			else if (compare < 0)
			{
				current++;
			}
			AllDrawerTypes.Insert(current, new DrawerAndPriority
			{
				Drawer = drawer,
				Priority = priority,
				Name = name
			});
		}

		private static void ProcessDrawerType(Type type, bool isOdin, bool isUnity)
		{
			if ((!isOdin && !isUnity) || (isUnity && !SupportsUnityDrawers) || type.IsDefined(typeof(OdinDontRegisterAttribute), inherit: false))
			{
				return;
			}
			string ns = type.Namespace;
			if (ns != null && FastStartsWith(ns, "Unity") && ExcludeUnityDrawers.Contains(type.Name))
			{
				return;
			}
			if (isOdin)
			{
				InsertSortedIntoAllDrawerTypes(type);
			}
			else
			{
				if (type.IsGenericTypeDefinition || type.GetConstructor(Type.EmptyTypes) == null)
				{
					return;
				}
				bool isPropertyDrawer = typeof(PropertyDrawer).IsAssignableFrom(type);
				bool isDecoratorDrawer = !isPropertyDrawer && typeof(DecoratorDrawer).IsAssignableFrom(type);
				if (!isPropertyDrawer && !isDecoratorDrawer)
				{
					return;
				}
				object[] customPropertyDrawerAttributes = type.GetCustomAttributes(typeof(CustomPropertyDrawer), inherit: false);
				foreach (object attribute in customPropertyDrawerAttributes)
				{
					Type drawnType = CustomPropertyDrawerTypeField.GetValue(attribute) as Type;
					if (!(drawnType == null))
					{
						bool isPropertyAttribute = typeof(PropertyAttribute).IsAssignableFrom(drawnType);
						if (!isDecoratorDrawer || isPropertyAttribute)
						{
							bool useForChildren = (bool)CustomPropertyDrawerUseForChildrenField.GetValue(attribute);
							Type wrapper = (isPropertyDrawer ? (isPropertyAttribute ? ((!useForChildren && !drawnType.IsAbstract) ? typeof(UnityPropertyAttributeDrawer<, , >).MakeGenericType(type, drawnType, typeof(PropertyAttribute)) : typeof(UnityPropertyAttributeDrawer<, , >).MakeGenericType(type, UnityPropertyAttributeDrawer_TArg1, drawnType)) : ((!useForChildren && !drawnType.IsAbstract) ? typeof(UnityPropertyDrawer<, >).MakeGenericType(type, drawnType) : ((!drawnType.IsGenericTypeDefinition) ? typeof(AbstractTypeUnityPropertyDrawer<, , >).MakeGenericType(type, drawnType, AbstractTypeUnityPropertyDrawer_TArg2) : typeof(AbstractTypeUnityPropertyDrawer<, , >).MakeGenericType(type, drawnType, drawnType)))) : ((!useForChildren && !drawnType.IsAbstract) ? typeof(UnityDecoratorAttributeDrawer<, , >).MakeGenericType(type, drawnType, typeof(PropertyAttribute)) : typeof(UnityDecoratorAttributeDrawer<, , >).MakeGenericType(type, UnityDecoratorAttributeDrawer_TArg1, drawnType)));
							InsertSortedIntoAllDrawerTypes(wrapper);
						}
					}
				}
			}
		}

		public static void GetDefaultPropertyDrawers(InspectorProperty property, ref TypeSearchResult[] resultArray, ref int resultCount)
		{
			resultCount = 0;
			int queryCount = 0;
			CachedQueryResultArray[queryCount++] = SearchIndex.GetMatches(Type.EmptyTypes, TypeSearchIndex.EmptyCategoryArray);
			if (property.ValueEntry != null)
			{
				CachedQueryResultArray[queryCount++] = SearchIndex.GetMatches(property.ValueEntry.TypeOfValue, TargetMatchCategory.Value);
			}
			int maxNeededSize = 2 + property.Attributes.Count * 3;
			while (CachedQueryResultArray.Length <= maxNeededSize)
			{
				ExpandArray(ref CachedQueryResultArray);
			}
			for (int i = 0; i < property.Attributes.Count; i++)
			{
				Type attr = property.Attributes[i].GetType();
				CachedQueryResultArray[queryCount++] = SearchIndex.GetMatches(attr, TargetMatchCategory.Attribute);
				if (property.ValueEntry != null)
				{
					CachedQueryResultArray[queryCount++] = SearchIndex.GetMatches(attr, property.ValueEntry.TypeOfValue, TargetMatchCategory.Attribute, TargetMatchCategory.Value);
					if (InvalidAttributeTargetUtility.ShowInvalidAttributeErrorFor(property, attr))
					{
						CachedQueryResultArray[queryCount++] = GetInvalidAttributeTypeSearchResult(attr);
					}
				}
			}
			TypeSearchResult[] finalResults = TypeSearchIndex.GetCachedMergedQueryResults(CachedQueryResultArray, queryCount);
			for (int j = 0; j < finalResults.Length; j++)
			{
				TypeSearchResult result = finalResults[j];
				if (DrawerTypeCanDrawProperty(result.MatchedType, property))
				{
					if (resultCount == resultArray.Length)
					{
						ExpandArray(ref resultArray);
					}
					resultArray[resultCount++] = finalResults[j];
				}
			}
		}

		private static void ExpandArray<T>(ref T[] array)
		{
			T[] newArray = new T[array.Length * 2];
			for (int i = 0; i < array.Length; i++)
			{
				newArray[i] = array[i];
			}
			array = newArray;
		}

		private static TypeSearchResult[] GetInvalidAttributeTypeSearchResult(Type attr)
		{
			if (!InvalidAttributeTypeSearchResults.TryGetValue(attr, out var result))
			{
				result = new TypeSearchResult[1]
				{
					new TypeSearchResult
					{
						MatchedInfo = new TypeSearchInfo
						{
							MatchType = typeof(InvalidAttributeNotificationDrawer<>),
							Priority = double.MaxValue,
							Targets = Type.EmptyTypes
						},
						MatchedRule = InvalidAttributeRule,
						MatchedTargets = Type.EmptyTypes,
						MatchedType = typeof(InvalidAttributeNotificationDrawer<>).MakeGenericType(attr)
					}
				};
				InvalidAttributeTypeSearchResults.Add(attr, result);
			}
			return result;
		}

		/// <summary>
		/// Gets the priority of a given drawer type.
		/// </summary>
		public static DrawerPriority GetDrawerPriority(Type drawerType)
		{
			if (!DrawerTypePriorityLookup.TryGetValue(drawerType, out var result))
			{
				result = CalculateDrawerPriority(drawerType);
				DrawerTypePriorityLookup[drawerType] = result;
			}
			return result;
		}

		private static DrawerPriority CalculateDrawerPriority(Type drawerType)
		{
			DrawerPriority priority = DrawerPriority.AutoPriority;
			DrawerPriority adjustment = default(DrawerPriority);
			DrawerPriorityAttribute priorityAttribute = null;
			if (DrawerIsUnityAlias(drawerType))
			{
				Type[] drawerArgs = drawerType.GetGenericArguments();
				Type innerDrawer = drawerArgs[0];
				if (innerDrawer.IsDefined(typeof(DrawerPriorityAttribute), inherit: false))
				{
					priorityAttribute = innerDrawer.GetCustomAttribute<DrawerPriorityAttribute>(inherit: false);
				}
				if (priorityAttribute == null)
				{
					AssemblyCategory flag = AssemblyUtilities.GetAssemblyCategory(innerDrawer.Assembly);
					Type drawnType = drawerArgs[1];
					if ((flag & AssemblyCategory.UnityEngine) != AssemblyCategory.None)
					{
						adjustment.Value -= 0.1;
					}
					if (drawnType.IsInterface)
					{
						adjustment.Value -= 0.0003;
					}
					else if (drawnType.IsAbstract)
					{
						adjustment.Value -= 0.0002;
					}
				}
			}
			if (priorityAttribute == null && drawerType.IsDefined(typeof(DrawerPriorityAttribute), inherit: false))
			{
				priorityAttribute = drawerType.GetCustomAttribute<DrawerPriorityAttribute>(inherit: false);
			}
			if (priorityAttribute != null)
			{
				priority = priorityAttribute.Priority;
			}
			if (priority == DrawerPriority.AutoPriority)
			{
				priority = ((!drawerType.ImplementsOpenGenericClass(typeof(OdinAttributeDrawer<>))) ? DrawerPriority.ValuePriority : DrawerPriority.AttributePriority);
				if (drawerType.Assembly == typeof(OdinEditor).Assembly)
				{
					priority.Value -= 0.001;
				}
			}
			return priority + adjustment;
		}

		private static bool DrawerIsUnityAlias(Type drawerType)
		{
			if (!drawerType.IsGenericType || drawerType.IsGenericTypeDefinition)
			{
				return false;
			}
			Type definition = drawerType.GetGenericTypeDefinition();
			if (!(definition == typeof(UnityPropertyDrawer<, >)) && !(definition == typeof(UnityPropertyAttributeDrawer<, , >)) && !(definition == typeof(UnityDecoratorAttributeDrawer<, , >)))
			{
				return definition == typeof(AbstractTypeUnityPropertyDrawer<, , >);
			}
			return true;
		}

		public static bool DrawerTypeCanDrawProperty(Type drawerType, InspectorProperty property)
		{
			OdinDrawer drawer = GetCachedUninitializedDrawer(drawerType);
			return drawer.CanDrawProperty(property);
		}

		public static OdinDrawer GetCachedUninitializedDrawer(Type drawerType)
		{
			if (!UninitializedDrawers.TryGetValue(drawerType, out var result))
			{
				result = (OdinDrawer)FormatterServices.GetUninitializedObject(drawerType);
				UninitializedDrawers[drawerType] = result;
			}
			return result;
		}

		public static bool HasAttributeDrawer(Type attributeType)
		{
			return (from d in AllDrawerTypes
				select d.Drawer.GetBaseClasses().FirstOrDefault((Type x) => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(OdinAttributeDrawer<>)) into d
				where d != null
				select d).Any((Type d) => d.GetGenericArguments()[0] == attributeType);
		}
	}
}
