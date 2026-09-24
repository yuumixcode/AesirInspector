using System;
using System.Globalization;
using Sirenix.Utilities;
using UnityEditor;

namespace Sirenix.OdinInspector.Editor
{
	public class EditorPrefEnum<T> : EditorPref<T>
	{
		static EditorPrefEnum()
		{
			if (!typeof(T).IsEnum)
			{
				throw new InvalidOperationException(typeof(T).GetNiceName() + " is not an enum.");
			}
		}

		public EditorPrefEnum(string key, T defaultValue)
			: base(key, defaultValue)
		{
		}

		protected override T GetValue(string key, T defaultValue)
		{
			string str = EditorPrefs.GetString(key, Convert.ToInt64(defaultValue).ToString("D", CultureInfo.InvariantCulture));
			if (!long.TryParse(str, out var parsedValue))
			{
				parsedValue = 0L;
			}
			return (T)Enum.ToObject(typeof(T), parsedValue);
		}

		protected override void SetValue(string key, T value)
		{
			EditorPrefs.SetString(key, Convert.ToInt64(value).ToString("D", CultureInfo.InvariantCulture));
		}
	}
}
