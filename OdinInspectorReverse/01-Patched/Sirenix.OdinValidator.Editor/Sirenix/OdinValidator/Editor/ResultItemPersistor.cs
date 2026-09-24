using System;
using System.Collections.Generic;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.Validation;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Sirenix.OdinValidator.Editor
{
	public static class ResultItemPersistor
	{
		public struct PersistenceContext
		{
			public object Validator;

			public InspectorProperty Property;

			public IPropertyValueEntry ValueEntry;

			public IPropertyValueEntry BaseValueEntry;

			public PropertyTree Tree;

			public object Root;

			public object Value;
		}

		public static PersistenceContext CreateContextFromValidator(object validator)
		{
			if (validator == null)
			{
				throw new ArgumentNullException("validator");
			}
			PersistenceContext context = new PersistenceContext
			{
				Validator = validator
			};
			if (validator is Validator val)
			{
				context.Property = val.Property;
				context.ValueEntry = context.Property.ValueEntry;
				context.BaseValueEntry = context.Property.BaseValueEntry;
				context.Tree = context.Property.Tree;
				context.Root = context.Tree.WeakTargets[0];
				if (context.ValueEntry != null)
				{
					context.Value = context.ValueEntry.WeakSmartValue;
				}
			}
			return context;
		}

		public static PersistentResultItem[] CreatePersistentResultItems(ValidationResult result)
		{
			PersistentResultItem[] results = new PersistentResultItem[result.Count];
			CreatePersistentResultItems(result, ref results);
			return results;
		}

		public static void CreatePersistentResultItems(ValidationResult result, ref PersistentResultItem[] results)
		{
			if (result.Setup.Validator == null)
			{
				Debug.LogError("Result contains data to persist, but has no validator. All data needing persistence has been removed. Use an overload that passes in a PersistenceContext instead.");
				for (int i = 0; i < result.Count; i++)
				{
					ref ResultItem r = ref result[i];
					results[i] = new PersistentResultItem(in r);
				}
			}
			else
			{
				CreatePersistentResultItems(result, CreateContextFromValidator(result.Setup.Validator), ref results);
			}
		}

		public static PersistentResultItem[] CreatePersistentResultItems(ICollection<ResultItem> resultItems, in PersistenceContext context)
		{
			PersistentResultItem[] results = new PersistentResultItem[resultItems.Count];
			CreatePersistentResultItems(resultItems, in context, ref results);
			return results;
		}

		public static void CreatePersistentResultItems(ICollection<ResultItem> resultItems, in PersistenceContext context, ref PersistentResultItem[] results)
		{
			if (results == null || results.Length < resultItems.Count)
			{
				results = new PersistentResultItem[resultItems.Count];
			}
			int i = 0;
			foreach (ResultItem resultItem in resultItems)
			{
				ResultItem r = resultItem;
				results[i++] = new PersistentResultItem(in r);
			}
		}

		public static bool TryGenerateNewValidatorResults(in PersistenceContext context, bool openSceneIfNeeded, out ResultItem[] results)
		{
			if (context.Validator is GlobalValidator globalValidator)
			{
				List<ResultItem> resultsList = new List<ResultItem>();
				foreach (ValidationResult result in globalValidator.RunValidation())
				{
					if (result == null)
					{
						continue;
					}
					foreach (ResultItem subResult in (IEnumerable<ResultItem>)result)
					{
						resultsList.Add(subResult);
					}
				}
				results = resultsList.ToArray();
				return true;
			}
			if (context.Validator is SceneValidator sceneValidator)
			{
				if (!sceneValidator.ValidatedScene.IsValid)
				{
					results = null;
					return false;
				}
				if (!sceneValidator.ValidatedScene.IsLoaded && (!openSceneIfNeeded || !sceneValidator.ValidatedScene.TryOpenScene(OpenSceneMode.Additive, out var _)))
				{
					results = null;
					return false;
				}
				ValidationResult result2 = new ValidationResult();
				sceneValidator.RunValidation(ref result2);
				results = new ResultItem[result2.Count];
				for (int i = 0; i < result2.Count; i++)
				{
					results[i] = result2[i];
				}
				return true;
			}
			if (context.Validator is Validator validator)
			{
				ValidationResult result3 = new ValidationResult();
				validator.RunValidation(ref result3);
				results = new ResultItem[result3.Count];
				for (int j = 0; j < result3.Count; j++)
				{
					results[j] = result3[j];
				}
				return true;
			}
			results = null;
			return false;
		}

		[Obsolete("Use TryRebuildResultItems instead, as this operation may now fail if results could not be recovered/rebuilt.", false)]
		public static ResultItem[] RebuildResultItems(PersistentResultItem[] items, ref PersistenceContext context, bool openSceneIfNeeded)
		{
			TryRebuildResultItems(items, ref context, openSceneIfNeeded, out var results);
			return results;
		}

		public static bool TryRebuildResultItem(in PersistentResultItem item, ref PersistenceContext context, bool openSceneIfNeeded, out ResultItem result)
		{
			if (TryRebuildResultItems(new PersistentResultItem[1] { item }, ref context, openSceneIfNeeded, out var results))
			{
				result = results[0];
				return result.ResultType == item.ResultType;
			}
			result = default(ResultItem);
			return false;
		}

		public static bool TryRebuildResultItems(PersistentResultItem[] items, ref PersistenceContext context, bool openSceneIfNeeded, out ResultItem[] results)
		{
			if (TryGenerateNewValidatorResults(in context, openSceneIfNeeded, out var newResults))
			{
				results = new ResultItem[items.Length];
				for (int persistentIndex = 0; persistentIndex < items.Length; persistentIndex++)
				{
					ref PersistentResultItem item = ref items[persistentIndex];
					for (int newResultIndex = 0; newResultIndex < newResults.Length; newResultIndex++)
					{
						ref ResultItem newResult = ref newResults[newResultIndex];
						if (newResult.ResultType != ValidationResultType.IgnoreResult && newResult.Fix != null && item.Message == newResult.Message && item.ResultType == newResult.ResultType && item.Data.MetaData?.GetType() == newResult.MetaData?.GetType() && item.Data.Fix.ArgType == newResult.Fix.ArgType && item.Data.Fix.Action?.Method == newResult.Fix.Action?.Method)
						{
							newResult.MetaData = item.Data.MetaData;
							results[persistentIndex] = newResult;
							newResults[newResultIndex] = default(ResultItem);
							break;
						}
					}
				}
				return true;
			}
			results = null;
			return false;
		}
	}
}
