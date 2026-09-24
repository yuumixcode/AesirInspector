using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Bool property drawer.
	/// </summary>
	public sealed class BooleanDrawer : OdinValueDrawer<bool>
	{
		private GUILayoutOption[] options;

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			bool value = base.ValueEntry.SmartValue;
			EditorGUI.BeginChangeCheck();
			if (label == null)
			{
				options = options ?? new GUILayoutOption[1] { GUILayout.ExpandWidth(expand: false) };
				float w = GUIHelper.CurrentIndentAmount;
				Rect rect = GUILayoutUtility.GetRect(15f + w, EditorGUIUtility.singleLineHeight, options).AddXMin(w);
				GUIHelper.PushIndentLevel(0);
				value = EditorGUI.Toggle(rect, value);
				GUIHelper.PopIndentLevel();
			}
			else
			{
				value = EditorGUILayout.Toggle(label, value);
			}
			if (EditorGUI.EndChangeCheck())
			{
				base.ValueEntry.SmartValue = value;
			}
		}
	}
}
