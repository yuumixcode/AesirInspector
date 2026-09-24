using System;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Internal
{
	[Serializable]
	public class ProjectSettingInt : ProjectSetting<int>
	{
		protected override int GetLocalValue(string key, int defaultValue)
		{
			return EditorPrefs.GetInt(key, defaultValue);
		}

		protected override void SetLocalValue(string key, int value)
		{
			EditorPrefs.SetInt(key, value);
		}

		protected override int Draw(Rect rect, int value, GUIContent label)
		{
			return SirenixEditorFields.IntField(rect, label, value);
		}
	}
}
