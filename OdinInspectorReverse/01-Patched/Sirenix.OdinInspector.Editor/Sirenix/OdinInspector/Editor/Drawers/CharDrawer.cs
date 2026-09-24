using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Char property drawer.
	/// </summary>
	public sealed class CharDrawer : OdinValueDrawer<char>
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			IPropertyValueEntry<char> entry = base.ValueEntry;
			EditorGUI.BeginChangeCheck();
			string s = new string(entry.SmartValue, 1);
			s = SirenixEditorFields.TextField(label, s);
			if (EditorGUI.EndChangeCheck() && s.Length > 0)
			{
				entry.SmartValue = s[0];
			}
		}
	}
}
