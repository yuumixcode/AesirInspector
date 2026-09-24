using System;
using UnityEditor;
using UnityEngine;

namespace Sirenix.Utilities.Editor
{
	/// <summary>
	/// This class is due to undergo refactoring.
	/// </summary>
	public sealed class DropZoneHandle
	{
		private static DropZoneHandle hoveringDropZone;

		private bool isBeingHovered;

		internal EditorWindow SourceWindow;

		public Rect ScreenRect;

		public Rect Rect;

		public bool Enabled = true;

		public bool IsAccepted { get; private set; }

		public bool IsBeingHovered
		{
			get
			{
				if (isBeingHovered)
				{
					return DragAndDropManager.IsDragInProgress;
				}
				return false;
			}
		}

		public Type Type { get; internal set; }

		public int LayoutDepth { get; set; }

		public bool CanAcceptMove { get; internal set; }

		public bool IsCrossWindowDrag { get; private set; }

		public bool IsReadyToClaim
		{
			get
			{
				if (IsAccepted && hoveringDropZone == this && DragAndDropManager.IsDragInProgress && DragAndDropManager.CurrentDraggingHandle.IsReadyToBeClaimed && !DragAndDropManager.CurrentDraggingHandle.IsBeingClaimed)
				{
					return Event.current.type == EventType.Repaint;
				}
				return false;
			}
		}

		public object ClaimObject()
		{
			if (!IsReadyToClaim)
			{
				throw new Exception("Check IsReadyToClaim before claiming the object.");
			}
			return DragAndDropManager.CurrentDraggingHandle.DropObject();
		}

		internal void Update()
		{
			if (hoveringDropZone == this)
			{
				hoveringDropZone = null;
				DragAndDropManager.CurrentHoveringDropZone = null;
			}
			isBeingHovered = false;
			IsAccepted = false;
			IsCrossWindowDrag = false;
			if (!DragAndDropManager.IsDragInProgress || !Enabled || !DragAndDropManager.AllowDrop || !GUI.enabled)
			{
				return;
			}
			IsAccepted = DragAndDropManager.IsDragInProgress && ((DragAndDropManager.CurrentDraggingHandle.Object != null && DragAndDropManager.CurrentDraggingHandle.Object.GetType().InheritsFrom(Type)) || (Type.IsNullableType() && DragAndDropManager.CurrentDraggingHandle.Object == null));
			if (ScreenRect.Contains(GUIHelper.MouseScreenPosition) && GUIHelper.CurrentWindowHasFocus)
			{
				if ((hoveringDropZone == null || LayoutDepth >= hoveringDropZone.LayoutDepth || !hoveringDropZone.ScreenRect.Contains(GUIHelper.MouseScreenPosition)) && IsAccepted)
				{
					hoveringDropZone = this;
				}
				if (IsAccepted && hoveringDropZone == this)
				{
					DragAndDropManager.CurrentHoveringDropZone = this;
				}
				else
				{
					isBeingHovered = false;
				}
			}
			else if (DragAndDropManager.CurrentHoveringDropZone == this)
			{
				isBeingHovered = false;
				DragAndDropManager.CurrentHoveringDropZone = null;
			}
			isBeingHovered = DragAndDropManager.CurrentHoveringDropZone == this;
			if (isBeingHovered)
			{
				IsCrossWindowDrag = DragAndDropManager.CurrentDraggingHandle.SourceWindow != SourceWindow;
				DragAndDropManager.CurrentDraggingHandle.IsCrossWindowDrag = IsCrossWindowDrag;
			}
		}
	}
}
