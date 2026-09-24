using UnityEditor;

namespace Sirenix.OdinInspector.Editor
{
	public class EditorPrefString : EditorPref<string>
	{
		public EditorPrefString(string key, string defaultValue)
			: base(key, defaultValue)
		{
		}

		protected override string GetValue(string key, string defaultValue)
		{
			return EditorPrefs.GetString(key, defaultValue);
		}

		protected override void SetValue(string key, string value)
		{
			EditorPrefs.SetString(key, value);
		}
	}
}
