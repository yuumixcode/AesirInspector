using System;
using UnityEditor;
using UnityEngine;

namespace Sirenix.Utilities.Editor
{
	/// <summary>
	/// <para>This class is due to undergo refactoring. Use the new DragAndDropUtilities instead.</para>
	/// </summary>
	/// <seealso cref="T:Sirenix.Utilities.Editor.DragAndDropUtilities" />
	public static class DragAndDropManager
	{
		private static GUIScopeStack<DragHandle> dragHandles = new GUIScopeStack<DragHandle>();

		private static GUIScopeStack<DropZoneHandle> dropZoneHandles = new GUIScopeStack<DropZoneHandle>();

		internal static bool WasDragPerformed = false;

		private static object dropZoneKey = new object();

		private static object draggableKey = new object();

		public static bool AllowDrop = true;

		private static GUIFrameCounter guiState = new GUIFrameCounter();

		public static DragHandle CurrentDraggingHandle { get; internal set; }

		public static DropZoneHandle CurrentHoveringDropZone { get; internal set; }

		public static bool IsDragInProgress => CurrentDraggingHandle != null;

		public static bool IsHoveringDropZone => CurrentHoveringDropZone != null;

		public static DropZoneHandle BeginDropZone<T>(object key) where T : struct
		{
			return BeginDropZone(key, typeof(T), canAcceptMove: false);
		}

		public static DropZoneHandle BeginDropZone<T>(object key, bool canAcceptMove) where T : class
		{
			return BeginDropZone(key, typeof(T), canAcceptMove);
		}

		public static DropZoneHandle BeginDropZone(object key, Type type, bool canAcceptMove)
		{
			Update();
			GUILayout.BeginVertical();
			Rect rect = GUIHelper.GetCurrentLayoutRect();
			DropZoneHandle dropZoneHandle = GUIHelper.GetTemporaryContext<DropZoneHandle>(dropZoneKey, key).Value;
			dropZoneHandle.Type = type;
			dropZoneHandle.CanAcceptMove = canAcceptMove;
			dropZoneHandle.LayoutDepth = dropZoneHandles.Count;
			dropZoneHandles.Push(dropZoneHandle);
			dropZoneHandle.Update();
			dropZoneHandle.SourceWindow = GUIHelper.CurrentWindow;
			if (Event.current.type == EventType.Repaint)
			{
				dropZoneHandle.Rect = rect;
				dropZoneHandle.ScreenRect = GUIUtility.GUIToScreenRect(rect);
			}
			return dropZoneHandle;
		}

		public static DropZoneHandle EndDropZone()
		{
			DropZoneHandle dropZoneHandle = dropZoneHandles.Pop();
			GUILayout.EndVertical();
			dropZoneHandle.Update();
			return dropZoneHandle;
		}

		public static DragHandle BeginDragHandle(object key, object obj, DragAndDropMethods defaultMethod = DragAndDropMethods.Move)
		{
			return BeginDragHandle(key, obj, isVirtualDragHandle: false, defaultMethod);
		}

		public static DragHandle BeginDragHandle(object key, object obj, bool isVirtualDragHandle, DragAndDropMethods defaultMethod = DragAndDropMethods.Move)
		{
			Update();
			if (Event.current.type == EventType.Repaint)
			{
				GUIHelper.BeginLayoutMeasuring();
			}
			DragHandle dragHandle = GUIHelper.GetTemporaryContext<DragHandle>(draggableKey, key).Value;
			dragHandle.Object = obj;
			dragHandle.DragAndDropMethod = defaultMethod;
			dragHandle.LayoutDepth = dragHandles.Count;
			dragHandles.Push(dragHandle);
			dragHandle.SourceWindow = GUIHelper.CurrentWindow;
			return dragHandle;
		}

		public static DragHandle EndDragHandle()
		{
			DragHandle dragHandle = dragHandles.Pop();
			if (Event.current.type == EventType.Repaint)
			{
				Rect rect = GUIHelper.EndLayoutMeasuring();
				if (!dragHandle.IsDragging)
				{
					dragHandle.TempRect = rect;
				}
			}
			dragHandle.Update();
			dragHandle.Rect = dragHandle.TempRect;
			return dragHandle;
		}

		private static void Update()
		{
			if (guiState.Update().IsNewFrame && IsDragInProgress)
			{
				AllowDrop = true;
				if (IsDragInProgress)
				{
					GUIHelper.RequestRepaint();
				}
			}
			if (IsDragInProgress)
			{
				if (!WasDragPerformed && (Event.current.type == EventType.DragPerform || Event.current.type == EventType.MouseMove || Event.current.type == EventType.MouseUp))
				{
					WasDragPerformed = true;
					if (Event.current.type == EventType.DragPerform && IsHoveringDropZone)
					{
						Event.current.Use();
					}
					GUIHelper.RequestRepaint();
				}
				if (IsHoveringDropZone && GUIHelper.CurrentWindowHasFocus)
				{
					if (!CurrentHoveringDropZone.IsAccepted)
					{
						DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
					}
					else if (CurrentDraggingHandle.CurrentMethod == DragAndDropMethods.Move)
					{
						DragAndDrop.visualMode = DragAndDropVisualMode.Move;
					}
					else if (CurrentDraggingHandle.CurrentMethod == DragAndDropMethods.Reference)
					{
						DragAndDrop.visualMode = DragAndDropVisualMode.Link;
					}
					else if (CurrentDraggingHandle.CurrentMethod == DragAndDropMethods.Copy)
					{
						DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
					}
				}
			}
			else
			{
				_ = Event.current.type;
				_ = 9;
			}
		}
	}
}
