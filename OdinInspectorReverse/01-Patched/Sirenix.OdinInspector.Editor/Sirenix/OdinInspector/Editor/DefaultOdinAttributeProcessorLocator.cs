using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.OdinInspector.Editor.TypeSearch;
using Sirenix.Utilities;
using UnityEditor;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// Default implementation and the version that will be used when no other OdinAttributeProcessorLocator instance have been given to a PropertyTree.
	/// This implementation will find all AttributeProcessor definitions not marked with the <see cref="T:Sirenix.OdinInspector.Editor.OdinDontRegisterAttribute" />.
	/// </summary>
	public sealed class DefaultOdinAttributeProcessorLocator : OdinAttributeProcessorLocator
	{
		/// <summary>
		/// Singleton instance of the DefaultOdinAttributeProcessorLocator class.
		/// </summary>
		public static readonly DefaultOdinAttributeProcessorLocator Instance;

		/// <summary>
		/// Type search index used for matching <see cref="T:Sirenix.OdinInspector.Editor.OdinAttributeProcessor" /> to properties.
		/// </summary>
		public static readonly TypeSearchIndex SearchIndex;

		private static Dictionary<Type, OdinAttributeProcessor> ResolverInstanceMap;

		private List<TypeSearchResult[]> CachedMatchesList = new List<TypeSearchResult[]>();

		static DefaultOdinAttributeProcessorLocator()
		{
			Instance = new DefaultOdinAttributeProcessorLocator();
			SearchIndex = new TypeSearchIndex();
			ResolverInstanceMap = new Dictionary<Type, OdinAttributeProcessor>(FastTypeComparer.Instance);
			using (SimpleProfiler.Section("DefaultOdinAttributeProcessorLocator - TypeCache"))
			{
				foreach (Type type in TypeCache.GetTypesDerivedFrom(typeof(OdinAttributeProcessor)))
				{
					if (!type.IsAbstract && !type.IsDefined<OdinDontRegisterAttribute>())
					{
						TypeSearchInfo info = new TypeSearchInfo
						{
							MatchType = type,
							Priority = ResolverUtilities.GetResolverPriority(type)
						};
						Type[] args;
						if ((args = type.GetArgumentsOfInheritedOpenGenericClass(typeof(OdinAttributeProcessor<>))) != null)
						{
							info.Targets = args;
							info.TargetCategories = TypeSearchIndex.ValueMatchCategoryArray;
						}
						else
						{
							info.Targets = Type.EmptyTypes;
							info.TargetCategories = TypeSearchIndex.EmptyCategoryArray;
						}
						SearchIndex.AddIndexedType(info);
					}
				}
			}
		}

		private static void IndexType(Type type)
		{
		}

		/// <summary>
		/// Gets a list of <see cref="T:Sirenix.OdinInspector.Editor.OdinAttributeProcessor" /> to process attributes for the specified child member of the parent property.
		/// </summary>
		/// <param name="parentProperty">The parent of the member.</param>
		/// <param name="member">Child member of the parent property.</param>
		/// <returns>List of <see cref="T:Sirenix.OdinInspector.Editor.OdinAttributeProcessor" /> to process attributes for the specified member.</returns>
		public override List<OdinAttributeProcessor> GetChildProcessors(InspectorProperty parentProperty, MemberInfo member)
		{
			CachedMatchesList.Clear();
			CachedMatchesList.Add(SearchIndex.GetMatches(Type.EmptyTypes, TypeSearchIndex.EmptyCategoryArray));
			if (parentProperty.ValueEntry != null)
			{
				CachedMatchesList.Add(SearchIndex.GetMatches(parentProperty.ValueEntry.TypeOfValue, TargetMatchCategory.Value));
			}
			TypeSearchResult[] results = TypeSearchIndex.GetCachedMergedQueryResults(CachedMatchesList);
			List<OdinAttributeProcessor> processors = new List<OdinAttributeProcessor>(results.Length);
			for (int i = 0; i < results.Length; i++)
			{
				TypeSearchResult result = results[i];
				OdinAttributeProcessor resolver = GetResolverInstance(result.MatchedType);
				if (resolver.CanProcessChildMemberAttributes(parentProperty, member))
				{
					processors.Add(resolver);
				}
			}
			return processors;
		}

		/// <summary>
		/// Gets a list of <see cref="T:Sirenix.OdinInspector.Editor.OdinAttributeProcessor" /> to process attributes for the specified property.
		/// </summary>
		/// <param name="property">The property to find attribute porcessors for.</param>
		/// <returns>List of <see cref="T:Sirenix.OdinInspector.Editor.OdinAttributeProcessor" /> to process attributes for the speicied member.</returns>
		public override List<OdinAttributeProcessor> GetSelfProcessors(InspectorProperty property)
		{
			CachedMatchesList.Clear();
			CachedMatchesList.Add(SearchIndex.GetMatches(Type.EmptyTypes, TypeSearchIndex.EmptyCategoryArray));
			if (property.ValueEntry != null)
			{
				CachedMatchesList.Add(SearchIndex.GetMatches(property.ValueEntry.TypeOfValue, TargetMatchCategory.Value));
			}
			TypeSearchResult[] results = TypeSearchIndex.GetCachedMergedQueryResults(CachedMatchesList);
			List<OdinAttributeProcessor> processors = new List<OdinAttributeProcessor>(results.Length);
			for (int i = 0; i < results.Length; i++)
			{
				TypeSearchResult result = results[i];
				OdinAttributeProcessor resolver = GetResolverInstance(result.MatchedType);
				if (resolver.CanProcessSelfAttributes(property))
				{
					processors.Add(resolver);
				}
			}
			return processors;
		}

		private static OdinAttributeProcessor GetResolverInstance(Type type)
		{
			if (!ResolverInstanceMap.TryGetValue(type, out var result))
			{
				result = (OdinAttributeProcessor)Activator.CreateInstance(type);
				ResolverInstanceMap.Add(type, result);
			}
			return result;
		}
	}
}
