using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws string properties marked with <see cref="T:Sirenix.OdinInspector.MultiLinePropertyAttribute" />.
	/// This drawer works for both string field and properties, unlike <see cref="T:Sirenix.OdinInspector.Editor.Drawers.MultiLineAttributeDrawer" />.
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.MultiLinePropertyAttribute" />
	/// <seealso cref="T:UnityEngine.MultilineAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.DisplayAsStringAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.InfoBoxAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.DetailedInfoBoxAttribute" />
	public sealed class MultiLinePropertyAttributeDrawer : OdinAttributeDrawer<MultiLinePropertyAttribute, string>
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			IPropertyValueEntry<string> entry = base.ValueEntry;
			MultiLinePropertyAttribute attribute = base.Attribute;
			Rect position = EditorGUILayout.GetControlRect(label != null, EditorGUIUtility.singleLineHeight * (float)attribute.Lines);
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
