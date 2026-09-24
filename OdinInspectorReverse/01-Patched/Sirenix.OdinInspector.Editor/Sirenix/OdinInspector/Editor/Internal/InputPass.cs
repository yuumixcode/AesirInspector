using Sirenix.Reflection.Editor;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal static class InputPass
	{
		private static readonly int ControlID = GUIUtility_Internals.GetPermanentControlID();

		public static void Run(DesignerEditorContext ctx, Slot slot, Rect rect, DragAndDropState dragAndDropState)
		{
			bool isLocked = ctx.IsNonDeclMembersLocked && slot.Node.NodeColor != Color.white;
			Event e = Event.current;
			if (ctx.HoverNode == null && e.IsHovering(rect))
			{
				ctx.HoverNode = slot.Node;
			}
			if (!isLocked)
			{
				if (e.OnMouseDown(rect, 0, useEvent: false))
				{
					if (dragAndDropState.DragPending)
					{
						return;
					}
					ctx.Select(slot.Node);
					dragAndDropState.DragPending = true;
					dragAndDropState.DragActive = false;
					dragAndDropState.NodeBeingDragged = slot.Node;
					dragAndDropState.DragStartPosition = e.mousePosition;
				}
				if (e.OnMouseDown(rect, 1))
				{
					slot.Node.OpenContextMenu(ctx);
				}
				if (dragAndDropState.DragActive && e.OnKeyDown(KeyCode.Escape))
				{
					dragAndDropState.Reset();
					if (GUIUtility.hotControl == ControlID)
					{
						GUIUtility.hotControl = 0;
					}
				}
				if (dragAndDropState.DragPending && e.OnMouseMoveDrag(useEvent: false) && (e.mousePosition - dragAndDropState.DragStartPosition).sqrMagnitude > 225f)
				{
					dragAndDropState.DragPending = false;
					dragAndDropState.DragActive = true;
					GUIUtility.hotControl = ControlID;
				}
			}
			if (dragAndDropState.DragActive)
			{
				DropDirection dropDirection = DropDirection.None;
				foreach (DropZone dropZoneRelative in slot.DropZonesRelative)
				{
					float x = rect.x;
					Rect rect2 = dropZoneRelative.Rect;
					float x2 = x + rect2.x;
					float y = rect.y;
					rect2 = dropZoneRelative.Rect;
					float y2 = y + rect2.y;
					rect2 = dropZoneRelative.Rect;
					float width = rect2.width;
					rect2 = dropZoneRelative.Rect;
					Rect dropZoneAbsolut = new Rect(x2, y2, width, rect2.height);
					if (dropZoneAbsolut.Contains(e.mousePosition) && slot.Node != dragAndDropState.NodeBeingDragged)
					{
						dropDirection = dropZoneRelative.DropDirection;
						break;
					}
				}
				if (dropDirection != DropDirection.None)
				{
					dragAndDropState.SlotUnderCursor = slot;
					dragAndDropState.DropDirection = dropDirection;
				}
				else if (dragAndDropState.SlotUnderCursor == slot)
				{
					dragAndDropState.DropDirection = DropDirection.None;
					dragAndDropState.SlotUnderCursor = null;
				}
			}
			if (e.rawType == EventType.MouseUp && e.button == 0 && (dragAndDropState.DragPending || dragAndDropState.DragActive))
			{
				if (DropCommit.TryCommit(ctx, dragAndDropState))
				{
					GUI.changed = true;
				}
				if (dragAndDropState.DragActive)
				{
					e.Use();
				}
				dragAndDropState.Reset();
				if (GUIUtility.hotControl == ControlID)
				{
					GUIUtility.hotControl = 0;
				}
			}
		}
	}
}
