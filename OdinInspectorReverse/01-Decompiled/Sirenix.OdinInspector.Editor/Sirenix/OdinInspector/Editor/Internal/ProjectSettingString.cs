using System;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Internal
{
	[Serializable]
	public sealed class ProjectSettingString : ProjectSetting<string>
	{
		protected override string GetLocalValue(string key, string defaultValue)
		{
			return EditorPrefs.GetString(key, defaultValue);
		}

		protected override void SetLocalValue(string key, string value)
		{
			EditorPrefs.SetString(key, value);
		}

		protected override string Draw(Rect rect, string value, GUIContent label)
		{
			return SirenixEditorFields.TextField(rect, label, value);
		}
	}
}
