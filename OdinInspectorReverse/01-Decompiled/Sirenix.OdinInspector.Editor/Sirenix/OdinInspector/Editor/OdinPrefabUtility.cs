using System;
using System.Collections.Generic;
using Sirenix.Serialization;
using Sirenix.Utilities.Editor;
using Sirenix.Utilities.Editor.Expressions;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	public static class OdinPrefabUtility
	{
		private static readonly ExpressionFunc<GameObject, bool> isPartOfPrefabStage;

		private static readonly ExpressionFunc<GameObject, string> assetPathOfPrefabStage;

		private static readonly ExpressionFunc<GameObject, GameObject> prefabContentsRoot;

		private static readonly ExpressionFunc<string, GameObject, GameObject> openPrefabStage;

		static OdinPrefabUtility()
		{
			Type prefabStageUtilityType = TwoWaySerializationBinder.Default.BindToType("UnityEditor.SceneManagement.PrefabStageUtility") ?? TwoWaySerializationBinder.Default.BindToType("UnityEditor.Experimental.SceneManagement.PrefabStageUtility");
			Type prefabStageType = TwoWaySerializationBinder.Default.BindToType("UnityEditor.SceneManagement.PrefabStage") ?? TwoWaySerializationBinder.Default.BindToType("UnityEditor.Experimental.SceneManagement.PrefabStage");
			isPartOfPrefabStage = ExpressionUtility.ParseFunc<GameObject, bool>("GetPrefabStage($0) != null", isStatic: true, prefabStageUtilityType, out var e);
			if (e != null)
			{
				Debug.LogError(e);
			}
			if (prefabStageType.GetProperty("assetPath") != null)
			{
				assetPathOfPrefabStage = ExpressionUtility.ParseFunc<GameObject, string>("GetPrefabStage($0).assetPath", isStatic: true, prefabStageUtilityType, out e);
				if (e != null)
				{
					Debug.LogError(e);
				}
			}
			else
			{
				assetPathOfPrefabStage = ExpressionUtility.ParseFunc<GameObject, string>("GetPrefabStage($0).prefabAssetPath", isStatic: true, prefabStageUtilityType, out e);
				if (e != null)
				{
					Debug.LogError(e);
				}
			}
			prefabContentsRoot = ExpressionUtility.ParseFunc<GameObject, GameObject>("GetPrefabStage($0).prefabContentsRoot", isStatic: true, prefabStageUtilityType, out e);
			if (e != null)
			{
				Debug.LogError(e);
			}
			openPrefabStage = ExpressionUtility.ParseFunc<string, GameObject, GameObject>("OpenPrefab($0, $1).prefabContentsRoot", isStatic: true, prefabStageUtilityType, out e);
			if (e != null)
			{
				Debug.LogError(e);
			}
		}

		public static GameObject OpenPrefabStage(string prefabAssetPath, GameObject openedFromInstance)
		{
			return openPrefabStage(prefabAssetPath, openedFromInstance);
		}

		public static PrefabKind GetPrefabKind(InspectorProperty prop)
		{
			UnityEngine.Object obj = prop.Tree.WeakTargets[0] as UnityEngine.Object;
			return GetPrefabKind(obj);
		}

		internal static bool IsPartOfPrefabStage(GameObject obj)
		{
			return isPartOfPrefabStage(obj);
		}

		public static GameObject GetNearestPrefabAsset(UnityEngine.Object obj)
		{
			if ((bool)obj && (obj is Component || obj is GameObject))
			{
				GameObject go = ((obj is Component cmp) ? cmp.gameObject : ((GameObject)obj));
				if (PrefabUtility.IsPartOfAnyPrefab(obj) || isPartOfPrefabStage(go))
				{
					string path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(obj);
					if (string.IsNullOrEmpty(path) && isPartOfPrefabStage(go))
					{
						path = assetPathOfPrefabStage(go);
					}
					if (!string.IsNullOrEmpty(path))
					{
						return AssetDatabase.LoadAssetAtPath<GameObject>(path);
					}
				}
			}
			return null;
		}

		public static PrefabKind GetPrefabKind(UnityEngine.Object obj)
		{
			if ((bool)obj && (obj is Component || obj is GameObject))
			{
				GameObject go = ((obj is Component cmp) ? cmp.gameObject : ((GameObject)obj));
				if (isPartOfPrefabStage(go))
				{
					if (PrefabUtility.GetPrefabAssetType(obj) == PrefabAssetType.NotAPrefab && PrefabUtility.GetPrefabInstanceStatus(obj) == PrefabInstanceStatus.NotAPrefab)
					{
						return PrefabKind.Regular;
					}
					GameObject nearest = PrefabUtility.GetOutermostPrefabInstanceRoot(obj);
					if (!(nearest == null) && !(nearest == prefabContentsRoot(go)))
					{
						return PrefabKind.InstanceInPrefab;
					}
					string path = assetPathOfPrefabStage(go);
					GameObject stagePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
					switch (PrefabUtility.GetPrefabAssetType(stagePrefab))
					{
					case PrefabAssetType.Regular:
					case PrefabAssetType.Model:
						return PrefabKind.Regular;
					case PrefabAssetType.Variant:
						return PrefabKind.Variant;
					}
				}
				else
				{
					switch (PrefabUtility.GetPrefabInstanceStatus(obj))
					{
					case PrefabInstanceStatus.NotAPrefab:
						switch (PrefabUtility.GetPrefabAssetType(obj))
						{
						case PrefabAssetType.Regular:
						case PrefabAssetType.Model:
							return PrefabKind.Regular;
						case PrefabAssetType.Variant:
							if (PrefabUtility.IsPartOfPrefabInstance(obj))
							{
								return PrefabKind.Variant;
							}
							return PrefabKind.Regular;
						default:
							return PrefabKind.NonPrefabInstance;
						}
					case PrefabInstanceStatus.Connected:
					{
						GameObject nearest2 = PrefabUtility.GetOutermostPrefabInstanceRoot(obj);
						PrefabKind inPrefab = ((nearest2 != go) ? PrefabKind.InstanceInPrefab : PrefabKind.None);
						PrefabAssetType type = PrefabUtility.GetPrefabAssetType(obj);
						if ((uint)(type - 1) <= 2u)
						{
							return PrefabKind.InstanceInScene | inPrefab;
						}
						break;
					}
					}
				}
			}
			return PrefabKind.None;
		}

		public static void UpdatePrefabInstancePropertyModifications(UnityEngine.Object prefabInstance, bool withUndo)
		{
			if (prefabInstance == null)
			{
				throw new ArgumentNullException("prefabInstance");
			}
			if (!(prefabInstance is ISupportsPrefabSerialization))
			{
				throw new ArgumentException("Type must implement ISupportsPrefabSerialization");
			}
			if (!(prefabInstance is ISerializationCallbackReceiver))
			{
				throw new ArgumentException("Type must implement ISerializationCallbackReceiver");
			}
			if (!OdinPrefabSerializationEditorUtility.ObjectIsPrefabInstance(prefabInstance))
			{
				throw new ArgumentException("Value must be a prefab instance");
			}
			Action action = null;
			EditorApplication.ProjectWindowItemCallback projectCallback = delegate
			{
				action();
			};
			Action<SceneView> sceneCallback = delegate
			{
				action();
			};
			EditorApplication.projectWindowItemOnGUI = (EditorApplication.ProjectWindowItemCallback)Delegate.Combine(EditorApplication.projectWindowItemOnGUI, projectCallback);
			SceneView.duringSceneGui += sceneCallback;
			action = delegate
			{
				EditorApplication.projectWindowItemOnGUI = (EditorApplication.ProjectWindowItemCallback)Delegate.Remove(EditorApplication.projectWindowItemOnGUI, projectCallback);
				SceneView.duringSceneGui -= sceneCallback;
				ISupportsPrefabSerialization supportsPrefabSerialization = (ISupportsPrefabSerialization)prefabInstance;
				if (supportsPrefabSerialization.SerializationData.PrefabModifications != null)
				{
					supportsPrefabSerialization.SerializationData.PrefabModifications.Clear();
				}
				if (supportsPrefabSerialization.SerializationData.PrefabModificationsReferencedUnityObjects != null)
				{
					supportsPrefabSerialization.SerializationData.PrefabModificationsReferencedUnityObjects.Clear();
				}
				UnitySerializationUtility.PrefabModificationCache.CachePrefabModifications(prefabInstance, new List<PrefabModification>());
				try
				{
					if (!(prefabInstance == null))
					{
						if (Event.current == null)
						{
							throw new InvalidOperationException("Delayed property modification delegate can only be called during the GUI event loop; Event.current must be accessible.");
						}
						try
						{
							PrefabUtility.RecordPrefabInstancePropertyModifications(prefabInstance);
						}
						catch (Exception exception)
						{
							Debug.LogError("Exception occurred while calling Unity's PrefabUtility.RecordPrefabInstancePropertyModifications:");
							Debug.LogException(exception);
						}
						PropertyTree propertyTree = PropertyTree.Create(prefabInstance);
						propertyTree.DrawMonoScriptObjectField = false;
						bool flag = Event.current.type == EventType.Repaint;
						if (!flag)
						{
							GUIHelper.PushEventType(EventType.Repaint);
						}
						propertyTree.BeginDraw(withUndo);
						foreach (InspectorProperty current in propertyTree.EnumerateTree())
						{
							if (current.ValueEntry != null && current.SupportsPrefabModifications)
							{
								current.Update(forceUpdate: true);
								if (current.ChildResolver is IKeyValueMapResolver)
								{
									if (current.ValueEntry.DictionaryChangedFromPrefab)
									{
										propertyTree.PrefabModificationHandler.RegisterPrefabDictionaryDeltaModification(current, 0);
									}
									else
									{
										InspectorProperty propertyAtPath = propertyTree.PrefabModificationHandler.PrefabPropertyTree.GetPropertyAtPath(current.Path);
										if (propertyAtPath != null && propertyAtPath.ValueEntry != null && current.SupportsPrefabModifications && current.ChildResolver is IKeyValueMapResolver)
										{
											propertyTree.PrefabModificationHandler.RegisterPrefabDictionaryDeltaModification(current, 0);
										}
									}
								}
							}
						}
						propertyTree.EndDraw();
						if (!flag)
						{
							GUIHelper.PopEventType();
						}
						ISerializationCallbackReceiver serializationCallbackReceiver = (ISerializationCallbackReceiver)prefabInstance;
						serializationCallbackReceiver.OnBeforeSerialize();
						serializationCallbackReceiver.OnAfterDeserialize();
					}
				}
				catch (Exception exception2)
				{
					Debug.LogException(exception2);
				}
			};
			foreach (SceneView scene in SceneView.sceneViews)
			{
				scene.Repaint();
			}
		}
	}
}
