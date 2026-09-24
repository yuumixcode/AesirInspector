using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Sirenix.Reflection.Editor
{
	public static class AssetDatabase_Internals
	{
		private static readonly MethodInfo AssetDatabase_EnumerateAllAssets_Method;

		private static readonly MethodInfo EditorUtility_InstanceIDToObject_Method;

		private static readonly MethodInfo Object_GetInstanceID_Method;

		static AssetDatabase_Internals()
		{
			AssetDatabaseAssetInfo.EnsureInitialized();
			AssetDatabase_EnumerateAllAssets_Method = typeof(AssetDatabase).GetMethod("EnumerateAllAssets", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[1] { typeof(UnityEditor.SearchFilter) }, null);
			EditorUtility_InstanceIDToObject_Method = typeof(EditorUtility).GetMethod("InstanceIDToObject", BindingFlags.Static | BindingFlags.Public, null, new Type[1] { typeof(int) }, null);
			Object_GetInstanceID_Method = typeof(UnityEngine.Object).GetMethod("GetInstanceID", BindingFlags.Instance | BindingFlags.Public, null, Type.EmptyTypes, null);
		}

		public static Dictionary<string, float> GetAllLabels()
		{
			return AssetDatabase.GetAllLabels();
		}

		public static IEnumerator<AssetDatabaseAssetInfo> EnumerateAllAssets(string filter, bool includeHidden, AssetDatabaseSearchArea searchArea)
		{
			AssetDatabaseAssetInfo.EnsureInitialized();
			UnityEditor.SearchFilter searchFilter = new UnityEditor.SearchFilter
			{
				searchArea = (UnityEditor.SearchFilter.SearchArea)searchArea,
				skipHidden = !includeHidden
			};
			UnityEditor.SearchUtility.ParseSearchString(filter, searchFilter);
			if (AssetDatabase_EnumerateAllAssets_Method == null)
			{
				Debug.LogError("[Odin] Sirenix.Reflection.Editor failed to find 'AssetDatabase.EnumerateAllAssets'.");
			}
			else
			{
				if (!(AssetDatabase_EnumerateAllAssets_Method.Invoke(null, new object[1] { searchFilter }) is IEnumerator enumerator))
				{
					yield break;
				}
				while (enumerator.MoveNext())
				{
					object current = enumerator.Current;
					if (current != null)
					{
						yield return new AssetDatabaseAssetInfo
						{
							UnityHierarchyReference = current
						};
					}
				}
			}
		}

		public static UnityEngine.Object[] FindAssets(string filter, bool includeHidden, AssetDatabaseSearchArea searchArea)
		{
			IEnumerator<AssetDatabaseAssetInfo> enumerator = EnumerateAllAssets(filter, includeHidden, searchArea);
			List<UnityEngine.Object> result = new List<UnityEngine.Object>(32);
			while (enumerator.MoveNext())
			{
				OdinEntityId currentId = enumerator.Current.Id;
				if (currentId.IsValid)
				{
					UnityEngine.Object obj = currentId.ToObject();
					if (!(obj == null))
					{
						result.Add(obj);
					}
				}
			}
			return result.ToArray();
		}

		public static TUnityObject[] FindAssets<TUnityObject>(string filter, bool includeHidden, AssetDatabaseSearchArea searchArea) where TUnityObject : UnityEngine.Object
		{
			IEnumerator<AssetDatabaseAssetInfo> enumerator = EnumerateAllAssets("t:" + typeof(TUnityObject).Name + " " + filter, includeHidden, searchArea);
			List<TUnityObject> result = new List<TUnityObject>(32);
			while (enumerator.MoveNext())
			{
				OdinEntityId currentId = enumerator.Current.Id;
				if (currentId.IsValid)
				{
					UnityEngine.Object obj = currentId.ToObject();
					if (!(obj == null))
					{
						result.Add((TUnityObject)obj);
					}
				}
			}
			return result.ToArray();
		}

		public static OdinEntityId[] FindAssetEntityIds(string filter, bool includeHidden, AssetDatabaseSearchArea searchArea)
		{
			IEnumerator<AssetDatabaseAssetInfo> enumerator = EnumerateAllAssets(filter, includeHidden, searchArea);
			List<OdinEntityId> result = new List<OdinEntityId>(32);
			while (enumerator.MoveNext())
			{
				result.Add(enumerator.Current.Id);
			}
			return result.ToArray();
		}

		private static UnityEngine.Object InstanceIDToObject(int instanceID)
		{
			if (!(EditorUtility_InstanceIDToObject_Method == null))
			{
				return EditorUtility_InstanceIDToObject_Method.Invoke(null, new object[1] { instanceID }) as UnityEngine.Object;
			}
			return null;
		}

		private static int GetInstanceID(UnityEngine.Object obj)
		{
			if (obj == null || Object_GetInstanceID_Method == null)
			{
				return 0;
			}
			object result = Object_GetInstanceID_Method.Invoke(obj, null);
			if (result is int)
			{
				return (int)result;
			}
			return 0;
		}

		private static bool AssetPathMatchesSearchArea(string assetPath, AssetDatabaseSearchArea searchArea)
		{
			switch (searchArea)
			{
			case AssetDatabaseSearchArea.InAssetsOnly:
			case AssetDatabaseSearchArea.SelectedFolders:
				if (!(assetPath == "Assets"))
				{
					return assetPath.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase);
				}
				return true;
			case AssetDatabaseSearchArea.InPackagesOnly:
				if (!(assetPath == "Packages"))
				{
					return assetPath.StartsWith("Packages/", StringComparison.OrdinalIgnoreCase);
				}
				return true;
			case AssetDatabaseSearchArea.AssetStore:
				return assetPath.StartsWith("Assets/Asset Store", StringComparison.OrdinalIgnoreCase);
			default:
				return true;
			}
		}

		private static bool IsHiddenAssetPath(string assetPath)
		{
			string fileName = Path.GetFileName(assetPath);
			if (!string.IsNullOrEmpty(fileName))
			{
				return fileName[0] == '.';
			}
			return false;
		}

		private static T GetHierarchyPropertyValue<T>(object hierarchyProperty, PropertyInfo property)
		{
			if (property == null)
			{
				return default(T);
			}
			object value = property.GetValue(hierarchyProperty, null);
			if (value is T)
			{
				return (T)value;
			}
			return default(T);
		}
	}
}
