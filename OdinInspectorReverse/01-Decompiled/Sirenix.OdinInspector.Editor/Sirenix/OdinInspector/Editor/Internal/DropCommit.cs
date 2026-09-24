using System;
using System.Collections.Generic;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal static class DropCommit
	{
		/// <summary>
		/// Commit a vertical drop using DragAndDropState.
		/// Center on an empty group inserts INTO that group.
		/// Top/Bottom on any item inserts as a SIBLING in the parent container.
		/// Returns true if the model changed.
		/// </summary>
		public static bool TryCommit(DesignerEditorContext ctx, DragAndDropState state)
		{
			if (state == null || !state.DragActive)
			{
				return false;
			}
			if (state.SlotUnderCursor == null || state.NodeBeingDragged == null)
			{
				return false;
			}
			if (state.DropDirection == DropDirection.None)
			{
				return false;
			}
			if (!RenderPass.IsValidDragAndDrop(state))
			{
				return false;
			}
			DesignerEditorNode dragNode = state.NodeBeingDragged;
			DesignerEditorNode hoverNode = state.SlotUnderCursor.Node;
			ctx.BeginUndo();
			switch (state.DropDirection)
			{
			case DropDirection.Top:
				ctx.MoveNodeNextTo(dragNode, hoverNode, above: true);
				break;
			case DropDirection.Bottom:
				ctx.MoveNodeNextTo(dragNode, hoverNode, above: false);
				break;
			case DropDirection.Left:
				ctx.SplitNode(dragNode, hoverNode, splitLeft: true);
				break;
			case DropDirection.Right:
				ctx.SplitNode(dragNode, hoverNode, splitLeft: false);
				break;
			case DropDirection.Center:
				ctx.MoveNode(dragNode, hoverNode.GetDesignerId(), 0);
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			ctx.EndUndo(isEditorOutOfSync: true);
			return true;
		}

		private static List<DesignerEditorNode> SiblingsOf(DesignerEditorNode n)
		{
			if (n == null)
			{
				return null;
			}
			if (n.Parent != null)
			{
				return n.Parent.Children;
			}
			return FindTop(n)?.Children;
		}

		private static DesignerEditorNode FindTop(DesignerEditorNode n)
		{
			DesignerEditorNode t = n;
			while (t != null && t.Parent != null)
			{
				t = t.Parent;
			}
			return t;
		}

		private static int IndexOfRef(List<DesignerEditorNode> list, DesignerEditorNode node)
		{
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i] == node)
				{
					return i;
				}
			}
			return -1;
		}

		private static bool IsAncestor(DesignerEditorNode possibleAncestor, DesignerEditorNode node)
		{
			for (DesignerEditorNode p = node; p != null; p = p.Parent)
			{
				if (p == possibleAncestor)
				{
					return true;
				}
			}
			return false;
		}
	}
}
