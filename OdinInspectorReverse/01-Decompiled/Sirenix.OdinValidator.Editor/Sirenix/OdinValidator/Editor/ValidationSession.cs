using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.Internal;
using Sirenix.OdinInspector.Editor.Validation;
using Sirenix.Reflection.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Rendering;

namespace Sirenix.OdinValidator.Editor
{
	public class ValidationSession : IDisposable
	{
		public struct ValidationSessionResult
		{
			public enum ValidationSessionResultType
			{
				/// <summary>
				/// Result can be null.
				/// </summary>
				Ignore,
				ResultAddedOrChanged,
				/// <summary>
				/// Result will be null if the object is deleted.
				/// </summary>
				ObjectDeleted
			}

			public ValidationSessionResultType Type;

			public ValidationWorkItem WorkItem;

			public PersistentValidationResultBatch Result;

			public bool Equals(ValidationSessionResult other)
			{
				if (other.Result != null && Result != null)
				{
					return Result == other.Result;
				}
				return WorkItem == other.WorkItem;
			}
		}

		public static List<ValidationSession> ActiveValidationSessions = new List<ValidationSession>();

		private static ValidationSessionAssetHandle mainValidationSessionHandle;

		private bool shouldDisplayProgressBar;

		private StackTrace allocationStackTrace;

		private BackgroundTaskHandle backgroundTaskHandle;

		private IEnumerator<ValidationSessionResult> currentlyBackgroundProcessingWorkItemEnumerator;

		private readonly Queue<ValidationSessionResult> removedObjectResults = new Queue<ValidationSessionResult>();

		internal ValidationWorkItem? currentlyProcessingWorkItem;

		internal ValidationWorkItem prevProcessedWorkItem;

		internal uint workDoneCountSample;

		internal uint remainingWorkCountSample;

		public readonly string Name;

		public OdinValidationRunner Runner;

		public readonly ValidationWorkItemQueue WorkQueue = new ValidationWorkItemQueue();

		public readonly HashSet<ValidationWorkItem> WorkDone = new HashSet<ValidationWorkItem>(default(ValidationWorkItem.Comparer));

		public readonly SessionConfig Config;

		internal ValidationSessionResultCollector Results;

		private static bool IsHeadlessOrBatchMode
		{
			get
			{
				if (SystemInfo.graphicsDeviceType != GraphicsDeviceType.Null)
				{
					return InternalEditorUtility.inBatchMode;
				}
				return true;
			}
		}

		[Obsolete("Use ValidationProfile.MainValidationProfile instead", false)]
		public static ValidationProfile MainValidationSessionAsset => ValidationProfile.MainValidationProfile;

		[Obsolete("MainValidationSession only works when 'Keep main session alive in background' is enabled in the Validator config. Use 'ValidationProfile.MainValidationProfile.ClaimSessionHandle()' instead and dispose of it when done using it.", false)]
		public static ValidationSession MainValidationSession
		{
			get
			{
				MaintainActiveHandleToActiveValidationSession();
				return mainValidationSessionHandle.Session;
			}
		}

		public bool IsDisposed { get; private set; }

		public int CurrentWarningCount => Results.TotalWarningCount;

		public int CurrentErrorCount => Results.TotalErrorCount;

		public int CurrentValidCount => Results.TotalValidCount;

		public bool IsValidatingInBackground { get; private set; }

		public bool IsWatching { get; private set; }

		internal bool ShouldDisplayProgressBar
		{
			get
			{
				if (shouldDisplayProgressBar)
				{
					if (WorkQueue.Count > 0)
					{
						return true;
					}
					if (currentlyProcessingWorkItem.HasValue)
					{
						return true;
					}
					PrepareNextProgressBar(showProgressBar: false);
				}
				else
				{
					if (WorkQueue.Count == 0 && !currentlyProcessingWorkItem.HasValue)
					{
						return false;
					}
					if (WorkQueue.TotalResultsEstimate > 1500 || WorkQueue.AllQueuedSceneReferences.Count > 0 || (currentlyProcessingWorkItem.HasValue && currentlyProcessingWorkItem.Value.SceneContent.HasValue))
					{
						PrepareNextProgressBar(showProgressBar: true);
					}
				}
				return false;
			}
		}

		[InitializeOnLoadMethod]
		private static void Init()
		{
			if (IsHeadlessOrBatchMode)
			{
				return;
			}
			EditorApplication.playModeStateChanged -= ToggleBackgroundValidation;
			EditorApplication.playModeStateChanged += ToggleBackgroundValidation;
			AssemblyReloadEvents.beforeAssemblyReload += delegate
			{
				foreach (ValidationSession current in ActiveValidationSessions.ToList())
				{
					current.Dispose();
				}
			};
			EditorApplication.delayCall = (EditorApplication.CallbackFunction)Delegate.Combine(EditorApplication.delayCall, (EditorApplication.CallbackFunction)delegate
			{
				MaintainActiveHandleToActiveValidationSession();
				EditorApplication.update = (EditorApplication.CallbackFunction)Delegate.Combine(EditorApplication.update, new EditorApplication.CallbackFunction(MaintainActiveHandleToActiveValidationSession));
				ToggleBackgroundValidation(PlayModeStateChange.EnteredEditMode);
				if (!GlobalConfig<GlobalValidationConfig>.Instance.HasShownValidationConfig)
				{
					string assetPath = AssetDatabase.GetAssetPath(GlobalConfig<GlobalValidationConfig>.Instance);
					string assetGuid = AssetDatabase.AssetPathToGUID(assetPath);
					foreach (ValidationSession current in ActiveValidationSessions)
					{
						current.Enqueue(ValidationWorkItem.CreateForAssetGuid(assetGuid, ProjectEventSource.Other), insert: true);
					}
				}
			});
		}

		private static void MaintainActiveHandleToActiveValidationSession()
		{
			ProjectSettingBool keepAlive = GlobalConfig<GlobalValidationConfig>.Instance.KeepMainValidationSessionAliveInBackground;
			if (mainValidationSessionHandle != null && (!keepAlive || mainValidationSessionHandle.Asset != ValidationProfile.MainValidationProfile))
			{
				mainValidationSessionHandle.Dispose();
				mainValidationSessionHandle = null;
			}
			if (mainValidationSessionHandle == null && (bool)keepAlive)
			{
				mainValidationSessionHandle = ValidationProfile.MainValidationProfile.ClaimSessionHandle();
				ValidationSession session = mainValidationSessionHandle.Session;
				if (!IsHeadlessOrBatchMode)
				{
					session.StartSession(GlobalConfig<GlobalValidationConfig>.Instance.WatchForChanges, GlobalConfig<GlobalValidationConfig>.Instance.ValidateInBackground);
				}
				ToggleBackgroundValidation(PlayModeStateChange.EnteredEditMode);
			}
		}

		private static void ToggleBackgroundValidation(PlayModeStateChange change)
		{
			if (IsHeadlessOrBatchMode)
			{
				return;
			}
			foreach (ValidationSessionEditor item in ValidationSessionEditor.ActiveEditors)
			{
				if (item?.Window != null)
				{
					item.Window.Repaint();
				}
			}
			if (change == PlayModeStateChange.EnteredPlayMode || Application.isPlaying)
			{
				foreach (ValidationSession session in ActiveValidationSessions)
				{
					session.Clear(clearResults: false, clearQueue: true);
					session.StopSession();
				}
				return;
			}
			if (change != PlayModeStateChange.EnteredEditMode)
			{
				return;
			}
			foreach (ValidationSession session2 in ActiveValidationSessions)
			{
				session2.Clear(clearResults: false, clearQueue: true);
				if ((bool)GlobalConfig<GlobalValidationConfig>.Instance.ValidateMainProfileOnLoad && (bool)GlobalConfig<GlobalValidationConfig>.Instance.KeepMainValidationSessionAliveInBackground)
				{
					session2.PopulateQueue(clearCurrentQueue: true, populateUnloadedScenes: false, GlobalConfig<GlobalValidationConfig>.Instance.QueueScenesOnLoad, GlobalConfig<GlobalValidationConfig>.Instance.QueueAssetsOnLoad);
				}
				session2.StartSession(GlobalConfig<GlobalValidationConfig>.Instance.WatchForChanges, GlobalConfig<GlobalValidationConfig>.Instance.ValidateInBackground);
			}
		}

		public PersistentValidationResult[] GetCurrentResults(bool applyFilters = false)
		{
			ValidationSessionResultCollector.ResultItemSingle[] items = (applyFilters ? Results.GetFilteredItems() : Results.GetAllItems());
			PersistentValidationResult[] results = new PersistentValidationResult[items.Length];
			for (int i = 0; i < items.Length; i++)
			{
				results[i] = items[i].Result;
			}
			return results;
		}

		public ValidationSessionEditor OpenEditor()
		{
			foreach (ValidationSessionEditor item in ValidationSessionEditor.ActiveEditors)
			{
				if (item.ValidationSession == this)
				{
					item.Window.Show();
					item.Window.Focus();
					return item;
				}
			}
			return OdinValidatorWindow.OpenWindow(this, disposeSessionOnWindowDestroy: false);
		}

		public ValidationSession(ValidationProfile profile)
			: this(profile.name, new SessionConfig(profile))
		{
		}

		public ValidationSession(string name, IValidationProfile[] profiles)
			: this(name, new SessionConfig(profiles))
		{
		}

		public ValidationSession(string sessionName, params ValidationItem[] include)
			: this(sessionName, new SessionConfig(new SessionConfig.SerializableSessionConfigData(include, null)))
		{
		}

		public ValidationSession(string sessionName, IList<ValidationItem> include, IList<ValidationItem> exclude)
			: this(sessionName, new SessionConfig(new SessionConfig.SerializableSessionConfigData(include, exclude)))
		{
		}

		public ValidationSession(string sessionName, SessionConfig config)
		{
			if (config == null)
			{
				throw new ArgumentNullException("config");
			}
			Runner = new OdinValidationRunner();
			Name = sessionName;
			Config = config;
			Results = new ValidationSessionResultCollector(this);
			if ((bool)GlobalConfig<GlobalValidationConfig>.Instance.EnableLeakDetection)
			{
				allocationStackTrace = new StackTrace(fNeedFileInfo: true);
			}
			ActiveValidationSessions.Add(this);
		}

		public void ValidateQueuedUpWorkNow(bool showProgressBar = true, bool processResultQueue = true)
		{
			Stopwatch sw = Stopwatch.StartNew();
			bool enableWatchingAgain = IsWatching;
			try
			{
				if (IsWatching)
				{
					StopSession(stopWatching: true, stopBackgroundValidation: false);
				}
				uint workDoneThisSecond = 0u;
				int currentSecond = -1;
				int prevWorkDonePerSecond = 0;
				PersistentValidationResultBatch latest = null;
				while (removedObjectResults.Count != 0 || WorkQueue.Count != 0 || currentlyBackgroundProcessingWorkItemEnumerator != null)
				{
					while (removedObjectResults.Count > 0)
					{
						ValidationSessionResult e = removedObjectResults.Dequeue();
						latest = e.Result ?? latest;
						Results.Enqueue(e);
					}
					if (currentlyBackgroundProcessingWorkItemEnumerator == null && WorkQueue.Count > 0)
					{
						currentlyBackgroundProcessingWorkItemEnumerator = ProcessWorkItem(WorkQueue.Dequeue(), populateResults: true);
					}
					if (currentlyBackgroundProcessingWorkItemEnumerator != null)
					{
						if (currentlyBackgroundProcessingWorkItemEnumerator.MoveNext())
						{
							latest = currentlyBackgroundProcessingWorkItemEnumerator.Current.Result ?? latest;
						}
						else
						{
							currentlyBackgroundProcessingWorkItemEnumerator?.Dispose();
							currentlyBackgroundProcessingWorkItemEnumerator = null;
						}
					}
					if (sw.Elapsed.Seconds != currentSecond)
					{
						prevWorkDonePerSecond = (int)((workDoneCountSample - workDoneThisSecond) / 1);
						workDoneThisSecond = workDoneCountSample;
						currentSecond = sw.Elapsed.Seconds;
					}
					if (showProgressBar && GUIHelper.ShouldDisplaySmartCancellableProgressBar())
					{
						string title = $"Validating: {workDoneCountSample} / {remainingWorkCountSample}, {prevWorkDonePerSecond}/s";
						string name = latest?.DynamicObjectAddress?.LatestAddress?.Name;
						float t = CalculateCurrentValidationProgress();
						string details = "Object: " + name;
						if (GUIHelper.DisplaySmartUpdatingCancellableProgressBar(title, details, t))
						{
							break;
						}
					}
				}
			}
			finally
			{
				if (showProgressBar)
				{
					EditorUtility.ClearProgressBar();
				}
				currentlyBackgroundProcessingWorkItemEnumerator?.Dispose();
				currentlyBackgroundProcessingWorkItemEnumerator = null;
				StartSession(enableWatchingAgain, startBackgroundValidation: false);
				if (Results != null && processResultQueue)
				{
					Results.ProcessQueue();
				}
			}
		}

		internal IEnumerable<PersistentValidationResultBatch> ValidateEverythingEnumeratorBatched(bool openClosedScenes, bool showProgressBar, bool populateResults)
		{
			Stopwatch sw = Stopwatch.StartNew();
			bool enableValidationAgain = IsValidatingInBackground;
			bool enableWatchingAgain = IsWatching;
			int ignoreCount = 0;
			int errorCount = 0;
			int warningCount = 0;
			uint workDone = 0u;
			int unloadUnusedAssetsCounter = 0;
			uint workDoneThisSecond = 0u;
			int currentSecond = -1;
			int prevWorkDonePerSecond = 0;
			try
			{
				StopSession();
				PopulateQueue(clearCurrentQueue: true, openClosedScenes);
				foreach (ValidationSessionResult item in ValidateCurrentWorkQueueEnumerator(populateResults, openClosedScenes))
				{
					workDone = Math.Max(workDone, workDoneCountSample);
					if (item.Type == ValidationSessionResult.ValidationSessionResultType.Ignore)
					{
						ignoreCount++;
					}
					if (item.Result == null)
					{
						continue;
					}
					warningCount += item.Result.WarningCount;
					errorCount += item.Result.ErrorCount;
					yield return item.Result;
					int count10s = sw.Elapsed.Seconds % 10;
					if (unloadUnusedAssetsCounter != count10s)
					{
						unloadUnusedAssetsCounter = count10s;
						Resources.UnloadUnusedAssets();
					}
					if (!showProgressBar)
					{
						continue;
					}
					if (sw.Elapsed.Seconds != currentSecond)
					{
						prevWorkDonePerSecond = (int)((workDoneCountSample - workDoneThisSecond) / 1);
						workDoneThisSecond = workDoneCountSample;
						currentSecond = sw.Elapsed.Seconds;
					}
					if (GUIHelper.ShouldDisplaySmartCancellableProgressBar())
					{
						string title = $"Validating: {workDoneCountSample} / {remainingWorkCountSample}, {prevWorkDonePerSecond}/s";
						string name = item.Result?.DynamicObjectAddress?.LatestAddress?.Name;
						float t = Mathf.Clamp01((float)workDoneCountSample / (float)remainingWorkCountSample);
						string details = $"Errors: {errorCount}, warnings: {warningCount}, object: {name}";
						if (GUIHelper.DisplaySmartUpdatingCancellableProgressBar(title, details, t))
						{
							break;
						}
					}
				}
				if (Results != null)
				{
					Results.ProcessQueue();
				}
			}
			finally
			{
				ValidationSession validationSession = this;
				sw.Stop();
				UnityEngine.Debug.Log($"Completed validating {workDone} values in {sw.Elapsed.TotalSeconds} seconds.");
				validationSession.Clear(clearResults: false, clearQueue: true);
				if (showProgressBar)
				{
					EditorUtility.ClearProgressBar();
				}
				validationSession.StartSession(enableWatchingAgain, enableValidationAgain);
				ProjectWatcher.RestartWatching(skipLoadEvents: true);
				Resources.UnloadUnusedAssets();
			}
		}

		public IEnumerable<PersistentValidationResult> ValidateEverythingEnumerator(bool openClosedScenes = true, bool showProgressBar = false)
		{
			foreach (PersistentValidationResultBatch batch in ValidateEverythingEnumeratorBatched(openClosedScenes, showProgressBar, populateResults: true))
			{
				if (batch.Count <= 0)
				{
					continue;
				}
				foreach (PersistentValidationResult item in batch.Explode())
				{
					yield return item;
				}
			}
		}

		public IEnumerable<PersistentValidationResultBatch> ValidateEverythingEnumeratorBatched(bool openClosedScenes = true, bool showProgressBar = false)
		{
			return ValidateEverythingEnumeratorBatched(openClosedScenes, showProgressBar, populateResults: true);
		}

		internal void ValidateEverythingNow(bool openClosedScenes, bool showProgressBar)
		{
			IEnumerator<PersistentValidationResultBatch> e = ValidateEverythingEnumeratorBatched(openClosedScenes, showProgressBar, populateResults: true).GetEnumerator();
			while (e.MoveNext())
			{
			}
		}

		public void Clear(bool clearResults, bool clearQueue)
		{
			if (clearQueue)
			{
				WorkQueue.Clear();
				WorkDone.Clear();
				currentlyBackgroundProcessingWorkItemEnumerator?.Dispose();
				currentlyBackgroundProcessingWorkItemEnumerator = null;
				currentlyProcessingWorkItem = null;
				remainingWorkCountSample = 0u;
				workDoneCountSample = 0u;
				shouldDisplayProgressBar = false;
				if (IsValidatingInBackground)
				{
					StopSession(stopWatching: false);
					StartSession(startWatching: false);
				}
			}
			if (clearResults)
			{
				Results.Clear();
			}
		}

		public IEnumerable<ValidationSessionResult> ValidateCurrentWorkQueueEnumerator(bool populateResults, bool openClosedScenes)
		{
			while (removedObjectResults.Count > 0)
			{
				ValidationSessionResult e = removedObjectResults.Dequeue();
				if (populateResults)
				{
					Results?.Enqueue(e);
				}
				yield return e;
			}
			SceneSetup[] setupToRestore = null;
			foreach (SceneReference allQueuedSceneReference in WorkQueue.AllQueuedSceneReferences)
			{
				if (!allQueuedSceneReference.IsLoaded)
				{
					if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
					{
						break;
					}
					yield break;
				}
			}
			try
			{
				while (WorkQueue.Count > 0)
				{
					ValidationWorkItem workItem = WorkQueue.Dequeue();
					SceneReference? sceneRefNullable = workItem.GetSceneReference();
					if (sceneRefNullable.HasValue)
					{
						SceneReference sceneRef = sceneRefNullable.Value;
						if (sceneRef.IsLoaded)
						{
							if (!sceneRef.TryGetScene(out var _))
							{
								UnityEngine.Debug.LogError("Scene was loaded but could not be found: '" + sceneRef.Path + "'");
								continue;
							}
						}
						else
						{
							if (!openClosedScenes)
							{
								continue;
							}
							if (setupToRestore == null)
							{
								setupToRestore = EditorSceneManager.GetSceneManagerSetup();
							}
							if (!sceneRef.TryOpenScene(OpenSceneMode.Single, out var _))
							{
								continue;
							}
						}
					}
					using (IEnumerator<ValidationSessionResult> enumerator2 = ProcessWorkItem(workItem, populateResults))
					{
						while (enumerator2.MoveNext())
						{
							yield return enumerator2.Current;
						}
					}
					while (removedObjectResults.Count > 0)
					{
						ValidationSessionResult e2 = removedObjectResults.Dequeue();
						if (populateResults)
						{
							Results?.Enqueue(e2);
						}
						yield return e2;
					}
				}
				while (removedObjectResults.Count > 0)
				{
					ValidationSessionResult e3 = removedObjectResults.Dequeue();
					if (populateResults)
					{
						Results?.Enqueue(e3);
					}
					yield return e3;
				}
			}
			finally
			{
				if (setupToRestore != null && setupToRestore.Length != 0)
				{
					EditorSceneManager.RestoreSceneManagerSetup(setupToRestore);
				}
			}
		}

		public void PopulateQueue(bool clearCurrentQueue, bool populateUnloadedScenes, bool queueScenes = true, bool queueAssets = true, bool queueGlobalValidators = true)
		{
			if (clearCurrentQueue)
			{
				Clear(clearResults: false, clearQueue: true);
			}
			if (queueScenes)
			{
				HashSet<string> sceneGuids = Config.GetSceneGuidsToValidate();
				foreach (string item in sceneGuids)
				{
					SceneReference sceneRef = new SceneReference(item);
					if (populateUnloadedScenes)
					{
						AddToQueue(ValidationWorkItem.CreateForSceneValidators(sceneRef, ProjectEventSource.Other), insertFirst: false);
						AddToQueue(ValidationWorkItem.CreateForSceneContent(sceneRef, ProjectEventSource.Other), insertFirst: false);
					}
					else if (sceneRef.IsLoaded)
					{
						AddToQueue(ValidationWorkItem.CreateForSceneValidators(sceneRef, ProjectEventSource.Other), insertFirst: true);
						AddToQueue(ValidationWorkItem.CreateForSceneContent(sceneRef, ProjectEventSource.Other), insertFirst: true);
					}
				}
			}
			if (queueAssets)
			{
				HashSet<UnityEngine.Object> objs = Config.GetObjectsToValidate();
				foreach (UnityEngine.Object uObj in objs)
				{
					if ((object)uObj != null)
					{
						ValidationWorkItem key = ValidationWorkItem.CreateForEntityId(OdinEntityId.FromObject(uObj), ProjectEventSource.Other);
						AddToQueue(key, insertFirst: false);
					}
				}
				HashSet<string> assetsToValidate = Config.GetAssetsToValidate();
				foreach (string guid in assetsToValidate)
				{
					ValidationWorkItem key2 = ValidationWorkItem.CreateForAssetGuid(guid, ProjectEventSource.Other);
					AddToQueue(key2, insertFirst: false);
				}
			}
			if (queueGlobalValidators)
			{
				IList<GlobalValidator> globalValidators = DefaultValidatorLocator.Instance.GetGlobalValidators();
				foreach (GlobalValidator validator in globalValidators)
				{
					ValidationWorkItem key3 = ValidationWorkItem.CreateForGlobalValidator(validator, ProjectEventSource.Other);
					AddToQueue(key3, insertFirst: false);
				}
			}
			PrepareNextProgressBar(showProgressBar: true);
		}

		public void StartSession(bool startWatching = true, bool startBackgroundValidation = true)
		{
			if (startWatching && !IsWatching)
			{
				ProjectWatcher.OnProjectEvent += OnProjectEvent;
				IsWatching = true;
			}
			if (!startBackgroundValidation || IsValidatingInBackground)
			{
				return;
			}
			if (backgroundTaskHandle != null)
			{
				if (backgroundTaskHandle.IsPaused)
				{
					backgroundTaskHandle.Continue();
				}
			}
			else
			{
				backgroundTaskHandle = BackgroundTaskRunner.StartTask(Name, BackgroundValidationTicker());
			}
			IsValidatingInBackground = true;
		}

		public void StopSession(bool stopWatching = true, bool stopBackgroundValidation = true)
		{
			if (stopWatching && IsWatching)
			{
				ProjectWatcher.OnProjectEvent -= OnProjectEvent;
				IsWatching = false;
			}
			if (!stopBackgroundValidation || !IsValidatingInBackground)
			{
				return;
			}
			if (backgroundTaskHandle != null)
			{
				if (backgroundTaskHandle.IsAlive)
				{
					backgroundTaskHandle.Kill();
				}
				backgroundTaskHandle = null;
			}
			IsValidatingInBackground = false;
		}

		public void RestartBackgroundValidation()
		{
			Clear(clearResults: true, clearQueue: true);
			PopulateQueue(clearCurrentQueue: true, populateUnloadedScenes: false);
			if (!IsValidatingInBackground)
			{
				StartSession();
			}
		}

		public float CalculateCurrentValidationProgress()
		{
			uint remaining = remainingWorkCountSample;
			if (remaining == 0)
			{
				return 0f;
			}
			uint workDone = workDoneCountSample;
			return Mathf.Clamp01((float)workDone / (float)remaining);
		}

		internal void Enqueue(ValidationWorkItem workItem, bool insert)
		{
			if (!Application.isPlaying)
			{
				AddToQueue(workItem, insert);
			}
		}

		internal void Enqueue(ProjectEvent e, bool insert)
		{
			if (Application.isPlaying)
			{
				return;
			}
			if (e.Type == ProjectEventType.SceneLoaded)
			{
				if (!GlobalConfig<GlobalValidationConfig>.Instance.ValidateScenesOnSceneLoad || !GlobalConfig<GlobalValidationConfig>.Instance.KeepMainValidationSessionAliveInBackground)
				{
					return;
				}
				string assetGuid = e.AssetGuid ?? e.Scene.GUID;
				if (assetGuid == null)
				{
					return;
				}
				Config.UpdateScenes();
				HashSet<string> scenesToValidate = Config.GetSceneGuidsToValidate();
				if (scenesToValidate.Contains(assetGuid))
				{
					SceneReference sceneRef = new SceneReference(assetGuid);
					ValidationWorkItem w1 = ValidationWorkItem.CreateForSceneContent(sceneRef, ProjectEventSource.Other);
					ValidationWorkItem w2 = ValidationWorkItem.CreateForSceneValidators(sceneRef, ProjectEventSource.Other);
					if (!WorkDone.Contains(w1))
					{
						AddToQueue(w1, insert);
					}
					if (!WorkDone.Contains(w2))
					{
						AddToQueue(w2, insert);
					}
				}
			}
			else if (e.Type == ProjectEventType.Revalidation)
			{
				if (e.EntityId.IsValid)
				{
					if (Config.ShouldValidateUnityObjectReference(e.EntityId))
					{
						ValidationWorkItem workItem = ValidationWorkItem.CreateForEntityId(e.EntityId, e.Source);
						AddToQueue(workItem, insert);
					}
				}
				else if (e.Scene.GUID != null)
				{
					HashSet<string> scenesToValidate2 = Config.GetSceneGuidsToValidate();
					if (scenesToValidate2.Contains(e.Scene.GUID))
					{
						SceneReference sceneRef2 = new SceneReference(e.Scene.GUID);
						AddToQueue(ValidationWorkItem.CreateForSceneContent(sceneRef2, ProjectEventSource.OdinValidationEvents), insert);
						AddToQueue(ValidationWorkItem.CreateForSceneValidators(sceneRef2, ProjectEventSource.Other), insert);
					}
				}
			}
			else if (e.Type == ProjectEventType.AssetImported || e.Type == ProjectEventType.AssetModified || e.Type == ProjectEventType.AssetMoved || e.Type == ProjectEventType.AssetRemoved)
			{
				ValidationWorkItem workItem2 = ValidationWorkItem.CreateForAssetGuid(e.AssetGuid, e.EntityId, e.Source);
				if (e.Type == ProjectEventType.AssetImported || e.Type == ProjectEventType.AssetModified || e.Type == ProjectEventType.AssetMoved)
				{
					HashSet<string> assetsToValidate = Config.GetAssetsToValidate();
					if (assetsToValidate.Contains(e.AssetGuid))
					{
						AddToQueue(workItem2, insert);
					}
				}
				else if (e.Type == ProjectEventType.AssetRemoved)
				{
					WorkDone.Remove(workItem2);
					WorkQueue.Remove(workItem2);
					if (currentlyProcessingWorkItem.HasValue && currentlyProcessingWorkItem.Value == workItem2)
					{
						currentlyProcessingWorkItem = null;
						currentlyBackgroundProcessingWorkItemEnumerator?.Dispose();
						currentlyBackgroundProcessingWorkItemEnumerator = null;
					}
					removedObjectResults.Enqueue(new ValidationSessionResult
					{
						WorkItem = workItem2,
						Result = null,
						Type = ValidationSessionResult.ValidationSessionResultType.ObjectDeleted
					});
					if ((bool)GlobalConfig<GlobalValidationConfig>.Instance.PopulateQueueOnAssetDeleted)
					{
						PopulateQueue(clearCurrentQueue: false, populateUnloadedScenes: false);
					}
				}
			}
			else
			{
				if (e.Type != ProjectEventType.SceneObjectCreated && e.Type != ProjectEventType.SceneObjectModified && e.Type != ProjectEventType.SceneObjectDeleted)
				{
					return;
				}
				ValidationWorkItem workItem3 = ValidationWorkItem.CreateForEntityId(e.EntityId, e.Source);
				if (e.Type == ProjectEventType.SceneObjectDeleted)
				{
					WorkDone.Remove(workItem3);
					WorkQueue.Remove(workItem3);
					if (currentlyProcessingWorkItem.HasValue && currentlyProcessingWorkItem.Value == workItem3)
					{
						currentlyProcessingWorkItem = null;
						currentlyBackgroundProcessingWorkItemEnumerator?.Dispose();
						currentlyBackgroundProcessingWorkItemEnumerator = null;
					}
					removedObjectResults.Enqueue(new ValidationSessionResult
					{
						WorkItem = workItem3,
						Result = null,
						Type = ValidationSessionResult.ValidationSessionResultType.ObjectDeleted
					});
					if ((bool)GlobalConfig<GlobalValidationConfig>.Instance.PopulateQueueOnGameObjectDeleted)
					{
						if (e.Scene.IsValid)
						{
							OnProjectEvent(new ProjectEvent[1]
							{
								new ProjectEvent
								{
									Scene = e.Scene,
									Type = ProjectEventType.Revalidation,
									Source = ProjectEventSource.Other
								}
							});
						}
						else
						{
							PopulateQueue(clearCurrentQueue: false, populateUnloadedScenes: false, queueScenes: true, queueAssets: false);
						}
					}
					return;
				}
				UnityEngine.Object uObj = e.UnityObject;
				GameObject go = uObj as GameObject;
				Component cmp = uObj as Component;
				if (!go && (bool)cmp)
				{
					go = cmp.gameObject;
				}
				if (!go || !go.scene.IsValid() || string.IsNullOrEmpty(go.scene.path))
				{
					return;
				}
				string guid = AssetDatabase.AssetPathToGUID(go.scene.path);
				if (!Config.ShouldValidateScene(guid))
				{
					return;
				}
				if (e.Type == ProjectEventType.SceneObjectModified && (e.Source == ProjectEventSource.UndoOrRedoModification || e.Source == ProjectEventSource.UndoOrRedo) && cmp is Transform)
				{
					Transform[] transforms = go.GetComponentsInChildren<Transform>();
					Transform[] array = transforms;
					foreach (Transform trs in array)
					{
						Component[] components = trs.GetComponents(typeof(Component));
						foreach (Component item in components)
						{
							if ((bool)item)
							{
								AddToQueue(ValidationWorkItem.CreateForEntityId(OdinEntityId.FromObject(item), e.Source), insert);
							}
						}
						AddToQueue(ValidationWorkItem.CreateForEntityId(OdinEntityId.FromObject(trs.gameObject), e.Source), insert);
					}
				}
				else
				{
					AddToQueue(workItem3, insert);
				}
			}
		}

		private void AddToQueue(ValidationWorkItem workItem, bool insertFirst)
		{
			if (!(currentlyProcessingWorkItem != workItem))
			{
				return;
			}
			if (insertFirst)
			{
				if (WorkQueue.InsertFirst(workItem))
				{
					remainingWorkCountSample += workItem.ResultCountEstimate;
				}
			}
			else if (WorkQueue.Enqueue(workItem))
			{
				remainingWorkCountSample += workItem.ResultCountEstimate;
			}
		}

		protected virtual void Dispose(bool finalizer)
		{
			if (IsDisposed)
			{
				return;
			}
			if (finalizer)
			{
				if (allocationStackTrace != null)
				{
					UnityEngine.Debug.LogWarning("An Odin ValidationSession instance is being garbage collected without first having been disposed. ValidationSession instances must be disposed once they are no longer needed. This instance was allocated at the following location: \n\n" + allocationStackTrace.ToString());
				}
				UnityEditorEventUtility.DelayActionThreadSafe(ActuallyDispose);
			}
			else
			{
				ActuallyDispose();
			}
		}

		private void OnProjectEvent(ProjectEvent[] projectEvent)
		{
			if (Application.isPlaying)
			{
				return;
			}
			for (int i = 0; i < projectEvent.Length; i++)
			{
				ProjectEvent item = projectEvent[i];
				if (item.Type == ProjectEventType.AssetImported || item.Type == ProjectEventType.AssetRemoved)
				{
					Config.UpdateAll();
					break;
				}
			}
			foreach (ProjectEvent e in projectEvent)
			{
				Enqueue(e, insert: true);
			}
		}

		private void PrepareNextProgressBar(bool showProgressBar)
		{
			remainingWorkCountSample = WorkQueue.TotalResultsEstimate;
			workDoneCountSample = 0u;
			shouldDisplayProgressBar = showProgressBar;
			if (currentlyProcessingWorkItem.HasValue)
			{
				remainingWorkCountSample += currentlyProcessingWorkItem.Value.ResultCountEstimate;
			}
		}

		private void ActuallyDispose()
		{
			if (!IsDisposed)
			{
				ActiveValidationSessions.Remove(this);
				StopSession();
				Clear(clearResults: true, clearQueue: true);
				Runner?.Dispose();
				Results?.Dispose();
				IsDisposed = true;
				allocationStackTrace = null;
			}
		}

		private IEnumerator<ValidationSessionResult> ProcessWorkItem(ValidationWorkItem workItem, bool populateResults)
		{
			ValidationSessionResult[] batch = new ValidationSessionResult[100];
			int batchIndex = 0;
			DynamicObjectAddress prevBatchObj = null;
			currentlyProcessingWorkItem = workItem;
			try
			{
				uint workEstimate = workItem.ResultCountEstimate;
				uint currentWorkItemResultCountSoFar = 0u;
				if (workItem.NonUnityObjectValue != null)
				{
					IEnumerator<PersistentValidationResultBatch> enumerator = Runner.ValidateObject(workItem.NonUnityObjectValue).GetEnumerator();
					while (true)
					{
						try
						{
							if (!enumerator.MoveNext())
							{
								break;
							}
						}
						catch (OdinValidationRunner.AssetUnloadedWhileValidatingException exception)
						{
							UnityEngine.Debug.LogException(exception);
							break;
						}
						IncrementWorkDone();
						PersistentValidationResultBatch result = enumerator.Current;
						if (result == null || result.HighestSeverityResult.ResultType == ValidationResultType.IgnoreResult)
						{
							yield return new ValidationSessionResult
							{
								Type = ValidationSessionResult.ValidationSessionResultType.Ignore
							};
							continue;
						}
						ValidationSessionResult e = new ValidationSessionResult
						{
							Result = enumerator.Current,
							Type = ValidationSessionResult.ValidationSessionResultType.ResultAddedOrChanged,
							WorkItem = workItem
						};
						AddToBatch(e);
						yield return e;
					}
					CompleteBatch();
				}
				else if (workItem.AssetGuid != null)
				{
					string path = AssetDatabase.GUIDToAssetPath(workItem.AssetGuid);
					foreach (PersistentValidationResultBatch result2 in Runner.ValidateAllAssetsAtPath(path))
					{
						IncrementWorkDone();
						if (result2 == null || result2.HighestSeverityResult.ResultType == ValidationResultType.IgnoreResult)
						{
							yield return new ValidationSessionResult
							{
								Type = ValidationSessionResult.ValidationSessionResultType.Ignore
							};
							continue;
						}
						ValidationSessionResult r = new ValidationSessionResult
						{
							Result = result2,
							Type = ValidationSessionResult.ValidationSessionResultType.ResultAddedOrChanged,
							WorkItem = workItem
						};
						AddToBatch(r);
						yield return r;
					}
					CompleteBatch();
					if (!string.IsNullOrWhiteSpace(workItem.AssetGuid))
					{
						WorkItemResultCountCache.RegisterWorkItemResultCount(workItem.AssetGuid, currentWorkItemResultCountSoFar);
					}
				}
				else if (workItem.SceneValidators.HasValue)
				{
					if (workItem.SceneValidators.Value.IsLoaded)
					{
						IEnumerable<PersistentValidationResultBatch> results = Runner.ValidateSceneValidators(workItem.SceneValidators.Value);
						foreach (PersistentValidationResultBatch result3 in results)
						{
							IncrementWorkDone();
							if (result3 == null || result3.HighestSeverityResult.ResultType == ValidationResultType.IgnoreResult)
							{
								yield return new ValidationSessionResult
								{
									Type = ValidationSessionResult.ValidationSessionResultType.Ignore
								};
								continue;
							}
							ValidationSessionResult r2 = new ValidationSessionResult
							{
								Result = result3,
								Type = ValidationSessionResult.ValidationSessionResultType.ResultAddedOrChanged,
								WorkItem = workItem
							};
							AddToBatch(r2);
							yield return r2;
						}
						CompleteBatch();
					}
					else
					{
						workDoneCountSample++;
					}
				}
				else if (workItem.SceneContent.HasValue)
				{
					uint count;
					if (workItem.SceneContent.Value.IsLoaded)
					{
						IEnumerable<PersistentValidationResultBatch> results2 = Runner.ValidateSceneContent(workItem.SceneContent.Value);
						foreach (PersistentValidationResultBatch result4 in results2)
						{
							IncrementWorkDone();
							if (result4 == null || result4.HighestSeverityResult.ResultType == ValidationResultType.IgnoreResult)
							{
								yield return new ValidationSessionResult
								{
									Type = ValidationSessionResult.ValidationSessionResultType.Ignore
								};
								continue;
							}
							ValidationSessionResult r3 = new ValidationSessionResult
							{
								Result = result4,
								Type = ValidationSessionResult.ValidationSessionResultType.ResultAddedOrChanged,
								WorkItem = ValidationWorkItem.CreateForEntityId(result4.DynamicObjectAddress.LatestEntityId, workItem.Source)
							};
							AddToBatch(r3);
							yield return r3;
						}
						CompleteBatch();
						if (!string.IsNullOrWhiteSpace(workItem.SceneContent.Value.GUID))
						{
							WorkItemResultCountCache.RegisterWorkItemResultCount(workItem.SceneContent.Value.GUID, currentWorkItemResultCountSoFar);
						}
					}
					else if (WorkItemResultCountCache.TryGetLastWorkItemResultCount(workItem.SceneContent.Value.GUID, out count))
					{
						workDoneCountSample += count;
					}
					else
					{
						workDoneCountSample++;
					}
				}
				else if (workItem.EntityId.IsValid)
				{
					UnityEngine.Object obj = workItem.EntityId.ToObject();
					if ((object)obj == null)
					{
						yield break;
					}
					foreach (PersistentValidationResultBatch result5 in Runner.ValidateObject(obj))
					{
						IncrementWorkDone();
						if (result5 == null || result5.HighestSeverityResult.ResultType == ValidationResultType.IgnoreResult)
						{
							yield return new ValidationSessionResult
							{
								Type = ValidationSessionResult.ValidationSessionResultType.Ignore
							};
							continue;
						}
						ValidationSessionResult r4 = new ValidationSessionResult
						{
							Result = result5,
							Type = ValidationSessionResult.ValidationSessionResultType.ResultAddedOrChanged,
							WorkItem = workItem
						};
						AddToBatch(r4);
						yield return r4;
					}
					CompleteBatch();
				}
				else if (workItem.GlobalValidator != null)
				{
					foreach (ValidationResult result6 in Runner.ValidateGlobalValidator(workItem.GlobalValidator))
					{
						if (result6 != null)
						{
							ValidationSessionResult r5 = new ValidationSessionResult
							{
								Result = new PersistentValidationResultBatch(result6, DynamicObjectAddress.Unknown),
								Type = ValidationSessionResult.ValidationSessionResultType.ResultAddedOrChanged,
								WorkItem = workItem
							};
							AddToBatch(r5);
							yield return r5;
						}
						else
						{
							yield return new ValidationSessionResult
							{
								Type = ValidationSessionResult.ValidationSessionResultType.Ignore
							};
						}
					}
					CompleteBatch();
				}
				else
				{
					UnityEngine.Debug.LogError("There was no work to be done in a work item?");
				}
				void IncrementWorkDone()
				{
					workDoneCountSample++;
					currentWorkItemResultCountSoFar++;
					if (currentWorkItemResultCountSoFar > workEstimate)
					{
						remainingWorkCountSample++;
					}
				}
			}
			finally
			{
				ValidationSession validationSession = this;
				validationSession.WorkDone.Add(workItem);
				if (validationSession.WorkQueue.Count == 0)
				{
					validationSession.PrepareNextProgressBar(showProgressBar: false);
				}
				validationSession.prevProcessedWorkItem = workItem;
				validationSession.currentlyProcessingWorkItem = null;
			}
			void AddToBatch(ValidationSessionResult validationSessionResult)
			{
				if (populateResults && validationSessionResult.Type != ValidationSessionResult.ValidationSessionResultType.Ignore)
				{
					if (validationSessionResult.Type == ValidationSessionResult.ValidationSessionResultType.ObjectDeleted)
					{
						CompleteBatch();
					}
					else if (validationSessionResult.Result.DynamicObjectAddress != prevBatchObj)
					{
						CompleteBatch();
					}
					prevBatchObj = validationSessionResult.Result?.DynamicObjectAddress;
					if (batchIndex >= batch.Length)
					{
						Array.Resize(ref batch, batchIndex * 2);
					}
					batch[batchIndex++] = validationSessionResult;
				}
			}
			void CompleteBatch()
			{
				if (populateResults && batchIndex > 0)
				{
					Results.Enqueue(new ValidationSessionResultCollector.BatchKey(batch[0].Result), new ArraySlice<ValidationSessionResult>(batch, 0, batchIndex));
					batchIndex = 0;
				}
			}
		}

		private IEnumerator BackgroundValidationTicker()
		{
			while (true)
			{
				if (removedObjectResults.Count == 0 && WorkQueue.Count == 0 && currentlyBackgroundProcessingWorkItemEnumerator == null)
				{
					yield return BackgroundTaskRunner.Relax;
					continue;
				}
				while (removedObjectResults.Count > 0)
				{
					ValidationSessionResult removed = removedObjectResults.Dequeue();
					Results?.Enqueue(removed);
					yield return null;
				}
				if (currentlyBackgroundProcessingWorkItemEnumerator == null && WorkQueue.Count > 0)
				{
					currentlyBackgroundProcessingWorkItemEnumerator = ProcessWorkItem(WorkQueue.Dequeue(), populateResults: true);
				}
				if (currentlyBackgroundProcessingWorkItemEnumerator != null)
				{
					if (currentlyBackgroundProcessingWorkItemEnumerator.MoveNext())
					{
						yield return null;
						continue;
					}
					currentlyBackgroundProcessingWorkItemEnumerator?.Dispose();
					currentlyBackgroundProcessingWorkItemEnumerator = null;
				}
			}
		}

		public override string ToString()
		{
			return Name;
		}

		public void Dispose()
		{
			Dispose(finalizer: false);
		}

		~ValidationSession()
		{
			if (!IsDisposed)
			{
				Dispose(finalizer: true);
			}
		}
	}
}
