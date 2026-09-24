using System;
using System.Globalization;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Internal
{
	[Serializable]
	public class ProjectSettingEnum<T> : ProjectSetting<T> where T : struct
	{
		static ProjectSettingEnum()
		{
			if (!typeof(T).IsEnum)
			{
				throw new InvalidOperationException(typeof(T).GetNiceName() + " is not an enum.");
			}
		}

		protected override T GetLocalValue(string key, T defaultValue)
		{
			string str = EditorPrefs.GetString(key, Convert.ToInt64(defaultValue).ToString("D", CultureInfo.InvariantCulture));
			if (!long.TryParse(str, out var parsedValue))
			{
				parsedValue = 0L;
			}
			return (T)Enum.ToObject(typeof(T), parsedValue);
		}

		protected override void SetLocalValue(string key, T value)
		{
			EditorPrefs.SetString(key, Convert.ToInt64(value).ToString("D", CultureInfo.InvariantCulture));
		}

		protected override T Draw(Rect rect, T value, GUIContent label)
		{
			throw new NotImplementedException();
		}
	}
}
