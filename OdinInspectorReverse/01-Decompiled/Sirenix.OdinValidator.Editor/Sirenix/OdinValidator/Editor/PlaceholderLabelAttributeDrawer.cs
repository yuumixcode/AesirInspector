using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinValidator.Editor
{
	internal class PlaceholderLabelAttributeDrawer : OdinAttributeDrawer<PlaceholderLabelAttribute, string>
	{
		private GUIStyle style;

		private ValueResolver<string> labelResolver;

		protected override void Initialize()
		{
			style = new GUIStyle(EditorStyles.textField);
			if (base.Attribute.LabelText != null)
			{
				labelResolver = ValueResolver.GetForString(base.Property, base.Attribute.LabelText);
			}
		}

		protected override void DrawPropertyLayout(GUIContent label)
		{
			Rect rect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight, EditorStyles.textField);
			float indent = 0f;
			int iconSize = 14;
			int iconPadding = 5;
			if (base.Attribute.Icon != SdfIconType.None)
			{
				indent += (float)iconSize;
				indent += (float)iconPadding;
			}
			style.padding.left = EditorStyles.textField.padding.left + (int)indent;
			base.ValueEntry.SmartValue = EditorGUI.TextField(rect, base.ValueEntry.SmartValue, style);
			if (Event.current.type != EventType.Repaint)
			{
				return;
			}
			if (base.Attribute.Icon != SdfIconType.None)
			{
				Rect iconRect = rect;
				iconRect.x += EditorStyles.textField.padding.left;
				iconRect.width = iconSize;
				iconRect = iconRect.AlignCenterY(iconSize);
				iconRect.y += 1f;
				Color activeColor = SirenixGUIStyles.Label.normal.textColor;
				SdfIcons.DrawIcon(iconRect, base.Attribute.Icon, activeColor);
			}
			if (string.IsNullOrEmpty(base.ValueEntry.SmartValue))
			{
				string labelText = null;
				if (labelResolver != null)
				{
					labelText = labelResolver.GetValue();
				}
				else if (!string.IsNullOrEmpty(label.text))
				{
					labelText = label.text;
				}
				if (labelText != null)
				{
					rect.xMin += indent;
					GUI.Label(rect, labelText, SirenixGUIStyles.LeftAlignedGreyLabel);
				}
			}
		}
	}
}
