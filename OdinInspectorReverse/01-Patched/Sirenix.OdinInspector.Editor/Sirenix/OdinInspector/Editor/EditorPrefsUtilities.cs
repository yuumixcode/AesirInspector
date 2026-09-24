using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	internal static class EditorPrefsUtilities
	{
		public static string ConvertToProjectKey(string key)
		{
			return Application.dataPath + key;
		}

		public static void SaveList(string key, List<string> list)
		{
			string listLengthKey = key + ".length";
			ClearList(key);
			EditorPrefs.SetInt(listLengthKey, list.Count);
			for (int i = 0; i < list.Count; i++)
			{
				EditorPrefs.SetString(key + "[" + i + "]", list[i]);
			}
		}

		public static List<string> LoadList(string key)
		{
			string listLengthKey = key + ".length";
			if (!EditorPrefs.HasKey(listLengthKey))
			{
				return new List<string>();
			}
			List<string> result = new List<string>();
			int listLength = EditorPrefs.GetInt(listLengthKey);
			for (int i = 0; i < listLength; i++)
			{
				result.Add(EditorPrefs.GetString(key + "[" + i + "]", ""));
			}
			return result;
		}

		public static void ClearList(string key)
		{
			string listLengthKey = key + ".length";
			int listLength = EditorPrefs.GetInt(listLengthKey, 0);
			for (int i = 0; i < listLength; i++)
			{
				EditorPrefs.DeleteKey(key + "[" + i + "]");
			}
			EditorPrefs.DeleteKey(listLengthKey);
		}
	}
}
