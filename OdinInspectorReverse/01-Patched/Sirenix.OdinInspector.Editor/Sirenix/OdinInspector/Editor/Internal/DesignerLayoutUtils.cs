using System;
using Sirenix.Utilities;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal static class DesignerLayoutUtils
	{
		public const float GROUP_HEADER = 28f;

		public const float GROUP_CONTENT_MIN = 18f;

		public const float GROUP_PADDING = 8f;

		public const float NODE_SPACE = 10f;

		public const float NODE_SPACE_HALF = 5f;

		public const float PROPERTY_HEIGHT = 42f;

		public static void Layout(RefList<DesignerLayoutNode> layout, DesignerEditor editor, Rect scrollView, ref Rect rect)
		{
			DesignerEditorNode root = editor.RootNode;
			DesignerEditorContext context = editor.Context;
			float scrollPosition = editor.NodeScrollView.Position;
			layout.Clear();
			Vector2 bounds = new Vector2(scrollView.yMin + scrollPosition, scrollView.yMax + scrollPosition);
			Rect contentRect = rect;
			contentRect.x += 16f;
			contentRect.width -= 32f;
			contentRect.height += 16f;
			for (int i = 0; i < root.Children.Count; i++)
			{
				LayoutNode(layout, context, root.Children[i], bounds, 0, ref contentRect);
			}
			rect.height = contentRect.height + 16f;
			if (context.IsDragging)
			{
				HandleDropNode(layout, context);
			}
		}

		public static void LayoutNode(RefList<DesignerLayoutNode> layout, DesignerEditorContext context, DesignerEditorNode editorNode, Vector2 scrollBounds, int depth, ref Rect contentRect)
		{
			ref DesignerLayoutNode node = ref layout.AddDefaultAndGetHandle().Ref;
			node.EditorNode = editorNode;
			node.Depth = depth;
			node.IsDragZone = context.DragNode == editorNode;
			depth++;
			if (editorNode.NodeType == DesignerEditorNodeType.Member || node.IsDragZone)
			{
				node.Rect = GetRect(ref contentRect, 42f);
				node.InteractRect = node.Rect;
				node.Rect = node.Rect.Padding(0f, 5f);
				node.DetermineVisibility(scrollBounds);
			}
			else if (!editorNode.IsRow)
			{
				node.Rect = GetRect(ref contentRect, 36f);
				float lastHeight = contentRect.height;
				float lastX = contentRect.x;
				float lastWidth = contentRect.width;
				contentRect.x += 8f;
				contentRect.width -= 16f;
				for (int i = 0; i < editorNode.Children.Count; i++)
				{
					DesignerEditorNode current = editorNode.Children[i];
					LayoutNode(layout, context, current, scrollBounds, depth, ref contentRect);
				}
				contentRect.x = lastX;
				contentRect.width = lastWidth;
				node.Rect.height += contentRect.height - lastHeight;
				node.Rect.height += GetRect(ref contentRect, 8f).height;
				node.InteractRect = node.Rect;
				node.Rect = node.Rect.Padding(0f, 5f);
				node.DetermineVisibility(scrollBounds);
			}
		}

		public static void HandleDropNode(RefList<DesignerLayoutNode> layout, DesignerEditorContext context)
		{
			if (context.DropDirection == DropDirection.None)
			{
				return;
			}
			for (int i = 0; i < layout.Length; i++)
			{
				ref DesignerLayoutNode node = ref layout[i];
				DesignerEditorNode editorNode = node.EditorNode;
				if (editorNode != null && context.HoverNode == editorNode)
				{
					DesignerLayoutNode dropNode = new DesignerLayoutNode
					{
						Depth = node.Depth,
						IsVisible = true,
						IsDropZone = true
					};
					if (!node.IsColumn)
					{
						AdjustRectsForDrop(layout, context, i, ref dropNode);
					}
					switch (context.DropDirection)
					{
					default:
						throw new ArgumentOutOfRangeException();
					case DropDirection.Top:
					case DropDirection.Bottom:
					case DropDirection.Left:
					case DropDirection.Right:
					case DropDirection.Center:
						break;
					}
					break;
				}
			}
		}

		public static void AdjustRectsForDrop(RefList<DesignerLayoutNode> layout, DesignerEditorContext context, int index, ref DesignerLayoutNode dropNode)
		{
			int insertIndex = index;
			switch (context.DropDirection)
			{
			case DropDirection.Top:
			{
				for (int j = index; j < layout.Length; j++)
				{
					layout[j].Rect.y += 42f;
				}
				break;
			}
			case DropDirection.Bottom:
			{
				int nextIndex2 = GetNextChildIndex(layout, index);
				if (nextIndex2 != -1)
				{
					for (int i = nextIndex2; i < layout.Length; i++)
					{
						layout[i].Rect.y += 42f;
					}
				}
				break;
			}
			case DropDirection.Right:
			{
				int nextIndex = GetNextChildIndex(layout, index);
				break;
			}
			case DropDirection.Center:
				insertIndex++;
				break;
			case DropDirection.Left:
				break;
			}
		}

		public static int GetNextChildIndex(RefList<DesignerLayoutNode> layout, int index)
		{
			ref DesignerLayoutNode node = ref layout[index];
			for (int i = index + 1; i < layout.Length; i++)
			{
				if (layout[i].Depth == node.Depth)
				{
					return i;
				}
			}
			return -1;
		}

		public static int GetParentOrSelfIndex(RefList<DesignerLayoutNode> layout, int index)
		{
			ref DesignerLayoutNode self = ref layout[index];
			for (int i = index; i >= 0; i--)
			{
				if (layout[i].Depth < self.Depth)
				{
					return i;
				}
			}
			return index;
		}

		public static void AdjustExistingForDropNode(RefList<DesignerLayoutNode> layout, int index, DropDirection dropDirection)
		{
			ref DesignerLayoutNode node = ref layout[index];
			switch (dropDirection)
			{
			case DropDirection.Top:
				return;
			case DropDirection.Bottom:
				return;
			case DropDirection.Left:
				return;
			case DropDirection.Right:
				return;
			case DropDirection.Center:
				return;
			}
			throw new ArgumentOutOfRangeException("dropDirection", dropDirection, null);
		}

		public static Rect GetRect(ref Rect rect, float height)
		{
			Rect result = new Rect(rect.x, rect.height, rect.width, height);
			rect.height += height;
			return result;
		}
	}
}
