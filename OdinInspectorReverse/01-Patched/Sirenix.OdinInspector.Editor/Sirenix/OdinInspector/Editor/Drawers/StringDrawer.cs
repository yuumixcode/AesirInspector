using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// String property drawer.
	/// </summary>
	public sealed class StringDrawer : OdinValueDrawer<string>
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			IPropertyValueEntry<string> entry = base.ValueEntry;
			entry.SmartValue = ((label == null) ? EditorGUILayout.TextField(entry.SmartValue, EditorStyles.textField) : EditorGUILayout.TextField(label, entry.SmartValue, EditorStyles.textField));
		}
	}
}
