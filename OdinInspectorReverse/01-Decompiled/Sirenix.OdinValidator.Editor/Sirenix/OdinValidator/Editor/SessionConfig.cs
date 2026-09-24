using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.Validation;
using Sirenix.Reflection.Editor;
using Sirenix.Utilities;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Sirenix.OdinValidator.Editor
{
	public class SessionConfig
	{
		[Serializable]
		public class SerializableSessionConfigData : IValidationProfile
		{
			public List<ValidationItem> Include;

			public List<ValidationItem> Exclude;

			[NonSerialized]
			public Action OnSaveChanges;

			public SessionConfigDataType Type => SessionConfigDataType.Custom;

			public SdfIconType Icon => SdfIconType.Person;

			IList<ValidationItem> IValidationProfile.Include => Include;

			IList<ValidationItem> IValidationProfile.Exclude => Exclude;

			public SerializableSessionConfigData(IList<ValidationItem> include, IList<ValidationItem> exclude, Action onSaveChanges = null)
			{
				if (include != null)
				{
					Include = include.ToList();
				}
				else
				{
					Include = new List<ValidationItem>();
				}
				if (exclude != null)
				{
					Exclude = exclude.ToList();
				}
				else
				{
					Exclude = new List<ValidationItem>();
				}
				OnSaveChanges = onSaveChanges;
			}

			void IValidationProfile.SaveChanges()
			{
				if (OnSaveChanges != null)
				{
					OnSaveChanges();
				}
			}
		}

		[NonSerialized]
		private HashSet<string> assetsToValidate = new HashSet<string>();

		[NonSerialized]
		private HashSet<string> sceneGuidsToValidate = new HashSet<string>();

		[NonSerialized]
		private HashSet<string> sceneGuidsNotToValidate = new HashSet<string>();

		[NonSerialized]
		private HashSet<UnityEngine.Object> unityObjectsToValidate = new HashSet<UnityEngine.Object>();

		private HashSet<string> allIncludeItemGuidHashset = new HashSet<string>();

		private HashSet<string> allExcludeItemGuidHashset = new HashSet<string>();

		public bool IsDirty = true;

		public List<IValidationProfile> SessionData = new List<IValidationProfile>();

		public IEnumerable<ValidationItem> Include => SessionData.SelectMany((IValidationProfile x) => x.Include) ?? Enumerable.Empty<ValidationItem>();

		public IEnumerable<ValidationItem> Exclude => SessionData.SelectMany((IValidationProfile x) => x.Exclude) ?? Enumerable.Empty<ValidationItem>();

		public SessionConfig(params IValidationProfile[] configDataContainers)
		{
			foreach (IValidationProfile data in configDataContainers)
			{
				SessionData.Add(data);
			}
		}

		public SessionConfig(List<ValidationItem> include, List<ValidationItem> exclude)
		{
			SessionData.Add(new SerializableSessionConfigData(include, exclude));
		}

		public void SaveChanges()
		{
			IsDirty = true;
			foreach (IValidationProfile item in SessionData)
			{
				item.SaveChanges();
			}
		}

		public void MarkDirty()
		{
			IsDirty = true;
			foreach (IValidationProfile item in SessionData)
			{
				if (item is UnityEngine.Object uObj)
				{
					EditorUtility.SetDirty(uObj);
				}
			}
		}

		public HashSet<string> GetAssetsToValidate()
		{
			if (IsDirty)
			{
				UpdateAll();
			}
			return assetsToValidate;
		}

		public HashSet<UnityEngine.Object> GetObjectsToValidate()
		{
			if (IsDirty)
			{
				UpdateAll();
			}
			return unityObjectsToValidate;
		}

		public HashSet<string> GetSceneGuidsToValidate()
		{
			if (IsDirty)
			{
				UpdateAll();
			}
			return sceneGuidsToValidate;
		}

		public bool ShouldValidateScene(string guid)
		{
			if (IsDirty)
			{
				UpdateAll();
			}
			if (sceneGuidsNotToValidate.Contains(guid))
			{
				return false;
			}
			if (sceneGuidsToValidate.Contains(guid))
			{
				return true;
			}
			UpdateScenes();
			if (sceneGuidsNotToValidate.Contains(guid))
			{
				return false;
			}
			if (sceneGuidsToValidate.Contains(guid))
			{
				return true;
			}
			sceneGuidsNotToValidate.Add(guid);
			return false;
		}

		public void UpdateAll()
		{
			assetsToValidate.Clear();
			sceneGuidsToValidate.Clear();
			sceneGuidsNotToValidate.Clear();
			unityObjectsToValidate.Clear();
			allIncludeItemGuidHashset.Clear();
			allExcludeItemGuidHashset.Clear();
			UpdateScenes();
			UpdateAssets();
			UpdateUnityObjects();
			UpdateIncludeExcludeHashsets();
			IsDirty = false;
		}

		private void UpdateIncludeExcludeHashsets()
		{
			foreach (ValidationItem item2 in Include)
			{
				if (item2.TryGetAssetGuid(out var guid))
				{
					allIncludeItemGuidHashset.Add(guid);
				}
			}
			foreach (ValidationItem item3 in Exclude)
			{
				if (item3.TryGetAssetGuid(out var guid2))
				{
					allExcludeItemGuidHashset.Add(guid2);
				}
			}
		}

		internal bool ContainsExcludeItemWithAssetGuid(string guid)
		{
			if (IsDirty)
			{
				UpdateAll();
			}
			return allExcludeItemGuidHashset.Contains(guid);
		}

		internal bool ContainsIncludeItemWithAssetGuid(string guid)
		{
			if (IsDirty)
			{
				UpdateAll();
			}
			return allIncludeItemGuidHashset.Contains(guid);
		}

		private void UpdateUnityObjects()
		{
			foreach (ValidationItem item in Include.Where((ValidationItem x) => x.Enabled && x.Type == ValidationItem.ValidationItemType.Object && (bool)x.Object))
			{
				unityObjectsToValidate.Add(item.Object);
			}
			foreach (ValidationItem item2 in Exclude.Where((ValidationItem x) => x.Enabled && x.Type == ValidationItem.ValidationItemType.Object && (bool)x.Object))
			{
				unityObjectsToValidate.Remove(item2.Object);
			}
		}

		internal void UpdateScenes()
		{
			if (Application.isPlaying)
			{
				return;
			}
			sceneGuidsToValidate.Clear();
			sceneGuidsNotToValidate.Clear();
			HashSet<string> sceneGuidsToCollectAssetDepsFrom = new HashSet<string>();
			IEnumerable<ValidationItem> inlcude = Include.Where((ValidationItem x) => x.Enabled && x.Type == ValidationItem.ValidationItemType.Scene);
			foreach (ValidationItem s in inlcude)
			{
				if (s.Scene.Type == ValidationItem.SceneIncludeType.OpenScenes)
				{
					SceneSetup[] setupScenes = EditorSceneManager.GetSceneManagerSetup();
					SceneSetup[] array = setupScenes;
					foreach (SceneSetup scene in array)
					{
						if (string.IsNullOrEmpty(scene.path))
						{
							continue;
						}
						string guid = AssetDatabase.AssetPathToGUID(scene.path);
						if (!string.IsNullOrEmpty(guid))
						{
							sceneGuidsToValidate.Add(guid);
							if (s.Scene.IncludeAssetDependencies)
							{
								sceneGuidsToCollectAssetDepsFrom.Add(guid);
							}
						}
					}
				}
				else if (s.Scene.Type == ValidationItem.SceneIncludeType.SceneGuid)
				{
					if (string.IsNullOrEmpty(s.Scene.Value) || s.Scene.Value.Length != 32)
					{
						continue;
					}
					string guid2 = s.Scene.Value;
					if (!string.IsNullOrEmpty(guid2))
					{
						sceneGuidsToValidate.Add(guid2);
						if (s.Scene.IncludeAssetDependencies)
						{
							sceneGuidsToCollectAssetDepsFrom.Add(guid2);
						}
					}
				}
				else if (s.Scene.Type == ValidationItem.SceneIncludeType.ScenesInBuildOptions)
				{
					EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
					foreach (EditorBuildSettingsScene scene2 in scenes)
					{
						if (!scene2.enabled)
						{
							continue;
						}
						string guid3 = AssetDatabase.AssetPathToGUID(scene2.path);
						if (!string.IsNullOrEmpty(guid3))
						{
							sceneGuidsToValidate.Add(guid3);
							if (s.Scene.IncludeAssetDependencies)
							{
								sceneGuidsToCollectAssetDepsFrom.Add(guid3);
							}
						}
					}
				}
				else
				{
					if (s.Scene.Type != ValidationItem.SceneIncludeType.ScenesInFolder || string.IsNullOrEmpty(s.Scene.Value))
					{
						continue;
					}
					string[] array2 = AssetDatabase.FindAssets("t:Scene", new string[1] { s.Scene.Value });
					foreach (string guid4 in array2)
					{
						if (!string.IsNullOrEmpty(guid4))
						{
							sceneGuidsToValidate.Add(guid4);
							if (s.Scene.IncludeAssetDependencies)
							{
								sceneGuidsToCollectAssetDepsFrom.Add(guid4);
							}
						}
					}
				}
			}
			IEnumerable<ValidationItem> exclude = Exclude.Where((ValidationItem x) => x.Enabled && x.Type == ValidationItem.ValidationItemType.Scene);
			foreach (ValidationItem s2 in exclude)
			{
				if (s2.Scene.Type == ValidationItem.SceneIncludeType.OpenScenes)
				{
					SceneSetup[] setupScenes2 = EditorSceneManager.GetSceneManagerSetup();
					SceneSetup[] array3 = setupScenes2;
					foreach (SceneSetup scene3 in array3)
					{
						if (!string.IsNullOrEmpty(scene3.path))
						{
							string guid5 = AssetDatabase.AssetPathToGUID(scene3.path);
							sceneGuidsToValidate.Remove(guid5);
							sceneGuidsNotToValidate.Add(guid5);
						}
					}
				}
				else if (s2.Scene.Type == ValidationItem.SceneIncludeType.SceneGuid)
				{
					if (!string.IsNullOrEmpty(s2.Scene.Value) && s2.Scene.Value.Length == 32)
					{
						sceneGuidsToValidate.Remove(s2.Scene.Value);
						sceneGuidsNotToValidate.Add(s2.Scene.Value);
					}
				}
				else if (s2.Scene.Type == ValidationItem.SceneIncludeType.ScenesInBuildOptions)
				{
					EditorBuildSettingsScene[] scenes2 = EditorBuildSettings.scenes;
					foreach (EditorBuildSettingsScene scene4 in scenes2)
					{
						if (scene4.enabled)
						{
							string guid6 = AssetDatabase.AssetPathToGUID(scene4.path);
							sceneGuidsToValidate.Remove(guid6);
							sceneGuidsNotToValidate.Add(guid6);
						}
					}
				}
				else if (s2.Scene.Type == ValidationItem.SceneIncludeType.ScenesInFolder)
				{
					string[] array4 = AssetDatabase.FindAssets("t:Scene", new string[1] { s2.Scene.Value });
					foreach (string guid7 in array4)
					{
						sceneGuidsToValidate.Remove(guid7);
						sceneGuidsNotToValidate.Add(guid7);
					}
				}
			}
			foreach (string item in sceneGuidsNotToValidate)
			{
				sceneGuidsToCollectAssetDepsFrom.Remove(item);
			}
			assetsToValidate.AddRange(GetDepGuids((from x in sceneGuidsToCollectAssetDepsFrom
				select new SceneReference(x).Path into x
				where !string.IsNullOrWhiteSpace(x)
				select x).ToArray()));
		}

		private static IEnumerable<string> GetDepGuids(string[] paths)
		{
			if (paths.Length == 0)
			{
				yield break;
			}
			string[] dependencies = AssetDatabase.GetDependencies(paths, recursive: true);
			foreach (string item in dependencies)
			{
				string guid = AssetDatabase.AssetPathToGUID(item);
				if (!string.IsNullOrEmpty(guid) && guid.Length == 32)
				{
					yield return guid;
				}
			}
		}

		private void UpdateAssets()
		{
			HashSet<string> assetBundleNames = new HashSet<string>(AssetDatabase.GetAllAssetBundleNames());
			IEnumerable<IGrouping<string, ValidationItem>> includePaths = from x in Include
				where x.Enabled && x.Type == ValidationItem.ValidationItemType.Asset && x.Asset.IsValid
				group x by x.Asset.Filter;
			foreach (IGrouping<string, ValidationItem> filterGroup in includePaths)
			{
				string[] folderPaths = (from x in filterGroup
					where Directory.Exists(x.Asset.Path)
					select x.Asset.Path).ToArray();
				string[] filePaths = (from x in filterGroup
					where File.Exists(x.Asset.Path)
					select x.Asset.Path).ToArray();
				string filter = filterGroup.Key;
				if (folderPaths.Length != 0)
				{
					string[] guids = AssetDatabase.FindAssets(filter, folderPaths);
					string[] array = guids;
					foreach (string guid in array)
					{
						if (!string.IsNullOrWhiteSpace(guid))
						{
							assetsToValidate.Add(guid);
						}
					}
					string[] array2 = folderPaths;
					foreach (string folderAssetPath in array2)
					{
						string guid2 = AssetDatabase.AssetPathToGUID(folderAssetPath);
						if (!string.IsNullOrWhiteSpace(guid2))
						{
							assetsToValidate.Add(guid2);
						}
					}
				}
				if (filePaths.Length == 0)
				{
					continue;
				}
				string[] array3 = filePaths;
				foreach (string path in array3)
				{
					string guid3 = AssetDatabase.AssetPathToGUID(path);
					if (!string.IsNullOrEmpty(guid3))
					{
						assetsToValidate.Add(guid3);
					}
				}
			}
			IEnumerable<ValidationItem> bundlePaths = Include.Where((ValidationItem x) => x.Enabled && x.Type == ValidationItem.ValidationItemType.AssetBundle && x.AssetBundle != null && assetBundleNames.Contains(x.AssetBundle));
			foreach (ValidationItem item in bundlePaths)
			{
				string[] assetPathsFromAssetBundle = AssetDatabase.GetAssetPathsFromAssetBundle(item.AssetBundle);
				foreach (string path2 in assetPathsFromAssetBundle)
				{
					string guid4 = AssetDatabase.AssetPathToGUID(path2);
					if (!string.IsNullOrEmpty(guid4))
					{
						assetsToValidate.Add(guid4);
					}
				}
			}
			if (AddressablesUtility.AddressablesAvailable)
			{
				HashSet<string> addressableGroups = new HashSet<string>(AddressablesUtility.GetAddressableGroupNames());
				IEnumerable<ValidationItem> addressables = Include.Where((ValidationItem x) => x.Enabled && x.Type == ValidationItem.ValidationItemType.AddressableGroup && x.AddressableGroup != null && addressableGroups.Contains(x.AddressableGroup));
				foreach (ValidationItem item2 in addressables)
				{
					foreach (string path3 in AddressablesUtility.GetAssetPathsInGroup(item2.AddressableGroup))
					{
						string guid5 = AssetDatabase.AssetPathToGUID(path3);
						if (!string.IsNullOrEmpty(guid5))
						{
							assetsToValidate.Add(guid5);
						}
					}
				}
			}
			IEnumerable<IGrouping<string, ValidationItem>> excludePaths = from x in Exclude
				where x.Enabled && x.Type == ValidationItem.ValidationItemType.Asset && x.Asset.IsValid
				group x by x.Asset.Filter;
			foreach (IGrouping<string, ValidationItem> filterGroup2 in excludePaths)
			{
				string[] folderPaths2 = (from x in filterGroup2
					where Directory.Exists(x.Asset.Path)
					select x.Asset.Path).ToArray();
				string[] filePaths2 = (from x in filterGroup2
					where File.Exists(x.Asset.Path)
					select x.Asset.Path).ToArray();
				string filter2 = filterGroup2.Key;
				if (folderPaths2.Length != 0)
				{
					string[] guids2 = AssetDatabase.FindAssets(filter2, folderPaths2);
					string[] array4 = guids2;
					foreach (string guid6 in array4)
					{
						assetsToValidate.Remove(guid6);
					}
					string[] array5 = folderPaths2;
					foreach (string folderAssetPath2 in array5)
					{
						string guid7 = AssetDatabase.AssetPathToGUID(folderAssetPath2);
						if (!string.IsNullOrWhiteSpace(guid7))
						{
							assetsToValidate.Remove(guid7);
						}
					}
				}
				if (filePaths2.Length == 0)
				{
					continue;
				}
				string[] array6 = filePaths2;
				foreach (string path4 in array6)
				{
					string guid8 = AssetDatabase.AssetPathToGUID(path4);
					if (!string.IsNullOrEmpty(guid8))
					{
						assetsToValidate.Remove(guid8);
					}
				}
			}
			IEnumerable<string> bundlePaths2 = Exclude.Where((ValidationItem x) => x.Enabled && x.Type == ValidationItem.ValidationItemType.AssetBundle && x.AssetBundle != null && assetBundleNames.Contains(x.AssetBundle)).SelectMany((ValidationItem x) => AssetDatabase.GetAssetPathsFromAssetBundle(x.AssetBundle));
			foreach (string path5 in bundlePaths2)
			{
				string guid9 = AssetDatabase.AssetPathToGUID(path5);
				if (!string.IsNullOrEmpty(guid9))
				{
					assetsToValidate.Remove(guid9);
				}
			}
			if (!AddressablesUtility.AddressablesAvailable)
			{
				return;
			}
			HashSet<string> addressableGroups2 = new HashSet<string>(AddressablesUtility.GetAddressableGroupNames());
			IEnumerable<ValidationItem> addressables2 = Exclude.Where((ValidationItem x) => x.Enabled && x.Type == ValidationItem.ValidationItemType.AddressableGroup && x.AddressableGroup != null && addressableGroups2.Contains(x.AddressableGroup));
			foreach (ValidationItem item3 in addressables2)
			{
				foreach (string path6 in AddressablesUtility.GetAssetPathsInGroup(item3.AddressableGroup))
				{
					string guid10 = AssetDatabase.AssetPathToGUID(path6);
					if (!string.IsNullOrEmpty(guid10))
					{
						assetsToValidate.Remove(guid10);
					}
				}
			}
		}

		public void SelectAllIncludedAssetsInProjectWindow(bool includeScenes = true, bool includeAssets = true, bool includeObjects = true)
		{
			if (IsDirty)
			{
				UpdateAll();
			}
			List<UnityEngine.Object> objects = new List<UnityEngine.Object>();
			if (includeAssets)
			{
				foreach (string guid in GetAssetsToValidate())
				{
					objects.Add(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(AssetDatabase.GUIDToAssetPath(guid)));
				}
			}
			if (includeScenes)
			{
				foreach (string guid2 in GetSceneGuidsToValidate())
				{
					objects.Add(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(AssetDatabase.GUIDToAssetPath(guid2)));
				}
			}
			if (includeObjects)
			{
				foreach (UnityEngine.Object o in GetObjectsToValidate())
				{
					objects.Add(o);
				}
			}
			Selection.objects = objects.Where((UnityEngine.Object x) => x).ToArray();
		}

		public bool ShouldValidateUnityObjectReference(OdinEntityId entityId)
		{
			if (!entityId.IsValid)
			{
				return false;
			}
			UnityEngine.Object uObj = entityId.ToObject();
			GameObject go = uObj as GameObject;
			if (!go && uObj is Component cmp)
			{
				go = cmp.gameObject;
			}
			if ((bool)go && !string.IsNullOrEmpty(go.scene.path))
			{
				string guid = AssetDatabase.AssetPathToGUID(go.scene.path);
				if (ShouldValidateScene(guid))
				{
					return true;
				}
			}
			if (entityId.IsInAssetDatabase())
			{
				string path = entityId.GetAssetPath();
				if (!string.IsNullOrEmpty(path))
				{
					string guid2 = AssetDatabase.AssetPathToGUID(path);
					if (guid2 != null && guid2.Length == 32 && GetAssetsToValidate().Contains(guid2))
					{
						return true;
					}
				}
			}
			return false;
		}

		[Obsolete("Use ShouldValidateUnityObjectReference(OdinEntityId) instead.", false)]
		public bool ShouldValidateUnityObjectReference(int instanceID)
		{
			return ShouldValidateUnityObjectReference(OdinEntityId.Internal.FromInstanceId(instanceID));
		}
	}
}
