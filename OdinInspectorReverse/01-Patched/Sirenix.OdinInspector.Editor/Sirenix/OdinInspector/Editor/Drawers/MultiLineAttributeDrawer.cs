using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws string properties marked with <see cref="T:UnityEngine.MultilineAttribute" />.
	/// This drawer only works for string fields, unlike <see cref="T:Sirenix.OdinInspector.Editor.Drawers.MultiLinePropertyAttributeDrawer" />.
	/// </summary>
	/// <seealso cref="T:UnityEngine.MultilineAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.Drawers.MultiLineAttributeDrawer" />
	/// <seealso cref="T:Sirenix.OdinInspector.DisplayAsStringAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.InfoBoxAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.DetailedInfoBoxAttribute" />
	public sealed class MultiLineAttributeDrawer : OdinAttributeDrawer<MultilineAttribute, string>
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			IPropertyValueEntry<string> entry = base.ValueEntry;
			MultilineAttribute attribute = base.Attribute;
			Rect position = EditorGUILayout.GetControlRect(label != null, EditorGUIUtility.singleLineHeight * (float)attribute.lines);
			position.height -= 2f;
			if (label == null)
			{
				entry.SmartValue = EditorGUI.TextArea(position, entry.SmartValue, EditorStyles.textArea);
				return;
			}
			int controlID = GUIUtility.GetControlID(label, FocusType.Keyboard, position);
			Rect areaPosition = EditorGUI.PrefixLabel(position, controlID, label, EditorStyles.label);
			entry.SmartValue = EditorGUI.TextArea(areaPosition, entry.SmartValue, EditorStyles.textArea);
		}
	}
}
