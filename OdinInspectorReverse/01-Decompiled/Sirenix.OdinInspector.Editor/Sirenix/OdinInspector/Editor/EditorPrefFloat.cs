using UnityEditor;

namespace Sirenix.OdinInspector.Editor
{
	public class EditorPrefFloat : EditorPref<float>
	{
		public EditorPrefFloat(string key, float defaultValue)
			: base(key, defaultValue)
		{
		}

		protected override float GetValue(string key, float defaultValue)
		{
			return EditorPrefs.GetFloat(key, defaultValue);
		}

		protected override void SetValue(string key, float value)
		{
			EditorPrefs.SetFloat(key, value);
		}
	}
}
