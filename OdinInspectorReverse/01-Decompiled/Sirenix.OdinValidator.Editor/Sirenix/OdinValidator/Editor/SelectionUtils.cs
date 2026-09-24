using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Reflection.Editor;
using Sirenix.Utilities;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;

namespace Sirenix.OdinValidator.Editor
{
	internal static class SelectionUtils
	{
		private static int selectionRequestedId = -1;

		private static HashSet<OdinEntityId> selection = new HashSet<OdinEntityId>();

		public static int SelectionChangedId { get; private set; }

		public static HashSet<OdinEntityId> Selection
		{
			get
			{
				if (SelectionChangedId != selectionRequestedId)
				{
					foreach (OdinEntityId item2 in selection)
					{
						if (item2.ToObject() == null)
						{
							ProjectWatcher.boostWatchSpeed = true;
							break;
						}
					}
					selection.Clear();
					UnityEngine.Object[] objects = UnityEditor.Selection.objects;
					foreach (UnityEngine.Object item in objects)
					{
						if (!item)
						{
							continue;
						}
						GameObject go = item as GameObject;
						if ((bool)go)
						{
							Component[] components = go.GetComponents<Component>();
							foreach (Component cmp in components)
							{
								if ((bool)cmp)
								{
									selection.Add(OdinEntityId.FromObject(cmp));
								}
							}
						}
						selection.Add(OdinEntityId.FromObject(item));
					}
					selectionRequestedId = SelectionChangedId;
				}
				return selection;
			}
		}

		[InitializeOnLoadMethod]
		private static void Init()
		{
			UnityEditor.Selection.selectionChanged = (Action)Delegate.Combine(UnityEditor.Selection.selectionChanged, (Action)delegate
			{
				SelectionChangedId++;
			});
		}

		public static void AddToSelection(IEnumerable<DynamicObjectAddress> objAddress)
		{
			HashSet<UnityEngine.Object> selection = new HashSet<UnityEngine.Object>(UnityEditor.Selection.objects);
			HashSet<Component> toExpand = new HashSet<Component>();
			HashSet<Component> toCollapse = new HashSet<Component>();
			foreach (DynamicObjectAddress item in objAddress)
			{
				if (item == null || item.IsUnloaded || !item.TryGetObjectReference(openSceneIfNeeded: false, autoSaveIfOpenScene: false, out var result, out var _) || !result)
				{
					continue;
				}
				if (result is Component)
				{
					Component cmp = (Component)result;
					if ((bool)cmp.gameObject)
					{
						Component[] cmps = cmp.gameObject.GetComponents<Component>();
						Component[] array = cmps;
						foreach (Component c in array)
						{
							toCollapse.Add(c);
						}
						toExpand.Add(cmp);
						selection.Add(cmp.gameObject);
					}
				}
				else
				{
					selection.Add(result);
				}
			}
			UnityEngine.Object[] capturedSelection = selection.ToArray();
			UnityEditorEventUtility.DelayAction(delegate
			{
				UnityEditor.Selection.objects = capturedSelection;
			});
			if ((bool)GlobalConfig<GlobalValidationConfig>.Instance.OpenComponentInInspectorAndCloseOthers)
			{
				foreach (Component item2 in toCollapse)
				{
					InternalEditorUtility.SetIsInspectorExpanded(item2, isExpanded: false);
				}
				foreach (Component item3 in toExpand)
				{
					InternalEditorUtility.SetIsInspectorExpanded(item3, isExpanded: true);
				}
			}
			ActiveEditorTracker.sharedTracker.ForceRebuild();
			if ((bool)GlobalConfig<GlobalValidationConfig>.Instance.FrameSelection)
			{
				Bounds bounds = InternalEditorUtility.CalculateSelectionBounds(usePivotOnlyForParticles: false, onlyUseActiveSelection: false);
				SceneView.lastActiveSceneView.Frame(bounds, instant: false);
			}
		}

		public static void SelectInInspector(DynamicObjectAddress objAddress, bool allowOpenScene, bool ping)
		{
			if (objAddress == null)
			{
				return;
			}
			UnityEngine.Object objReference;
			string error;
			UnityEngine.Object closest;
			bool success = objAddress.TryGetObjectReference(allowOpenScene, autoSaveIfOpenScene: false, out objReference, out error, out closest);
			if (!success)
			{
				if (closest != null)
				{
					objReference = closest;
					success = true;
				}
				else if (objAddress.IsBroken && objAddress.LatestAddress.SubAsset.Index > 0 && objAddress.LatestAddress.Type == ObjectAddress.AddressType.Asset && !string.IsNullOrEmpty(objAddress.LatestAddress.AssetPath))
				{
					objReference = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(objAddress.LatestAddress.AssetPath);
					success = objReference;
				}
			}
			if (success)
			{
				Component cmp = objReference as Component;
				GameObject go = objReference as GameObject;
				if ((bool)cmp || (bool)go)
				{
					if ((bool)cmp)
					{
						go = cmp.gameObject;
					}
					if ((bool)go)
					{
						if (allowOpenScene)
						{
							if ((OdinPrefabUtility.GetPrefabKind(objReference) & PrefabKind.PrefabAsset) != PrefabKind.None)
							{
								string path = null;
								GameObject obj = PrefabUtility.GetOutermostPrefabInstanceRoot(objReference);
								path = ((!obj) ? PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(objReference) : AssetDatabase.GetAssetPath(obj));
								if (path != null && ObjectAddress.TryCreateObjectAddress(go, out var address, out var _))
								{
									GameObject stageRoot = OdinPrefabUtility.OpenPrefabStage(path, null);
									if (address.Hierarchy.SubIndices.Length == 0)
									{
										go = stageRoot;
										objReference = stageRoot;
									}
									else
									{
										Transform[] roots = new Transform[stageRoot.transform.childCount];
										for (int i = 0; i < roots.Length; i++)
										{
											roots[i] = stageRoot.transform.GetChild(i);
										}
										if (address.Hierarchy.TryFindGameObject(roots, out var stageGo, out var _, out var _))
										{
											go = stageGo;
											objReference = stageGo;
										}
									}
								}
							}
							else if (go.scene.IsValid())
							{
								StageUtility.GoToMainStage();
							}
						}
						UnityEngine.Object capturedGo = go;
						UnityEditorEventUtility.DelayAction(delegate
						{
							UnityEditor.Selection.objects = new UnityEngine.Object[1] { capturedGo };
						});
						if ((bool)cmp && (bool)GlobalConfig<GlobalValidationConfig>.Instance.OpenComponentInInspectorAndCloseOthers)
						{
							Component[] components = cmp.GetComponents<Component>();
							foreach (Component item in components)
							{
								InternalEditorUtility.SetIsInspectorExpanded(item, isExpanded: false);
							}
							InternalEditorUtility.SetIsInspectorExpanded(objReference, isExpanded: true);
						}
						ActiveEditorTracker.sharedTracker.ForceRebuild();
					}
				}
				else
				{
					UnityEngine.Object capturedObjReference = objReference;
					UnityEditorEventUtility.DelayAction(delegate
					{
						UnityEditor.Selection.objects = new UnityEngine.Object[1] { capturedObjReference };
					});
				}
				if (ping)
				{
					if ((bool)GlobalConfig<GlobalValidationConfig>.Instance.PingOnDoubleClick)
					{
						EditorGUIUtility.PingObject(FindClosestSelectableObject(objReference));
					}
					if ((bool)go && (bool)GlobalConfig<GlobalValidationConfig>.Instance.FocusObjectOnDoubleClick)
					{
						Bounds bounds = InternalEditorUtility.CalculateSelectionBounds(usePivotOnlyForParticles: false, onlyUseActiveSelection: false);
						bounds.extents += Vector3.one;
						SceneView.lastActiveSceneView.Frame(bounds, instant: false);
					}
				}
			}
			else
			{
				if (!ping)
				{
					return;
				}
				string assetPath = objAddress.LatestAddress?.AssetPath;
				if (!string.IsNullOrEmpty(assetPath))
				{
					string dir = PathUtilities.GetDirectoryName(assetPath);
					UnityEngine.Object folder = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(dir);
					if ((bool)folder)
					{
						EditorGUIUtility.PingObject(folder);
					}
				}
			}
		}

		public static UnityEngine.Object FindClosestSelectableObject(UnityEngine.Object result)
		{
			if (!result)
			{
				return result;
			}
			if (result is ScriptableObject so && (so.hideFlags & HideFlags.HideInHierarchy) != HideFlags.None)
			{
				return result;
			}
			if (AssetDatabase.Contains(result))
			{
				string path = AssetDatabase.GetAssetPath(result);
				return AssetDatabase.LoadMainAssetAtPath(path);
			}
			return result;
		}
	}
}
