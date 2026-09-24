using System;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Internal
{
	[Serializable]
	public sealed class ProjectSettingAssetRef<T> : ProjectSetting<T> where T : UnityEngine.Object
	{
		protected override T GetLocalValue(string key, T asset)
		{
			string defaultGuid = null;
			if ((bool)asset)
			{
				string path = AssetDatabase.GetAssetPath(asset);
				if (!string.IsNullOrWhiteSpace(path))
				{
					defaultGuid = AssetDatabase.AssetPathToGUID(path);
				}
			}
			string guid = EditorPrefs.GetString(key, defaultGuid);
			if (!string.IsNullOrWhiteSpace(guid) && guid != defaultGuid)
			{
				string path2 = AssetDatabase.GUIDToAssetPath(guid);
				asset = (string.IsNullOrEmpty(path2) ? null : AssetDatabase.LoadAssetAtPath<T>(path2));
			}
			return asset;
		}

		protected override void SetLocalValue(string key, T asset)
		{
			string guid = null;
			if ((bool)asset)
			{
				string path = AssetDatabase.GetAssetPath(asset);
				if (!string.IsNullOrWhiteSpace(path))
				{
					guid = AssetDatabase.AssetPathToGUID(path);
				}
			}
			EditorPrefs.SetString(key, guid);
		}

		protected override T Draw(Rect rect, T asset, GUIContent label)
		{
			if (label == null)
			{
				return SirenixEditorFields.UnityObjectField(rect, asset, typeof(T), allowSceneObjects: false) as T;
			}
			return SirenixEditorFields.UnityObjectField(rect, label, asset, typeof(T), allowSceneObjects: false) as T;
		}
	}
}
