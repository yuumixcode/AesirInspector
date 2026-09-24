using System;
using UnityEditor;
using UnityEngine;

namespace Sirenix.Utilities.Editor
{
	public class GUITableRowLayoutGroup
	{
		private struct TableRow
		{
			public int yMin;

			public int yMax;

			public int rowHeight;

			public int avgRowHeight;

			public int tmpRowHeight;

			public GUILayoutOption[] layoutOptions;
		}

		private static GUIStyle marginFix;

		private TableRow[] rows;

		private int spaceBefore;

		private int spaceAfter;

		private int avgCellHeight;

		private int currColIndex;

		private int totalHeight;

		private int rowCount;

		private Vector2 outerVisibleRect;

		private Rect innerVisibleRect;

		private Vector2 nextScrollPos;

		private Rect contentRect;

		private bool ignoreScrollView;

		private int isDirty = 3;

		/// <summary>
		/// Whether to draw a draw scroll view.
		/// </summary>
		public bool DrawScrollView = true;

		/// <summary>
		/// The number of pixels before a scroll view appears.
		/// </summary>
		public int MinScrollViewHeight;

		/// <summary>
		/// The maximum scroll view height.
		/// </summary>
		public int MaxScrollViewHeight;

		/// <summary>
		/// The scroll position
		/// </summary>
		public Vector2 ScrollPos;

		/// <summary>
		/// The cell style
		/// </summary>
		public GUIStyle CellStyle;

		/// <summary>
		/// Gets the rect containing all rows. 
		/// </summary>
		public Rect ContentRect => contentRect;

		/// <summary>
		/// Gets the first visible row index.
		/// </summary>
		public int RowIndexFrom { get; private set; }

		/// <summary>
		/// Gets the last visible row index.
		/// </summary>
		public int RowIndexTo { get; private set; }

		/// <summary>
		/// Gets the outer rect. The height of this &lt;= <see cref="P:Sirenix.Utilities.Editor.GUITableRowLayoutGroup.ContentRect" />.height.
		/// </summary>
		public Rect OuterRect { get; private set; }

		/// <summary>
		/// Gets the row rect.
		/// </summary>
		public Rect GetRowRect(int index)
		{
			TableRow r = rows[index];
			return new Rect(contentRect.x, contentRect.y + (float)r.yMin, contentRect.width, r.yMax - r.yMin);
		}

		/// <summary>
		/// Begins the table.
		/// </summary>
		public void BeginTable(int rowCount)
		{
			marginFix = marginFix ?? new GUIStyle();
			currColIndex = 0;
			rows = rows ?? new TableRow[0];
			if (this.rowCount != rowCount)
			{
				isDirty = 3;
				this.rowCount = rowCount;
			}
			RowIndexFrom = Math.Min(this.rowCount, RowIndexFrom);
			RowIndexTo = Math.Min(this.rowCount, RowIndexTo);
			if (rows.Length != rowCount)
			{
				Array.Resize(ref rows, rowCount);
			}
			if (Event.current.type == EventType.Layout)
			{
				for (int i = RowIndexFrom; i < RowIndexTo; i++)
				{
					TableRow row = rows[i];
					if (row.tmpRowHeight > 0)
					{
						if (row.rowHeight != row.tmpRowHeight)
						{
							isDirty = 3;
							row.rowHeight = row.tmpRowHeight;
						}
						row.tmpRowHeight = 0;
						rows[i] = row;
					}
				}
				if (isDirty > 0)
				{
					UpdateSpaceAllocation();
					GUIHelper.RequestRepaint();
				}
			}
			if (Event.current.type == EventType.Repaint)
			{
				Vector2 p = GUIUtility.GUIToScreenPoint(OuterRect.position);
				if (outerVisibleRect != p)
				{
					outerVisibleRect = p;
					isDirty = 3;
				}
			}
			Rect outerRect = EditorGUILayout.BeginVertical(GetOuterRectLayoutOptions());
			if (((Event.current.type != EventType.Layout && outerRect.height > 1f) || Event.current.type == EventType.Repaint) && OuterRect != outerRect)
			{
				isDirty = 3;
				OuterRect = outerRect;
			}
			Vector2 newScrollPos;
			if (DrawScrollBars())
			{
				newScrollPos = GUILayout.BeginScrollView(ScrollPos, alwaysShowHorizontal: false, alwaysShowVertical: false, GetScrollViewOptions(drawScrollBars: true));
			}
			else
			{
				ignoreScrollView = Event.current.rawType == EventType.ScrollWheel;
				if (ignoreScrollView)
				{
					GUIHelper.PushEventType(EventType.Ignore);
				}
				newScrollPos = GUILayout.BeginScrollView(ScrollPos, alwaysShowHorizontal: false, alwaysShowVertical: false, GUIStyle.none, GUIStyle.none, GetScrollViewOptions(drawScrollBars: false));
				if (ignoreScrollView)
				{
					GUIHelper.PopEventType();
					isDirty = 3;
				}
			}
			if (newScrollPos != ScrollPos)
			{
				nextScrollPos = newScrollPos;
				isDirty = 3;
			}
			Rect rect = GUILayoutUtility.GetRect(0f, totalHeight);
			if (Event.current.type != EventType.Layout && rect.width > 1f)
			{
				innerVisibleRect = GUIClipInfo.VisibleRect;
				if (DrawScrollView)
				{
					Vector2 scrollDelta = nextScrollPos - ScrollPos;
					innerVisibleRect.y += scrollDelta.y;
					if (scrollDelta != Vector2.zero)
					{
						isDirty = 3;
					}
				}
				if (rect != contentRect)
				{
					isDirty = 3;
				}
				if (contentRect != rect)
				{
					isDirty = 3;
				}
				contentRect = rect;
			}
			if (isDirty > 0)
			{
				GUIHelper.RequestRepaint();
				if (Event.current.type == EventType.Repaint)
				{
					isDirty--;
				}
			}
		}

		private bool DrawScrollBars()
		{
			return DrawScrollView;
		}

		private GUILayoutOption[] GetScrollViewOptions(bool drawScrollBars)
		{
			if (drawScrollBars)
			{
				return GUILayoutOptions.ExpandHeight(expand: false);
			}
			return GUILayoutOptions.Height(contentRect.height);
		}

		private GUILayoutOption[] GetOuterRectLayoutOptions()
		{
			if (!DrawScrollView)
			{
				return GUILayoutOptions.Height(contentRect.height);
			}
			GUILayoutOptions.GUILayoutOptionsInstance g = GUILayoutOptions.ExpandHeight(expand: false);
			if (MinScrollViewHeight > 0)
			{
				g = g.MinHeight(Mathf.Min(MinScrollViewHeight, contentRect.height));
			}
			g = ((MaxScrollViewHeight <= 0) ? g.MaxHeight(Mathf.Min(99999f, contentRect.height)) : g.MaxHeight(Mathf.Min(MaxScrollViewHeight, contentRect.height)));
			return g;
		}

		/// <summary>
		/// Begins the column.
		/// </summary>
		public void BeginColumn(int xOffset, int width)
		{
			currColIndex++;
			Rect rect = contentRect;
			rect.x += xOffset;
			rect.width = width;
			GUILayout.BeginArea(rect);
			GUILayout.Space(spaceBefore);
		}

		/// <summary>
		/// Begins the cell.
		/// </summary>
		public void BeginCell(int rowIndex)
		{
			TableRow row = rows[rowIndex];
			int id = Math.Abs(117 * rowIndex + 3102919 + currColIndex * 79) + 1;
			GUIUtility.GetControlID(id, FocusType.Passive);
			GUILayout.BeginVertical(row.layoutOptions);
			GUILayout.BeginVertical(CellStyle ?? marginFix);
		}

		/// <summary>
		/// Ends the cell.
		/// </summary>
		public void EndCell(int rowIndex)
		{
			TableRow row = rows[rowIndex];
			if (Event.current.type == EventType.Repaint)
			{
				Rect r = GUIHelper.GetCurrentLayoutRect();
				row.tmpRowHeight = Mathf.Max(row.tmpRowHeight, (int)r.height);
				rows[rowIndex] = row;
			}
			GUILayout.EndVertical();
			GUILayout.EndVertical();
		}

		/// <summary>
		/// Ends the column.
		/// </summary>
		public void EndColumn()
		{
			GUILayout.Space(spaceAfter);
			GUILayout.EndArea();
		}

		/// <summary>
		/// Ends the table.
		/// </summary>
		public void EndTable()
		{
			if (ignoreScrollView)
			{
				GUIHelper.PushEventType(EventType.Ignore);
			}
			GUILayout.EndScrollView();
			if (ignoreScrollView)
			{
				GUIHelper.PopEventType();
			}
			if (Event.current.type == EventType.Repaint)
			{
				ScrollPos = nextScrollPos;
			}
			EditorGUILayout.EndVertical();
		}

		private void UpdateSpaceAllocation()
		{
			bool isFirstVisible = true;
			float visibleRectYMin = innerVisibleRect.yMin;
			float visibleRectYMax = innerVisibleRect.yMax;
			int visibleCellsHeight = 0;
			int visibleCellsCount = 0;
			RowIndexFrom = 0;
			RowIndexTo = 0;
			totalHeight = 0;
			spaceBefore = 0;
			spaceAfter = 0;
			float yOffset = contentRect.y;
			for (int i = 0; i < rowCount; i++)
			{
				TableRow row = rows[i];
				int heightToAllocate = row.rowHeight;
				if (heightToAllocate == 0)
				{
					heightToAllocate = row.avgRowHeight;
				}
				if (heightToAllocate == 0)
				{
					heightToAllocate = avgCellHeight;
				}
				if (heightToAllocate == 0)
				{
					heightToAllocate = 18;
				}
				row.yMin = totalHeight;
				row.yMax = totalHeight + heightToAllocate;
				if ((float)row.yMax + yOffset >= visibleRectYMin && (float)row.yMin + yOffset <= visibleRectYMax)
				{
					row.layoutOptions = GUILayoutOptions.Height(heightToAllocate);
					row.tmpRowHeight = 0;
					if (isFirstVisible)
					{
						RowIndexFrom = i;
						isFirstVisible = false;
					}
					RowIndexTo = i + 1;
				}
				else if ((float)row.yMin + yOffset < innerVisibleRect.yMax)
				{
					spaceBefore += heightToAllocate;
				}
				else
				{
					spaceAfter += heightToAllocate;
					row.avgRowHeight = avgCellHeight;
				}
				rows[i] = row;
				totalHeight = row.yMax;
				if (row.rowHeight > 0)
				{
					visibleCellsCount++;
					visibleCellsHeight += row.rowHeight;
				}
			}
			if (visibleCellsCount > 0 && visibleCellsHeight > 0)
			{
				avgCellHeight = visibleCellsHeight / visibleCellsCount;
			}
		}
	}
}
