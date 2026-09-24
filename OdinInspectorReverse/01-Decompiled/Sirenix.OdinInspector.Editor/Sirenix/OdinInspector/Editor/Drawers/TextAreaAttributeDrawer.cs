using System;
using System.Reflection;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// TextArea attribute drawer.
	/// </summary>
	public class TextAreaAttributeDrawer : OdinAttributeDrawer<TextAreaAttribute, string>
	{
		private delegate string ScrollableTextAreaInternalDelegate(Rect position, string text, ref Vector2 scrollPosition, GUIStyle style);

		private static readonly ScrollableTextAreaInternalDelegate EditorGUI_ScrollableTextAreaInternal;

		private static readonly FieldInfo EditorGUI_s_TextAreaHash_Field;

		private static readonly int EditorGUI_s_TextAreaHash;

		private Vector2 scrollPosition;

		static TextAreaAttributeDrawer()
		{
			MethodInfo method = typeof(EditorGUI).GetMethod("ScrollableTextAreaInternal", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			if (method != null)
			{
				EditorGUI_ScrollableTextAreaInternal = (ScrollableTextAreaInternalDelegate)Delegate.CreateDelegate(typeof(ScrollableTextAreaInternalDelegate), method);
			}
			EditorGUI_s_TextAreaHash_Field = typeof(EditorGUI).GetField("s_TextAreaHash", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			if (EditorGUI_s_TextAreaHash_Field != null)
			{
				EditorGUI_s_TextAreaHash = (int)EditorGUI_s_TextAreaHash_Field.GetValue(null);
			}
		}

		/// <summary>
		/// Draws the property in the Rect provided. This method does not support the GUILayout, and is only called by DrawPropertyImplementation if the GUICallType is set to Rect, which is not the default.
		/// If the GUICallType is set to Rect, both GetRectHeight and DrawPropertyRect needs to be implemented.
		/// If the GUICallType is set to GUILayout, implementing DrawPropertyLayout will suffice.
		/// </summary>
		/// <param name="label">The label. This can be null, so make sure your drawer supports that.</param>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			IPropertyValueEntry<string> entry = base.ValueEntry;
			TextAreaAttribute attribute = base.Attribute;
			string stringValue = entry.SmartValue;
			float num = EditorStyles.textArea.CalcHeight(GUIHelper.TempContent(stringValue), GUIHelper.ContextWidth);
			int num2 = Mathf.Clamp(Mathf.CeilToInt(num / 13f), attribute.minLines, attribute.maxLines);
			float height = 32f + (float)((num2 - 1) * 13);
			Rect position = EditorGUILayout.GetControlRect(label != null, height);
			if (EditorGUI_ScrollableTextAreaInternal == null || EditorGUI_s_TextAreaHash_Field == null)
			{
				EditorGUI.LabelField(position, label, GUIHelper.TempContent("Cannot draw TextArea because Unity's internal API has changed."));
				return;
			}
			if (label != null)
			{
				Rect labelPosition = position;
				labelPosition.height = 16f;
				position.yMin += labelPosition.height;
				GUIHelper.IndentRect(ref labelPosition);
				EditorGUI.HandlePrefixLabel(position, labelPosition, label);
			}
			if (Event.current.type == EventType.Layout)
			{
				GUIUtility.GetControlID(EditorGUI_s_TextAreaHash, FocusType.Keyboard, position);
			}
			entry.SmartValue = EditorGUI_ScrollableTextAreaInternal(position, entry.SmartValue, ref scrollPosition, EditorStyles.textArea);
		}
	}
}
