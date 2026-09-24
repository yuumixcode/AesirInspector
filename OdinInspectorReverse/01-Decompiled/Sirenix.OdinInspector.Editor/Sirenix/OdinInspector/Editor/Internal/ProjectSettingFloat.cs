using System;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Internal
{
	[Serializable]
	public sealed class ProjectSettingFloat : ProjectSetting<float>
	{
		protected override float GetLocalValue(string key, float defaultValue)
		{
			return EditorPrefs.GetFloat(key, defaultValue);
		}

		protected override void SetLocalValue(string key, float value)
		{
			EditorPrefs.SetFloat(key, value);
		}

		protected override float Draw(Rect rect, float value, GUIContent label)
		{
			return SirenixEditorFields.FloatField(rect, label, value);
		}
	}
}
