using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Sirenix.Serialization.Utilities;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Sirenix.Serialization.Editor
{
	public sealed class AOTSupportScanner : IDisposable
	{
		[Serializable]
		private class VersionDefine
		{
			public string name;

			public string expression;

			public string define;
		}

		[Serializable]
		private class AssemblyDefinitionData
		{
			public string name;

			public string rootNamespace;

			public string[] references;

			public string[] includePlatforms;

			public string[] excludePlatforms;

			public bool allowUnsafeCode;

			public bool overrideReferences;

			public string[] precompiledReferences;

			public bool autoReferenced;

			public string[] defineConstraints;

			public VersionDefine[] versionDefines;

			public bool noEngineReferences;
		}

		private bool scanning;

		private bool allowRegisteringScannedTypes;

		private HashSet<Type> seenSerializedTypes = new HashSet<Type>();

		private HashSet<string> scannedPathsNoDependencies = new HashSet<string>();

		private HashSet<string> scannedPathsWithDependencies = new HashSet<string>();

		private static Stopwatch smartProgressBarWatch = Stopwatch.StartNew();

		private static int smartProgressBarDisplaysSinceLastUpdate = 0;

		private static readonly MethodInfo PlayerSettings_GetPreloadedAssets_Method = typeof(PlayerSettings).GetMethod("GetPreloadedAssets", BindingFlags.Static | BindingFlags.Public, null, Type.EmptyTypes, null);

		private static readonly PropertyInfo Debug_Logger_Property = typeof(UnityEngine.Debug).GetProperty("unityLogger") ?? typeof(UnityEngine.Debug).GetProperty("logger");

		private static Dictionary<Assembly, bool> IsEditorOnlyAssembly_Cache = new Dictionary<Assembly, bool>();

		private static HashSet<string> EditorAssemblyNames = new HashSet<string>
		{
			"Assembly-CSharp-Editor",
			"Assembly-UnityScript-Editor",
			"Assembly-Boo-Editor",
			"Assembly-CSharp-Editor-firstpass",
			"Assembly-UnityScript-Editor-firstpass",
			"Assembly-Boo-Editor-firstpass",
			"Sirenix.OdinInspector.Editor",
			"Sirenix.Utilities.Editor",
			"Sirenix.Reflection.Editor",
			typeof(UnityEditor.Editor).Assembly.GetName().Name
		};

		public void BeginScan()
		{
			scanning = true;
			allowRegisteringScannedTypes = false;
			seenSerializedTypes.Clear();
			scannedPathsNoDependencies.Clear();
			scannedPathsWithDependencies.Clear();
			FormatterLocator.OnLocatedEmittableFormatterForType += OnLocatedEmitType;
			FormatterLocator.OnLocatedFormatter += OnLocatedFormatter;
			Serializer.OnSerializedType += OnSerializedType;
		}

		public bool ScanPreloadedAssets(bool showProgressBar)
		{
			if (PlayerSettings_GetPreloadedAssets_Method == null)
			{
				return true;
			}
			UnityEngine.Object[] assets = (UnityEngine.Object[])PlayerSettings_GetPreloadedAssets_Method.Invoke(null, null);
			if (assets == null)
			{
				return true;
			}
			try
			{
				for (int i = 0; i < assets.Length; i++)
				{
					if (showProgressBar && DisplaySmartUpdatingCancellableProgressBar("Scanning preloaded assets for AOT support", i + 1 + " / " + assets.Length, (float)i / (float)assets.Length))
					{
						return false;
					}
					UnityEngine.Object asset = assets[i];
					if (!(asset == null))
					{
						if (AssetDatabase.Contains(asset))
						{
							string path = AssetDatabase.GetAssetPath(asset);
							ScanAsset(path, includeAssetDependencies: true);
						}
						else
						{
							ScanObject(asset);
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
			}
			return true;
		}

		public bool ScanAssetBundle(string bundle)
		{
			string[] assets = AssetDatabase.GetAssetPathsFromAssetBundle(bundle);
			string[] array = assets;
			foreach (string asset in array)
			{
				ScanAsset(asset, includeAssetDependencies: true);
			}
			return true;
		}

		public bool ScanAllAssetBundles(bool showProgressBar)
		{
			try
			{
				string[] bundles = AssetDatabase.GetAllAssetBundleNames();
				for (int i = 0; i < bundles.Length; i++)
				{
					string bundle = bundles[i];
					if (showProgressBar && DisplaySmartUpdatingCancellableProgressBar("Scanning asset bundles for AOT support", bundle, (float)i / (float)bundles.Length))
					{
						return false;
					}
					ScanAssetBundle(bundle);
				}
			}
			finally
			{
				if (showProgressBar)
				{
					EditorUtility.ClearProgressBar();
				}
			}
			return true;
		}

		public bool ScanAllAddressables(bool includeAssetDependencies, bool showProgressBar)
		{
			bool progressBarWasDisplayed = false;
			try
			{
				Type AddressableAssetSettingsDefaultObject_Type = TwoWaySerializationBinder.Default.BindToType("UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject");
				if (AddressableAssetSettingsDefaultObject_Type == null)
				{
					return true;
				}
				PropertyInfo AddressableAssetSettingsDefaultObject_Settings = AddressableAssetSettingsDefaultObject_Type.GetProperty("Settings");
				if (AddressableAssetSettingsDefaultObject_Settings == null)
				{
					throw new NotSupportedException("AddressableAssetSettingsDefaultObject.Settings property not found");
				}
				ScriptableObject settings = (ScriptableObject)AddressableAssetSettingsDefaultObject_Settings.GetValue(null, null);
				if (settings == null)
				{
					return true;
				}
				Type AddressableAssetSettings_Type = settings.GetType();
				PropertyInfo AddressableAssetSettings_groups = AddressableAssetSettings_Type.GetProperty("groups");
				if (AddressableAssetSettings_groups == null)
				{
					throw new NotSupportedException("AddressableAssetSettings.groups property not found");
				}
				IList groups = (IList)AddressableAssetSettings_groups.GetValue(settings, null);
				if (groups == null)
				{
					return true;
				}
				Type PlayerDataGroupSchema_Type = TwoWaySerializationBinder.Default.BindToType("UnityEditor.AddressableAssets.Settings.GroupSchemas.PlayerDataGroupSchema");
				Type AddressableAssetGroup_Type = null;
				MethodInfo AddressableAssetGroup_HasSchema = null;
				MethodInfo AddressableAssetGroup_GatherAllAssets = null;
				Type AddressableAssetEntry_Type = TwoWaySerializationBinder.Default.BindToType("UnityEditor.AddressableAssets.Settings.AddressableAssetEntry");
				if (AddressableAssetEntry_Type == null)
				{
					throw new NotSupportedException("AddressableAssetEntry type not found");
				}
				Type List_AddressableAssetEntry_Type = typeof(List<>).MakeGenericType(AddressableAssetEntry_Type);
				Type Func_AddressableAssetEntry_bool_Type = typeof(Func<, >).MakeGenericType(AddressableAssetEntry_Type, typeof(bool));
				PropertyInfo AddressableAssetEntry_AssetPath = AddressableAssetEntry_Type.GetProperty("AssetPath");
				if (AddressableAssetEntry_AssetPath == null)
				{
					throw new NotSupportedException("AddressableAssetEntry.AssetPath property not found");
				}
				foreach (object groupObj in groups)
				{
					ScriptableObject group = (ScriptableObject)groupObj;
					if (group == null)
					{
						continue;
					}
					string groupName = group.name;
					if (AddressableAssetGroup_Type == null)
					{
						AddressableAssetGroup_Type = group.GetType();
						AddressableAssetGroup_HasSchema = AddressableAssetGroup_Type.GetMethod("HasSchema", BindingFlags.Instance | BindingFlags.Public, null, new Type[1] { typeof(Type) }, null);
						if (AddressableAssetGroup_HasSchema == null)
						{
							throw new NotSupportedException("AddressableAssetGroup.HasSchema(Type type) method not found");
						}
						AddressableAssetGroup_GatherAllAssets = AddressableAssetGroup_Type.GetMethod("GatherAllAssets", BindingFlags.Instance | BindingFlags.Public, null, new Type[5]
						{
							List_AddressableAssetEntry_Type,
							typeof(bool),
							typeof(bool),
							typeof(bool),
							Func_AddressableAssetEntry_bool_Type
						}, null);
						if (AddressableAssetGroup_GatherAllAssets == null)
						{
							throw new NotSupportedException("AddressableAssetGroup.GatherAllAssets(List<AddressableAssetEntry> results, bool includeSelf, bool recurseAll, bool includeSubObjects, Func<AddressableAssetEntry, bool> entryFilter) method not found");
						}
					}
					bool hasPlayerDataGroupSchema = false;
					if (PlayerDataGroupSchema_Type != null)
					{
						hasPlayerDataGroupSchema = (bool)AddressableAssetGroup_HasSchema.Invoke(group, new object[1] { PlayerDataGroupSchema_Type });
					}
					if (hasPlayerDataGroupSchema)
					{
						continue;
					}
					IList results = (IList)Activator.CreateInstance(List_AddressableAssetEntry_Type);
					AddressableAssetGroup_GatherAllAssets.Invoke(group, new object[5] { results, true, true, true, null });
					for (int i = 0; i < results.Count; i++)
					{
						object entry = results[i];
						if (entry == null)
						{
							continue;
						}
						string assetPath = (string)AddressableAssetEntry_AssetPath.GetValue(entry, null);
						if (showProgressBar)
						{
							progressBarWasDisplayed = true;
							if (DisplaySmartUpdatingCancellableProgressBar("Scanning addressables for AOT support", groupName + ": " + assetPath, (float)i / (float)results.Count))
							{
								return false;
							}
						}
						ScanAsset(assetPath, includeAssetDependencies);
					}
				}
			}
			catch (NotSupportedException ex)
			{
				UnityEngine.Debug.LogWarning("Could not AOT scan Addressables assets due to missing APIs: " + ex.Message);
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogError("Scanning addressables failed with the following exception...");
				UnityEngine.Debug.LogException(exception);
			}
			finally
			{
				if (progressBarWasDisplayed)
				{
					EditorUtility.ClearProgressBar();
				}
			}
			return true;
		}

		public bool ScanAllResources(bool includeResourceDependencies, bool showProgressBar, List<string> resourcesPaths = null)
		{
			if (resourcesPaths == null)
			{
				resourcesPaths = new List<string> { "" };
			}
			try
			{
				if (showProgressBar && DisplaySmartUpdatingCancellableProgressBar("Scanning resources for AOT support", "Loading resource assets", 0f))
				{
					return false;
				}
				HashSet<string> resourcesPathsSet = new HashSet<string>();
				for (int i = 0; i < resourcesPaths.Count; i++)
				{
					string resourcesPath = resourcesPaths[i];
					if (showProgressBar && DisplaySmartUpdatingCancellableProgressBar("Listing resources for AOT support", resourcesPath, (float)i / (float)resourcesPaths.Count))
					{
						return false;
					}
					UnityEngine.Object[] resources = Resources.LoadAll(resourcesPath);
					UnityEngine.Object[] array = resources;
					foreach (UnityEngine.Object resource in array)
					{
						try
						{
							string assetPath = AssetDatabase.GetAssetPath(resource);
							if (assetPath != null)
							{
								resourcesPathsSet.Add(assetPath);
							}
						}
						catch (MissingReferenceException exception)
						{
							UnityEngine.Debug.LogError("A resource threw a missing reference exception when scanning. Skipping resource and continuing scan.", resource);
							UnityEngine.Debug.LogException(exception, resource);
						}
					}
				}
				string[] resourcePaths = resourcesPathsSet.ToArray();
				for (int k = 0; k < resourcePaths.Length; k++)
				{
					if (resourcePaths[k] == null)
					{
						continue;
					}
					try
					{
						if (showProgressBar && DisplaySmartUpdatingCancellableProgressBar("Scanning resource " + k + " for AOT support", resourcePaths[k], (float)k / (float)resourcePaths.Length))
						{
							return false;
						}
						string assetPath2 = resourcePaths[k];
						if (!assetPath2.ToLower().Contains("/editor/"))
						{
							ScanAsset(assetPath2, includeResourceDependencies);
						}
					}
					catch (MissingReferenceException exception2)
					{
						UnityEngine.Debug.LogError("A resource '" + resourcePaths[k] + "' threw a missing reference exception when scanning. Skipping resource and continuing scan.");
						UnityEngine.Debug.LogException(exception2);
					}
				}
				return true;
			}
			finally
			{
				if (showProgressBar)
				{
					EditorUtility.ClearProgressBar();
				}
			}
		}

		public bool ScanBuildScenes(bool includeSceneDependencies, bool showProgressBar)
		{
			string[] scenePaths = (from n in EditorBuildSettings.scenes
				where n.enabled
				select n.path).ToArray();
			return ScanScenes(scenePaths, includeSceneDependencies, showProgressBar);
		}

		public bool ScanScenes(string[] scenePaths, bool includeSceneDependencies, bool showProgressBar)
		{
			if (scenePaths.Length == 0)
			{
				return true;
			}
			bool formerForceEditorModeSerialization = UnitySerializationUtility.ForceEditorModeSerialization;
			try
			{
				UnitySerializationUtility.ForceEditorModeSerialization = true;
				bool hasDirtyScenes = false;
				for (int i = 0; i < SceneManager.sceneCount; i++)
				{
					if (SceneManager.GetSceneAt(i).isDirty)
					{
						hasDirtyScenes = true;
						break;
					}
				}
				if (hasDirtyScenes && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
				{
					return false;
				}
				SceneSetup[] oldSceneSetup = EditorSceneManager.GetSceneManagerSetup();
				try
				{
					for (int j = 0; j < scenePaths.Length; j++)
					{
						string scenePath = scenePaths[j];
						if (showProgressBar && DisplaySmartUpdatingCancellableProgressBar("Scanning scenes for AOT support", "Scene " + (j + 1) + "/" + scenePaths.Length + " - " + scenePath, (float)j / (float)scenePaths.Length))
						{
							return false;
						}
						if (!File.Exists(scenePath))
						{
							UnityEngine.Debug.LogWarning("Skipped AOT scanning scene '" + scenePath + "' for a file not existing at the scene path.");
							continue;
						}
						Scene openScene = default(Scene);
						try
						{
							openScene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
						}
						catch
						{
							UnityEngine.Debug.LogWarning("Skipped AOT scanning scene '" + scenePath + "' for throwing exceptions when trying to load it.");
							continue;
						}
						GameObject[] sceneGOs = Resources.FindObjectsOfTypeAll<GameObject>();
						GameObject[] array = sceneGOs;
						foreach (GameObject go in array)
						{
							if (go.scene != openScene || (go.hideFlags & HideFlags.DontSaveInBuild) != HideFlags.None)
							{
								continue;
							}
							ISerializationCallbackReceiver[] components = go.GetComponents<ISerializationCallbackReceiver>();
							foreach (ISerializationCallbackReceiver component in components)
							{
								try
								{
									allowRegisteringScannedTypes = true;
									component.OnBeforeSerialize();
									if (component is ISupportsPrefabSerialization prefabSupporter)
									{
										List<UnityEngine.Object> objs = null;
										List<PrefabModification> mods = UnitySerializationUtility.DeserializePrefabModifications(prefabSupporter.SerializationData.PrefabModifications, prefabSupporter.SerializationData.PrefabModificationsReferencedUnityObjects);
										UnitySerializationUtility.SerializePrefabModifications(mods, ref objs);
									}
								}
								finally
								{
									allowRegisteringScannedTypes = false;
								}
							}
						}
					}
					UnityEngine.ILogger logger = null;
					if (Debug_Logger_Property != null)
					{
						logger = (UnityEngine.ILogger)Debug_Logger_Property.GetValue(null, null);
					}
					bool previous = true;
					try
					{
						if (logger != null)
						{
							previous = logger.logEnabled;
							logger.logEnabled = false;
						}
						EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
					}
					catch
					{
					}
					finally
					{
						if (logger != null)
						{
							logger.logEnabled = previous;
						}
					}
				}
				finally
				{
					if (oldSceneSetup != null && oldSceneSetup.Length != 0)
					{
						if (showProgressBar)
						{
							EditorUtility.DisplayProgressBar("Restoring scene setup", "", 1f);
						}
						EditorSceneManager.RestoreSceneManagerSetup(oldSceneSetup);
					}
				}
				if (includeSceneDependencies)
				{
					for (int m = 0; m < scenePaths.Length; m++)
					{
						string scenePath2 = scenePaths[m];
						if (showProgressBar && DisplaySmartUpdatingCancellableProgressBar("Scanning scene dependencies for AOT support", "Scene " + (m + 1) + "/" + scenePaths.Length + " - " + scenePath2, (float)m / (float)scenePaths.Length))
						{
							return false;
						}
						string[] dependencies = AssetDatabase.GetDependencies(scenePath2, recursive: true);
						string[] array2 = dependencies;
						foreach (string dependency in array2)
						{
							ScanAsset(dependency, includeAssetDependencies: false);
						}
					}
				}
				return true;
			}
			finally
			{
				if (showProgressBar)
				{
					EditorUtility.ClearProgressBar();
				}
				UnitySerializationUtility.ForceEditorModeSerialization = formerForceEditorModeSerialization;
			}
		}

		public bool ScanAsset(string assetPath, bool includeAssetDependencies)
		{
			if (includeAssetDependencies)
			{
				if (scannedPathsWithDependencies.Contains(assetPath))
				{
					return true;
				}
				scannedPathsWithDependencies.Add(assetPath);
				scannedPathsNoDependencies.Add(assetPath);
			}
			else
			{
				if (scannedPathsNoDependencies.Contains(assetPath))
				{
					return true;
				}
				scannedPathsNoDependencies.Add(assetPath);
			}
			if (assetPath.EndsWith(".unity"))
			{
				return ScanScenes(new string[1] { assetPath }, includeAssetDependencies, showProgressBar: false);
			}
			if (!assetPath.EndsWith(".asset") && !assetPath.EndsWith(".prefab"))
			{
				return false;
			}
			bool formerForceEditorModeSerialization = UnitySerializationUtility.ForceEditorModeSerialization;
			try
			{
				UnitySerializationUtility.ForceEditorModeSerialization = true;
				UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
				if (assets == null || assets.Length == 0)
				{
					return false;
				}
				UnityEngine.Object[] array = assets;
				foreach (UnityEngine.Object asset in array)
				{
					if (!(asset == null))
					{
						ScanObject(asset);
					}
				}
				if (includeAssetDependencies)
				{
					string[] dependencies = AssetDatabase.GetDependencies(assetPath, recursive: true);
					string[] array2 = dependencies;
					foreach (string dependency in array2)
					{
						ScanAsset(dependency, includeAssetDependencies: false);
					}
				}
				return true;
			}
			finally
			{
				UnitySerializationUtility.ForceEditorModeSerialization = formerForceEditorModeSerialization;
			}
		}

		public void ScanObject(UnityEngine.Object obj)
		{
			if (obj is ISerializationCallbackReceiver)
			{
				bool formerForceEditorModeSerialization = UnitySerializationUtility.ForceEditorModeSerialization;
				try
				{
					UnitySerializationUtility.ForceEditorModeSerialization = true;
					allowRegisteringScannedTypes = true;
					(obj as ISerializationCallbackReceiver).OnBeforeSerialize();
				}
				finally
				{
					allowRegisteringScannedTypes = false;
					UnitySerializationUtility.ForceEditorModeSerialization = formerForceEditorModeSerialization;
				}
			}
		}

		public List<Type> EndScan()
		{
			if (!scanning)
			{
				throw new InvalidOperationException("Cannot end a scan when scanning has not begun.");
			}
			HashSet<Type> results = new HashSet<Type>();
			foreach (Type type in seenSerializedTypes)
			{
				GatherValidAOTSupportTypes(type, results);
			}
			Dispose();
			return results.ToList();
		}

		public void Dispose()
		{
			if (scanning)
			{
				FormatterLocator.OnLocatedEmittableFormatterForType -= OnLocatedEmitType;
				FormatterLocator.OnLocatedFormatter -= OnLocatedFormatter;
				Serializer.OnSerializedType -= OnSerializedType;
				scanning = false;
				seenSerializedTypes.Clear();
				allowRegisteringScannedTypes = false;
			}
		}

		private void OnLocatedEmitType(Type type)
		{
			if (allowRegisteringScannedTypes)
			{
				seenSerializedTypes.Add(type);
			}
		}

		private void OnSerializedType(Type type)
		{
			if (allowRegisteringScannedTypes)
			{
				seenSerializedTypes.Add(type);
			}
		}

		private void OnLocatedFormatter(IFormatter formatter)
		{
			Type type = formatter.SerializedType;
			if (!(type == null) && allowRegisteringScannedTypes)
			{
				seenSerializedTypes.Add(type);
			}
		}

		public static bool AllowRegisterType(Type type)
		{
			if (IsEditorOnlyAssembly(type.Assembly))
			{
				return false;
			}
			if (type.IsGenericType)
			{
				Type[] genericArguments = type.GetGenericArguments();
				foreach (Type parameter in genericArguments)
				{
					if (!AllowRegisterType(parameter))
					{
						return false;
					}
				}
			}
			return true;
		}

		private static bool IsEditorOnlyAssembly(Assembly assembly)
		{
			if (EditorAssemblyNames.Contains(assembly.GetName().Name))
			{
				return true;
			}
			if (!IsEditorOnlyAssembly_Cache.TryGetValue(assembly, out var result))
			{
				try
				{
					string name = assembly.GetName().Name;
					string[] guids = AssetDatabase.FindAssets(name);
					string[] paths = new string[guids.Length];
					int dllCount = 0;
					int dllIndex = 0;
					for (int i = 0; i < guids.Length; i++)
					{
						paths[i] = AssetDatabase.GUIDToAssetPath(guids[i]);
						if (paths[i].EndsWith(".dll", StringComparison.OrdinalIgnoreCase) || paths[i].EndsWith(".asmdef", StringComparison.OrdinalIgnoreCase))
						{
							dllCount++;
							dllIndex = i;
						}
					}
					if (dllCount == 1)
					{
						string path = paths[dllIndex];
						AssetImporter assetImporter = AssetImporter.GetAtPath(path);
						if (assetImporter is PluginImporter)
						{
							PluginImporter pluginImporter = assetImporter as PluginImporter;
							if (!pluginImporter.GetCompatibleWithEditor())
							{
								result = false;
							}
							else if (pluginImporter.DefineConstraints.Any((string n) => n == "UNITY_EDITOR"))
							{
								result = true;
							}
							else
							{
								bool isCompatibleWithAnyNonEditorPlatform = false;
								FieldInfo[] fields = typeof(BuildTarget).GetFields(BindingFlags.Static | BindingFlags.Public);
								foreach (FieldInfo member in fields)
								{
									BuildTarget platform = (BuildTarget)member.GetValue(null);
									int asInt = Convert.ToInt32(platform);
									if (!member.IsDefined(typeof(ObsoleteAttribute)) && asInt >= 0 && pluginImporter.GetCompatibleWithPlatform(platform))
									{
										isCompatibleWithAnyNonEditorPlatform = true;
										break;
									}
								}
								result = !isCompatibleWithAnyNonEditorPlatform;
							}
						}
						else if (assetImporter is AssemblyDefinitionImporter)
						{
							AssemblyDefinitionImporter asmDefImporter = assetImporter as AssemblyDefinitionImporter;
							AssemblyDefinitionAsset asset = AssetDatabase.LoadAssetAtPath<AssemblyDefinitionAsset>(path);
							if (asset == null)
							{
								result = false;
							}
							else
							{
								AssemblyDefinitionData data = JsonUtility.FromJson<AssemblyDefinitionData>(asset.text);
								if (data != null && data.defineConstraints != null)
								{
									int i2 = 0;
									while (i2 < data.defineConstraints.Length)
									{
										if (!(data.defineConstraints[i2].Trim() == "UNITY_EDITOR"))
										{
											i2++;
											continue;
										}
										goto IL_01ec;
									}
								}
								result = false;
							}
						}
						else
						{
							result = false;
						}
					}
					else
					{
						result = false;
					}
					goto IL_020d;
					IL_01ec:
					result = true;
					goto IL_020d;
					IL_020d:
					IsEditorOnlyAssembly_Cache.Add(assembly, result);
				}
				catch (Exception exception)
				{
					UnityEngine.Debug.LogException(exception);
					IsEditorOnlyAssembly_Cache[assembly] = false;
				}
			}
			return result;
		}

		private static void GatherValidAOTSupportTypes(Type type, HashSet<Type> results)
		{
			if ((!type.IsGenericType || (!type.IsGenericTypeDefinition && type.IsFullyConstructedGenericType())) && AllowRegisterType(type) && results.Add(type) && type.IsGenericType)
			{
				Type[] genericArguments = type.GetGenericArguments();
				foreach (Type arg in genericArguments)
				{
					GatherValidAOTSupportTypes(arg, results);
				}
			}
		}

		private static bool DisplaySmartUpdatingCancellableProgressBar(string title, string details, float progress, int updateIntervalByMS = 200, int updateIntervalByCall = 50)
		{
			if (smartProgressBarWatch.ElapsedMilliseconds >= updateIntervalByMS || ++smartProgressBarDisplaysSinceLastUpdate >= updateIntervalByCall)
			{
				smartProgressBarWatch.Stop();
				smartProgressBarWatch.Reset();
				smartProgressBarWatch.Start();
				smartProgressBarDisplaysSinceLastUpdate = 0;
				if (EditorUtility.DisplayCancelableProgressBar(title, details, progress))
				{
					return true;
				}
			}
			return false;
		}
	}
}
