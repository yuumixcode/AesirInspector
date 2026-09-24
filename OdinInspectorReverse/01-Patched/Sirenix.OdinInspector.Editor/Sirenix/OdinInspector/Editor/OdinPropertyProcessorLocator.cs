using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Sirenix.OdinInspector.Editor.TypeSearch;
using Sirenix.Utilities;
using UnityEditor;

namespace Sirenix.OdinInspector.Editor
{
	public static class OdinPropertyProcessorLocator
	{
		private static readonly Dictionary<Type, OdinPropertyProcessor> EmptyInstances;

		public static readonly TypeSearchIndex SearchIndex;

		private static readonly List<TypeSearchResult[]> CachedQueryList;

		static OdinPropertyProcessorLocator()
		{
			EmptyInstances = new Dictionary<Type, OdinPropertyProcessor>(FastTypeComparer.Instance);
			SearchIndex = new TypeSearchIndex
			{
				MatchedTypeLogName = "member property processor"
			};
			CachedQueryList = new List<TypeSearchResult[]>();
			using (SimpleProfiler.Section("OdinPropertyProcessorLocator Type Cache"))
			{
				foreach (Type type in TypeCache.GetTypesDerivedFrom(typeof(OdinPropertyProcessor)))
				{
					if (!type.IsAbstract && !type.IsDefined<OdinDontRegisterAttribute>(inherit: false))
					{
						IndexType(type);
					}
				}
			}
		}

		private static void IndexType(Type type)
		{
			if (type.ImplementsOpenGenericClass(typeof(OdinPropertyProcessor<>)))
			{
				if (type.ImplementsOpenGenericClass(typeof(OdinPropertyProcessor<, >)))
				{
					SearchIndex.AddIndexedType(new TypeSearchInfo
					{
						MatchType = type,
						Targets = type.GetArgumentsOfInheritedOpenGenericClass(typeof(OdinPropertyProcessor<, >)),
						TargetCategories = TypeSearchIndex.ValueAttributeMatchCategoryArray,
						Priority = ResolverUtilities.GetResolverPriority(type)
					});
				}
				else
				{
					SearchIndex.AddIndexedType(new TypeSearchInfo
					{
						MatchType = type,
						Targets = type.GetArgumentsOfInheritedOpenGenericClass(typeof(OdinPropertyProcessor<>)),
						TargetCategories = TypeSearchIndex.ValueMatchCategoryArray,
						Priority = ResolverUtilities.GetResolverPriority(type)
					});
				}
			}
			else
			{
				SearchIndex.AddIndexedType(new TypeSearchInfo
				{
					MatchType = type,
					Targets = Type.EmptyTypes,
					TargetCategories = TypeSearchIndex.EmptyCategoryArray,
					Priority = ResolverUtilities.GetResolverPriority(type)
				});
			}
		}

		public static List<OdinPropertyProcessor> GetMemberProcessors(InspectorProperty property)
		{
			List<TypeSearchResult[]> queries = CachedQueryList;
			queries.Clear();
			queries.Add(SearchIndex.GetMatches(Type.EmptyTypes, TypeSearchIndex.EmptyCategoryArray));
			if (property.ValueEntry != null)
			{
				Type valueType = property.ValueEntry.TypeOfValue;
				queries.Add(SearchIndex.GetMatches(valueType, TargetMatchCategory.Value));
				for (int i = 0; i < property.Attributes.Count; i++)
				{
					queries.Add(SearchIndex.GetMatches(valueType, property.Attributes[i].GetType(), TargetMatchCategory.Value, TargetMatchCategory.Attribute));
				}
			}
			TypeSearchResult[] results = TypeSearchIndex.GetCachedMergedQueryResults(queries);
			List<OdinPropertyProcessor> processors = new List<OdinPropertyProcessor>();
			for (int j = 0; j < results.Length; j++)
			{
				TypeSearchResult result = results[j];
				if (GetEmptyInstance(result.MatchedType).CanProcessForProperty(property))
				{
					processors.Add(OdinPropertyProcessor.Create(result.MatchedType, property));
				}
			}
			return processors;
		}

		private static OdinPropertyProcessor GetEmptyInstance(Type type)
		{
			if (!EmptyInstances.TryGetValue(type, out var result))
			{
				result = (OdinPropertyProcessor)FormatterServices.GetUninitializedObject(type);
				EmptyInstances[type] = result;
			}
			return result;
		}
	}
}
