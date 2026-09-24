using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor.Internal;
using UnityEditor;
using UnityEngine;

namespace Sirenix.Utilities.Editor
{
	/// <summary>
	/// Drag and drop utilities for both Unity and non-unity objects.
	/// </summary>
	public static class DragAndDropUtilities
	{
		private const int SPECIAL_CONTROL_ID_START = 961999999;

		private static readonly Func<Hashtable> getDragAndDropGenericData;

		private static int mouseDownDragAndDropId;

		private static int mouseClickedId;

		private static bool currentDragIsMove;

		private static int draggingId;

		private static bool isAccepted;

		private static object dropZoneObject;

		private static object[] draggingObjects;

		private static bool isDragging;

		private static int hoveringAcceptedDropZone;

		private static int specialDragAndDropControlId;

		private static EventType prevEventType;

		private static Rect preventDropAreaRect;

		private static Rect nextPreventDropAreaRect;

		public static int PrevDragAndDropId { get; private set; }

		/// <summary>
		/// Gets the position from where the last drag started from in screen space.
		/// </summary>
		public static Vector2 OnDragStartMouseScreenPos { get; private set; }

		/// <summary>
		/// Gets the delta position between the currrent mouse position and where the last drag originated from.
		/// </summary>
		public static Vector2 MouseDragOffset
		{
			get
			{
				if (Event.current == null)
				{
					return Vector2.zero;
				}
				return GUIUtility.GUIToScreenPoint(Event.current.mousePosition) - OnDragStartMouseScreenPos;
			}
		}

		/// <summary>
		/// Gets the hovering accepted drop zone ID.
		/// </summary>
		public static int HoveringAcceptedDropZone => hoveringAcceptedDropZone;

		/// <summary>
		/// Gets a value indicating whether an instance is currently being dragged.
		/// </summary>
		public static bool IsDragging
		{
			get
			{
				switch (Event.current.rawType)
				{
				case EventType.MouseDown:
				case EventType.MouseUp:
				case EventType.MouseMove:
					isDragging = false;
					break;
				case EventType.MouseDrag:
				case EventType.DragUpdated:
				case EventType.DragPerform:
				case EventType.DragExited:
					isDragging = true;
					break;
				}
				return isDragging;
			}
		}

		/// <summary>
		/// Gets the currently dragging identifier.
		/// </summary>
		public static int CurrentDragId
		{
			get
			{
				if (!IsDragging)
				{
					return 0;
				}
				return draggingId;
			}
		}

		/// <summary>
		/// Gets the current hovering drop zone identifier.
		/// </summary>
		public static int CurrentDropId
		{
			get
			{
				if (!IsDragging)
				{
					return 0;
				}
				if (hoveringAcceptedDropZone == 0)
				{
					return DragAndDrop.activeControlID;
				}
				return hoveringAcceptedDropZone;
			}
		}

		static DragAndDropUtilities()
		{
			mouseDownDragAndDropId = -1;
			mouseClickedId = -1;
			draggingObjects = new object[0];
			isDragging = false;
			specialDragAndDropControlId = 961999999;
			prevEventType = EventType.Repaint;
			FieldInfo dragAndDrop_GenericData = typeof(DragAndDrop).GetField("s_GenericData", BindingFlags.Static | BindingFlags.NonPublic);
			getDragAndDropGenericData = EmitUtilities.CreateStaticFieldGetter<Hashtable>(dragAndDrop_GenericData);
		}

		/// <summary>
		/// Gets a more percistent id for drag and drop.
		/// </summary>
		public static int GetDragAndDropId(Rect rect)
		{
			return PrevDragAndDropId = GUIUtility.GetControlID(961999999, FocusType.Passive, rect);
		}

		/// <summary>
		/// Draws a objectpicker button in the given rect. This one is designed to look good on top of DrawDropZone().
		/// </summary>
		public static object ObjectPickerZone(Rect rect, object value, Type type, bool allowSceneObjects, int id)
		{
			return InternalOdinEditorWrapper.ObjectPickerZone(rect, rect, value, type, allowSceneObjects, id);
		}

		/// <summary>
		/// Draws a objectpicker butter, in the given rect. This one is designed to look good on top of DrawDropZone().
		/// </summary>
		public static T ObjectPickerZone<T>(Rect rect, T value, bool allowSceneObjects, int id)
		{
			return (T)ObjectPickerZone(rect, value, typeof(T), allowSceneObjects, id);
		}

		/// <summary>
		/// Draws the graphics for a DropZone.
		/// </summary>
		public static void DrawDropZone(Rect rect, object value, GUIContent label, int id)
		{
			bool isDragging = IsDragging;
			if (Event.current.type != EventType.Repaint)
			{
				return;
			}
			UnityEngine.Object objectToPaint = value as UnityEngine.Object;
			GUIStyle objectFieldThumb = EditorStyles.objectFieldThumb;
			bool on = GUI.enabled && hoveringAcceptedDropZone == id && rect.Contains(Event.current.mousePosition) && isDragging;
			objectFieldThumb.Draw(rect, GUIContent.none, id, on);
			if (EditorGUI.showMixedValue)
			{
				GUI.Label(rect, "—", SirenixGUIStyles.LabelCentered);
			}
			else if ((bool)objectToPaint)
			{
				Texture image = GUIHelper.GetPreviewTexture(objectToPaint);
				rect = rect.Padding(2f);
				float size = Mathf.Min(rect.width, rect.height);
				EditorGUI.DrawTextureTransparent(rect.AlignCenter(size, size), image, ScaleMode.ScaleToFit);
				if (label != null)
				{
					rect = rect.AlignBottom(16f);
					GUI.Label(rect, label, EditorStyles.label);
				}
			}
		}

		/// <summary>
		/// Draws the graphics for a DropZone.
		/// </summary>
		public static void DrawDropZone(Rect rect, Texture preview, GUIContent label, int id)
		{
			bool isDragging = IsDragging;
			if (Event.current.type != EventType.Repaint)
			{
				return;
			}
			GUIStyle objectFieldThumb = EditorStyles.objectFieldThumb;
			bool on = GUI.enabled && hoveringAcceptedDropZone == id && rect.Contains(Event.current.mousePosition) && isDragging;
			objectFieldThumb.Draw(rect, GUIContent.none, id, on);
			if (EditorGUI.showMixedValue)
			{
				GUI.Label(rect, "—", SirenixGUIStyles.LabelCentered);
				return;
			}
			rect = rect.Padding(2f);
			float size = Mathf.Min(rect.width, rect.height);
			if (preview != null)
			{
				EditorGUI.DrawTextureTransparent(rect.AlignCenter(size, size), preview, ScaleMode.ScaleToFit);
			}
			if (label != null)
			{
				rect = rect.AlignBottom(16f);
				GUI.Label(rect, label, EditorStyles.label);
			}
		}

		/// <summary>
		/// A draggable zone for both Unity and non-unity objects.
		/// </summary>
		public static object DragAndDropZone(Rect rect, object value, Type type, bool allowMove, bool allowSwap)
		{
			return DragAndDropZone(rect, value, type, allowMove, allowSwap, allowSceneObjects: true);
		}

		/// <summary>
		/// A draggable zone for both Unity and non-unity objects.
		/// </summary>
		public static object DragAndDropZone(Rect rect, object value, Type type, bool allowMove, bool allowSwap, bool allowSceneObjects)
		{
			int id = GetDragAndDropId(rect);
			value = DropZone(rect, value, type, allowSceneObjects, id);
			value = DragZone(rect, value, type, allowMove, allowSwap, id);
			return value;
		}

		/// <summary>
		/// A drop zone area for both Unity and non-unity objects.
		/// </summary>
		public static object DropZone(Rect rect, object value, Type type, bool allowSceneObjects, int id)
		{
			if (Event.current.type == EventType.Layout)
			{
				return value;
			}
			if (rect.Contains(Event.current.mousePosition))
			{
				EventType t = Event.current.type;
				if (t == EventType.DragUpdated || t == EventType.DragPerform)
				{
					if (preventDropAreaRect.Contains(new Vector2(rect.x, rect.y)) && preventDropAreaRect.Contains(new Vector2(rect.xMax, rect.yMax)))
					{
						return value;
					}
					object obj = null;
					if (obj == null)
					{
						obj = draggingObjects.Where((object obj2) => obj2?.GetType().InheritsFrom(type) ?? false).FirstOrDefault();
					}
					if (obj == null)
					{
						obj = DragAndDrop.objectReferences.Where((UnityEngine.Object obj2) => obj2 != null && obj2.GetType().InheritsFrom(type)).FirstOrDefault();
					}
					if (obj == null)
					{
						obj = (from value2 in draggingObjects.Concat(DragAndDrop.objectReferences)
							where value2 != null && ConvertUtility.CanConvert(value2.GetType(), type)
							select ConvertUtility.TryWeakConvert(value2, type, out var result) ? result : null into obj2
							where obj2 != null
							select obj2).FirstOrDefault();
					}
					if (obj == null)
					{
						Hashtable genericData = getDragAndDropGenericData();
						if (genericData != null)
						{
							foreach (object data in genericData.Values)
							{
								if (!(data is ICollection collection))
								{
									continue;
								}
								foreach (object x in collection)
								{
									if (x.GetType().InheritsFrom(type))
									{
										obj = x;
										break;
									}
									if (ConvertUtility.TryWeakConvert(x, type, out var converted))
									{
										obj = converted;
										break;
									}
								}
							}
						}
					}
					bool acceptsDrag = obj != null;
					if (acceptsDrag && !allowSceneObjects)
					{
						UnityEngine.Object uObj = obj as UnityEngine.Object;
						if (uObj != null)
						{
							if (typeof(Component).IsAssignableFrom(uObj.GetType()))
							{
								uObj = ((Component)uObj).gameObject;
							}
							acceptsDrag = EditorUtility.IsPersistent(uObj);
						}
					}
					if (acceptsDrag)
					{
						hoveringAcceptedDropZone = id;
						bool move = UnityShims.Misc.GetEventModifiers(Event.current) != 2 && draggingId != 0 && currentDragIsMove;
						if (move)
						{
							DragAndDrop.visualMode = DragAndDropVisualMode.Move;
						}
						else
						{
							DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
						}
						Event.current.Use();
						if (t == EventType.DragPerform)
						{
							if (!move)
							{
								draggingId = 0;
							}
							DragAndDrop.AcceptDrag();
							GUI.changed = true;
							GUIHelper.RemoveFocusControl();
							draggingObjects = new object[0];
							currentDragIsMove = false;
							isAccepted = true;
							dropZoneObject = value;
							preventDropAreaRect = default(Rect);
							DragAndDrop.activeControlID = 0;
							GUIHelper.RequestRepaint();
							return obj;
						}
						DragAndDrop.activeControlID = id;
					}
					else
					{
						hoveringAcceptedDropZone = 0;
						DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
					}
				}
			}
			else if (hoveringAcceptedDropZone == id)
			{
				hoveringAcceptedDropZone = 0;
			}
			return value;
		}

		/// <summary>
		/// A drop zone area for bot Unity and non-unity objects.
		/// </summary>
		public static object DropZone(Rect rect, object value, Type type, int id)
		{
			return DropZone(rect, value, type, allowSceneObjects: true, id);
		}

		/// <summary>
		/// A drop zone area for bot Unity and non-unity objects.
		/// </summary>
		public static object DropZone(Rect rect, object value, Type type)
		{
			int id = GetDragAndDropId(rect);
			return DropZone(rect, value, type, id);
		}

		/// <summary>
		/// A drop zone area for bot Unity and non-unity objects.
		/// </summary>
		public static object DropZone(Rect rect, object value, Type type, bool allowSceneObjects)
		{
			int id = GetDragAndDropId(rect);
			return DropZone(rect, value, type, allowSceneObjects, id);
		}

		/// <summary>
		/// A drop zone area for bot Unity and non-unity objects.
		/// </summary>
		public static T DropZone<T>(Rect rect, T value, bool allowSceneObjects, int id)
		{
			return (T)DropZone(rect, value, typeof(T), allowSceneObjects, id);
		}

		/// <summary>
		/// A drop zone area for bot Unity and non-unity objects.
		/// </summary>
		public static T DropZone<T>(Rect rect, T value, int id)
		{
			return (T)DropZone(rect, value, typeof(T), id);
		}

		/// <summary>
		/// A drop zone area for bot Unity and non-unity objects.
		/// </summary>
		public static T DropZone<T>(Rect rect, T value, bool allowSceneObjects)
		{
			int id = GetDragAndDropId(rect);
			return (T)DropZone(rect, value, typeof(T), allowSceneObjects, id);
		}

		/// <summary>
		/// A drop zone area for bot Unity and non-unity objects.
		/// </summary>
		public static T DropZone<T>(Rect rect, T value)
		{
			int id = GetDragAndDropId(rect);
			return (T)DropZone(rect, value, typeof(T), id);
		}

		/// <summary>
		/// Disalloweds the drop area for next drag zone. Follow this function call by a DragZone.
		/// </summary>
		public static void DisallowedDropAreaForNextDragZone(Rect rect)
		{
			nextPreventDropAreaRect = rect;
		}

		public static bool PrevDragZoneWasClicked()
		{
			return PrevDragAndDropId == mouseClickedId;
		}

		/// <summary>
		/// A draggable zone for both Unity and non-unity objects.
		/// </summary>
		public static object DragZone(Rect rect, object value, Type type, bool allowMove, bool allowSwap, int id)
		{
			if (value == null)
			{
				return null;
			}
			UnityEngine.Object unityObject = value as UnityEngine.Object;
			bool isUnityObject = unityObject;
			if (isUnityObject && unityObject == null)
			{
				return value;
			}
			bool isMouseOver = rect.Contains(Event.current.mousePosition);
			if (!IsDragging)
			{
				draggingId = 0;
				mouseDownDragAndDropId = -1;
			}
			switch (Event.current.type)
			{
			case EventType.MouseDrag:
				if (isMouseOver && mouseDownDragAndDropId != id)
				{
					GUIHelper.RemoveFocusControl();
					DragAndDrop.PrepareStartDrag();
					DragAndDrop.activeControlID = 0;
					if (isUnityObject)
					{
						DragAndDrop.objectReferences = new UnityEngine.Object[1] { unityObject };
						draggingObjects = new object[0];
					}
					else
					{
						DragAndDrop.objectReferences = new UnityEngine.Object[0];
						draggingObjects = new object[1] { value };
					}
					DragAndDrop.StartDrag("Dragging");
					GUIHelper.RequestRepaint();
					isAccepted = false;
					dropZoneObject = null;
					draggingId = id;
					currentDragIsMove = allowMove;
					mouseDownDragAndDropId = id;
					preventDropAreaRect = rect.Expand(1f);
					OnDragStartMouseScreenPos = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
					Event.current.Use();
				}
				break;
			case EventType.MouseUp:
				if (mouseDownDragAndDropId == id)
				{
					mouseClickedId = id;
					mouseDownDragAndDropId = -1;
					Event.current.Use();
				}
				break;
			}
			if (draggingId != id)
			{
				return value;
			}
			if (isAccepted)
			{
				GUIHelper.RequestRepaint();
				GUI.changed = true;
				draggingId = 0;
				preventDropAreaRect = default(Rect);
				if (allowMove)
				{
					if (allowSwap && dropZoneObject != null && ConvertUtility.TryWeakConvert(dropZoneObject, type, out var newObj))
					{
						return newObj;
					}
					return null;
				}
			}
			return value;
		}

		/// <summary>
		/// A draggable zone for both Unity and non-unity objects.
		/// </summary>
		public static object DragZone(Rect rect, object value, Type type, bool allowMove, bool allowSwap)
		{
			int id = GetDragAndDropId(rect);
			return DragZone(rect, value, type, allowMove, allowSwap, id);
		}

		/// <summary>
		/// A draggable zone for both Unity and non-unity objects.
		/// </summary>
		public static T DragZone<T>(Rect rect, T value, bool allowMove, bool allowSwap, int id)
		{
			return (T)DragZone(rect, value, typeof(T), allowMove, allowSwap, id);
		}

		/// <summary>
		/// A draggable zone for both Unity and non-unity objects.
		/// </summary>
		public static T DragZone<T>(Rect rect, T value, bool allowMove, bool allowSwap)
		{
			int id = GetDragAndDropId(rect);
			return (T)DragZone(rect, value, typeof(T), allowMove, allowSwap, id);
		}
	}
}
