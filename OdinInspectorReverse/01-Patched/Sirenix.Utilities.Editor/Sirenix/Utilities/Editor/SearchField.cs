using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Reflection.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.Utilities.Editor
{
	public class SearchField
	{
		private bool wantsFocus;

		private int controlID;

		private static GUIStyle searchFieldStyle;

		private static GUIStyle placeholderTextStyle;

		private static GUIStyle SearchFieldStyle
		{
			get
			{
				GUIStyle result = searchFieldStyle ?? new GUIStyle(EditorStyles.textField)
				{
					alignment = TextAnchor.MiddleLeft
				};
				searchFieldStyle = result;
				return result;
			}
		}

		private static GUIStyle PlaceholderTextStyle
		{
			get
			{
				GUIStyle result = placeholderTextStyle ?? new GUIStyle(SearchFieldStyle)
				{
					normal = 
					{
						textColor = Color.gray
					}
				};
				placeholderTextStyle = result;
				return result;
			}
		}

		/// <summary>Initializes the <see cref="T:Sirenix.Utilities.Editor.SearchField" /> and creates a permanent ID for the Control.</summary>
		/// <remarks>If you create this <see cref="T:Sirenix.Utilities.Editor.SearchField" /> on a <see cref="T:UnityEngine.ScriptableObject" /> such as <see cref="T:UnityEditor.EditorWindow" />,
		/// make sure to initialize this during OnEnable to ensure it gets initialized correctly.</remarks>
		public SearchField()
		{
			controlID = GUIUtility_Internals.GetPermanentControlID();
		}

		public void Focus()
		{
			wantsFocus = true;
		}

		public bool HasFocus()
		{
			return GUIUtility.keyboardControl == controlID;
		}

		public string Draw(string searchTerm, string placeholder = "")
		{
			return Draw(EditorGUILayout.GetControlRect(), searchTerm, placeholder);
		}

		public string Draw(Rect rect, string searchTerm, string placeholder = "")
		{
			int fontSize = Mathf.Min(Mathf.RoundToInt(rect.height * 0.65f), 12);
			SearchFieldStyle.fontSize = fontSize;
			PlaceholderTextStyle.fontSize = fontSize;
			RectOffset searchFieldPadding = new RectOffset(fontSize * 2, fontSize * 2, 0, 0);
			SearchFieldStyle.padding = searchFieldPadding;
			PlaceholderTextStyle.padding = searchFieldPadding;
			float buttonSize = (float)fontSize * 2f;
			float iconPadding = (float)fontSize * 0.5f;
			Rect searchButtonRect = rect.AlignLeft(buttonSize);
			Rect searchIconRect = searchButtonRect.HorizontalPadding(iconPadding);
			Rect clearButtonRect = rect.AlignRight(buttonSize);
			Rect clearIconRect = clearButtonRect.HorizontalPadding(iconPadding);
			EditorGUIUtility.AddCursorRect(searchButtonRect, MouseCursor.Arrow);
			bool shouldDrawClearButton = !searchTerm.IsNullOrWhitespace();
			if (shouldDrawClearButton)
			{
				EditorGUIUtility.AddCursorRect(clearButtonRect, MouseCursor.Arrow);
				if (Event.current.OnMouseDown(clearButtonRect, 0))
				{
					searchTerm = "";
					GUIHelper.RemoveFocusControl();
					GUI.changed = true;
				}
			}
			searchTerm = EditorGUI_Internals.DoTextField(controlID, rect, searchTerm, SearchFieldStyle, null, out var _, reset: false, multiline: false, passwordField: false);
			if (wantsFocus && Event.current.type == EventType.Repaint)
			{
				GUIUtility.keyboardControl = controlID;
				EditorGUIUtility.editingTextField = true;
				wantsFocus = false;
			}
			if (GUIUtility.keyboardControl != controlID && string.IsNullOrEmpty(searchTerm))
			{
				EditorGUI.LabelField(rect, placeholder, PlaceholderTextStyle);
			}
			if (shouldDrawClearButton)
			{
				SdfIcons.DrawIcon(clearIconRect, SdfIconType.X, GetIconColor(clearButtonRect));
			}
			SdfIcons.DrawIcon(searchIconRect, SdfIconType.Search, GetIconColor(searchButtonRect));
			return searchTerm;
		}

		private static Color GetIconColor(Rect iconRect)
		{
			if (!Event.current.IsMouseOver(iconRect))
			{
				if (!EditorGUIUtility.isProSkin)
				{
					return new Color(0.333f, 0.333f, 0.333f, 1f);
				}
				return EditorStyles.label.normal.textColor;
			}
			if (!EditorGUIUtility.isProSkin)
			{
				return Color.black;
			}
			return Color.white;
		}
	}
}
