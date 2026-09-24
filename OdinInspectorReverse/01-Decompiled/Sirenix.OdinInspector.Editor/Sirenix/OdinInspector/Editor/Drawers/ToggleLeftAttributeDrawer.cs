using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws properties marked with <see cref="T:Sirenix.OdinInspector.ToggleLeftAttribute" />.
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.ToggleLeftAttribute" />
	public sealed class ToggleLeftAttributeDrawer : OdinAttributeDrawer<ToggleLeftAttribute, bool>
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			IPropertyValueEntry<bool> entry = base.ValueEntry;
			EditorGUI.BeginChangeCheck();
			bool value = ((label == null) ? EditorGUILayout.ToggleLeft(GUIContent.none, entry.SmartValue) : EditorGUILayout.ToggleLeft(label, entry.SmartValue));
			if (EditorGUI.EndChangeCheck())
			{
				entry.SmartValue = value;
			}
		}
	}
}
