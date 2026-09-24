using Sirenix.Reflection.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Internal
{
	public static class SdfIconSelector
	{
		private static int selectorControlId = -1;

		private static SdfIconOverviewWindow overview;

		private static SdfIconType? selectorIcon = null;

		private static EditorWindow window;

		private static SdfIconType? MouseOverIcon
		{
			get
			{
				if (!overview)
				{
					return null;
				}
				return overview.MouseOverIcon;
			}
		}

		public static SdfIconType SelectIcon(SdfIconType selected, int controlId, bool show)
		{
			if (show)
			{
				window = GUIHelper.CurrentWindow;
				selectorControlId = controlId;
				overview = ScriptableObject.CreateInstance<SdfIconOverviewWindow>();
				overview.ShowAuxWindow();
				overview.selected = selected;
				overview.onSelect = delegate(SdfIconType x)
				{
					if ((bool)window)
					{
						window.Repaint();
						selectorIcon = x;
					}
					overview.Close();
					overview = null;
				};
			}
			if (selectorIcon.HasValue && controlId == selectorControlId)
			{
				GUIUtility.hotControl = controlId;
				window = null;
				GUI.changed = true;
				SdfIconType val = selectorIcon.Value;
				selectorIcon = null;
				selectorControlId = -1;
				overview = null;
				return val;
			}
			return selected;
		}

		public static SdfIconType DrawIconSelectorDropdownField(GUIContent label, SdfIconType selected)
		{
			Rect position = EditorGUILayout.GetControlRect();
			return DrawIconSelectorDropdownField(position, label, selected);
		}

		public static SdfIconType DrawIconSelectorDropdownField(Rect rect, GUIContent label, SdfIconType selected)
		{
			if (label != null)
			{
				rect = EditorGUI.PrefixLabel(rect, label);
			}
			GUIContent valLabel = GUIHelper.TempContent(selected.ToString(), EditorIcons.Transparent.Active);
			int id;
			bool pressed = GUI_Internals.Button(rect, FocusType.Keyboard, valLabel, EditorStyles.miniPullDown, out id);
			SdfIcons.DrawIcon(rect.Padding(4f).AlignLeft(rect.height), selected);
			selected = SelectIcon(selected, id, pressed);
			return selected;
		}

		public static SdfIconType DrawIconSelectorDropdownField(Rect rect, GUIContent label, SdfIconType selected, out SdfIconType? mouseOverIcon)
		{
			if (label != null)
			{
				rect = EditorGUI.PrefixLabel(rect, label);
			}
			GUIContent valLabel = GUIHelper.TempContent(selected.ToString(), EditorIcons.Transparent.Active);
			int id;
			bool pressed = GUI_Internals.Button(rect, FocusType.Keyboard, valLabel, EditorStyles.miniPullDown, out id);
			SdfIcons.DrawIcon(rect.Padding(4f).AlignLeft(rect.height), selected);
			selected = SelectIcon(selected, id, pressed);
			if (selectorControlId == id)
			{
				GUIHelper.RequestRepaint();
				mouseOverIcon = MouseOverIcon;
			}
			else
			{
				mouseOverIcon = null;
			}
			return selected;
		}
	}
}
