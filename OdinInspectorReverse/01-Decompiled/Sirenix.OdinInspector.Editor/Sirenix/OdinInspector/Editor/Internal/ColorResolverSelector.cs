using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal static class ColorResolverSelector
	{
		private static int selectorControlId = -1;

		private static ColorStringOverviewWindow overview;

		private static string selectorColorString = null;

		private static bool selectorHasValue = false;

		private static EditorWindow window;

		private static ValueResolver<Color?> colorResolver;

		private static GUIStyle _inputStyle;

		private static GUIStyle InputStyle
		{
			get
			{
				GUIStyle obj = _inputStyle ?? new GUIStyle(EditorStyles.textField)
				{
					padding = new RectOffset(3, 18, 2, 1),
					alignment = TextAnchor.MiddleLeft
				};
				_inputStyle = obj;
				return obj;
			}
		}

		public static string SelectColor(InspectorProperty property, string selected, int controlId, bool show)
		{
			if (show)
			{
				window = GUIHelper.CurrentWindow;
				selectorControlId = controlId;
				overview = ColorStringOverviewWindow.Show(property, selected);
				overview.OnSelect = delegate(string x)
				{
					if ((bool)window)
					{
						window.Repaint();
						selectorColorString = x;
						selectorHasValue = true;
					}
				};
			}
			if (selectorHasValue && controlId == selectorControlId)
			{
				GUI.changed = true;
				string val = selectorColorString;
				selectorColorString = null;
				selectorHasValue = false;
				return val;
			}
			return selected;
		}

		public static string Draw(Rect rect, GUIContent label, string selected, InspectorProperty property)
		{
			Event e = Event.current;
			int id = GUIUtility.GetControlID(FocusType.Passive);
			if (label != null)
			{
				rect = EditorGUI.PrefixLabel(rect, label);
			}
			Rect swatchRect = rect.AlignLeft(EditorGUIUtility.singleLineHeight);
			EditorGUIUtility.AddCursorRect(swatchRect, MouseCursor.Arrow);
			Rect dropdownRect = rect.AlignRight(EditorGUIUtility.singleLineHeight);
			EditorGUIUtility.AddCursorRect(dropdownRect, MouseCursor.Arrow);
			Rect inputRect = rect;
			EditorGUIUtility.AddCursorRect(inputRect, MouseCursor.Text);
			Color? selectedColor = ResolveColor(property, selected);
			RectOffset prev = InputStyle.padding;
			InputStyle.padding = new RectOffset(Mathf.RoundToInt(swatchRect.width), Mathf.RoundToInt(dropdownRect.width), 2, 1);
			selected = SelectColor(property, selected, id, e.OnLeftClick(swatchRect));
			selected = SelectColor(property, selected, id, e.OnLeftClick(dropdownRect));
			EditorGUI.BeginChangeCheck();
			selected = GUI.TextField(rect, selected, InputStyle);
			if (EditorGUI.EndChangeCheck())
			{
				selectedColor = ResolveColor(property, selected);
			}
			if (selectedColor.HasValue)
			{
				SirenixEditorGUI.DrawRoundRect(swatchRect.Padding(4f), selectedColor.Value, 2f);
			}
			else
			{
				SdfIcons.DrawIcon(swatchRect.Padding(4f), SdfIconType.QuestionSquareFill);
			}
			SdfIcons.DrawIcon(color: (!EditorGUIUtility.isProSkin) ? (dropdownRect.Contains(e.mousePosition) ? Color.black : new Color(0f, 0f, 0f, 0.5f)) : (dropdownRect.Contains(e.mousePosition) ? Color.white : new Color(1f, 1f, 1f, 0.5f)), rect: dropdownRect.Padding(4f), icon: SdfIconType.CaretDownFill);
			InputStyle.padding = prev;
			return selected;
		}

		private static Color? ResolveColor(InspectorProperty property, string resolvedString)
		{
			colorResolver = ValueResolver.Get<Color?>(property, resolvedString);
			return colorResolver.GetValue();
		}
	}
}
