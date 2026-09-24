using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Sirenix.OdinInspector.Editor.Internal;
using Sirenix.OdinInspector.Editor.TypeSearch;
using Sirenix.Utilities;
using UnityEditor;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// Default implementation and the version that will be used by <see cref="T:Sirenix.OdinInspector.Editor.PropertyTree" /> if no other <see cref="T:Sirenix.OdinInspector.Editor.OdinPropertyResolver" /> instance have been specified.
	/// </summary>
	public class DefaultOdinPropertyResolverLocator : OdinPropertyResolverLocator
	{
		/// <summary>
		/// Singleton instance of <see cref="T:Sirenix.OdinInspector.Editor.DefaultOdinPropertyResolverLocator" />.
		/// </summary>
		public static readonly DefaultOdinPropertyResolverLocator Instance;

		public static readonly TypeSearchIndex SearchIndex;

		private static Dictionary<Type, OdinPropertyResolver> resolverEmptyInstanceMap;

		private static readonly List<TypeSearchResult[]> QueryResultsList;

		private static readonly List<TypeSearchResult> MergedSearchResultsList;

		static DefaultOdinPropertyResolverLocator()
		{
			Instance = new DefaultOdinPropertyResolverLocator();
			SearchIndex = new TypeSearchIndex();
			resolverEmptyInstanceMap = new Dictionary<Type, OdinPropertyResolver>(FastTypeComparer.Instance);
			QueryResultsList = new List<TypeSearchResult[]>();
			MergedSearchResultsList = new List<TypeSearchResult>();
			using (SimpleProfiler.Section("DefaultOdinPropertyResolverLocator Type Cache"))
			{
				foreach (Type type in TypeCache.GetTypesDerivedFrom(typeof(OdinPropertyResolver)))
				{
					if (!type.IsAbstract && !type.IsDefined<OdinDontRegisterAttribute>())
					{
						IndexType(type);
					}
				}
			}
		}

		private static void IndexType(Type type)
		{
			TypeSearchInfo result = new TypeSearchInfo
			{
				MatchType = type,
				Priority = ResolverUtilities.GetResolverPriority(type)
			};
			if (type.ImplementsOpenGenericType(typeof(OdinPropertyResolver<>)))
			{
				if (type.ImplementsOpenGenericType(typeof(OdinPropertyResolver<, >)))
				{
					result.Targets = type.GetArgumentsOfInheritedOpenGenericType(typeof(OdinPropertyResolver<, >));
					result.TargetCategories = TypeSearchIndex.ValueAttributeMatchCategoryArray;
				}
				else
				{
					result.Targets = type.GetArgumentsOfInheritedOpenGenericType(typeof(OdinPropertyResolver<>));
					result.TargetCategories = TypeSearchIndex.ValueMatchCategoryArray;
				}
			}
			else
			{
				result.Targets = Type.EmptyTypes;
				result.TargetCategories = TypeSearchIndex.EmptyCategoryArray;
			}
			SearchIndex.AddIndexedType(result);
		}

		/// <summary>
		/// Gets an <see cref="T:Sirenix.OdinInspector.Editor.OdinPropertyResolver" /> instance for the specified property.
		/// </summary>
		/// <param name="property">The property to get an <see cref="T:Sirenix.OdinInspector.Editor.OdinPropertyResolver" /> instance for.</param>
		/// <returns>An instance of <see cref="T:Sirenix.OdinInspector.Editor.OdinPropertyResolver" /> to resolver the specified property.</returns>
		public override OdinPropertyResolver GetResolver(InspectorProperty property)
		{
			PropertyTree tree = property.Tree;
			bool isRoot = property.IsTreeRoot;
			if (isRoot && tree.IsStatic)
			{
				return OdinPropertyResolver.Create(typeof(StaticRootPropertyResolver<>).MakeGenericType(property.ValueEntry.TypeOfValue), property);
			}
			List<TypeSearchResult[]> queries = QueryResultsList;
			queries.Clear();
			queries.Add(SearchIndex.GetMatches(Type.EmptyTypes, TypeSearchIndex.EmptyCategoryArray));
			Type typeOfValue = ((property.ValueEntry != null) ? property.ValueEntry.TypeOfValue : null);
			if (typeOfValue != null)
			{
				queries.Add(SearchIndex.GetMatches(typeOfValue, TargetMatchCategory.Value));
				for (int i = 0; i < property.Attributes.Count; i++)
				{
					queries.Add(SearchIndex.GetMatches(typeOfValue, property.Attributes[i].GetType(), TargetMatchCategory.Value, TargetMatchCategory.Attribute));
				}
			}
			TypeSearchIndex.MergeQueryResultsIntoList(queries, MergedSearchResultsList);
			Type resolverToUseType = null;
			for (int j = 0; j < MergedSearchResultsList.Count; j++)
			{
				TypeSearchResult info = MergedSearchResultsList[j];
				if (GetEmptyResolverInstance(info.MatchedType).CanResolveForPropertyFilter(property))
				{
					resolverToUseType = info.MatchedType;
					break;
				}
			}
			if (isRoot && tree.IsDesignerTree && DesignerUtils.IsPropertyResolverSupported(resolverToUseType))
			{
				return OdinPropertyResolver.Create(typeof(DesignerEditorPropertyResolver<object>), property);
			}
			if (resolverToUseType != null)
			{
				return OdinPropertyResolver.Create(resolverToUseType, property);
			}
			return OdinPropertyResolver.Create<EmptyPropertyResolver>(property);
		}

		public OdinPropertyResolver GetEmptyResolverInstance(Type resolverType)
		{
			if (!resolverEmptyInstanceMap.TryGetValue(resolverType, out var result))
			{
				result = (OdinPropertyResolver)FormatterServices.GetUninitializedObject(resolverType);
				resolverEmptyInstanceMap[resolverType] = result;
			}
			return result;
		}
	}
}
