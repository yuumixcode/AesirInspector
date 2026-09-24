using UnityEditor;

namespace Sirenix.OdinInspector.Editor
{
	public class EditorPrefBool : EditorPref<bool>
	{
		public EditorPrefBool(string key, bool defaultValue)
			: base(key, defaultValue)
		{
		}

		protected override bool GetValue(string key, bool defaultValue)
		{
			return EditorPrefs.GetBool(key, defaultValue);
		}

		protected override void SetValue(string key, bool value)
		{
			EditorPrefs.SetBool(key, value);
		}
	}
}
