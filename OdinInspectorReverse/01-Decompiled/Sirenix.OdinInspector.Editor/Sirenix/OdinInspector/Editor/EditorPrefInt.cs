using UnityEditor;

namespace Sirenix.OdinInspector.Editor
{
	public class EditorPrefInt : EditorPref<int>
	{
		public EditorPrefInt(string key, int defaultValue)
			: base(key, defaultValue)
		{
		}

		protected override int GetValue(string key, int defaultValue)
		{
			return EditorPrefs.GetInt(key, defaultValue);
		}

		protected override void SetValue(string key, int value)
		{
			EditorPrefs.SetInt(key, value);
		}
	}
}
