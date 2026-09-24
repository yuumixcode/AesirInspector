using System;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Internal
{
	[Serializable]
	public sealed class ProjectSettingBool : ProjectSetting<bool>
	{
		protected override bool GetLocalValue(string key, bool defaultValue)
		{
			return EditorPrefs.GetBool(key, defaultValue);
		}

		protected override void SetLocalValue(string key, bool value)
		{
			EditorPrefs.SetBool(key, value);
		}

		protected override bool Draw(Rect rect, bool value, GUIContent label)
		{
			return EditorGUI.ToggleLeft(rect, label, value);
		}
	}
}
