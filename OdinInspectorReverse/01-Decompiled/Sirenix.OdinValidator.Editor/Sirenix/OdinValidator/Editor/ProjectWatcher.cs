using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.Validation;
using Sirenix.Reflection.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Sirenix.OdinValidator.Editor
{
	public static class ProjectWatcher
	{
		private class SceneMonitorEnumerator
		{
			public HashSet<OdinEntityId> AllLoadedObjectIds;

			public bool WasPaused;

			public BackgroundTaskHandle Task;
		}

		private class AssetModificationWatcher_AssetPostprocessor : AssetPostprocessor
		{
			private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
			{
				if (deletedAssets.Length != 0)
				{
					deletedAssets = deletedAssets.Where((string x) => !x.StartsWith("Assets/__DELETED_GUID_Trash/")).ToArray();
				}
				foreach (string path in importedAssets)
				{
					importedAssetsBuffer.Add(path);
				}
				string[] array = deletedAssets;
				foreach (string path2 in array)
				{
					deletedAssetsBuffer.Add(path2);
				}
				for (int i = 0; i < movedAssets.Length; i++)
				{
					string from = movedFromAssetPaths[i];
					string to = movedAssets[i];
					movedAssetsBuffer.Add((from, to));
				}
			}
		}

		private static bool isInitialized;

		private static int hierarchyWindowChangedCount;

		private static List<string> importedAssetsBuffer = new List<string>();

		private static List<string> deletedAssetsBuffer = new List<string>();

		private static List<(string, string)> movedAssetsBuffer = new List<(string, string)>();

		private static List<BackgroundTaskHandle> taskHandles = new List<BackgroundTaskHandle>();

		private static Action<ProjectEvent[]> onProjectEvent;

		internal static bool boostWatchSpeed = false;

		internal static CircularBuffer<ProjectEvent> latestEvents = new CircularBuffer<ProjectEvent>(10);

		public static event Action<ProjectEvent[]> OnProjectEvent
		{
			add
			{
				if (!isInitialized)
				{
					onProjectEvent = (Action<ProjectEvent[]>)Delegate.Combine(onProjectEvent, value);
					isInitialized = true;
					EditorApplication.hierarchyChanged += delegate
					{
						hierarchyWindowChangedCount++;
					};
					EditorApplication.update = (EditorApplication.CallbackFunction)Delegate.Combine(EditorApplication.update, new EditorApplication.CallbackFunction(MonitorAssetProcessorChanges));
					UndoTracker.OnRedoPerformed += OnUndoOrRedoPerformed;
					UndoTracker.OnUndoPerformed += OnUndoOrRedoPerformed;
					UndoTracker.OnObjectValueModified += UndoTracker_OnObjectValueModified;
					ValidationEvents.OnValidationStateChanged += OnValidationStateChanged;
					RestartWatching(skipLoadEvents: false);
					return;
				}
				if (onProjectEvent == null || onProjectEvent.GetInvocationList().Length == 0)
				{
					foreach (BackgroundTaskHandle item in taskHandles)
					{
						item.Continue();
					}
				}
				onProjectEvent = (Action<ProjectEvent[]>)Delegate.Combine(onProjectEvent, value);
			}
			remove
			{
				onProjectEvent = (Action<ProjectEvent[]>)Delegate.Remove(onProjectEvent, value);
				if (onProjectEvent != null && onProjectEvent.GetInvocationList().Length != 0)
				{
					return;
				}
				foreach (BackgroundTaskHandle item in taskHandles)
				{
					item.Pause();
				}
			}
		}

		internal static void RestartWatching(bool skipLoadEvents)
		{
			foreach (BackgroundTaskHandle item in taskHandles)
			{
				if (item.IsAlive)
				{
					item.Kill();
				}
			}
			taskHandles.Clear();
			taskHandles.Add(BackgroundTaskRunner.StartTask("Monitor loaded scenes", MonitorAllLoadedScenes(skipLoadEvents)));
		}

		private static void OnValidationStateChanged(ValidationStateChangeInfo obj)
		{
			if (!(obj.ValidationResult.Setup.Root as UnityEngine.Object == null))
			{
				UnityEngine.Object uObj = obj.ValidationResult.Setup.Root as UnityEngine.Object;
				string assetPath = null;
				string guid = null;
				if (AssetDatabase.Contains(uObj))
				{
					assetPath = AssetDatabase.GetAssetPath(uObj);
					guid = AssetDatabase.AssetPathToGUID(assetPath);
				}
				OdinEntityId entityId = OdinEntityId.FromObject(uObj);
				ProjectEvent[] arr = new ProjectEvent[1]
				{
					new ProjectEvent
					{
						Path = assetPath,
						AssetGuid = guid,
						InstanceID = OdinEntityId.Internal.ToInt32(entityId),
						EntityId = entityId,
						Source = ProjectEventSource.OdinValidationEvents,
						Type = ProjectEventType.Revalidation
					}
				};
				InvokeProjectEvent(arr);
			}
		}

		private static void OnUndoOrRedoPerformed(List<UnityEngine.Object> objects)
		{
			InvokeChangeEventsForUnityObjects(objects, ProjectEventSource.UndoOrRedo);
		}

		private static void InvokeChangeEventsForUnityObjects(IEnumerable<UnityEngine.Object> objects, ProjectEventSource source)
		{
			if (onProjectEvent == null)
			{
				return;
			}
			List<ProjectEvent> events = new List<ProjectEvent>();
			foreach (UnityEngine.Object obj in objects)
			{
				if (!obj)
				{
					continue;
				}
				GameObject go = obj as GameObject;
				if (obj is Component)
				{
					go = (obj as Component).gameObject;
				}
				else if (obj is GameObject)
				{
					go = obj as GameObject;
				}
				bool isAsset = AssetDatabase.Contains(obj);
				bool isInValidScene = false;
				if (go != null)
				{
					isInValidScene = go.scene.IsValid();
				}
				if (isAsset && isInValidScene)
				{
					continue;
				}
				if (isAsset && !isInValidScene)
				{
					string path = AssetDatabase.GetAssetPath(obj);
					if (!string.IsNullOrWhiteSpace(path))
					{
						string guid = AssetDatabase.AssetPathToGUID(path);
						OdinEntityId entityId = ((obj != null) ? OdinEntityId.FromObject(obj) : OdinEntityId.None);
						events.Add(new ProjectEvent
						{
							Type = ProjectEventType.AssetModified,
							Path = path,
							AssetGuid = guid,
							InstanceID = OdinEntityId.Internal.ToInt32(entityId),
							EntityId = entityId,
							Source = source
						});
					}
				}
				else if (isInValidScene && !isAsset && (bool)obj)
				{
					OdinEntityId entityId2 = OdinEntityId.FromObject(obj);
					events.Add(new ProjectEvent
					{
						Type = ProjectEventType.SceneObjectModified,
						Path = null,
						Source = source,
						InstanceID = OdinEntityId.Internal.ToInt32(entityId2),
						EntityId = entityId2
					});
				}
			}
			if (events.Count > 0)
			{
				InvokeProjectEvent(events.ToArray());
			}
		}

		private static void UndoTracker_OnObjectValueModified(UndoTracker.UndoPropertyModificationGroup[] obj)
		{
			InvokeChangeEventsForUnityObjects(obj.Select((UndoTracker.UndoPropertyModificationGroup x) => x.Target), ProjectEventSource.UndoOrRedoModification);
		}

		private static void MonitorAssetProcessorChanges()
		{
			if ((importedAssetsBuffer.Count == 0 && deletedAssetsBuffer.Count == 0 && movedAssetsBuffer.Count == 0) || EditorApplication.isCompiling)
			{
				return;
			}
			List<ProjectEvent> events = new List<ProjectEvent>();
			ProjectEventSource source = ProjectEventSource.MonitorAssetProcessorChanges;
			foreach (string path in importedAssetsBuffer)
			{
				string guid = AssetDatabase.AssetPathToGUID(path);
				if (!string.IsNullOrEmpty(guid))
				{
					events.Add(new ProjectEvent
					{
						Type = ProjectEventType.AssetImported,
						Path = path,
						AssetGuid = guid,
						Source = source
					});
				}
			}
			foreach (string path2 in deletedAssetsBuffer)
			{
				string guid2 = AssetDatabase.AssetPathToGUID(path2);
				if (!string.IsNullOrEmpty(guid2))
				{
					events.Add(new ProjectEvent
					{
						Type = ProjectEventType.AssetRemoved,
						Path = path2,
						AssetGuid = guid2,
						Source = source
					});
				}
			}
			foreach (var item in movedAssetsBuffer)
			{
				string pathTo = item.Item2;
				string guid3 = AssetDatabase.AssetPathToGUID(pathTo);
				if (!string.IsNullOrEmpty(guid3))
				{
					events.Add(new ProjectEvent
					{
						Type = ProjectEventType.AssetMoved,
						Path = pathTo,
						AssetGuid = guid3,
						Source = source
					});
				}
			}
			ProjectEvent[] eventArr = events.Distinct().ToArray();
			if (eventArr.Length != 0)
			{
				InvokeProjectEvent(eventArr);
			}
			importedAssetsBuffer.Clear();
			deletedAssetsBuffer.Clear();
			movedAssetsBuffer.Clear();
		}

		private static void InvokeProjectEvent(ProjectEvent[] eventArr)
		{
			foreach (ProjectEvent item in eventArr)
			{
				latestEvents.Add(item);
			}
			if (onProjectEvent == null)
			{
				return;
			}
			if (onProjectEvent != null)
			{
				Delegate[] delegates = onProjectEvent.GetInvocationList();
				Delegate[] array = delegates;
				foreach (Delegate del in array)
				{
					try
					{
						((Action<ProjectEvent[]>)del)(eventArr);
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
					}
				}
				return;
			}
			try
			{
				onProjectEvent(eventArr);
			}
			catch (Exception exception2)
			{
				Debug.LogException(exception2);
			}
		}

		private static IEnumerator MonitorAllLoadedScenes(bool skipLoadEventsForCurrentLoadedScenes)
		{
			Dictionary<SceneReference, SceneMonitorEnumerator> openScenes = new Dictionary<SceneReference, SceneMonitorEnumerator>();
			List<SceneReference> toRemove = new List<SceneReference>();
			while (true)
			{
				int sceneCount = SceneManager.sceneCount;
				for (int i = 0; i < sceneCount; i++)
				{
					SceneReference scene = new SceneReference(SceneManager.GetSceneAt(i));
					if (scene.IsLoaded && !openScenes.ContainsKey(scene))
					{
						SceneMonitorEnumerator e = new SceneMonitorEnumerator();
						e.AllLoadedObjectIds = new HashSet<OdinEntityId>();
						e.Task = BackgroundTaskRunner.StartTask("Monitor changes in " + scene.Name, MonitorSceneChangesFor(scene, e.AllLoadedObjectIds, skipLoadEventsForCurrentLoadedScenes));
						taskHandles.Add(e.Task);
						if (!skipLoadEventsForCurrentLoadedScenes)
						{
							InvokeProjectEvent(new ProjectEvent[1]
							{
								new ProjectEvent
								{
									Scene = scene,
									Type = ProjectEventType.SceneLoaded,
									Source = ProjectEventSource.SceneMonitor
								}
							});
						}
						openScenes.Add(scene, e);
					}
				}
				skipLoadEventsForCurrentLoadedScenes = false;
				foreach (SceneReference scene2 in openScenes.Keys)
				{
					if (!scene2.IsLoaded)
					{
						InvokeProjectEvent(new ProjectEvent[1]
						{
							new ProjectEvent
							{
								Scene = scene2,
								Type = ProjectEventType.SceneUnloaded,
								Source = ProjectEventSource.SceneMonitor
							}
						});
						toRemove.Add(scene2);
					}
				}
				if (toRemove.Count > 0)
				{
					foreach (KeyValuePair<SceneReference, SceneMonitorEnumerator> item in openScenes)
					{
						item.Value.WasPaused = item.Value.Task.IsPaused;
						item.Value.Task.Pause();
					}
					foreach (SceneReference item2 in toRemove)
					{
						SceneMonitorEnumerator e2 = openScenes[item2];
						taskHandles.Remove(e2.Task);
						e2.Task.Kill();
						int chunkSize = 100;
						int idx = 0;
						ProjectEvent[] chunkBuffer = new ProjectEvent[Math.Min(e2.AllLoadedObjectIds.Count, chunkSize)];
						foreach (OdinEntityId id in e2.AllLoadedObjectIds)
						{
							chunkBuffer[idx++] = new ProjectEvent
							{
								Type = ProjectEventType.SceneObjectUnloaded,
								Source = ProjectEventSource.SceneMonitor,
								InstanceID = OdinEntityId.Internal.ToInt32(id),
								EntityId = id
							};
							if (idx >= chunkBuffer.Length || idx == e2.AllLoadedObjectIds.Count)
							{
								if (chunkBuffer.Length != idx)
								{
									Array.Resize(ref chunkBuffer, idx);
								}
								InvokeProjectEvent(chunkBuffer);
								idx = 0;
							}
						}
						yield return null;
						openScenes.Remove(item2);
					}
					foreach (KeyValuePair<SceneReference, SceneMonitorEnumerator> item3 in openScenes)
					{
						if (!item3.Value.WasPaused)
						{
							item3.Value.Task.Continue();
						}
					}
					toRemove.Clear();
				}
				yield return BackgroundTaskRunner.Relax;
			}
		}

		private static IEnumerator MonitorSceneChangesFor(SceneReference scene, HashSet<OdinEntityId> allLoadedObjectIdsBuffer, bool skipLoadEvents)
		{
			bool changeDetected = false;
			HashSet<OdinEntityId> lastKnownHierarchyIds = new HashSet<OdinEntityId>();
			int lastHierarchyChangeCount = -1;
			bool setNext = false;
			List<OdinEntityId> a = new List<OdinEntityId>(300);
			List<OdinEntityId> b = new List<OdinEntityId>(300);
			List<OdinEntityId> currIds = a;
			while (true)
			{
				if (onProjectEvent == null || onProjectEvent.GetInvocationList().Length == 0)
				{
					yield return BackgroundTaskRunner.Relax;
					continue;
				}
				Queue<Transform> transformQueue = new Queue<Transform>();
				if (scene.TryGetScene(out var tmpScene))
				{
					IEnumerable<GameObject> roots = SceneUtilities.GetSceneRoots(tmpScene);
					foreach (GameObject root in roots)
					{
						transformQueue.Enqueue(root.transform);
					}
				}
				while (transformQueue.Count > 0)
				{
					Transform transform = transformQueue.Dequeue();
					if ((bool)transform)
					{
						currIds.Add(OdinEntityId.FromObject(transform.gameObject));
						Component[] components = transform.gameObject.GetComponents<Component>();
						foreach (Component cmp in components)
						{
							if ((object)cmp != null)
							{
								currIds.Add(OdinEntityId.FromObject(cmp));
							}
						}
						int childCount = transform.childCount;
						for (int j = 0; j < childCount; j++)
						{
							transformQueue.Enqueue(transform.GetChild(j));
						}
					}
					if (changeDetected || lastHierarchyChangeCount != hierarchyWindowChangedCount || boostWatchSpeed)
					{
						yield return null;
					}
					else
					{
						yield return BackgroundTaskRunner.Relax;
					}
				}
				List<OdinEntityId> other = ((currIds == a) ? b : a);
				bool hasDiff = currIds.Count != other.Count || !currIds.SequenceEqual(other);
				boostWatchSpeed = false;
				if (hasDiff)
				{
					changeDetected = true;
					yield return null;
				}
				else if (changeDetected)
				{
					ProjectEvent[] eventBuffer = new ProjectEvent[1];
					foreach (OdinEntityId entityId in currIds)
					{
						if (!lastKnownHierarchyIds.Remove(entityId))
						{
							ProjectEventType type = ((lastHierarchyChangeCount == -1) ? ProjectEventType.SceneObjectLoaded : ProjectEventType.SceneObjectCreated);
							if (!skipLoadEvents || type != ProjectEventType.SceneObjectLoaded)
							{
								eventBuffer[0] = new ProjectEvent
								{
									Type = ((lastHierarchyChangeCount == -1) ? ProjectEventType.SceneObjectLoaded : ProjectEventType.SceneObjectCreated),
									Source = ProjectEventSource.SceneMonitor,
									InstanceID = OdinEntityId.Internal.ToInt32(entityId),
									EntityId = entityId,
									Scene = scene
								};
								InvokeProjectEvent(eventBuffer);
							}
							allLoadedObjectIdsBuffer.Add(entityId);
							yield return null;
						}
					}
					int removedCount = 0;
					foreach (OdinEntityId entityId2 in lastKnownHierarchyIds)
					{
						eventBuffer[0] = new ProjectEvent
						{
							Type = ProjectEventType.SceneObjectDeleted,
							Source = ProjectEventSource.SceneMonitor,
							InstanceID = OdinEntityId.Internal.ToInt32(entityId2),
							EntityId = entityId2,
							Scene = scene
						};
						removedCount++;
						InvokeProjectEvent(eventBuffer);
						allLoadedObjectIdsBuffer.Remove(entityId2);
						yield return null;
					}
					changeDetected = false;
					lastKnownHierarchyIds.Clear();
					foreach (OdinEntityId item in currIds)
					{
						lastKnownHierarchyIds.Add(item);
					}
					lastHierarchyChangeCount = hierarchyWindowChangedCount;
					setNext = false;
				}
				else if (setNext)
				{
					lastHierarchyChangeCount = hierarchyWindowChangedCount;
				}
				else
				{
					setNext = true;
				}
				other.Clear();
				currIds = other;
				yield return BackgroundTaskRunner.Relax;
			}
		}
	}
}
