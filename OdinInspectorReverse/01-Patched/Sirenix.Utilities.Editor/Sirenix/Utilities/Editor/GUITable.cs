using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Sirenix.Utilities.Editor
{
	/// <summary>
	/// <para>A Utility class for creating tables in Unity's editor GUI.</para>
	/// <para>A table can either be created from scratch using new GUITable(xCount,yCount), or created using one of the static GUITable.Create overloads.</para>
	/// <para>See the online documentation, for examples and more information.</para>
	/// </summary>
	/// <example>
	/// <para>Creating a matrix table for a two-dimentional array.</para>
	/// <code>
	/// private GUITable table;
	///
	/// private void Init()
	/// {
	///     bool[,] boolArr = new bool[20,20];
	///
	///     this.table = GUITable.Create(
	///         twoDimArray: boolArr,
	///         drawElement: (rect, x, y) =&gt; boolArr[x, y] = EditorGUI.Toggle(rect, boolArr[x, y]),
	///         horizontalLabel: "Optional Horizontal Label",               // horizontalLabel is optional and can be null.
	///         columnLabels: (rect, x) =&gt; GUI.Label(rect, x.ToString()),   // columnLabels is optional and can be null.
	///         verticalLabel: "Optional Vertical Label",                   // verticalLabel is optional and can be null.
	///         rowLabels: (rect, x) =&gt; GUI.Label(rect, x.ToString())       // rowLabels is optional and can be null.
	///     );
	/// }
	///
	/// private void OnGUI()
	/// {
	///     this.table.DrawTable();
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// <para>Creating a table for a list.</para>
	/// <code>
	/// private GUITable table;
	///
	/// private void Init()
	/// {
	///     Listt&lt;SomeClasst&gt; someList = new List&lt;SomeClass&gt;() { new SomeClass(), new SomeClass(), new SomeClass() };
	///
	///     this.table = GUITable.Create(someList, "Optional Title",
	///         new GUITableColumn()
	///         {
	///             ColumnTitle = "A",
	///             OnGUI = (rect, i) =&gt; someList[i].A = EditorGUI.TextField(rect, someList[i].A),
	///             Width = 200,
	///             MinWidth = 100,
	///         },
	///         new GUITableColumn()
	///         {
	///             ColumnTitle = "B",
	///             OnGUI = (rect, i) =&gt; someList[i].B = EditorGUI.IntField(rect, someList[i].B),
	///             Resizable = false,
	///         },
	///         new GUITableColumn()
	///         {
	///             ColumnTitle = "C",
	///             OnGUI = (rect, i) =&gt; someList[i].C = EditorGUI.IntField(rect, someList[i].C),
	///             SpanColumnTitle = true,
	///         }
	///     );
	/// }
	///
	/// private void OnGUI()
	/// {
	///     this.table.DrawTable();
	/// }
	///
	/// private class SomeClass
	/// {
	///     public string A;
	///     public int B;
	///     public int C;
	///     public int D;
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// <para>Styling a cell.</para>
	/// <para>Each <see cref="T:Sirenix.Utilities.Editor.GUITableCell" /> has two events, OnGUI and OnGUIStyle. OnGUIStyle is called right before OnGUI, but only in repaint events.</para>
	/// <code>
	/// guiTable[x,y].GUIStyle += rect =&gt; EditorGUI.DrawRect(rect, Color.red);
	/// </code>
	/// </example>
	/// <example>
	/// <para>Row and column span.</para>
	/// <para>A cell will span and cover all neighbour cells that are null.</para>
	/// <code>
	/// // Span horizontally:
	/// guiTable[x - 2,y] = null;
	/// guiTable[x - 1,y] = null;
	/// guiTable[x,y].SpanX = true;
	/// guiTable[x + 1,y] = null;
	///
	/// // Span vertically:
	/// guiTable[x,y - 2] = null;
	/// guiTable[x,y - 1] = null;
	/// guiTable[x,y].SpanY = true;
	/// guiTable[x,y + 1] = null;
	/// </code>
	/// </example>
	/// <seealso cref="T:Sirenix.Utilities.Editor.GUITable" />
	/// <seealso cref="T:Sirenix.Utilities.Editor.GUITableCell" />
	public class GUITable
	{
		private class ColumnInfo
		{
			private float resizeOffset;

			public GUITable Table;

			public bool Resizable = true;

			public bool IsAutoSize;

			public float ColumnEnd;

			public float ColumnStart;

			public float ColumnWidth;

			public float ColumnMinWidth = 4f;

			public float ResizeOffset
			{
				get
				{
					return resizeOffset * Table.tableRect.width;
				}
				set
				{
					resizeOffset = value / Table.tableRect.width;
				}
			}
		}

		private readonly GUITableCell[,] cells;

		private readonly ColumnInfo[] columnInfos;

		private readonly float[] rowHeights;

		private bool isDirty;

		private Rect tableRect;

		private Vector2 minTalbeSize;

		private int numOfAutoWidthColumns;

		/// <summary>
		/// The row count.
		/// </summary>
		public readonly int RowCount;

		/// <summary>
		/// The column count.
		/// </summary>
		public readonly int ColumnCount;

		/// <summary>
		/// Whether to respect the current GUI indent level.
		/// </summary>
		public bool RespectIndentLevel = true;

		/// <summary>
		/// The Table Rect.
		/// </summary>
		public Rect TableRect => tableRect;

		/// <summary>
		/// Gets or sets a <see cref="T:Sirenix.Utilities.Editor.GUITableCell" /> from the <see cref="T:Sirenix.Utilities.Editor.GUITable" />.
		/// </summary>
		public GUITableCell this[int x, int y]
		{
			get
			{
				return cells[x, y];
			}
			set
			{
				if (value != null)
				{
					value.Table = this;
					value.X = x;
					value.Y = y;
				}
				cells[x, y] = value;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Sirenix.Utilities.Editor.GUITable" /> class.
		/// </summary>
		public GUITable(int columnCount, int rowCount)
		{
			cells = new GUITableCell[columnCount, rowCount];
			RowCount = rowCount;
			ColumnCount = columnCount;
			rowHeights = new float[rowCount];
			columnInfos = new ColumnInfo[columnCount];
			for (int i = 0; i < columnInfos.Length; i++)
			{
				columnInfos[i] = new ColumnInfo
				{
					Table = this
				};
			}
		}

		/// <summary>
		/// Draws the table.
		/// </summary>
		public void DrawTable()
		{
			EventType e = Event.current.type;
			if (minTalbeSize.y == 0f || isDirty)
			{
				ReCalculateSizes();
			}
			Rect newRect = GUILayoutUtility.GetRect(options: (numOfAutoWidthColumns != 0) ? GUILayoutOptions.ExpandWidth().MinWidth(minTalbeSize.x + (float)(numOfAutoWidthColumns * 10)) : GUILayoutOptions.ExpandWidth(expand: false).Width(minTalbeSize.x), width: 0f, height: (minTalbeSize.y > 0f) ? minTalbeSize.y : 10f);
			if (RespectIndentLevel)
			{
				newRect = EditorGUI.IndentedRect(newRect);
			}
			if (e == EventType.Repaint)
			{
				if (tableRect.width != newRect.width || tableRect.x != newRect.x || tableRect.y != newRect.y)
				{
					tableRect = newRect;
					ReCalculateSizes();
				}
				else
				{
					tableRect = newRect;
				}
			}
			for (int x = 0; x < ColumnCount - 1; x++)
			{
				if ((x < ColumnCount - 1 && !columnInfos[x + 1].Resizable) || !columnInfos[x].Resizable)
				{
					continue;
				}
				GUITableCell resizeCell = null;
				for (int y = 0; y < RowCount; y++)
				{
					GUITableCell candidate = cells[x, y];
					if (candidate != null && !candidate.SpanX)
					{
						resizeCell = candidate;
						break;
					}
				}
				if (resizeCell == null)
				{
					continue;
				}
				Rect rect = resizeCell.Rect;
				rect.x = rect.xMax - 5f;
				rect.width = 10f;
				GUIHelper.PushGUIEnabled(enabled: true);
				float mouseDelta = SirenixEditorGUI.SlideRect(rect).x;
				GUIHelper.PopGUIEnabled();
				if (mouseDelta == 0f)
				{
					continue;
				}
				if (mouseDelta > 0f)
				{
					ColumnInfo ci = columnInfos[x];
					ColumnInfo nextResizableCol = null;
					for (int j = x + 1; j < ColumnCount; j++)
					{
						if (columnInfos[j].Resizable)
						{
							nextResizableCol = columnInfos[j];
							break;
						}
					}
					if (nextResizableCol != null)
					{
						float remaining = nextResizableCol.ColumnWidth - nextResizableCol.ColumnMinWidth;
						if (nextResizableCol != null && remaining > 0f)
						{
							mouseDelta = Mathf.Min(mouseDelta, remaining);
							ci.ResizeOffset += mouseDelta;
							nextResizableCol.ResizeOffset -= mouseDelta;
						}
						ReCalculateSizes();
					}
					continue;
				}
				ColumnInfo ci2 = columnInfos[x + 1];
				mouseDelta *= -1f;
				ColumnInfo prevResizableCol = null;
				for (int j2 = x; j2 >= 0; j2--)
				{
					if (columnInfos[j2].Resizable)
					{
						prevResizableCol = columnInfos[j2];
						break;
					}
				}
				if (prevResizableCol != null)
				{
					float remaining2 = prevResizableCol.ColumnWidth - prevResizableCol.ColumnMinWidth;
					if (prevResizableCol != null && remaining2 > mouseDelta)
					{
						mouseDelta = Mathf.Min(mouseDelta, remaining2);
						ci2.ResizeOffset += mouseDelta;
						prevResizableCol.ResizeOffset -= mouseDelta;
					}
				}
				ReCalculateSizes();
			}
			GUIHelper.PushIndentLevel(0);
			for (int i = 0; i < ColumnCount; i++)
			{
				for (int k = 0; k < RowCount; k++)
				{
					cells[i, k]?.Draw();
				}
			}
			GUIHelper.PopIndentLevel();
		}

		/// <summary>
		/// Recaluclates cell and column sizes in the next frame.
		/// </summary>
		public void MarkDirty()
		{
			isDirty = true;
			GUIHelper.RequestRepaint();
		}

		private void SpanCellX(ref Rect rect, int x, int y)
		{
			int j = x - 1;
			while (j >= 0 && cells[j, y] == null)
			{
				rect.xMin = columnInfos[j].ColumnStart;
				j--;
			}
			for (int i = x + 1; i < ColumnCount && cells[i, y] == null; i++)
			{
				rect.xMax = columnInfos[i].ColumnEnd;
			}
		}

		private void SpanCellY(ref Rect rect, int x, int y)
		{
			for (int j = y + 1; j < RowCount && cells[x, j] == null; j++)
			{
				rect.height += rowHeights[j];
			}
			int j2 = y - 1;
			while (j2 >= 0 && cells[x, j2] == null)
			{
				rect.yMin -= rowHeights[j2];
				j2--;
			}
		}

		/// <summary>
		/// <para>Recalculates the layout for the entire table.</para>
		/// <para>This method gets called whenever the table is initialized, resized or adjusted. If you are manipulating
		/// the width or height of individual table cells, remember to call this method when you're done.</para>
		/// </summary>
		public void ReCalculateSizes()
		{
			minTalbeSize.x = 0f;
			numOfAutoWidthColumns = ColumnCount;
			for (int x = 0; x < ColumnCount; x++)
			{
				float width = 0f;
				float minWidth = 0f;
				if (ColumnCount != 1)
				{
					for (int y = 0; y < RowCount; y++)
					{
						GUITableCell cell = cells[x, y];
						if (cell != null)
						{
							width = Mathf.Max(width, cell.Width);
							minWidth = Mathf.Max(minWidth, cell.MinWidth);
						}
					}
				}
				ColumnInfo col = columnInfos[x];
				col.IsAutoSize = width <= 0f;
				col.ColumnMinWidth = minWidth;
				if (!col.IsAutoSize)
				{
					minTalbeSize.x += width;
					numOfAutoWidthColumns--;
				}
				col.ColumnWidth = width;
			}
			float autoWidth = (tableRect.width - minTalbeSize.x) / (float)numOfAutoWidthColumns;
			float currX = tableRect.x;
			for (int i = 0; i < columnInfos.Length; i++)
			{
				ColumnInfo ci = columnInfos[i];
				if (ci.ColumnWidth == 0f)
				{
					ci.ColumnWidth = Mathf.Max(0f, autoWidth);
				}
				ci.ColumnStart = currX;
				ci.ColumnWidth = Mathf.Max(ci.ColumnWidth + ci.ResizeOffset, ci.ColumnMinWidth);
				ci.ColumnEnd = ci.ColumnStart + ci.ColumnWidth;
				currX += ci.ColumnWidth;
			}
			minTalbeSize.y = 0f;
			for (int j = 0; j < RowCount; j++)
			{
				float height = 0f;
				for (int k = 0; k < ColumnCount; k++)
				{
					GUITableCell cell2 = cells[k, j];
					if (cell2 != null)
					{
						height = Mathf.Max(height, cell2.Height);
					}
				}
				minTalbeSize.y += height;
				rowHeights[j] = height;
			}
			for (int l = 0; l < ColumnCount; l++)
			{
				ColumnInfo ci2 = columnInfos[l];
				float currY = tableRect.y;
				for (int m = 0; m < RowCount; m++)
				{
					float height2 = rowHeights[m];
					GUITableCell cell3 = cells[l, m];
					if (cell3 != null)
					{
						Rect rect = new Rect(ci2.ColumnStart, currY, ci2.ColumnWidth, height2);
						if (cell3.SpanX)
						{
							SpanCellX(ref rect, l, m);
						}
						if (cell3.SpanY)
						{
							SpanCellY(ref rect, l, m);
						}
						cell3.Rect = rect;
					}
					currY += height2;
				}
			}
			isDirty = false;
		}

		private void ApplyTitleStyle(GUITableCell from, GUITableCell to)
		{
			if (from == null)
			{
				throw new NullReferenceException("from");
			}
			if (to == null)
			{
				throw new NullReferenceException("to");
			}
			bool horizontal = from.X < to.X;
			bool vertical = from.Y < to.Y;
			Color titleBgColor = SirenixGUIStyles.ListItemColorEven * 0.9f;
			titleBgColor.a = 0.7f;
			from.GUIStyle = (Action<Rect>)Delegate.Combine(from.GUIStyle, (Action<Rect>)delegate(Rect rect)
			{
				if (!horizontal && !vertical)
				{
					SirenixEditorGUI.DrawSolidRect(rect, titleBgColor);
					rect.height += 1f;
					rect.width += 1f;
					SirenixEditorGUI.DrawBorders(rect, 1, 1, 1, 1, SirenixGUIStyles.BorderColor);
				}
				else
				{
					float num = 0f;
					if (horizontal)
					{
						Rect rect2 = rect;
						num += rect2.height;
						rect2.xMax = to.Rect.xMax;
						SirenixEditorGUI.DrawSolidRect(rect2, titleBgColor);
						rect2.height += 1f;
						rect2.width += 1f;
						SirenixEditorGUI.DrawBorders(rect2, 1, 1, 1, 1, SirenixGUIStyles.BorderColor);
					}
					if (vertical)
					{
						Rect rect3 = rect;
						rect3.yMin += num;
						rect3.yMax = to.Rect.yMax;
						SirenixEditorGUI.DrawSolidRect(rect3, titleBgColor);
						rect3.height += 1f;
						rect3.width += 1f;
						SirenixEditorGUI.DrawBorders(rect3, 1, 1, 1, 1, SirenixGUIStyles.BorderColor);
					}
				}
			});
		}

		private void ApplyListStyle(int xStart, int yStart, int xCount, int yCount, bool startBlack)
		{
			int xEnd = Mathf.Min(xStart + xCount, ColumnCount);
			int yEnd = Mathf.Min(yStart + yCount, RowCount);
			GUITableCell fromCell = cells[xStart, yStart];
			GUITableCell toCell = cells[xEnd, yEnd];
			for (int y = yStart + ((!startBlack) ? 1 : 0); y <= yEnd; y += 2)
			{
				if (cells[xStart, y] != null)
				{
					GUITableCell gUITableCell = cells[xStart, y];
					gUITableCell.GUIStyle = (Action<Rect>)Delegate.Combine(gUITableCell.GUIStyle, (Action<Rect>)delegate(Rect rect)
					{
						rect.xMax = toCell.Rect.xMax;
						rect.x += 1f;
						rect.width -= 1f;
						SirenixEditorGUI.DrawSolidRect(rect, SirenixGUIStyles.ListItemColorEven);
					});
				}
			}
			for (int y2 = yStart; y2 <= yEnd; y2++)
			{
				if (cells[xStart, y2] != null)
				{
					GUITableCell gUITableCell2 = cells[xStart, y2];
					gUITableCell2.GUIStyle = (Action<Rect>)Delegate.Combine(gUITableCell2.GUIStyle, (Action<Rect>)delegate(Rect rect)
					{
						rect.xMax = toCell.Rect.xMax;
						rect.height = 1f;
						SirenixEditorGUI.DrawSolidRect(rect, SirenixGUIStyles.BorderColor);
					});
				}
			}
			for (int x = xStart; x <= xEnd; x++)
			{
				if (cells[x, yStart] != null)
				{
					GUITableCell gUITableCell3 = cells[x, yStart];
					gUITableCell3.GUIStyle = (Action<Rect>)Delegate.Combine(gUITableCell3.GUIStyle, (Action<Rect>)delegate(Rect rect)
					{
						rect.yMax = toCell.Rect.yMax;
						rect.width = 1f;
						SirenixEditorGUI.DrawSolidRect(rect, SirenixGUIStyles.BorderColor);
					});
				}
			}
			GUITableCell gUITableCell4 = toCell;
			gUITableCell4.GUIStyle = (Action<Rect>)Delegate.Combine(gUITableCell4.GUIStyle, (Action<Rect>)delegate(Rect rect)
			{
				Rect rect2 = rect;
				rect2.xMin = fromCell.Rect.xMin;
				rect2.yMin = rect2.yMax;
				rect2.height = 1f;
				SirenixEditorGUI.DrawSolidRect(rect2, SirenixGUIStyles.BorderColor);
				rect2 = rect;
				rect2.yMin = fromCell.Rect.yMin;
				rect2.xMin = rect2.xMax;
				rect2.width = 1f;
				SirenixEditorGUI.DrawSolidRect(rect2, SirenixGUIStyles.BorderColor);
			});
		}

		/// <summary>
		/// Creates a table.
		/// </summary>
		public static GUITable Create(int colCount, int rowCount, Action<Rect, int, int> drawElement, string horizontalLabel, Action<Rect, int> columnLabels, string verticalLabel, Action<Rect, int> rowLabels, bool resizable = true)
		{
			int cols = colCount;
			int rows = rowCount;
			if (!string.IsNullOrEmpty(verticalLabel))
			{
				cols++;
			}
			if (rowLabels != null)
			{
				cols++;
			}
			if (!string.IsNullOrEmpty(horizontalLabel))
			{
				rows++;
			}
			if (columnLabels != null)
			{
				rows++;
			}
			GUITable table = new GUITable(cols, rows);
			cols = colCount;
			rows = rowCount;
			int colStart = 0;
			int rowStart = 0;
			if (!string.IsNullOrEmpty(verticalLabel))
			{
				colStart++;
				table[0, table.RowCount - 1] = new GUITableCell
				{
					OnGUI = delegate(Rect rect)
					{
						if (Event.current.type == EventType.Repaint)
						{
							Color color = SirenixGUIStyles.ListItemColorEven * 0.9f;
							color.a = 0.7f;
							SirenixEditorGUI.DrawSolidRect(rect, color);
							rect.width += 1f;
							rect.height += 1f;
							SirenixEditorGUI.DrawBorders(rect, 1);
							rect.width -= 1f;
							rect.height -= 1f;
							Matrix4x4 matrix = GUI.matrix;
							float num = rect.x + rect.width * 0.5f;
							rect = rect.AlignCenter(rect.height, rect.height);
							rect.x = 0f;
							float num2 = rect.x + rect.width * 0.5f;
							GUIUtility.RotateAroundPivot(-90f, rect.center);
							GUI.matrix *= Matrix4x4.TRS(new Vector3(0f, num - num2, 0f), Quaternion.identity, Vector3.one);
							GUI.Label(rect, verticalLabel, SirenixGUIStyles.LabelCentered);
							GUI.matrix = matrix;
						}
					},
					SpanY = true,
					Width = 22f
				};
			}
			if (!string.IsNullOrEmpty(horizontalLabel))
			{
				rowStart++;
				table[Mathf.Min(2, table.ColumnCount - 1), 0] = new GUITableCell
				{
					OnGUI = delegate(Rect rect)
					{
						if (Event.current.type == EventType.Repaint)
						{
							Color color = SirenixGUIStyles.ListItemColorEven * 0.9f;
							color.a = 0.7f;
							SirenixEditorGUI.DrawSolidRect(rect, color);
							GUI.Label(rect, horizontalLabel, SirenixGUIStyles.LabelCentered);
							rect.width += 1f;
							rect.height += 1f;
							SirenixEditorGUI.DrawBorders(rect, 1);
						}
					},
					SpanX = true
				};
			}
			if (rowLabels != null)
			{
				colStart++;
			}
			if (columnLabels != null)
			{
				rowStart++;
			}
			if (rowLabels != null)
			{
				for (int y = 0; y < rows; y++)
				{
					int localY = y;
					table[colStart - 1, rowStart + y] = new GUITableCell
					{
						OnGUI = delegate(Rect rect)
						{
							rowLabels(rect, localY);
						},
						Width = 25f
					};
				}
				table.ApplyListStyle(colStart - 1, rowStart, 0, rows - 1, startBlack: false);
				table.ApplyTitleStyle(table[colStart - 1, rowStart], table[colStart - 1, rowStart + rows - 1]);
			}
			if (columnLabels != null)
			{
				for (int x = 0; x < cols; x++)
				{
					int localX = x;
					table[colStart + x, rowStart - 1] = new GUITableCell
					{
						OnGUI = delegate(Rect rect)
						{
							columnLabels(rect, localX);
						}
					};
				}
				table.ApplyListStyle(colStart, rowStart - 1, cols - 1, 0, startBlack: false);
				table.ApplyTitleStyle(table[colStart, rowStart - 1], table[colStart + cols - 1, rowStart - 1]);
			}
			for (int x2 = 0; x2 < cols; x2++)
			{
				for (int y2 = 0; y2 < rows; y2++)
				{
					int localX2 = x2;
					int localY2 = y2;
					table[x2 + colStart, y2 + rowStart] = new GUITableCell
					{
						OnGUI = delegate(Rect rect)
						{
							drawElement(rect, localX2, localY2);
						}
					};
				}
			}
			for (int x3 = 0; x3 < colStart; x3++)
			{
				for (int y3 = 0; y3 < rowStart; y3++)
				{
					table[x3, y3] = new GUITableCell();
				}
			}
			for (int i = 0; i < table.columnInfos.Length; i++)
			{
				table.columnInfos[i].Resizable = resizable;
			}
			table.ApplyListStyle(colStart, rowStart, cols - 1, rows - 1, startBlack: true);
			return table;
		}

		/// <summary>
		/// Creates a table.
		/// </summary>
		public static GUITable Create(int rowCount, string title, params GUITableColumn[] columns)
		{
			bool hasColLabels = columns.Any((GUITableColumn gUITableColumn) => gUITableColumn.ColumnTitle != null);
			bool hasTitle = title != null;
			int extraLineCount = (hasTitle ? 1 : 0) + (hasColLabels ? 1 : 0);
			GUITable table = new GUITable(columns.Length, rowCount + extraLineCount);
			if (hasTitle)
			{
				GUITableCell obj = new GUITableCell
				{
					SpanX = true,
					OnGUI = delegate(Rect rect)
					{
						GUI.Label(rect, title, SirenixGUIStyles.LabelCentered);
					}
				};
				GUITableCell gUITableCell = obj;
				table[0, 0] = obj;
				GUITableCell t = gUITableCell;
				table.ApplyTitleStyle(t, t);
			}
			for (int x = 0; x < columns.Length; x++)
			{
				GUITableColumn column = columns[x];
				for (int y = 0; y < rowCount; y++)
				{
					int localY = y;
					table[x, y + extraLineCount] = new GUITableCell
					{
						OnGUI = delegate(Rect rect)
						{
							column.OnGUI(rect.Padding(3f), localY);
						}
					};
				}
			}
			if (hasColLabels)
			{
				int colTitleStart = (hasTitle ? 1 : 0);
				for (int x2 = 0; x2 < columns.Length; x2++)
				{
					GUITableColumn column2 = columns[x2];
					if (column2.ColumnTitle != null)
					{
						table[x2, colTitleStart] = new GUITableCell
						{
							OnGUI = delegate(Rect rect)
							{
								GUI.Label(rect, column2.ColumnTitle, SirenixGUIStyles.LabelCentered);
							},
							Width = column2.Width,
							SpanX = column2.SpanColumnTitle
						};
					}
					else
					{
						for (int y2 = 0; y2 < rowCount; y2++)
						{
							GUITableCell cell = table[x2, y2 + extraLineCount];
							if (cell != null)
							{
								cell.Width = column2.Width;
								cell.MinWidth = column2.MinWidth;
								break;
							}
						}
					}
					table.columnInfos[x2].Resizable = column2.Resizable;
				}
				table.ApplyListStyle(0, colTitleStart, columns.Length - 1, 0, startBlack: false);
				table.ApplyTitleStyle(table[0, colTitleStart], table[columns.Length - 1, colTitleStart]);
			}
			if (rowCount > 0)
			{
				table.ApplyListStyle(0, extraLineCount, columns.Length - 1, rowCount - 1, startBlack: true);
			}
			return table;
		}

		/// <summary>
		/// Creates a table.
		/// </summary>
		public static GUITable Create<T>(T[,] twoDimArray, Action<Rect, int, int> drawElement, string horizontalLabel, Action<Rect, int> columnLabels, string verticalLabel, Action<Rect, int> rowLabels)
		{
			return Create(twoDimArray.GetLength(0), twoDimArray.GetLength(1), drawElement, horizontalLabel, columnLabels, verticalLabel, rowLabels);
		}

		/// <summary>
		/// Creates a table.
		/// </summary>
		public static GUITable Create<T>(IList<T> list, string title, params GUITableColumn[] columns)
		{
			return Create(list.Count, title, columns);
		}
	}
}
