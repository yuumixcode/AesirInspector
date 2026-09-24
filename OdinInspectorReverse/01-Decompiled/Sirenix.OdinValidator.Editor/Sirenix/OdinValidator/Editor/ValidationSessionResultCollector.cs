using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.Validation;
using Sirenix.Utilities.Editor;
using UnityEditor;

namespace Sirenix.OdinValidator.Editor
{
	internal class ValidationSessionResultCollector : IDisposable
	{
		public struct ResultItemBatch
		{
			public PersistentValidationResultBatch Result;

			public ValidationWorkItem WorkItem;

			internal int FilteredIndex;

			internal int FinalizeId;

			public int LastSceneSetupChangeId;

			public float LastStateChangeTime;
		}

		public struct ResultItemSingle
		{
			public PersistentValidationResult Result;

			public ValidationWorkItem WorkItem;

			public int Score;

			public int LastSceneSetupChangeId;

			public float LastStateChangeTime;

			internal int BatchIndex;

			internal void Update()
			{
				int id = UnitySceneSetupChangeId.SceneSetupChangeId;
				if (LastSceneSetupChangeId != id)
				{
					LastSceneSetupChangeId = id;
					GUIHelper.RequestRepaint();
					Result?.DynamicObjectAddress?.Refresh();
				}
			}
		}

		public class Filter<T>
		{
			public class FilteredItem
			{
				public int Count;

				public bool Enabled;

				public T Value;

				internal int NextCount;

				private string name;

				public string Name
				{
					get
					{
						if (name == null)
						{
							T value = Value;
							if (value is SceneReference scene)
							{
								if (scene.GUID == null)
								{
									name = "Assets";
								}
								else
								{
									name = scene.Name;
								}
							}
							else if (Value is Type t)
							{
								if (t == typeof(NullReferenceException))
								{
									name = "No object type";
								}
								else
								{
									name = t.GetNiceValidatorTypeName();
								}
							}
							else
							{
								value = Value;
								if (!(value is FixIdentifier fixIdentifier))
								{
									throw new NotImplementedException();
								}
								name = fixIdentifier.Name;
							}
						}
						return name;
					}
				}

				public FilteredItem(T item)
				{
					Value = item;
				}
			}

			private Dictionary<T, FilteredItem> items = new Dictionary<T, FilteredItem>();

			public bool Enabled = true;

			public List<FilteredItem> OrderedItems = new List<FilteredItem>();

			public FilteredItem GetOrCreate(T val)
			{
				if (!items.TryGetValue(val, out var item))
				{
					Dictionary<T, FilteredItem> dictionary = items;
					FilteredItem obj = new FilteredItem(val)
					{
						NextCount = 0,
						Enabled = true
					};
					FilteredItem filteredItem = obj;
					dictionary[val] = obj;
					item = filteredItem;
					OrderedItems.Add(item);
				}
				return item;
			}

			public bool ShouldInclude(T item)
			{
				if (!Enabled)
				{
					return true;
				}
				if (items.TryGetValue(item, out var result))
				{
					return result.Enabled;
				}
				return false;
			}

			public void Clear()
			{
				items.Clear();
				OrderedItems.Clear();
				Enabled = true;
			}

			public bool HasFilter()
			{
				if (Enabled)
				{
					foreach (FilteredItem item in OrderedItems)
					{
						if (!item.Enabled)
						{
							return true;
						}
					}
				}
				return false;
			}
		}

		internal struct BatchKey : IEquatable<BatchKey>
		{
			public DynamicObjectAddress DynamicObjectAddress;

			public Type ValidatorType;

			public BatchKey(PersistentValidationResultBatch result)
			{
				DynamicObjectAddress = result.DynamicObjectAddress;
				ValidatorType = result.ValidatorType;
			}

			public override bool Equals(object obj)
			{
				if (obj is BatchKey key)
				{
					return Equals(key);
				}
				return false;
			}

			public bool Equals(BatchKey other)
			{
				if (DynamicObjectAddress == other.DynamicObjectAddress)
				{
					return ValidatorType == other.ValidatorType;
				}
				return false;
			}

			public override int GetHashCode()
			{
				return (((!(DynamicObjectAddress == null)) ? DynamicObjectAddress.GetHashCode() : 0) + ValidatorType?.GetHashCode()) ?? (-8);
			}

			public static bool operator ==(BatchKey left, BatchKey right)
			{
				return left.Equals(right);
			}

			public static bool operator !=(BatchKey left, BatchKey right)
			{
				return !(left == right);
			}
		}

		private string searchTerm;

		private bool isDisposed;

		private bool applyFilters = true;

		private int filteredItemsVersion = 1;

		private BackgroundTaskHandle populatorTask;

		private IEnumerator populateResultQueueEnumeratorTask;

		private EditorPrefBool showErrors = new EditorPrefBool("Odin_Validator_showErrors", defaultValue: true);

		private EditorPrefBool showWarnings = new EditorPrefBool("Odin_Validator_showWarnings", defaultValue: true);

		private EditorPrefBool showValid = new EditorPrefBool("Odin_Validator_showValid", defaultValue: true);

		private ResizableColumn[] resizableColumns;

		private ResultItemSingle[] filteredItems;

		private ResultItemBatch[] allItems = new ResultItemBatch[0];

		private HashSet<PersistentValidationResultBatch> resultObjects = new HashSet<PersistentValidationResultBatch>(new PersistentValidationResultBatch.Comparer());

		private List<ValidationSession.ValidationSessionResult> queue = new List<ValidationSession.ValidationSessionResult>();

		private HashSet<BatchKey> queuedBatches = new HashSet<BatchKey>();

		private MiniPool<ResultItemSingle[]> resultItemPool = new MiniPool<ResultItemSingle[]>(() => new ResultItemSingle[100]);

		private List<int> toGiveTimeUpdate = new List<int>();

		private List<ResultItemBatch> nextResultList = new List<ResultItemBatch>(100);

		private List<ResultItemBatch> toAdd = new List<ResultItemBatch>(100);

		private HashSet<DynamicObjectAddress> toRemove = new HashSet<DynamicObjectAddress>();

		private Dictionary<PersistentValidationResultBatch, ResultItemBatch> toReplace = new Dictionary<PersistentValidationResultBatch, ResultItemBatch>(new PersistentValidationResultBatch.Comparer());

		private List<ResultItemBatch> toReplaceIfNotDirty = new List<ResultItemBatch>();

		private ValidationSession validationSession;

		public Filter<SceneReference> SceneFilters = new Filter<SceneReference>();

		public Filter<Type> ObjectTypeFilters = new Filter<Type>();

		public Filter<Type> ValidatorTypeFilters = new Filter<Type>();

		public Filter<FixIdentifier> FixTypeFilters = new Filter<FixIdentifier>();

		private int totalWarningCount;

		private int totalErrorCount;

		private int totalValidCount;

		private static object QueueProcessingCompleted => BackgroundTaskRunner.Relax;

		public int QueuedItems => queue.Count;

		public int Length => GetFilteredItems().Length;

		public ref ResultItemSingle this[int index] => ref GetFilteredItems()[index];

		public int TotalWarningCount => totalWarningCount;

		public int TotalErrorCount => totalErrorCount;

		public int TotalValidCount => totalValidCount;

		public string SearchTerm
		{
			get
			{
				return searchTerm;
			}
			set
			{
				if (searchTerm != value)
				{
					MarkFiltersDirty();
					searchTerm = value;
				}
			}
		}

		public bool ShowErrors
		{
			get
			{
				return showErrors.Value;
			}
			set
			{
				if (showErrors.Value != value)
				{
					MarkFiltersDirty();
					showErrors.Value = value;
				}
			}
		}

		public bool ShowWarnings
		{
			get
			{
				return showWarnings.Value;
			}
			set
			{
				if (showWarnings.Value != value)
				{
					MarkFiltersDirty();
					showWarnings.Value = value;
				}
			}
		}

		public bool ShowValid
		{
			get
			{
				return showValid.Value;
			}
			set
			{
				if (showValid.Value != value)
				{
					MarkFiltersDirty();
					showValid.Value = value;
				}
			}
		}

		internal bool ApplyFilters
		{
			get
			{
				return applyFilters;
			}
			set
			{
				if (applyFilters != value)
				{
					MarkFiltersDirty();
					applyFilters = value;
				}
			}
		}

		public static event Action<ValidationSession> OnAnyResultsChanged;

		public event Action OnResultsChanged;

		public ValidationSessionResultCollector(ValidationSession session)
		{
			validationSession = session;
			populateResultQueueEnumeratorTask = PopulateResultQueue();
			populatorTask = BackgroundTaskRunner.StartTask("Result view population for " + validationSession.Name, populateResultQueueEnumeratorTask);
		}

		private IEnumerator PopulateResultQueue()
		{
			while (true)
			{
				IEnumerator p = ProcessQueueEnumerator();
				while (p.MoveNext())
				{
					yield return p.Current;
				}
				yield return QueueProcessingCompleted;
			}
		}

		internal ResultItemSingle[] GetAllItems()
		{
			ResultItemBatch[] batches = allItems;
			List<ResultItemSingle> result = new List<ResultItemSingle>(batches.Length);
			for (int i = 0; i < batches.Length; i++)
			{
				ref ResultItemBatch batch = ref batches[i];
				batch.FinalizeId = filteredItemsVersion;
				batch.FilteredIndex = result.Count;
				for (int k = 0; k < batch.Result.Count; k++)
				{
					result.Add(new ResultItemSingle
					{
						Result = new PersistentValidationResult(batch.Result, k),
						LastSceneSetupChangeId = batch.LastSceneSetupChangeId,
						LastStateChangeTime = batch.LastStateChangeTime,
						WorkItem = batch.WorkItem
					});
				}
			}
			return result.ToArray();
		}

		internal ResultItemSingle[] GetFilteredItems()
		{
			if (filteredItems == null)
			{
				filteredItemsVersion++;
				if (allItems.Length == 0)
				{
					filteredItems = new ResultItemSingle[0];
				}
				else if (ShowErrors && ShowWarnings && ShowValid && !applyFilters && string.IsNullOrEmpty(SearchTerm))
				{
					filteredItems = GetAllItems();
				}
				else
				{
					ResultItemBatch[] batches = allItems;
					List<ResultItemSingle> result = new List<ResultItemSingle>(batches.Length);
					for (int i = 0; i < batches.Length; i++)
					{
						ref ResultItemBatch batch = ref batches[i];
						batch.FinalizeId = filteredItemsVersion;
						batch.FilteredIndex = -1;
						for (int k = 0; k < batch.Result.Count; k++)
						{
							result.Add(new ResultItemSingle
							{
								Result = new PersistentValidationResult(batch.Result, k),
								LastSceneSetupChangeId = batch.LastSceneSetupChangeId,
								LastStateChangeTime = batch.LastStateChangeTime,
								WorkItem = batch.WorkItem,
								BatchIndex = i
							});
						}
					}
					if (result.Count == 0)
					{
						filteredItems = new ResultItemSingle[0];
					}
					else
					{
						ResultItemSingle[] exploded = result.ToArray();
						OrderablePartitioner<Tuple<int, int>> partitioner = Partitioner.Create(0, exploded.Length);
						bool warnings = ShowWarnings;
						bool errors = ShowErrors;
						bool valids = ShowValid;
						bool strSearch = !string.IsNullOrEmpty(this.searchTerm);
						string searchTerm = this.searchTerm;
						ConcurrentBag<(int start, int len, ResultItemSingle[] buffer)> partitionResults = new ConcurrentBag<(int, int, ResultItemSingle[])>();
						bool filter = applyFilters;
						partitioner.AsParallel().ForAll(delegate(Tuple<int, int> range)
						{
							int item = 0;
							int item2 = range.Item2;
							int item3 = range.Item1;
							int num = item2 - item3;
							ResultItemSingle[] array = resultItemPool.Get();
							if (array.Length < num)
							{
								Array.Resize(ref array, num * 2);
							}
							for (int j = item3; j < item2; j++)
							{
								ref ResultItemSingle reference = ref exploded[j];
								ValidationResultType resultType = reference.Result.ResultType;
								if ((resultType != ValidationResultType.Error || errors) && (resultType != ValidationResultType.Warning || warnings) && (valids || (resultType != ValidationResultType.Valid && resultType != ValidationResultType.IgnoreResult)) && (!strSearch || FuzzySearch.Contains(searchTerm, reference.Result.Message, out reference.Score)))
								{
									if (filter)
									{
										if (FixTypeFilters.Enabled)
										{
											Fix fix = reference.Result.Data.Fix;
											if (fix == null || !FixTypeFilters.ShouldInclude(fix.CreateIdentifier()))
											{
												continue;
											}
										}
										if (!ValidatorTypeFilters.ShouldInclude(reference.Result.GetGenericValidatorType()) || !SceneFilters.ShouldInclude(reference.Result.GetSceneReference()) || !ObjectTypeFilters.ShouldInclude(reference.Result.GetObjectType()))
										{
											continue;
										}
									}
									array[item++] = reference;
								}
							}
							partitionResults.Add((item3, item, array));
						});
						(int, int, ResultItemSingle[])[] partitions = partitionResults.OrderBy(((int start, int len, ResultItemSingle[] buffer) x) => x.start).ToArray();
						ResultItemSingle[] combined = new ResultItemSingle[partitions.Sum(((int start, int len, ResultItemSingle[] buffer) x) => x.len)];
						int offset = 0;
						for (int i2 = 0; i2 < partitions.Length; i2++)
						{
							Array.Copy(partitions[i2].Item3, 0, combined, offset, partitions[i2].Item2);
							offset += partitions[i2].Item2;
							resultItemPool.Return(partitions[i2].Item3);
						}
						filteredItems = combined;
					}
				}
			}
			return filteredItems;
		}

		internal void Enqueue(BatchKey key, ArraySlice<ValidationSession.ValidationSessionResult> batch)
		{
			queuedBatches.Add(key);
			ArraySlice<ValidationSession.ValidationSessionResult>.Iterator enumerator = batch.GetEnumerator();
			while (enumerator.MoveNext())
			{
				ValidationSession.ValidationSessionResult item = enumerator.Current;
				queue.Add(item);
			}
			if (queue.Count > 1000)
			{
				ProcessQueue();
			}
		}

		internal void Enqueue(ValidationSession.ValidationSessionResult item)
		{
			queue.Add(item);
			if (queue.Count > 1000)
			{
				ProcessQueue();
			}
		}

		public void ProcessQueue()
		{
			while (populateResultQueueEnumeratorTask.Current != QueueProcessingCompleted)
			{
				populateResultQueueEnumeratorTask.MoveNext();
			}
			populateResultQueueEnumeratorTask.MoveNext();
			while (populateResultQueueEnumeratorTask.Current != QueueProcessingCompleted)
			{
				populateResultQueueEnumeratorTask.MoveNext();
			}
		}

		public IEnumerator ProcessQueueEnumerator()
		{
			if (queue.Count == 0)
			{
				yield break;
			}
			ValidationSession.ValidationSessionResult[] qCopy = queue.ToArray();
			HashSet<BatchKey> qAddressesCopy = new HashSet<BatchKey>(queuedBatches);
			queue.Clear();
			queuedBatches.Clear();
			nextResultList.Clear();
			toRemove.Clear();
			toReplace.Clear();
			toReplaceIfNotDirty.Clear();
			toAdd.Clear();
			toGiveTimeUpdate.Clear();
			resultObjects.Clear();
			ResultItemBatch[] array = allItems;
			for (int i = 0; i < array.Length; i++)
			{
				ResultItemBatch item = array[i];
				if (item.Result != null)
				{
					resultObjects.Add(item.Result);
				}
			}
			ValidationSession.ValidationSessionResult[] array2 = qCopy;
			for (int j = 0; j < array2.Length; j++)
			{
				ValidationSession.ValidationSessionResult item2 = array2[j];
				if (item2.Type == ValidationSession.ValidationSessionResult.ValidationSessionResultType.ObjectDeleted)
				{
					if (item2.WorkItem.EntityId.IsValid)
					{
						if (DynamicObjectAddress.TryGet(item2.WorkItem.EntityId, out var address))
						{
							toRemove.Add(address);
						}
					}
					else if (!string.IsNullOrEmpty(item2.WorkItem.AssetGuid))
					{
						foreach (DynamicObjectAddress assetAddress in DynamicObjectAddress.GetAllExistingAddressesForAssetGuid(item2.WorkItem.AssetGuid))
						{
							toRemove.Add(assetAddress);
						}
					}
				}
				else if (item2.Type == ValidationSession.ValidationSessionResult.ValidationSessionResultType.ResultAddedOrChanged)
				{
					if (item2.Result == null)
					{
						throw new NullReferenceException();
					}
					if (resultObjects.Contains(item2.Result))
					{
						toReplace[item2.Result] = new ResultItemBatch
						{
							Result = item2.Result,
							WorkItem = item2.WorkItem,
							LastSceneSetupChangeId = UnitySceneSetupChangeId.SceneSetupChangeId
						};
					}
					else
					{
						toAdd.Add(new ResultItemBatch
						{
							Result = item2.Result,
							WorkItem = item2.WorkItem,
							LastSceneSetupChangeId = UnitySceneSetupChangeId.SceneSetupChangeId
						});
					}
				}
				else if (item2.Type != ValidationSession.ValidationSessionResult.ValidationSessionResultType.Ignore)
				{
					throw new NotImplementedException();
				}
				yield return null;
			}
			bool isDirty = false;
			resultObjects.Clear();
			for (int j = 0; j < allItems.Length; j++)
			{
				ResultItemBatch item3 = allItems[j];
				if (toRemove.Count > 0 && toRemove.Contains(item3.Result.DynamicObjectAddress))
				{
					isDirty = true;
					toReplace.Remove(item3.Result);
					continue;
				}
				if (toReplace.TryGetValue(item3.Result, out var replacement) && resultObjects.Add(replacement.Result))
				{
					PersistentValidationResultBatch old = item3.Result;
					PersistentValidationResultBatch _new = replacement.Result;
					PersistentResultItem _newHighestSeverityResult = _new.HighestSeverityResult;
					PersistentResultItem oldHighestSeverityResult = old.HighestSeverityResult;
					if (_newHighestSeverityResult.ResultType != oldHighestSeverityResult.ResultType)
					{
						isDirty = true;
						toGiveTimeUpdate.Add(nextResultList.Count);
					}
					if (_newHighestSeverityResult.ResultType == ValidationResultType.Valid)
					{
						replacement.Result.HighestSeverityResult = _newHighestSeverityResult;
					}
					if (string.IsNullOrEmpty(_newHighestSeverityResult.Message))
					{
						_newHighestSeverityResult.Message = oldHighestSeverityResult.Message;
						replacement.Result.HighestSeverityResult = _newHighestSeverityResult;
					}
					if (_new.Count != old.Count)
					{
						isDirty = true;
					}
					if (!isDirty)
					{
						isDirty = _newHighestSeverityResult.Message != oldHighestSeverityResult.Message;
					}
					if (!isDirty)
					{
						replacement.FilteredIndex = item3.FilteredIndex;
						replacement.FinalizeId = item3.FinalizeId;
						toReplaceIfNotDirty.Add(replacement);
					}
					nextResultList.Add(replacement);
				}
				if (resultObjects.Add(item3.Result))
				{
					if (qAddressesCopy.Contains(new BatchKey(item3.Result)))
					{
						isDirty = true;
					}
					else
					{
						nextResultList.Add(item3);
					}
				}
				yield return null;
			}
			for (int j = 0; j < toAdd.Count; j++)
			{
				ResultItemBatch item4 = toAdd[j];
				ValidationResultType type = item4.Result.HighestSeverityResult.ResultType;
				if (type != ValidationResultType.Valid && type != ValidationResultType.IgnoreResult)
				{
					if (resultObjects.Add(item4.Result))
					{
						isDirty = true;
						toGiveTimeUpdate.Add(nextResultList.Count);
						nextResultList.Add(item4);
					}
					yield return null;
				}
			}
			if (isDirty)
			{
				int j = 0;
				int warningCount = 0;
				int validCount = 0;
				foreach (Filter<Type>.FilteredItem item5 in ObjectTypeFilters.OrderedItems)
				{
					item5.NextCount = 0;
				}
				foreach (Filter<SceneReference>.FilteredItem item6 in SceneFilters.OrderedItems)
				{
					item6.NextCount = 0;
				}
				foreach (Filter<Type>.FilteredItem item7 in ValidatorTypeFilters.OrderedItems)
				{
					item7.NextCount = 0;
				}
				foreach (Filter<FixIdentifier>.FilteredItem item8 in FixTypeFilters.OrderedItems)
				{
					item8.NextCount = 0;
				}
				foreach (ResultItemBatch item9 in nextResultList)
				{
					for (int k = 0; k < item9.Result.Count; k++)
					{
						PersistentResultItem subResult = item9.Result[k];
						if (subResult.Data.Fix != null)
						{
							FixTypeFilters.GetOrCreate(subResult.Data.Fix.CreateIdentifier((item9.Result.ValidatorType == null) ? "Fix" : item9.Result.ValidatorType.GetNiceValidatorTypeName())).NextCount++;
						}
					}
					ValidatorTypeFilters.GetOrCreate(item9.Result.GetGenericValidatorType()).NextCount++;
					SceneFilters.GetOrCreate(item9.Result.GetSceneReference()).NextCount++;
					ObjectTypeFilters.GetOrCreate(item9.Result.GetObjectType()).NextCount++;
					int issueCount = item9.Result.ErrorCount + item9.Result.WarningCount;
					j += item9.Result.ErrorCount;
					warningCount += item9.Result.WarningCount;
					validCount += item9.Result.Count - issueCount;
					yield return null;
				}
				foreach (Filter<Type>.FilteredItem item10 in ObjectTypeFilters.OrderedItems)
				{
					item10.Count = item10.NextCount;
				}
				foreach (Filter<SceneReference>.FilteredItem item11 in SceneFilters.OrderedItems)
				{
					item11.Count = item11.NextCount;
				}
				foreach (Filter<Type>.FilteredItem item12 in ValidatorTypeFilters.OrderedItems)
				{
					item12.Count = item12.NextCount;
				}
				foreach (Filter<FixIdentifier>.FilteredItem item13 in FixTypeFilters.OrderedItems)
				{
					item13.Count = item13.NextCount;
				}
				ResultItemBatch[] items = nextResultList.ToArray();
				float time = (float)EditorApplication.timeSinceStartup;
				foreach (int i2 in toGiveTimeUpdate)
				{
					items[i2].LastStateChangeTime = time;
				}
				allItems = items;
				totalErrorCount = j;
				totalWarningCount = warningCount;
				totalValidCount = validCount;
				MarkFiltersDirty();
				if (this.OnResultsChanged != null)
				{
					this.OnResultsChanged();
				}
				if (ValidationSessionResultCollector.OnAnyResultsChanged != null)
				{
					ValidationSessionResultCollector.OnAnyResultsChanged(validationSession);
				}
			}
			else
			{
				foreach (ResultItemBatch i3 in toReplaceIfNotDirty)
				{
					ResultItemBatch batch = i3;
					if (batch.FilteredIndex < 0 || batch.FinalizeId != filteredItemsVersion)
					{
						continue;
					}
					int batchSize = batch.Result.Count;
					for (int l = 0; l < batchSize; l++)
					{
						int filteredIndex = batch.FilteredIndex + l;
						if (filteredItems != null && filteredIndex < filteredItems.Length)
						{
							filteredItems[filteredIndex].LastStateChangeTime = batch.LastStateChangeTime;
							filteredItems[filteredIndex].Result = new PersistentValidationResult(batch.Result, l);
						}
					}
				}
			}
			toReplaceIfNotDirty.Clear();
			nextResultList.Clear();
			toRemove.Clear();
			toReplace.Clear();
			toAdd.Clear();
			toGiveTimeUpdate.Clear();
			resultObjects.Clear();
		}

		public void Clear()
		{
			totalErrorCount = 0;
			totalWarningCount = 0;
			totalValidCount = 0;
			queue.Clear();
			queuedBatches.Clear();
			resultObjects.Clear();
			allItems = new ResultItemBatch[0];
			SceneFilters.Clear();
			ObjectTypeFilters.Clear();
			ValidatorTypeFilters.Clear();
			FixTypeFilters.Clear();
			populatorTask.Kill();
			populatorTask = BackgroundTaskRunner.StartTask("Result view population for " + validationSession.Name, populateResultQueueEnumeratorTask);
			MarkFiltersDirty();
		}

		public void MarkFiltersDirty()
		{
			filteredItemsVersion++;
			filteredItems = null;
		}

		public void Dispose()
		{
			isDisposed = true;
			Clear();
			populatorTask.Kill();
			populatorTask = null;
		}
	}
}
