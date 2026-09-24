using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Sirenix.Utilities.Editor
{
	/// <summary>
	/// Utility functions for Unity assets.
	/// </summary>
	public static class AssetUtilities
	{
		/// <summary>
		/// Asset search helper.
		/// </summary>
		public struct AssetSearchResult
		{
			/// <summary>
			/// The asset object.
			/// </summary>
			public UnityEngine.Object Asset;

			/// <summary>
			/// Current index.
			/// </summary>
			public int CurrentIndex;

			/// <summary>
			/// Search result count.
			/// </summary>
			public int NumberOfResults;
		}

		private static List<UnityEngine.Component> componentListBuffer = new List<UnityEngine.Component>();

		private static readonly Type[] createableAssetTypes = new Type[3]
		{
			typeof(ScriptableObject),
			typeof(MonoBehaviour),
			typeof(GameObject)
		};

		/// <summary>
		/// Gets all assets of the specified type.
		/// </summary>
		public static IEnumerable<T> GetAllAssetsOfType<T>() where T : UnityEngine.Object
		{
			foreach (UnityEngine.Object item in GetAllAssetsOfType(typeof(T)))
			{
				yield return (T)item;
			}
		}

		/// <summary>
		/// Gets all assets of the specified type.
		/// </summary>
		/// <param name="type">The type of assets to find.</param>
		/// <param name="folderPath">The asset folder path.</param>
		public static IEnumerable<UnityEngine.Object> GetAllAssetsOfType(Type type, string folderPath = null)
		{
			foreach (AssetSearchResult item in GetAllAssetsOfTypeWithProgress(type, folderPath))
			{
				yield return item.Asset;
			}
		}

		/// <summary>
		/// Gets all assets of the specified type.
		/// </summary>
		/// <param name="type">The type of assets to find.</param>
		/// <param name="folderPath">The asset folder path.</param>
		public static IEnumerable<AssetSearchResult> GetAllAssetsOfTypeWithProgress(Type type, string folderPath = null)
		{
			AssetSearchResult item = default(AssetSearchResult);
			if (folderPath != null)
			{
				folderPath = folderPath.Trim(new char[1] { '/' });
				if (!folderPath.StartsWith("Assets/", StringComparison.InvariantCultureIgnoreCase))
				{
					folderPath = "Assets/" + folderPath;
				}
			}
			if (type == typeof(GameObject))
			{
				string[] goGuids = ((folderPath == null) ? AssetDatabase.FindAssets("t:Prefab") : AssetDatabase.FindAssets("t:Prefab", new string[1] { folderPath }));
				item.NumberOfResults = goGuids.Length;
				for (int i = 0; i < goGuids.Length; i++)
				{
					string goPath = AssetDatabase.GUIDToAssetPath(goGuids[i]);
					GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(goPath);
					item.CurrentIndex = i;
					item.Asset = go;
					yield return item;
				}
			}
			else if (type.InheritsFrom(typeof(UnityEngine.Component)))
			{
				string[] goGuids = ((folderPath == null) ? AssetDatabase.FindAssets("t:Prefab") : AssetDatabase.FindAssets("t:Prefab", new string[1] { folderPath }));
				item.NumberOfResults = goGuids.Length;
				for (int i = 0; i < goGuids.Length; i++)
				{
					string goPath2 = AssetDatabase.GUIDToAssetPath(goGuids[i]);
					GameObject go2 = AssetDatabase.LoadAssetAtPath<GameObject>(goPath2);
					go2.GetComponents(type, componentListBuffer);
					item.CurrentIndex = i;
					for (int j = 0; j < componentListBuffer.Count; j++)
					{
						item.Asset = componentListBuffer[j];
						yield return item;
					}
				}
			}
			else
			{
				string typeNameToUse = ((type.FullName.StartsWith("UnityEngine.") || type.FullName.StartsWith("UnityEditor.")) ? type.Name : type.FullName);
				string[] goGuids = ((folderPath == null) ? AssetDatabase.FindAssets("t:" + typeNameToUse) : AssetDatabase.FindAssets("t:" + typeNameToUse, new string[1] { folderPath }));
				item.NumberOfResults = goGuids.Length;
				for (int i = 0; i < goGuids.Length; i++)
				{
					item.CurrentIndex = i;
					item.Asset = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(goGuids[i]), type);
					yield return item;
				}
			}
		}

		/// <summary>
		/// Tests if an asset can be created from a type.
		/// </summary>
		/// <typeparam name="T">The type to test.</typeparam>
		/// <returns><c>true</c> if an asset can be created. Otherwise <c>false</c>.</returns>
		public static bool CanCreateNewAsset<T>()
		{
			for (int i = 0; i < createableAssetTypes.Length; i++)
			{
				if (typeof(T).InheritsFrom(createableAssetTypes[i]))
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Tests if an asset can be created from a type.
		/// </summary>
		/// <typeparam name="T">The type to test.</typeparam>
		/// <param name="baseType">The base asset type.</param>
		/// <returns><c>true</c> if an asset can be created. Otherwise <c>false</c>.</returns>
		public static bool CanCreateNewAsset<T>(out Type baseType)
		{
			for (int i = 0; i < createableAssetTypes.Length; i++)
			{
				if (typeof(T).InheritsFrom(createableAssetTypes[i]))
				{
					baseType = createableAssetTypes[i];
					return true;
				}
			}
			baseType = null;
			return false;
		}

		/// <summary>
		/// Gets project path to the specified asset.
		/// </summary>
		/// <param name="obj">The asset object.</param>
		/// <returns>The path to the asset.</returns>
		public static string GetAssetLocation(UnityEngine.Object obj)
		{
			string path = AssetDatabase.GetAssetPath(obj);
			return path.Substring(0, path.LastIndexOf('/'));
		}

		/// <summary>
		/// Creates a new asset of the specified type.
		/// </summary>
		/// <typeparam name="T">The type of the asset.</typeparam>
		/// <param name="path">Project path to the new asset.</param>
		/// <param name="assetName">The name of the asset.</param>
		[Obsolete("This will eventually be removed and is only used by the AssetList attribute drawer. Use the AssetDatabase manually instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static void CreateNewAsset<T>(string path, string assetName) where T : UnityEngine.Object
		{
			if (!CanCreateNewAsset<T>(out var assetBaseType))
			{
				Debug.LogError("Unable to create new asset of type " + typeof(T).GetNiceName());
				return;
			}
			if (path == null)
			{
				path = "";
			}
			else
			{
				path = path.Trim().TrimStart(new char[1] { '/' }).TrimEnd(new char[1] { '/' })
					.Trim();
				if (path.ToLower(CultureInfo.InvariantCulture).StartsWith("assets", StringComparison.InvariantCulture))
				{
					path = path.Substring(6, path.Length - 6).TrimStart(new char[1] { '/' });
				}
			}
			string fullPath = Application.dataPath + "/" + path;
			if (!Directory.Exists(fullPath))
			{
				Directory.CreateDirectory(fullPath);
			}
			assetName = assetName ?? typeof(T).GetNiceName();
			if (assetName.IndexOf('.') < 0)
			{
				assetName = assetName + "." + GetAssetFileExtensionName(assetBaseType);
			}
			path = AssetDatabase.GenerateUniqueAssetPath("Assets/" + path + "/" + assetName);
			GameObject prefab = null;
			UnityEngine.Object asset;
			if (assetBaseType == typeof(ScriptableObject))
			{
				asset = ScriptableObject.CreateInstance(typeof(T));
			}
			else if (assetBaseType == typeof(MonoBehaviour))
			{
				GameObject go = new GameObject();
				go.AddComponent(typeof(T));
				asset = go;
				prefab = go;
			}
			else
			{
				if (!(assetBaseType == typeof(GameObject)))
				{
					throw new NotImplementedException();
				}
				asset = (prefab = new GameObject());
			}
			if (prefab != null)
			{
				asset = PrefabUtility.CreatePrefab(path, prefab);
			}
			else
			{
				AssetDatabase.CreateAsset(asset, path);
			}
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
			EditorGUIUtility.PingObject(asset);
			if (prefab != null)
			{
				UnityEngine.Object.DestroyImmediate(prefab);
			}
		}

		private static string GetAssetFileExtensionName(Type type)
		{
			if (type == typeof(ScriptableObject))
			{
				return "asset";
			}
			if (type == typeof(GameObject) || type == typeof(MonoBehaviour))
			{
				return "prefab";
			}
			return null;
		}
	}
}
