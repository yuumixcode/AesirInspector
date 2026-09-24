using System.Collections.Generic;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal static class LayoutPass
	{
		public static List<Slot> Run(DesignerEditorContext ctx, DesignerEditorNode root, Rect viewport)
		{
			List<Slot> slots = new List<Slot>(128);
			if (root == null)
			{
				return slots;
			}
			Dictionary<DesignerEditorNode, float> heightCache = new Dictionary<DesignerEditorNode, float>(128);
			foreach (DesignerEditorNode child in root.Children)
			{
				MeasureNodeHeight(ctx, child, heightCache);
			}
			bool isNodePlaced = false;
			float y = viewport.yMin;
			for (int i = 0; i < root.Children.Count; i++)
			{
				DesignerEditorNode child2 = root.Children[i];
				if ((ctx.IsShowHideMode || child2.NodeType != DesignerEditorNodeType.Member || child2.IsShownInInspector) && !child2.IsExcluded)
				{
					if (isNodePlaced)
					{
						y += 8f;
					}
					PlaceNode(ctx, child2, 0, 0, viewport, heightCache, slots, ref y);
					isNodePlaced = true;
				}
			}
			return slots;
		}

		private static float MeasureNodeHeight(DesignerEditorContext ctx, DesignerEditorNode node, Dictionary<DesignerEditorNode, float> heightCache)
		{
			if ((!ctx.IsShowHideMode && node.NodeType == DesignerEditorNodeType.Member && !node.IsShownInInspector) || node.IsExcluded)
			{
				return 0f;
			}
			switch (node.NodeType)
			{
			case DesignerEditorNodeType.None:
			case DesignerEditorNodeType.Root:
				return 0f;
			case DesignerEditorNodeType.Group:
			{
				if (node.IsRow)
				{
					float maxColumnHeight = 0f;
					foreach (DesignerEditorNode column in node.Children)
					{
						maxColumnHeight = Mathf.Max(maxColumnHeight, MeasureNodeHeight(ctx, column, heightCache));
					}
					maxColumnHeight += 16f;
					return heightCache[node] = maxColumnHeight;
				}
				if (node.IsColumn)
				{
					float columnHeight = 28f;
					for (int i = 0; i < node.Children.Count; i++)
					{
						DesignerEditorNode child = node.Children[i];
						columnHeight += MeasureNodeHeight(ctx, child, heightCache);
						if (i > 0)
						{
							columnHeight += 8f;
						}
					}
					return heightCache[node] = Mathf.Max(columnHeight, 58f);
				}
				float verticalGroupHeight = 44f;
				for (int j = 0; j < node.Children.Count; j++)
				{
					DesignerEditorNode child2 = node.Children[j];
					if (j > 0)
					{
						verticalGroupHeight += 8f;
					}
					verticalGroupHeight += MeasureNodeHeight(ctx, child2, heightCache);
				}
				return heightCache[node] = Mathf.Max(verticalGroupHeight, 74f);
			}
			case DesignerEditorNodeType.Member:
				return heightCache[node] = 30f;
			default:
				return 0f;
			}
		}

		private static void PlaceNode(DesignerEditorContext ctx, DesignerEditorNode node, int zIndex, int indentLevel, Rect view, Dictionary<DesignerEditorNode, float> heightCache, List<Slot> slots, ref float y)
		{
			float x = view.xMin + (float)(indentLevel * 8);
			float width = view.width - (float)(indentLevel * 8) * 2f;
			float height = heightCache[node];
			switch (node.NodeType)
			{
			case DesignerEditorNodeType.None:
			case DesignerEditorNodeType.Root:
				break;
			case DesignerEditorNodeType.Group:
			{
				if (node.IsRow)
				{
					List<DropZone> dropZones2 = new List<DropZone>
					{
						new DropZone(DropDirection.Left, new Rect(0f, 0f, 15f, height)),
						new DropZone(DropDirection.Right, new Rect(width - 15f, 0f, 15f, height)),
						new DropZone(DropDirection.Top, new Rect(15f, 0f, width - 30f, 15f)),
						new DropZone(DropDirection.Bottom, new Rect(15f, height - 15f, width - 30f, 15f))
					};
					slots.Add(new Slot
					{
						Node = node,
						ZIndex = zIndex,
						IndentLevel = indentLevel,
						Index = slots.Count,
						Rect = new Rect(x, y, width, height),
						DropZonesRelative = dropZones2
					});
					int columnCount = node.Children.Count;
					int innerPadding = (columnCount - 1) * 8;
					float innerWidth = width - 16f - (float)innerPadding;
					float columnWidth = innerWidth / (float)columnCount;
					float startX = x + 8f;
					float equalizedColumnHeight = height - 16f;
					float contentY = y + 8f;
					for (int i = 0; i < columnCount; i++)
					{
						DesignerEditorNode column = node.Children[i];
						heightCache[column] = equalizedColumnHeight;
						float columnX = startX + (float)i * (columnWidth + 8f);
						Rect columnRect = new Rect(columnX, -1f, columnWidth, -1f);
						PlaceNode(ctx, column, zIndex + 1, indentLevel, columnRect, heightCache, slots, ref contentY);
					}
					y += height;
					break;
				}
				if (node.IsColumn)
				{
					List<DropZone> dropZones3 = new List<DropZone>
					{
						new DropZone(DropDirection.Left, new Rect(0f, 0f, 15f, height)),
						new DropZone(DropDirection.Right, new Rect(width - 15f, 0f, 15f, height))
					};
					if (node.Children.Count == 0)
					{
						dropZones3.Add(new DropZone(DropDirection.Center, new Rect(8f, 28f, width - 8f, height - 28f - 16f)));
					}
					slots.Add(new Slot
					{
						Node = node,
						ZIndex = zIndex,
						IndentLevel = indentLevel,
						Index = slots.Count,
						Rect = new Rect(x, y, width, height),
						DropZonesRelative = dropZones3
					});
					bool isNodePlaced = false;
					float contentY2 = y + 20f + 8f;
					for (int j = 0; j < node.Children.Count; j++)
					{
						DesignerEditorNode child = node.Children[j];
						if ((ctx.IsShowHideMode || child.NodeType != DesignerEditorNodeType.Member || child.IsShownInInspector) && !child.IsExcluded)
						{
							if (isNodePlaced)
							{
								contentY2 += 8f;
							}
							PlaceNode(ctx, child, zIndex + 1, indentLevel, view, heightCache, slots, ref contentY2);
							isNodePlaced = true;
						}
					}
					break;
				}
				List<DropZone> dropZones4 = new List<DropZone>
				{
					new DropZone(DropDirection.Left, new Rect(0f, 0f, 15f, height)),
					new DropZone(DropDirection.Right, new Rect(width - 15f, 0f, 15f, height)),
					new DropZone(DropDirection.Top, new Rect(15f, 0f, width - 30f, 15f)),
					new DropZone(DropDirection.Bottom, new Rect(15f, height - 15f, width - 30f, 15f))
				};
				if (node.Children.Count == 0)
				{
					dropZones4.Add(new DropZone(DropDirection.Center, new Rect(8f, 36f, width - 8f, height - 28f - 16f)));
				}
				slots.Add(new Slot
				{
					Node = node,
					ZIndex = zIndex,
					IndentLevel = indentLevel,
					Index = slots.Count,
					Rect = new Rect(x, y, width, height),
					DropZonesRelative = dropZones4
				});
				float contentY3 = y + 28f + 8f;
				indentLevel++;
				bool isNodePlaced2 = false;
				for (int k = 0; k < node.Children.Count; k++)
				{
					DesignerEditorNode child2 = node.Children[k];
					if ((ctx.IsShowHideMode || child2.NodeType != DesignerEditorNodeType.Member || child2.IsShownInInspector) && !child2.IsExcluded)
					{
						if (isNodePlaced2)
						{
							contentY3 += 8f;
						}
						PlaceNode(ctx, child2, zIndex + 1, indentLevel, view, heightCache, slots, ref contentY3);
						isNodePlaced2 = true;
					}
				}
				y += height;
				break;
			}
			case DesignerEditorNodeType.Member:
			{
				List<DropZone> dropZones = new List<DropZone>
				{
					new DropZone(DropDirection.Left, new Rect(0f, 0f, 50f, height)),
					new DropZone(DropDirection.Right, new Rect(width - 50f, 0f, 50f, height)),
					new DropZone(DropDirection.Top, new Rect(15f, 0f, width - 30f, 15f)),
					new DropZone(DropDirection.Bottom, new Rect(15f, height - 15f, width - 30f, 15f))
				};
				slots.Add(new Slot
				{
					Node = node,
					ZIndex = zIndex,
					IndentLevel = indentLevel,
					Index = slots.Count,
					Rect = new Rect(x, y, width, height),
					DropZonesRelative = dropZones
				});
				y += height;
				break;
			}
			}
		}
	}
}
