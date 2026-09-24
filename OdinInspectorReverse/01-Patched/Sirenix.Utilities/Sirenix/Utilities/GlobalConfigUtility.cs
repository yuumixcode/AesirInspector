using System;
using System.IO;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.Utilities
{
	public static class GlobalConfigUtility<T> where T : ScriptableObject
	{
		private static T instance;

		/// <summary>
		/// Gets a value indicating whether this instance has instance loaded.
		/// </summary>
		public static bool HasInstanceLoaded => instance != null;

		/// <summary>
		/// Gets the singleton instance.
		/// </summary>
		public static T GetInstance(string defaultAssetFolderPath, string defaultFileNameWithoutExtension = null)
		{
			if (instance == null)
			{
				LoadInstanceIfAssetExists(defaultAssetFolderPath, defaultFileNameWithoutExtension);
				T inst = instance;
				string fileName = defaultFileNameWithoutExtension ?? typeof(T).GetNiceName();
				string fullPath = Application.dataPath + "/" + defaultAssetFolderPath + fileName + ".asset";
				if (inst == null && EditorPrefs.HasKey("PREVENT_SIRENIX_FILE_GENERATION"))
				{
					Debug.LogWarning(defaultAssetFolderPath + fileName + ".asset was prevented from being generated because the PREVENT_SIRENIX_FILE_GENERATION key was defined in Unity's EditorPrefs.");
					instance = ScriptableObject.CreateInstance<T>();
					return instance;
				}
				if (inst == null && File.Exists(fullPath) && EditorSettings.serializationMode == SerializationMode.ForceText)
				{
					if (AssetScriptGuidUtility.TryUpdateAssetScriptGuid(fullPath, typeof(T)))
					{
						Debug.Log("Could not load config asset at first, but successfully detected forced text asset serialization, and corrected the config asset m_Script guid.");
						LoadInstanceIfAssetExists(defaultAssetFolderPath, defaultFileNameWithoutExtension);
						inst = instance;
					}
					else
					{
						Debug.LogWarning("Could not load config asset, and failed to auto-correct config asset m_Script guid.");
					}
				}
				if (inst == null)
				{
					inst = ScriptableObject.CreateInstance<T>();
					string assetPathWithAssetsPrefix = defaultAssetFolderPath;
					if (!assetPathWithAssetsPrefix.StartsWith("Assets/"))
					{
						assetPathWithAssetsPrefix = "Assets/" + assetPathWithAssetsPrefix.TrimStart(new char[1] { '/' });
					}
					if (!Directory.Exists(assetPathWithAssetsPrefix))
					{
						Directory.CreateDirectory(new DirectoryInfo(assetPathWithAssetsPrefix).FullName);
						AssetDatabase.Refresh();
					}
					string niceName = fileName;
					string assetPath;
					if (defaultAssetFolderPath.StartsWith("Assets/"))
					{
						assetPath = defaultAssetFolderPath + niceName + ".asset";
					}
					else
					{
						assetPath = "Assets/" + defaultAssetFolderPath + niceName + ".asset";
					}
					if (File.Exists(fullPath))
					{
						Debug.LogWarning("Could not load config asset of type " + niceName + " from project path '" + assetPath + "', but an asset file already exists at the path, so could not create a new asset either. The config asset for '" + niceName + "' has been lost, probably due to an invalid m_Script guid. Set forced text serialization in Edit -> Project Settings -> Editor -> Asset Serialization -> Mode and trigger a script reload to allow Odin to auto-correct this.");
					}
					else
					{
						instance = inst;
						if (inst is IGlobalConfigEvents ee)
						{
							ee.OnConfigAutoCreated();
						}
						EditorApplication.delayCall = (EditorApplication.CallbackFunction)Delegate.Combine(EditorApplication.delayCall, (EditorApplication.CallbackFunction)delegate
						{
							if (inst != null)
							{
								AssetDatabase.CreateAsset(inst, assetPath);
								AssetDatabase.SaveAssets();
								AssetDatabase.Refresh();
							}
						});
					}
				}
				instance = inst;
				if (instance is IGlobalConfigEvents e)
				{
					e.OnConfigInstanceFirstAccessed();
				}
			}
			return instance;
		}

		internal static void LoadInstanceIfAssetExists(string assetPath, string defaultFileNameWithoutExtension = null)
		{
			string fileName = defaultFileNameWithoutExtension ?? typeof(T).GetNiceName();
			if (StringExtensions.Contains(assetPath, "/resources/", StringComparison.OrdinalIgnoreCase))
			{
				string resourcesPath = assetPath;
				int i = resourcesPath.LastIndexOf("/resources/", StringComparison.OrdinalIgnoreCase);
				if (i >= 0)
				{
					resourcesPath = resourcesPath.Substring(i + "/resources/".Length);
				}
				string niceName = fileName;
				instance = Resources.Load<T>(resourcesPath + niceName);
			}
			else
			{
				string niceName2 = fileName;
				instance = AssetDatabase.LoadAssetAtPath<T>(assetPath + niceName2 + ".asset");
				if (instance == null)
				{
					instance = AssetDatabase.LoadAssetAtPath<T>("Assets/" + assetPath + niceName2 + ".asset");
				}
			}
			if (instance == null)
			{
				string[] relocatedScriptableObject = AssetDatabase.FindAssets("t:" + typeof(T).Name);
				if (relocatedScriptableObject.Length != 0)
				{
					instance = AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(relocatedScriptableObject[0]));
				}
			}
		}
	}
}
