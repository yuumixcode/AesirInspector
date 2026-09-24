using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using Sirenix.OdinInspector.Editor.TypeSearch;
using Sirenix.Utilities;

namespace Sirenix.OdinInspector.Editor
{
	public class DefaultStateUpdaterLocator : StateUpdaterLocator
	{
		public static readonly DefaultStateUpdaterLocator Instance;

		public static readonly TypeSearchIndex SearchIndex;

		private static readonly Dictionary<Type, Func<StateUpdater>> FastCreators;

		private static readonly Dictionary<Type, StateUpdater> EmptyInstances;

		private static readonly StateUpdater[] EmptyResult;

		private static TypeSearchResult[][] CachedQueryResultArray;

		private static StateUpdater[] CachedResultBuilderArray;

		static DefaultStateUpdaterLocator()
		{
			Instance = new DefaultStateUpdaterLocator();
			SearchIndex = new TypeSearchIndex
			{
				MatchedTypeLogName = "state updater"
			};
			FastCreators = new Dictionary<Type, Func<StateUpdater>>(FastTypeComparer.Instance);
			EmptyInstances = new Dictionary<Type, StateUpdater>(FastTypeComparer.Instance);
			EmptyResult = new StateUpdater[0];
			CachedQueryResultArray = new TypeSearchResult[32][];
			CachedResultBuilderArray = new StateUpdater[16];
			List<Assembly> assemblies = ResolverUtilities.GetResolverAssemblies();
			for (int i = 0; i < assemblies.Count; i++)
			{
				object[] attributes;
				try
				{
					attributes = assemblies[i].SafeGetCustomAttributes(typeof(RegisterStateUpdaterAttribute), inherit: false);
				}
				catch
				{
					continue;
				}
				for (int j = 0; j < attributes.Length; j++)
				{
					RegisterStateUpdaterAttribute attribute = (RegisterStateUpdaterAttribute)attributes[j];
					if (!attribute.Type.IsAbstract && typeof(StateUpdater).IsAssignableFrom(attribute.Type))
					{
						IndexType(attribute.Type, attribute.Priority);
					}
				}
			}
		}

		private static void IndexType(Type type, double priority)
		{
			TypeSearchInfo result = new TypeSearchInfo
			{
				MatchType = type,
				Priority = priority
			};
			if (type.ImplementsOpenGenericType(typeof(AttributeStateUpdater<>)))
			{
				if (type.ImplementsOpenGenericType(typeof(AttributeStateUpdater<, >)))
				{
					result.Targets = type.GetArgumentsOfInheritedOpenGenericType(typeof(AttributeStateUpdater<, >));
					result.TargetCategories = TypeSearchIndex.AttributeValueMatchCategoryArray;
				}
				else
				{
					result.Targets = type.GetArgumentsOfInheritedOpenGenericType(typeof(AttributeStateUpdater<>));
					result.TargetCategories = TypeSearchIndex.AttributeMatchCategoryArray;
				}
			}
			else if (type.ImplementsOpenGenericType(typeof(ValueStateUpdater<>)))
			{
				result.Targets = type.GetArgumentsOfInheritedOpenGenericType(typeof(ValueStateUpdater<>));
				result.TargetCategories = TypeSearchIndex.ValueMatchCategoryArray;
			}
			else
			{
				result.Targets = Type.EmptyTypes;
				result.TargetCategories = TypeSearchIndex.EmptyCategoryArray;
			}
			SearchIndex.AddIndexedType(result);
		}

		public override StateUpdater[] GetStateUpdaters(InspectorProperty property)
		{
			int queryCount = 0;
			CachedQueryResultArray[queryCount++] = SearchIndex.GetMatches(Type.EmptyTypes, TypeSearchIndex.EmptyCategoryArray);
			IPropertyValueEntry valueEntry = property.ValueEntry;
			if (valueEntry != null)
			{
				CachedQueryResultArray[queryCount++] = SearchIndex.GetMatches(valueEntry.TypeOfValue, TargetMatchCategory.Value);
			}
			int maxNeededSize = 2 + property.Attributes.Count * 2;
			while (CachedQueryResultArray.Length <= maxNeededSize)
			{
				ExpandArray(ref CachedQueryResultArray);
			}
			for (int i = 0; i < property.Attributes.Count; i++)
			{
				Type attr = property.Attributes[i].GetType();
				CachedQueryResultArray[queryCount++] = SearchIndex.GetMatches(attr, TargetMatchCategory.Attribute);
				if (valueEntry != null)
				{
					CachedQueryResultArray[queryCount++] = SearchIndex.GetMatches(attr, valueEntry.TypeOfValue, TargetMatchCategory.Attribute, TargetMatchCategory.Value);
				}
			}
			TypeSearchResult[] finalResults = TypeSearchIndex.GetCachedMergedQueryResults(CachedQueryResultArray, queryCount);
			int resultCount = 0;
			while (CachedResultBuilderArray.Length < finalResults.Length)
			{
				ExpandArray(ref CachedResultBuilderArray);
			}
			for (int j = 0; j < finalResults.Length; j++)
			{
				TypeSearchResult result = finalResults[j];
				if (GetEmptyUpdaterInstance(result.MatchedType).CanUpdateProperty(property))
				{
					CachedResultBuilderArray[resultCount++] = CreateStateUpdater(result.MatchedType);
				}
			}
			if (resultCount == 0)
			{
				return EmptyResult;
			}
			StateUpdater[] finalResult = new StateUpdater[resultCount];
			for (int k = 0; k < resultCount; k++)
			{
				finalResult[k] = CachedResultBuilderArray[k];
				CachedResultBuilderArray[k] = null;
			}
			return finalResult;
		}

		public StateUpdater GetEmptyUpdaterInstance(Type type)
		{
			if (!EmptyInstances.TryGetValue(type, out var result))
			{
				result = (StateUpdater)FormatterServices.GetUninitializedObject(type);
				EmptyInstances[type] = result;
			}
			return result;
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

		private static StateUpdater CreateStateUpdater(Type type)
		{
			if (!FastCreators.TryGetValue(type, out var fastCreator))
			{
				ConstructorInfo constructor = type.GetConstructor(Type.EmptyTypes);
				DynamicMethod method = new DynamicMethod("FastCreator", typeof(StateUpdater), Type.EmptyTypes);
				ILGenerator il = method.GetILGenerator();
				il.Emit(OpCodes.Newobj, constructor);
				il.Emit(OpCodes.Ret);
				fastCreator = (Func<StateUpdater>)method.CreateDelegate(typeof(Func<StateUpdater>));
				FastCreators.Add(type, fastCreator);
			}
			return fastCreator();
		}
	}
}
