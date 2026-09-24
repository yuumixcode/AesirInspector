using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Sirenix.Utilities.Editor
{
	public class MultilineWrapLayoutUtility
	{
		private struct Row
		{
			public int from;

			public int to;

			public int rowIndex;

			public float remainingWidth;

			public Row(int from, int to, int rowIndex, float remainingWidth)
			{
				this.from = from;
				this.to = to;
				this.rowIndex = rowIndex;
				this.remainingWidth = remainingWidth;
			}
		}

		public struct Item
		{
			public int Index;

			public Rect Rect;

			public float width;

			public float xOffset;

			public bool isFirstInRow;

			public bool isLastInRow;
		}

		public Item[] Items;

		public int SelectedIndex;

		public float LineHeight;

		private Rect lastWidth;

		private bool isFirstFrame;

		private int selectedRowIndex = -1;

		private List<Row> rows = new List<Row>();

		private int[] rowOrder = new int[0];

		public int marginLeft;

		public int marginRight;

		public int marginTop;

		public int marginBottom;

		public MultilineWrapLayoutUtility(int lineHeight)
		{
			Items = new Item[0];
			isFirstFrame = true;
			LineHeight = lineHeight;
		}

		public Rect ComputeAndAllocateRect()
		{
			Rect newRect = GUILayoutUtilityCalcHeightBasedOnWidthLayoutEntry.GetRect(CalcHeight);
			if (Event.current.type != EventType.Repaint)
			{
				return default(Rect);
			}
			newRect.x += marginLeft;
			newRect.y += marginTop;
			newRect.height -= marginBottom;
			float yOffset = 0f;
			if (newRect != lastWidth)
			{
				isFirstFrame = true;
			}
			lastWidth = newRect;
			if (isFirstFrame)
			{
				isFirstFrame = false;
				selectedRowIndex = -1;
				for (int j = 0; j < rows.Count; j++)
				{
					for (int i = rows[j].from; i < rows[j].to; i++)
					{
						ref Item item = ref Items[i];
						item.Rect.height = LineHeight;
						item.Rect.x = newRect.x + item.xOffset;
						item.Rect.y = newRect.y + yOffset;
					}
					yOffset += LineHeight;
				}
			}
			else
			{
				float lastRowY = newRect.y + (float)rows.Count * LineHeight;
				for (int k = 0; k < rows.Count; k++)
				{
					for (int l = rows[k].from; l < rows[k].to; l++)
					{
						ref Item item2 = ref Items[l];
						item2.Rect.height = LineHeight;
						item2.Rect.x = newRect.x + item2.xOffset;
						item2.Rect.y = Mathf.Lerp(item2.Rect.y, newRect.y + yOffset, GUITimeHelper.RepaintDeltaTime * 20f);
					}
					yOffset += LineHeight;
				}
			}
			return newRect;
		}

		private float CalcHeight(float totalWidth)
		{
			totalWidth -= (float)(marginRight + marginLeft);
			rows.Clear();
			int prevSelectedRowIndex = selectedRowIndex;
			selectedRowIndex = 0;
			float offsetY = 0f;
			float offsetX = 0f;
			int rowStart = 0;
			int rowIndex = 0;
			for (int i = 0; i < Items.Length; i++)
			{
				offsetX += Items[i].width;
				if (i == Items.Length - 1 || offsetX + Items[i + 1].width >= totalWidth)
				{
					float w = 0f;
					for (int j = rowStart; j < i + 1; j++)
					{
						w += Items[j].width;
					}
					rows.Add(new Row(rowStart, i + 1, rowIndex, totalWidth - w));
					offsetX = 0f;
					offsetY += LineHeight;
					rowStart = i + 1;
					rowIndex++;
				}
			}
			if (rows.Count > 1)
			{
				int totalNumberOfItems = rows.Sum((Row x) => x.to - x.from);
				int bestNumberOfItemsInEachRow = totalNumberOfItems / rows.Count;
				if (bestNumberOfItemsInEachRow > 1)
				{
					int k = 0;
					while (k < 3)
					{
						bool movedItem = false;
						for (int i2 = 0; i2 < rows.Count; i2++)
						{
							Row row = rows[i2];
							int length = row.to - row.from;
							int delta = length - bestNumberOfItemsInEachRow;
							if (delta > 0 && i2 < rows.Count - 1)
							{
								Row nextRow = rows[i2 + 1];
								float lastItemWidthInThisRow = Items[row.to - 1].width;
								float spaceInNextRow = nextRow.remainingWidth;
								if (lastItemWidthInThisRow <= spaceInNextRow)
								{
									nextRow.remainingWidth -= lastItemWidthInThisRow;
									row.remainingWidth += lastItemWidthInThisRow;
									row.to--;
									nextRow.from--;
									movedItem = true;
									rows[i2 + 1] = nextRow;
									rows[i2] = row;
								}
							}
						}
						if (!movedItem)
						{
							k++;
							break;
						}
					}
				}
			}
			foreach (Row row2 in rows)
			{
				float remaining = totalWidth;
				if (SelectedIndex >= row2.from && SelectedIndex < row2.to)
				{
					selectedRowIndex = row2.rowIndex;
				}
				for (int i3 = row2.from; i3 < row2.to; i3++)
				{
					Item item = Items[i3];
					remaining -= item.width;
				}
				int split = (int)(remaining / (float)(row2.to - row2.from));
				float currX = 0f;
				for (int i4 = row2.from; i4 < row2.to; i4++)
				{
					ref Item item2 = ref Items[i4];
					item2.isLastInRow = i4 == row2.to - 1;
					item2.isFirstInRow = i4 == row2.from;
					if (item2.isLastInRow)
					{
						split = (int)(totalWidth - currX - item2.width);
					}
					float width = item2.width + (float)split;
					item2.Rect.width = width;
					item2.xOffset = currX;
					currX += width;
				}
			}
			if (rowOrder.Length != rows.Count)
			{
				rowOrder = new int[rows.Count];
				for (int i5 = 0; i5 < rowOrder.Length; i5++)
				{
					rowOrder[i5] = i5;
				}
			}
			if (prevSelectedRowIndex != selectedRowIndex || isFirstFrame)
			{
				int currentOrder = rowOrder[selectedRowIndex];
				for (int i6 = 0; i6 < rows.Count; i6++)
				{
					if (rowOrder[i6] > currentOrder)
					{
						rowOrder[i6]--;
					}
				}
				rowOrder[selectedRowIndex] = rows.Count - 1;
				prevSelectedRowIndex = selectedRowIndex;
			}
			rows.Sort((Row a, Row b) => rowOrder[a.rowIndex].CompareTo(rowOrder[b.rowIndex]));
			return offsetY;
		}
	}
}
