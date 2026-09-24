using System;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal static class RenderPass
	{
		public static void Run(Slot slot, Rect localProjectedRect, DragAndDropState dragAndDropState, DesignerEditorContext context)
		{
			slot.Node.Draw(localProjectedRect.Padding(1f), context);
			if (dragAndDropState.DragActive && slot.Node == dragAndDropState.NodeBeingDragged)
			{
				DrawDragZoneGhost(localProjectedRect);
			}
		}

		private static void DrawDragZoneGhost(Rect rectLocal)
		{
			SirenixEditorGUI.DrawRoundRect(rectLocal, new Color(0.6824f, 0.7961f, 0.9804f, 0.3f), 3f, new Color(0.6824f, 0.7961f, 0.9804f), 1f);
		}

		public static void DrawDropZoneGhost(Rect rectLocal, DragAndDropState dragAndDropState)
		{
			bool isValidDragAndDrop = IsValidDragAndDrop(dragAndDropState);
			if (isValidDragAndDrop)
			{
				SirenixEditorGUI.DrawRoundRect(rectLocal, new Color(0.6588f, 0.902f, 0.8118f, 0.3f), 3f, new Color(0.6588f, 0.902f, 0.8118f), 1f);
			}
			else
			{
				SirenixEditorGUI.DrawRoundRect(rectLocal, new Color(0.902f, 0.6588f, 0.6588f, 0.3f), 3f, new Color(0.902f, 0.6588f, 0.6588f), 1f);
			}
			if (!(rectLocal.width < 14f) && !(rectLocal.height < 14f))
			{
				SdfIconType icon;
				float t;
				switch (dragAndDropState.DropDirection)
				{
				default:
					return;
				case DropDirection.Left:
					icon = SdfIconType.ArrowBarLeft;
					t = rectLocal.width / 60f;
					break;
				case DropDirection.Right:
					icon = SdfIconType.ArrowBarRight;
					t = rectLocal.width / 60f;
					break;
				case DropDirection.Top:
				case DropDirection.Bottom:
					icon = SdfIconType.ArrowsCollapse;
					t = rectLocal.height / 30f;
					break;
				case DropDirection.Center:
					icon = SdfIconType.PlusLg;
					t = rectLocal.height / 30f;
					break;
				}
				SdfIcons.DrawIcon(rectLocal.AlignCenterXY(14f), isValidDragAndDrop ? icon : SdfIconType.XCircle, new Color(1f, 1f, 1f, t));
			}
		}

		public static bool IsValidDragAndDrop(DragAndDropState dragAndDropState)
		{
			if (dragAndDropState == null)
			{
				return true;
			}
			if (dragAndDropState.SlotUnderCursor == null)
			{
				return true;
			}
			if (dragAndDropState.NodeBeingDragged == null)
			{
				return true;
			}
			DropDirection dir = dragAndDropState.DropDirection;
			DesignerEditorNode target = dragAndDropState.SlotUnderCursor.Node;
			DesignerEditorNode drag = dragAndDropState.NodeBeingDragged;
			if ((dir == DropDirection.Left || dir == DropDirection.Right) && target != null && target.IsRow)
			{
				return false;
			}
			for (DesignerEditorNode currentTargetParent = target.Parent; currentTargetParent != null; currentTargetParent = currentTargetParent.Parent)
			{
				if (currentTargetParent == drag)
				{
					return false;
				}
			}
			Type dragGroupType = drag.GroupAttribute?.GetType();
			Type dropGroupType = target.GroupAttribute?.GetType();
			bool isDragSubGroup = DesignerRegistry.SubGroups.Contains(dragGroupType);
			if (DesignerRegistry.SubGroups.Contains(dropGroupType))
			{
				if (isDragSubGroup)
				{
					if (dragGroupType == typeof(ColumnGroupAttribute.ColumnSubGroupAttribute))
					{
						if (dragAndDropState.DropDirection != DropDirection.Left && dragAndDropState.DropDirection != DropDirection.Right)
						{
							return false;
						}
					}
					else if (dragAndDropState.DropDirection != DropDirection.Top && dragAndDropState.DropDirection != DropDirection.Bottom)
					{
						return false;
					}
					return drag.Parent.GroupAttribute.GetType() == target.Parent.GroupAttribute.GetType();
				}
				if (dropGroupType == typeof(ColumnGroupAttribute.ColumnSubGroupAttribute))
				{
					if (dragAndDropState.DropDirection != DropDirection.Top)
					{
						return dragAndDropState.DropDirection != DropDirection.Bottom;
					}
					return false;
				}
				return dragAndDropState.DropDirection == DropDirection.Center;
			}
			if (dragAndDropState.DropDirection == DropDirection.Center && dropGroupType != null && DesignerRegistry.SubGroupMap.TryGetValue(dropGroupType, out var subGroupTarget))
			{
				if (isDragSubGroup)
				{
					return dragGroupType == subGroupTarget;
				}
				return false;
			}
			return !isDragSubGroup;
		}

		private static DesignerEditorNode FindOwningTabGroup(DesignerEditorNode n)
		{
			while (n != null)
			{
				if (n.GroupAttribute is TabGroupAttribute)
				{
					return n;
				}
				n = n.Parent;
			}
			return null;
		}

		private static bool AreInSameOwningTabGroup(DesignerEditorNode a, DesignerEditorNode b)
		{
			DesignerEditorNode ga = FindOwningTabGroup(a);
			DesignerEditorNode gb = FindOwningTabGroup(b);
			if (ga != null)
			{
				return ga == gb;
			}
			return false;
		}
	}
}
