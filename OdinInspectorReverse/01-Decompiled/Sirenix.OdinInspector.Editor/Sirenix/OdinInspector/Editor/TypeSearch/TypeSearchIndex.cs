using System;
using System.Collections.Generic;
using Sirenix.Utilities;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.TypeSearch
{
	public sealed class TypeSearchIndex
	{
		private class ProcessedTypeSearchInfo
		{
			public TypeSearchInfo Info;

			public List<TypeMatcher> Matchers;
		}

		private class TypeArrayEqualityComparer : IEqualityComparer<Type[]>
		{
			public bool Equals(Type[] x, Type[] y)
			{
				if (x == y)
				{
					return true;
				}
				if (x == null || y == null)
				{
					return false;
				}
				if (x.Length != y.Length)
				{
					return false;
				}
				for (int i = 0; i < x.Length; i++)
				{
					if (x[i] != y[i])
					{
						return false;
					}
				}
				return true;
			}

			public int GetHashCode(Type[] obj)
			{
				if (obj == null)
				{
					return 0;
				}
				int result = 1;
				foreach (Type type in obj)
				{
					int typeHash = ((type == null) ? 1 : type.GetHashCode());
					result = 137 * result + (typeHash ^ (typeHash >> 16));
				}
				return result;
			}
		}

		public struct TypeMatchQuery
		{
			public Type[] Targets;

			public TargetMatchCategory[] Categories;
		}

		public class TypeMatchCacheSignatureEqualityComparer : IEqualityComparer<TypeMatchQuery>
		{
			private static readonly TypeArrayEqualityComparer typeArrayEqualityComparer = new TypeArrayEqualityComparer();

			public bool Equals(TypeMatchQuery x, TypeMatchQuery y)
			{
				if (!typeArrayEqualityComparer.Equals(x.Targets, y.Targets))
				{
					return false;
				}
				if (x.Categories == y.Categories)
				{
					return true;
				}
				if (x.Categories != null)
				{
					if (x.Categories.Length != y.Categories.Length)
					{
						return false;
					}
					for (int i = 0; i < x.Categories.Length; i++)
					{
						if (x.Categories[i] != y.Categories[i])
						{
							return false;
						}
					}
				}
				return true;
			}

			public int GetHashCode(TypeMatchQuery obj)
			{
				int hash = typeArrayEqualityComparer.GetHashCode(obj.Targets);
				if (obj.Categories != null)
				{
					for (int i = 0; i < obj.Categories.Length; i++)
					{
						hash ^= obj.Categories[i].GetHashCode();
					}
				}
				return hash;
			}
		}

		private struct QueryResult
		{
			public int CurrentIndex;

			public double CurrentPriority;

			public TypeSearchResult[] Result;

			public QueryResult(TypeSearchResult[] result)
			{
				Result = result;
				CurrentIndex = 0;
				CurrentPriority = result[0].MatchedInfo.Priority;
			}
		}

		private class MergeSignatureComparer : IEqualityComparer<MergeSignature>
		{
			public bool Equals(MergeSignature x, MergeSignature y)
			{
				if (x.Hash != y.Hash)
				{
					return false;
				}
				if (x.Results == y.Results)
				{
					return true;
				}
				int count = x.ResultCount;
				if (count != y.ResultCount)
				{
					return false;
				}
				for (int i = 0; i < count; i++)
				{
					if (x.Results[i] != y.Results[i])
					{
						return false;
					}
				}
				return true;
			}

			public int GetHashCode(MergeSignature obj)
			{
				return obj.Hash;
			}
		}

		private struct MergeSignature
		{
			public int Hash;

			public IList<TypeSearchResult[]> Results;

			public int ResultCount;

			public MergeSignature(IList<TypeSearchResult[]> results, int count)
			{
				Results = results;
				ResultCount = count;
				int result = 1;
				for (int i = 0; i < count; i++)
				{
					int hash = results[i].GetHashCode();
					result = 137 * result + (hash ^ (hash >> 16));
				}
				Hash = result;
			}

			public override int GetHashCode()
			{
				return Hash;
			}
		}

		public string MatchedTypeLogName = "matched type";

		public List<TypeMatchIndexingRule> IndexingRules = new List<TypeMatchIndexingRule>();

		public List<TypeMatchRule> MatchRules = new List<TypeMatchRule>();

		public Action<string, TypeSearchInfo> LogInvalidTypeInfo = delegate(string message, TypeSearchInfo info)
		{
			Debug.LogError(message);
		};

		private readonly List<ProcessedTypeSearchInfo> indexedTypes = new List<ProcessedTypeSearchInfo>();

		private readonly TypeMatchQuery CachedQuery1 = new TypeMatchQuery
		{
			Targets = new Type[1],
			Categories = new TargetMatchCategory[1]
		};

		private readonly TypeMatchQuery CachedQuery2 = new TypeMatchQuery
		{
			Targets = new Type[2],
			Categories = new TargetMatchCategory[2]
		};

		public static readonly TargetMatchCategory[] ValueMatchCategoryArray = new TargetMatchCategory[1] { TargetMatchCategory.Value };

		public static readonly TargetMatchCategory[] AttributeMatchCategoryArray = new TargetMatchCategory[1] { TargetMatchCategory.Attribute };

		public static readonly TargetMatchCategory[] AttributeValueMatchCategoryArray = new TargetMatchCategory[2]
		{
			TargetMatchCategory.Attribute,
			TargetMatchCategory.Value
		};

		public static readonly TargetMatchCategory[] ValueAttributeMatchCategoryArray = new TargetMatchCategory[2]
		{
			TargetMatchCategory.Value,
			TargetMatchCategory.Attribute
		};

		public static readonly TargetMatchCategory[] EmptyCategoryArray = new TargetMatchCategory[0];

		/// <summary>
		/// To safely change anything in the type cache, you must be holding this lock.
		/// </summary>
		public readonly object LOCK = new object();

		public List<TypeMatcherCreator> TypeMatcherCreators = new List<TypeMatcherCreator>();

		private Dictionary<TypeMatchQuery, TypeSearchResult[]> resultCache = new Dictionary<TypeMatchQuery, TypeSearchResult[]>(new TypeMatchCacheSignatureEqualityComparer());

		private static readonly List<QueryResult> CachedQueryResultList = new List<QueryResult>();

		private static readonly object STATIC_LOCK = new object();

		private static readonly Dictionary<MergeSignature, TypeSearchResult[]> KnownMergeSignatures = new Dictionary<MergeSignature, TypeSearchResult[]>(new MergeSignatureComparer());

		private static readonly List<TypeSearchResult> CachedFastMergeList = new List<TypeSearchResult>();

		private static readonly TypeSearchResult[] EmptyResultArray = new TypeSearchResult[0];

		public TypeSearchIndex(bool addDefaultValidationRules = true, bool addDefaultMatchRules = true)
		{
			if (addDefaultValidationRules)
			{
				AddDefaultIndexingRules();
			}
			if (addDefaultMatchRules)
			{
				AddDefaultMatchRules();
				AddDefaultMatchCreators();
			}
		}

		public static List<List<Type[]>> GetAllCachedMergeSignatures(TypeSearchIndex index)
		{
			lock (STATIC_LOCK)
			{
				List<List<Type[]>> result = new List<List<Type[]>>();
				foreach (MergeSignature mergeSignature in KnownMergeSignatures.Keys)
				{
					if (!IsMergeSignatureForIndex(mergeSignature, index))
					{
						continue;
					}
					List<Type[]> signatureList = new List<Type[]>();
					for (int i = 0; i < mergeSignature.ResultCount; i++)
					{
						TypeSearchResult[] searchResult = mergeSignature.Results[i];
						if (searchResult.Length == 0)
						{
							signatureList.Add(Type.EmptyTypes);
						}
						else
						{
							signatureList.Add(searchResult[0].MatchedTargets);
						}
					}
					result.Add(signatureList);
				}
				return result;
			}
		}

		private static bool IsMergeSignatureForIndex(MergeSignature signature, TypeSearchIndex index)
		{
			for (int i = 0; i < signature.ResultCount; i++)
			{
				TypeSearchResult[] resultSet = signature.Results[i];
				if (resultSet.Length != 0)
				{
					return resultSet[0].MatchedIndex == index;
				}
			}
			return false;
		}

		public static TypeSearchResult[] GetCachedMergedQueryResults(TypeSearchResult[][] results, int resultsCount)
		{
			switch (resultsCount)
			{
			case 0:
				return EmptyResultArray;
			case 1:
				return results[0];
			default:
				lock (STATIC_LOCK)
				{
					MergeSignature mergeSignature = new MergeSignature(results, resultsCount);
					if (KnownMergeSignatures.TryGetValue(mergeSignature, out var fastResultArray))
					{
						return fastResultArray;
					}
					List<TypeSearchResult> mergeIntoList = CachedFastMergeList;
					mergeIntoList.Clear();
					List<QueryResult> queries = CachedQueryResultList;
					queries.Clear();
					for (int i = 0; i < resultsCount; i++)
					{
						if (results[i].Length != 0)
						{
							queries.Add(new QueryResult(results[i]));
						}
					}
					int queriesCount = queries.Count;
					while (true)
					{
						double highestPriority = double.MinValue;
						int highestIndex = -1;
						for (int j = 0; j < queriesCount; j++)
						{
							QueryResult query = queries[j];
							if (query.CurrentIndex < query.Result.Length && query.CurrentPriority > highestPriority)
							{
								highestPriority = query.CurrentPriority;
								highestIndex = j;
							}
						}
						if (highestIndex == -1)
						{
							break;
						}
						QueryResult highest = queries[highestIndex];
						mergeIntoList.Add(highest.Result[highest.CurrentIndex]);
						highest.CurrentIndex++;
						if (highest.CurrentIndex < highest.Result.Length)
						{
							highest.CurrentPriority = highest.Result[highest.CurrentIndex].MatchedInfo.Priority;
						}
						queries[highestIndex] = highest;
					}
					mergeSignature.Results = new List<TypeSearchResult[]>(mergeSignature.Results);
					TypeSearchResult[] arr = mergeIntoList.ToArray();
					KnownMergeSignatures.Add(mergeSignature, arr);
					return arr;
				}
			}
		}

		public static TypeSearchResult[] GetCachedMergedQueryResults(List<TypeSearchResult[]> results)
		{
			if (results.Count == 0)
			{
				return EmptyResultArray;
			}
			if (results.Count == 1)
			{
				return results[0];
			}
			lock (STATIC_LOCK)
			{
				MergeSignature mergeSignature = new MergeSignature(results, results.Count);
				if (KnownMergeSignatures.TryGetValue(mergeSignature, out var fastResultArray))
				{
					return fastResultArray;
				}
				List<TypeSearchResult> mergeIntoList = CachedFastMergeList;
				mergeIntoList.Clear();
				List<QueryResult> queries = CachedQueryResultList;
				queries.Clear();
				for (int i = 0; i < results.Count; i++)
				{
					if (results[i].Length != 0)
					{
						queries.Add(new QueryResult(results[i]));
					}
				}
				int queriesCount = queries.Count;
				while (true)
				{
					double highestPriority = double.MinValue;
					int highestIndex = -1;
					for (int j = 0; j < queriesCount; j++)
					{
						QueryResult query = queries[j];
						if (query.CurrentIndex < query.Result.Length && query.CurrentPriority > highestPriority)
						{
							highestPriority = query.CurrentPriority;
							highestIndex = j;
						}
					}
					if (highestIndex == -1)
					{
						break;
					}
					QueryResult highest = queries[highestIndex];
					mergeIntoList.Add(highest.Result[highest.CurrentIndex]);
					highest.CurrentIndex++;
					if (highest.CurrentIndex < highest.Result.Length)
					{
						highest.CurrentPriority = highest.Result[highest.CurrentIndex].MatchedInfo.Priority;
					}
					queries[highestIndex] = highest;
				}
				mergeSignature.Results = new List<TypeSearchResult[]>(mergeSignature.Results);
				TypeSearchResult[] arr = mergeIntoList.ToArray();
				KnownMergeSignatures.Add(mergeSignature, arr);
				return arr;
			}
		}

		public static void MergeQueryResultsIntoList(List<TypeSearchResult[]> results, List<TypeSearchResult> mergeIntoList)
		{
			mergeIntoList.Clear();
			if (results.Count == 0)
			{
				return;
			}
			if (results.Count == 1)
			{
				TypeSearchResult[] arr = results[0];
				for (int i = 0; i < arr.Length; i++)
				{
					mergeIntoList.Add(arr[i]);
				}
				return;
			}
			lock (STATIC_LOCK)
			{
				MergeSignature mergeSignature = new MergeSignature(results, results.Count);
				if (KnownMergeSignatures.TryGetValue(mergeSignature, out var fastResultArray))
				{
					for (int j = 0; j < fastResultArray.Length; j++)
					{
						mergeIntoList.Add(fastResultArray[j]);
					}
					return;
				}
				List<QueryResult> queries = CachedQueryResultList;
				queries.Clear();
				for (int k = 0; k < results.Count; k++)
				{
					if (results[k].Length != 0)
					{
						queries.Add(new QueryResult(results[k]));
					}
				}
				int queriesCount = queries.Count;
				while (true)
				{
					double highestPriority = double.MinValue;
					int highestIndex = -1;
					for (int l = 0; l < queriesCount; l++)
					{
						QueryResult query = queries[l];
						if (query.CurrentIndex < query.Result.Length && query.CurrentPriority > highestPriority)
						{
							highestPriority = query.CurrentPriority;
							highestIndex = l;
						}
					}
					if (highestIndex == -1)
					{
						break;
					}
					QueryResult highest = queries[highestIndex];
					mergeIntoList.Add(highest.Result[highest.CurrentIndex]);
					highest.CurrentIndex++;
					if (highest.CurrentIndex < highest.Result.Length)
					{
						highest.CurrentPriority = highest.Result[highest.CurrentIndex].MatchedInfo.Priority;
					}
					queries[highestIndex] = highest;
				}
				mergeSignature.Results = new List<TypeSearchResult[]>(mergeSignature.Results);
				KnownMergeSignatures.Add(mergeSignature, mergeIntoList.ToArray());
			}
		}

		public List<TypeMatchQuery> GetAllCachedTargets()
		{
			List<TypeMatchQuery> result = new List<TypeMatchQuery>();
			lock (LOCK)
			{
				foreach (TypeMatchQuery key in resultCache.Keys)
				{
					result.Add(key);
				}
				return result;
			}
		}

		public void AddIndexedType(TypeSearchInfo typeToIndex)
		{
			lock (LOCK)
			{
				ProcessedTypeSearchInfo indexedType = ProcessInfo(typeToIndex);
				if (indexedType != null)
				{
					InsertIndexedTypeSorted(indexedTypes, indexedType);
				}
				if (resultCache.Count > 0)
				{
					resultCache.Clear();
				}
			}
		}

		public void AddIndexedTypeUnsorted(TypeSearchInfo typeToIndex)
		{
			lock (LOCK)
			{
				ProcessedTypeSearchInfo indexedType = ProcessInfo(typeToIndex);
				if (indexedType != null)
				{
					indexedTypes.Add(indexedType);
				}
				if (resultCache.Count > 0)
				{
					resultCache.Clear();
				}
			}
		}

		public void AddIndexedTypes(List<TypeSearchInfo> typesToIndex)
		{
			lock (LOCK)
			{
				for (int i = 0; i < typesToIndex.Count; i++)
				{
					ProcessedTypeSearchInfo indexedType = ProcessInfo(typesToIndex[i]);
					if (indexedType != null)
					{
						InsertIndexedTypeSorted(indexedTypes, indexedType);
					}
				}
				if (resultCache.Count > 0)
				{
					resultCache.Clear();
				}
			}
		}

		public void ClearResultCache()
		{
			lock (LOCK)
			{
				resultCache.Clear();
			}
		}

		public TypeSearchResult[] GetMatches(Type target, TargetMatchCategory category)
		{
			if (target == null)
			{
				throw new ArgumentNullException("target");
			}
			TypeSearchResult[] result;
			lock (LOCK)
			{
				TypeMatchQuery query = CachedQuery1;
				query.Targets[0] = target;
				query.Categories[0] = category;
				if (!resultCache.TryGetValue(query, out result))
				{
					query = new TypeMatchQuery
					{
						Targets = new Type[1] { target },
						Categories = new TargetMatchCategory[1] { category }
					};
					result = FindAllMatches(query.Targets, query.Categories);
					resultCache[query] = result;
				}
			}
			return result;
		}

		public TypeSearchResult[] GetMatches(Type target1, Type target2, TargetMatchCategory category1, TargetMatchCategory category2)
		{
			if (target1 == null)
			{
				throw new ArgumentNullException("target1");
			}
			if (target2 == null)
			{
				throw new ArgumentNullException("target2");
			}
			TypeSearchResult[] result;
			lock (LOCK)
			{
				TypeMatchQuery query = CachedQuery2;
				query.Targets[0] = target1;
				query.Targets[1] = target2;
				query.Categories[0] = category1;
				query.Categories[1] = category2;
				if (!resultCache.TryGetValue(query, out result))
				{
					query = new TypeMatchQuery
					{
						Targets = new Type[2] { target1, target2 },
						Categories = new TargetMatchCategory[2] { category1, category2 }
					};
					result = FindAllMatches(query.Targets, query.Categories);
					resultCache[query] = result;
				}
			}
			return result;
		}

		public TypeSearchResult[] GetMatches(Type[] targets, TargetMatchCategory[] categories)
		{
			if (targets == null)
			{
				throw new ArgumentNullException("targets");
			}
			TypeSearchResult[] result;
			lock (LOCK)
			{
				TypeMatchQuery query = new TypeMatchQuery
				{
					Targets = targets,
					Categories = categories
				};
				if (!resultCache.TryGetValue(query, out result))
				{
					result = FindAllMatches(targets, categories);
					resultCache[query] = result;
				}
			}
			return result;
		}

		private TypeSearchResult[] FindAllMatches(Type[] targets, TargetMatchCategory[] categories)
		{
			List<TypeSearchResult> sortedMatches = new List<TypeSearchResult>();
			for (int i = 0; i < indexedTypes.Count; i++)
			{
				ProcessedTypeSearchInfo info = indexedTypes[i];
				if (targets.Length != info.Info.Targets.Length)
				{
					continue;
				}
				TargetMatchCategory[] cats = info.Info.TargetCategories;
				if (cats != null && categories != null)
				{
					bool cont = false;
					for (int j = 0; j < cats.Length; j++)
					{
						if ((cats[j] & categories[j]) == 0)
						{
							cont = true;
							break;
						}
					}
					if (cont)
					{
						continue;
					}
				}
				for (int k = 0; k < info.Matchers.Count; k++)
				{
					bool stopMatchingForInfo = false;
					TypeMatcher matcher = info.Matchers[k];
					Type match = matcher.Match(targets, ref stopMatchingForInfo);
					if (match != null)
					{
						sortedMatches.Add(new TypeSearchResult
						{
							MatchedInfo = info.Info,
							MatchedMatcher = matcher,
							MatchedType = match,
							MatchedTargets = targets,
							MatchedIndex = this
						});
						break;
					}
					if (stopMatchingForInfo)
					{
						break;
					}
				}
				for (int l = 0; l < MatchRules.Count; l++)
				{
					TypeMatchRule rule = MatchRules[l];
					bool stopMatchingForInfo2 = false;
					Type match2 = rule.Match(info.Info, targets, ref stopMatchingForInfo2);
					if (match2 != null)
					{
						sortedMatches.Add(new TypeSearchResult
						{
							MatchedInfo = info.Info,
							MatchedRule = rule,
							MatchedType = match2,
							MatchedTargets = targets,
							MatchedIndex = this
						});
						break;
					}
					if (stopMatchingForInfo2)
					{
						break;
					}
				}
			}
			return sortedMatches.ToArray();
		}

		private static void InsertIndexedTypeSorted(List<ProcessedTypeSearchInfo> indexedTypes, ProcessedTypeSearchInfo typeInfo)
		{
			double priority = typeInfo.Info.Priority;
			int left = 0;
			int right = indexedTypes.Count - 1;
			int current = 0;
			int compare = 0;
			while (left <= right)
			{
				current = (left + right) / 2;
				ProcessedTypeSearchInfo middle = indexedTypes[current];
				compare = ((priority < middle.Info.Priority) ? (-1) : ((priority > middle.Info.Priority) ? 1 : 0));
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
				for (int count = indexedTypes.Count; current + 1 < count; current++)
				{
					ProcessedTypeSearchInfo next = indexedTypes[current + 1];
					if (priority > next.Info.Priority)
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
			indexedTypes.Insert(current, typeInfo);
		}

		private ProcessedTypeSearchInfo ProcessInfo(TypeSearchInfo info)
		{
			TypeSearchInfo originalInfo = info;
			if (info.Targets == null)
			{
				info.Targets = Type.EmptyTypes;
			}
			for (int i = 0; i < info.Targets.Length; i++)
			{
				if (info.Targets[i] == null)
				{
					throw new ArgumentNullException("Target at index " + i + " in info for match type " + info.MatchType.GetNiceFullName() + " is null.");
				}
			}
			if (info.TargetCategories != null && info.TargetCategories.Length != info.Targets.Length)
			{
				throw new ArgumentException("TypeSearchInfo's TargetCategories array is a different length than its Targets array.");
			}
			for (int j = 0; j < IndexingRules.Count; j++)
			{
				TypeMatchIndexingRule rule = IndexingRules[j];
				string errorMessage = null;
				if (rule.Process(ref info, ref errorMessage))
				{
					continue;
				}
				if (LogInvalidTypeInfo != null)
				{
					if (errorMessage == null)
					{
						LogInvalidTypeInfo("Invalid " + MatchedTypeLogName + " declaration '" + originalInfo.MatchType.GetNiceFullName() + "'! Rule '" + rule.Name.Replace("{name}", MatchedTypeLogName) + "' failed.", originalInfo);
					}
					else
					{
						errorMessage = errorMessage.Replace("{name}", MatchedTypeLogName);
						LogInvalidTypeInfo("Invalid " + MatchedTypeLogName + " declaration '" + originalInfo.MatchType.GetNiceFullName() + "'! Rule '" + rule.Name.Replace("{name}", MatchedTypeLogName) + "' failed with message: " + errorMessage, originalInfo);
					}
				}
				return null;
			}
			ProcessedTypeSearchInfo processedInfo = new ProcessedTypeSearchInfo();
			processedInfo.Info = info;
			processedInfo.Matchers = new List<TypeMatcher>(TypeMatcherCreators.Count);
			for (int k = 0; k < TypeMatcherCreators.Count; k++)
			{
				if (TypeMatcherCreators[k].TryCreateMatcher(info, out var matcher))
				{
					processedInfo.Matchers.Add(matcher);
				}
			}
			return processedInfo;
		}

		public void AddDefaultMatchRules()
		{
		}

		public void AddDefaultMatchCreators()
		{
			lock (LOCK)
			{
				TypeMatcherCreators.Add(new ExactTypeMatcher.Creator());
				TypeMatcherCreators.Add(new DerivedTypeMatcher.Creator());
				TypeMatcherCreators.Add(new GenericSingleTargetTypeMatcher.Creator());
				TypeMatcherCreators.Add(new TargetsSatisfyGenericParameterConstraintsTypeMatcher.Creator());
				TypeMatcherCreators.Add(new GenericParameterInferenceTypeMatcher.Creator());
				TypeMatcherCreators.Add(new NestedInSameGenericTypeTypeMatcher.Creator());
			}
		}

		public void AddDefaultIndexingRules()
		{
			lock (LOCK)
			{
				IndexingRules.Add(DefaultIndexingRules.MustBeAbleToInstantiateType);
				IndexingRules.Add(DefaultIndexingRules.GenericMatchTypeValidation);
				IndexingRules.Add(DefaultIndexingRules.GenericDefinitionSanityCheck);
			}
		}
	}
}
