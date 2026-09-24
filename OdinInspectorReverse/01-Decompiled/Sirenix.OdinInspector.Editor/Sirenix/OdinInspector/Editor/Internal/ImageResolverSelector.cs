using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal static class ImageResolverSelector
	{
		private static GUIStyle _inputStyle;

		private static GUIStyle InputStyle
		{
			get
			{
				GUIStyle obj = _inputStyle ?? new GUIStyle(EditorStyles.textField)
				{
					padding = new RectOffset(3, 3, 2, 1),
					alignment = TextAnchor.MiddleLeft
				};
				_inputStyle = obj;
				return obj;
			}
		}

		public static string Draw(Rect rect, GUIContent label, string selected)
		{
			if (label != null)
			{
				rect = EditorGUI.PrefixLabel(rect, label);
			}
			selected = selected ?? string.Empty;
			string objectPickerKey = typeof(ImageResolverSelector).FullName + "+" + GUIUtility.GetControlID(FocusType.Passive);
			Object currentObject = ResolveAsset(selected);
			ObjectPicker objectPicker = ObjectPicker.GetObjectPicker(objectPickerKey, typeof(Object));
			Rect iconRect = new Rect(rect.x, rect.y, EditorGUIUtility.singleLineHeight, rect.height);
			Rect pickerRect = new Rect(rect.xMax - EditorGUIUtility.singleLineHeight, rect.y, EditorGUIUtility.singleLineHeight, rect.height);
			selected = HandleDragAndDrop(rect, selected);
			selected = HandleObjectPicker(rect, iconRect, pickerRect, selected, currentObject, objectPicker);
			RectOffset previousPadding = InputStyle.padding;
			InputStyle.padding = new RectOffset(Mathf.RoundToInt(iconRect.width), Mathf.RoundToInt(pickerRect.width), 2, 1);
			EditorGUI.BeginChangeCheck();
			selected = GUI.TextField(rect, selected, InputStyle);
			if (EditorGUI.EndChangeCheck())
			{
				GUI.changed = true;
			}
			DrawIcon(iconRect, currentObject);
			DrawPickerButton(pickerRect);
			InputStyle.padding = previousPadding;
			return selected;
		}

		private static Object ResolveAsset(string source)
		{
			if (string.IsNullOrEmpty(source))
			{
				return null;
			}
			if (GlobalObjectId.TryParse(source, out var globalObjectId))
			{
				return GlobalObjectId.GlobalObjectIdentifierToObjectSlow(globalObjectId);
			}
			return AssetDatabase.LoadAssetAtPath<Object>(source);
		}

		private static string ToImageSource(Object selectedObject, Object currentObject, string currentSource)
		{
			if (selectedObject == null)
			{
				if (!(currentObject == null))
				{
					return string.Empty;
				}
				return currentSource;
			}
			if (AssetDatabase.Contains(selectedObject) && (selectedObject is Sprite || AssetDatabase.IsSubAsset(selectedObject)))
			{
				return GlobalObjectId.GetGlobalObjectIdSlow(selectedObject).ToString();
			}
			string path = AssetDatabase.GetAssetPath(selectedObject);
			if (!string.IsNullOrEmpty(path))
			{
				return path;
			}
			return currentSource;
		}

		private static string HandleObjectPicker(Rect rect, Rect iconRect, Rect pickerRect, string selected, Object currentObject, ObjectPicker objectPicker)
		{
			Event evt = Event.current;
			if (evt.OnLeftClick(iconRect) || evt.OnLeftClick(pickerRect))
			{
				objectPicker.ShowObjectPicker(currentObject, allowSceneObjects: false, rect);
				evt.Use();
			}
			if (objectPicker.IsReadyToClaim && evt.type == EventType.Repaint)
			{
				selected = ToImageSource(objectPicker.ClaimObject() as Object, currentObject, selected);
				GUI.changed = true;
			}
			return selected;
		}

		private static void DrawIcon(Rect iconRect, Object currentObject)
		{
			EditorGUIUtility.AddCursorRect(iconRect, MouseCursor.Arrow);
			if (currentObject != null)
			{
				Texture2D preview = AssetPreview.GetMiniThumbnail(currentObject);
				if (preview != null)
				{
					GUI.DrawTexture(Pad(iconRect, 3f), preview, ScaleMode.ScaleToFit, alphaBlend: true);
					return;
				}
			}
			SdfIcons.DrawIcon(Pad(iconRect, 4f), SdfIconType.ImageAlt, GetIconColor(iconRect));
		}

		private static void DrawPickerButton(Rect pickerRect)
		{
			EditorGUIUtility.AddCursorRect(pickerRect, MouseCursor.Arrow);
			SdfIcons.DrawIcon(Pad(pickerRect, 4f), SdfIconType.CaretDownFill, GetIconColor(pickerRect));
		}

		private static Rect Pad(Rect rect, float padding)
		{
			rect.x += padding;
			rect.y += padding;
			rect.width -= padding * 2f;
			rect.height -= padding * 2f;
			return rect;
		}

		private static Color GetIconColor(Rect rect)
		{
			bool isHovering = rect.Contains(Event.current.mousePosition);
			if (EditorGUIUtility.isProSkin)
			{
				if (!isHovering)
				{
					return new Color(1f, 1f, 1f, 0.5f);
				}
				return Color.white;
			}
			if (!isHovering)
			{
				return new Color(0f, 0f, 0f, 0.5f);
			}
			return Color.black;
		}

		private static string HandleDragAndDrop(Rect rect, string selected)
		{
			Event evt = Event.current;
			if (!rect.Contains(evt.mousePosition))
			{
				return selected;
			}
			if (evt.type != EventType.DragUpdated && evt.type != EventType.DragPerform)
			{
				return selected;
			}
			string path = GetDraggedImageSource(selected);
			if (path == null)
			{
				return selected;
			}
			DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
			if (evt.type == EventType.DragPerform)
			{
				DragAndDrop.AcceptDrag();
				selected = path;
				GUI.changed = true;
			}
			evt.Use();
			return selected;
		}

		private static string GetDraggedImageSource(string selected)
		{
			Object[] objects = DragAndDrop.objectReferences;
			for (int i = 0; i < objects.Length; i++)
			{
				string path = ToImageSource(objects[i], null, selected);
				if (!string.IsNullOrEmpty(path))
				{
					return path;
				}
			}
			return null;
		}
	}
}
