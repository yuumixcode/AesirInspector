using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Base class for two-dimensional array drawers.
	/// </summary>
	public abstract class TwoDimensionalArrayDrawer<TArray, TElement> : OdinValueDrawer<TArray> where TArray : IList
	{
		protected internal class Context
		{
			public int RowCount;

			public int ColCount;

			public GUITable Table;

			public TElement[,] Value;

			public int DraggingRow = -1;

			public int DraggingCol = -1;

			public TableMatrixAttribute Attribute;

			public ValueResolver<TElement> DrawElement;

			public ValueResolver<string> HorizontalTitleGetter;

			public ValueResolver<string> VerticalTitleGetter;

			public ValueResolver<(string, LabelDirection)> LabelGetter;

			public Vector2 dragStartPos;

			public bool IsDraggingColumn;

			public int ColumnDragFrom;

			public int ColumnDragTo;

			public bool IsDraggingRow;

			public int RowDragFrom;

			public int RowDragTo;

			public string ExtraErrorMessage;
		}

		private static readonly NamedValue[] DrawElementNamedArgs = new NamedValue[6]
		{
			new NamedValue("rect", typeof(Rect)),
			new NamedValue("element", typeof(TElement)),
			new NamedValue("value", typeof(TElement)),
			new NamedValue("array", typeof(TArray)),
			new NamedValue("x", typeof(int)),
			new NamedValue("y", typeof(int))
		};

		private Context context;

		/// <summary>
		/// Not yet documented.
		/// </summary>
		protected TableMatrixAttribute TableMatrixAttribute { get; private set; }

		/// <summary>
		/// <para>Override this method in order to define custom type constraints to specify whether or not a type should be drawn by the drawer.</para>
		/// <para>Note that Odin's <see cref="!:DrawerLocator" /> has full support for generic class constraints, so most often you can get away with not overriding CanDrawTypeFilter.</para>
		/// </summary>
		public override bool CanDrawTypeFilter(Type type)
		{
			if (type.IsArray && type.GetArrayRank() == 2)
			{
				return type.GetElementType() == typeof(TElement);
			}
			return false;
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		protected virtual TableMatrixAttribute GetDefaultTableMatrixAttributeSettings()
		{
			return new TableMatrixAttribute();
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			TElement[,] value = base.ValueEntry.Values[0] as TElement[,];
			bool rowLengthConflict = false;
			bool colLengthConflict = false;
			TableMatrixAttribute attribute = base.ValueEntry.Property.GetAttribute<TableMatrixAttribute>() ?? GetDefaultTableMatrixAttributeSettings();
			int colIndex = (attribute.Transpose ? 1 : 0);
			int rowIndex = 1 - colIndex;
			int colCount = value.GetLength(colIndex);
			int rowCount = value.GetLength(rowIndex);
			for (int i = 1; i < base.ValueEntry.Values.Count; i++)
			{
				TElement[,] arr = base.ValueEntry.Values[i] as TElement[,];
				colLengthConflict = colLengthConflict || arr.GetLength(colIndex) != colCount;
				rowLengthConflict = rowLengthConflict || arr.GetLength(rowIndex) != rowCount;
				colCount = Mathf.Min(colCount, arr.GetLength(colIndex));
				rowCount = Mathf.Min(rowCount, arr.GetLength(rowIndex));
			}
			if (context == null || colCount != context.ColCount || rowCount != context.RowCount)
			{
				context = new Context();
				context.Value = value;
				context.ColCount = colCount;
				context.RowCount = rowCount;
				context.Attribute = attribute;
				if (context.Attribute.DrawElementMethod != null)
				{
					context.DrawElement = ValueResolver.Get<TElement>(base.Property, context.Attribute.DrawElementMethod, DrawElementNamedArgs);
				}
				context.HorizontalTitleGetter = ValueResolver.GetForString(base.Property, context.Attribute.HorizontalTitle);
				context.VerticalTitleGetter = ValueResolver.GetForString(base.Property, context.Attribute.VerticalTitle);
				context.LabelGetter = ValueResolver.Get<(string, LabelDirection)>(base.Property, context.Attribute.Labels, new NamedValue[3]
				{
					new NamedValue("array", typeof(TArray), context.Value),
					new NamedValue("index", typeof(int)),
					new NamedValue("axis", typeof(TableAxis))
				});
				context.Table = GUITable.Create(Mathf.Max(colCount, 1) + (colLengthConflict ? 1 : 0), Mathf.Max(rowCount, 1) + (rowLengthConflict ? 1 : 0), delegate(Rect rect, int x3, int y2)
				{
					DrawElement(rect, base.ValueEntry, context, x3, y2);
				}, context.HorizontalTitleGetter.GetValue(), context.Attribute.HideColumnIndices ? null : ((Action<Rect, int>)delegate(Rect rect, int columnIndex)
				{
					DrawColumn(rect, base.ValueEntry, context, columnIndex);
				}), context.VerticalTitleGetter.GetValue(), context.Attribute.HideRowIndices ? null : ((Action<Rect, int>)delegate(Rect rect, int rowIndex2)
				{
					DrawRows(rect, base.ValueEntry, context, rowIndex2);
				}), context.Attribute.ResizableColumns);
				context.Table.RespectIndentLevel = context.Attribute.RespectIndentLevel;
				if (context.Attribute.RowHeight != 0)
				{
					for (int y = 0; y < context.RowCount; y++)
					{
						int _y = context.Table.RowCount - 1 - y;
						for (int x = 0; x < context.Table.ColumnCount; x++)
						{
							GUITableCell cell = context.Table[x, _y];
							if (cell != null)
							{
								cell.Height = context.Attribute.RowHeight;
							}
						}
					}
				}
				if (colLengthConflict)
				{
					context.Table[context.Table.ColumnCount - 1, 1].Width = 15f;
				}
				if (colLengthConflict)
				{
					for (int x2 = 0; x2 < context.Table.ColumnCount; x2++)
					{
						context.Table[x2, context.Table.RowCount - 1].Height = 15f;
					}
				}
			}
			if (context.Attribute.SquareCells)
			{
				SetSquareRowHeights(context);
			}
			TableMatrixAttribute = context.Attribute;
			context.Value = value;
			bool prev = EditorGUI.showMixedValue;
			OnBeforeDrawTable(base.ValueEntry, context, label);
			ValueResolver.DrawErrors(context.DrawElement, context.HorizontalTitleGetter, context.VerticalTitleGetter, context.LabelGetter);
			if (context.ExtraErrorMessage != null)
			{
				SirenixEditorGUI.MessageBox(context.ExtraErrorMessage, MessageType.Error, GlobalConfig<GeneralDrawerConfig>.Instance.MessageBoxFontSize);
			}
			if (context.DrawElement == null || !context.DrawElement.HasError)
			{
				try
				{
					context.Table.DrawTable();
					GUILayout.Space(3f);
				}
				catch (ExitGUIException ex)
				{
					throw ex;
				}
				catch (Exception ex2)
				{
					if (ex2.IsExitGUIException())
					{
						throw ex2.AsExitGUIException();
					}
					Debug.LogException(ex2);
				}
			}
			EditorGUI.showMixedValue = prev;
		}

		private static void SetSquareRowHeights(Context context)
		{
			if (context.ColCount == 1)
			{
				float lastWidth = context.Table[context.Table.ColumnCount - 1, context.Table.RowCount - 1].Rect.width;
				for (int y = 0; y < context.RowCount; y++)
				{
					int _y = context.Table.RowCount - 1 - y;
					for (int x = 0; x < context.Table.ColumnCount; x++)
					{
						GUITableCell cell = context.Table[x, _y];
						if (cell != null)
						{
							cell.Height = lastWidth;
						}
					}
				}
				context.Table.ReCalculateSizes();
				GUIHelper.RequestRepaint();
			}
			else
			{
				if (context.ColCount <= 0 || context.RowCount <= 0)
				{
					return;
				}
				int offsetY = ((context.HorizontalTitleGetter.GetValue() == null) ? 1 : 0);
				GUITableCell lastCell = context.Table[context.ColCount - 1, context.RowCount - offsetY];
				if (lastCell == null || !(Mathf.Abs(lastCell.Rect.height - lastCell.Rect.width) > 0f))
				{
					return;
				}
				for (int i = 0; i < context.RowCount; i++)
				{
					int _y2 = context.Table.RowCount - 1 - i;
					for (int j = 0; j < context.Table.ColumnCount; j++)
					{
						GUITableCell cell2 = context.Table[j, _y2];
						if (cell2 != null)
						{
							cell2.Height = lastCell.Rect.width;
						}
					}
				}
				context.Table.ReCalculateSizes();
				GUIHelper.RequestRepaint();
			}
		}

		/// <summary>
		/// This method gets called from DrawPropertyLayout right before the table and error message is drawn.
		/// </summary>
		protected internal virtual void OnBeforeDrawTable(IPropertyValueEntry<TArray> entry, Context value, GUIContent label)
		{
		}

		private void DrawRows(Rect rect, IPropertyValueEntry<TArray> entry, Context context, int rowIndex)
		{
			if (rowIndex < context.RowCount)
			{
				context.LabelGetter.Context.NamedValues.Set("index", rowIndex);
				context.LabelGetter.Context.NamedValues.Set("axis", TableAxis.Y);
				var (label, rowLabelDirection) = context.LabelGetter.GetValue();
				if (label == null)
				{
					label = rowIndex.ToString();
				}
				if (rowLabelDirection == LabelDirection.LeftToRight)
				{
					GUI.Label(rect, GUIHelper.TempContent(label, label), SirenixGUIStyles.LabelCentered);
				}
				else
				{
					float width = rect.width;
					float height = rect.height;
					ref Rect reference = ref rect;
					ref Rect reference2 = ref rect;
					float width2 = height;
					float height2 = width;
					reference.width = width2;
					reference2.height = height2;
					Matrix4x4 oldMatrix = GUI.matrix;
					switch (rowLabelDirection)
					{
					case LabelDirection.TopToBottom:
						rect = rect.AddX(width);
						GUIUtility.RotateAroundPivot(90f, rect.position);
						break;
					case LabelDirection.BottomToTop:
						rect = rect.AddY(height);
						GUIUtility.RotateAroundPivot(270f, rect.position);
						break;
					}
					GUI.Label(rect.HorizontalPadding(3f), GUIHelper.TempContent(label, label), SirenixGUIStyles.LabelCentered);
					GUI.matrix = oldMatrix;
				}
				if (!context.Attribute.IsReadOnly)
				{
					int id = GUIUtility.GetControlID(FocusType.Passive);
					if (GUI.enabled && Event.current.type == EventType.MouseDown && Event.current.button == 0 && rect.Contains(Event.current.mousePosition))
					{
						GUIHelper.RemoveFocusControl();
						GUIUtility.hotControl = id;
						EditorGUIUtility.SetWantsMouseJumping(1);
						Event.current.Use();
						context.RowDragFrom = rowIndex;
						context.RowDragTo = rowIndex;
						context.dragStartPos = Event.current.mousePosition;
					}
					else if (GUIUtility.hotControl == id)
					{
						if ((context.dragStartPos - Event.current.mousePosition).sqrMagnitude > 25f)
						{
							context.IsDraggingRow = true;
						}
						if (Event.current.type == EventType.MouseDrag)
						{
							Event.current.Use();
						}
						else if (Event.current.type == EventType.MouseUp)
						{
							GUIUtility.hotControl = 0;
							EditorGUIUtility.SetWantsMouseJumping(0);
							Event.current.Use();
							context.IsDraggingRow = false;
							if (context.Attribute.Transpose)
							{
								ApplyArrayModifications(entry, (TElement[,] arr) => MultiDimArrayUtilities.MoveColumn(arr, context.RowDragFrom, context.RowDragTo));
							}
							else
							{
								ApplyArrayModifications(entry, (TElement[,] arr) => MultiDimArrayUtilities.MoveRow(arr, context.RowDragFrom, context.RowDragTo));
							}
						}
					}
					if (context.IsDraggingRow && Event.current.type == EventType.Repaint)
					{
						float mouseY = Event.current.mousePosition.y;
						if (mouseY > rect.y - 1f && mouseY < rect.y + rect.height + 1f)
						{
							Rect arrowRect;
							if (mouseY > rect.y + rect.height * 0.5f)
							{
								arrowRect = rect.AlignBottom(16f);
								arrowRect.width = 16f;
								arrowRect.y += 8f;
								arrowRect.x -= 13f;
								context.RowDragTo = rowIndex;
							}
							else
							{
								arrowRect = rect.AlignTop(16f);
								arrowRect.width = 16f;
								arrowRect.y -= 8f;
								arrowRect.x -= 13f;
								context.RowDragTo = rowIndex - 1;
							}
							entry.Property.Tree.DelayActionUntilRepaint(delegate
							{
								GUI.DrawTexture(arrowRect, EditorIcons.ArrowRight.Active);
								Rect rect2 = arrowRect;
								rect2.y = rect2.center.y - 2f + 1f;
								rect2.height = 3f;
								rect2.x += 14f;
								rect2.xMax = context.Table.TableRect.xMax;
								EditorGUI.DrawRect(rect2, new Color(0f, 0f, 0f, 0.6f));
							});
						}
						if (rowIndex == context.RowCount - 1)
						{
							entry.Property.Tree.DelayActionUntilRepaint(delegate
							{
								GUITableCell gUITableCell = context.Table[context.Table.ColumnCount - 1, context.Table.RowCount - context.RowCount + context.RowDragFrom];
								Rect rect2 = gUITableCell.Rect;
								rect2.xMin = rect.xMin;
								SirenixEditorGUI.DrawSolidRect(rect2, new Color(0f, 0f, 0f, 0.2f));
							});
						}
					}
				}
			}
			else
			{
				GUI.Label(rect, "...", EditorStyles.centeredGreyMiniLabel);
			}
			if (context.Attribute.IsReadOnly || Event.current.type != EventType.MouseDown || Event.current.button != 1 || !rect.Contains(Event.current.mousePosition))
			{
				return;
			}
			Event.current.Use();
			GenericMenu menu = new GenericMenu();
			menu.AddItem(new GUIContent("Insert 1 above"), on: false, delegate
			{
				ApplyArrayModifications(entry, (TElement[,] arr) => (!TableMatrixAttribute.Transpose) ? MultiDimArrayUtilities.InsertOneRowAbove(arr, rowIndex) : MultiDimArrayUtilities.InsertOneColumnLeft(arr, rowIndex));
			});
			menu.AddItem(new GUIContent("Insert 1 below"), on: false, delegate
			{
				ApplyArrayModifications(entry, (TElement[,] arr) => (!TableMatrixAttribute.Transpose) ? MultiDimArrayUtilities.InsertOneRowBelow(arr, rowIndex) : MultiDimArrayUtilities.InsertOneColumnRight(arr, rowIndex));
			});
			menu.AddItem(new GUIContent("Duplicate"), on: false, delegate
			{
				ApplyArrayModifications(entry, (TElement[,] arr) => (!TableMatrixAttribute.Transpose) ? MultiDimArrayUtilities.DuplicateRow(arr, rowIndex) : MultiDimArrayUtilities.DuplicateColumn(arr, rowIndex));
			});
			menu.AddSeparator("");
			menu.AddItem(new GUIContent("Delete"), on: false, delegate
			{
				ApplyArrayModifications(entry, (TElement[,] arr) => (!TableMatrixAttribute.Transpose) ? MultiDimArrayUtilities.DeleteRow(arr, rowIndex) : MultiDimArrayUtilities.DeleteColumn(arr, rowIndex));
			});
			menu.ShowAsContext();
		}

		private void DrawColumn(Rect rect, IPropertyValueEntry<TArray> entry, Context context, int columnIndex)
		{
			if (columnIndex < context.ColCount)
			{
				context.LabelGetter.Context.NamedValues.Set("index", columnIndex);
				context.LabelGetter.Context.NamedValues.Set("axis", TableAxis.X);
				string label = context.LabelGetter.GetValue().Item1;
				if (label == null)
				{
					label = columnIndex.ToString();
				}
				GUI.Label(rect.HorizontalPadding(3f), GUIHelper.TempContent(label, label), SirenixGUIStyles.LabelCentered);
				if (!context.Attribute.IsReadOnly)
				{
					int id = GUIUtility.GetControlID(FocusType.Passive);
					if (GUI.enabled && Event.current.type == EventType.MouseDown && Event.current.button == 0 && rect.Contains(Event.current.mousePosition))
					{
						GUIHelper.RemoveFocusControl();
						GUIUtility.hotControl = id;
						EditorGUIUtility.SetWantsMouseJumping(1);
						Event.current.Use();
						context.ColumnDragFrom = columnIndex;
						context.ColumnDragTo = columnIndex;
						context.dragStartPos = Event.current.mousePosition;
					}
					else if (GUIUtility.hotControl == id)
					{
						if ((context.dragStartPos - Event.current.mousePosition).sqrMagnitude > 25f)
						{
							context.IsDraggingColumn = true;
						}
						if (Event.current.type == EventType.MouseDrag)
						{
							Event.current.Use();
						}
						else if (Event.current.type == EventType.MouseUp)
						{
							GUIUtility.hotControl = 0;
							EditorGUIUtility.SetWantsMouseJumping(0);
							Event.current.Use();
							context.IsDraggingColumn = false;
							if (context.Attribute.Transpose)
							{
								ApplyArrayModifications(entry, (TElement[,] arr) => MultiDimArrayUtilities.MoveRow(arr, context.ColumnDragFrom, context.ColumnDragTo));
							}
							else
							{
								ApplyArrayModifications(entry, (TElement[,] arr) => MultiDimArrayUtilities.MoveColumn(arr, context.ColumnDragFrom, context.ColumnDragTo));
							}
						}
					}
					if (context.IsDraggingColumn && Event.current.type == EventType.Repaint)
					{
						float mouseX = Event.current.mousePosition.x;
						if (mouseX > rect.x - 1f && mouseX < rect.x + rect.width + 1f)
						{
							Rect arrowRect;
							if (mouseX > rect.x + rect.width * 0.5f)
							{
								arrowRect = rect.AlignRight(16f);
								arrowRect.height = 16f;
								arrowRect.y -= 13f;
								arrowRect.x += 8f;
								context.ColumnDragTo = columnIndex;
							}
							else
							{
								arrowRect = rect.AlignLeft(16f);
								arrowRect.height = 16f;
								arrowRect.y -= 13f;
								arrowRect.x -= 8f;
								context.ColumnDragTo = columnIndex - 1;
							}
							entry.Property.Tree.DelayActionUntilRepaint(delegate
							{
								GUI.DrawTexture(arrowRect, EditorIcons.ArrowDown.Active);
								Rect rect2 = arrowRect;
								rect2.x = rect2.center.x - 2f + 1f;
								rect2.width = 3f;
								rect2.y += 14f;
								rect2.yMax = context.Table.TableRect.yMax;
								EditorGUI.DrawRect(rect2, new Color(0f, 0f, 0f, 0.6f));
							});
						}
						if (columnIndex == context.ColCount - 1)
						{
							entry.Property.Tree.DelayActionUntilRepaint(delegate
							{
								GUITableCell gUITableCell = context.Table[context.Table.ColumnCount - context.ColCount + context.ColumnDragFrom, context.Table.RowCount - 1];
								Rect rect2 = gUITableCell.Rect;
								rect2.yMin = rect.yMin;
								SirenixEditorGUI.DrawSolidRect(rect2, new Color(0f, 0f, 0f, 0.2f));
							});
						}
					}
				}
			}
			else
			{
				GUI.Label(rect, "-", EditorStyles.centeredGreyMiniLabel);
			}
			if (context.Attribute.IsReadOnly || Event.current.type != EventType.MouseDown || Event.current.button != 1 || !rect.Contains(Event.current.mousePosition))
			{
				return;
			}
			Event.current.Use();
			GenericMenu menu = new GenericMenu();
			menu.AddItem(new GUIContent("Insert 1 left"), on: false, delegate
			{
				ApplyArrayModifications(entry, (TElement[,] arr) => (!TableMatrixAttribute.Transpose) ? MultiDimArrayUtilities.InsertOneColumnLeft(arr, columnIndex) : MultiDimArrayUtilities.InsertOneRowAbove(arr, columnIndex));
			});
			menu.AddItem(new GUIContent("Insert 1 right"), on: false, delegate
			{
				ApplyArrayModifications(entry, (TElement[,] arr) => (!TableMatrixAttribute.Transpose) ? MultiDimArrayUtilities.InsertOneColumnRight(arr, columnIndex) : MultiDimArrayUtilities.InsertOneRowBelow(arr, columnIndex));
			});
			menu.AddItem(new GUIContent("Duplicate"), on: false, delegate
			{
				ApplyArrayModifications(entry, (TElement[,] arr) => (!TableMatrixAttribute.Transpose) ? MultiDimArrayUtilities.DuplicateColumn(arr, columnIndex) : MultiDimArrayUtilities.DuplicateRow(arr, columnIndex));
			});
			menu.AddSeparator("");
			menu.AddItem(new GUIContent("Delete"), on: false, delegate
			{
				ApplyArrayModifications(entry, (TElement[,] arr) => (!TableMatrixAttribute.Transpose) ? MultiDimArrayUtilities.DeleteColumn(arr, columnIndex) : MultiDimArrayUtilities.DeleteRow(arr, columnIndex));
			});
			menu.ShowAsContext();
		}

		private void ApplyArrayModifications(IPropertyValueEntry<TArray> entry, Func<TElement[,], TElement[,]> modification)
		{
			for (int i = 0; i < entry.Values.Count; i++)
			{
				int localI = i;
				TElement[,] newArr = modification(entry.Values[localI] as TElement[,]);
				entry.Property.Tree.DelayActionUntilRepaint(delegate
				{
					entry.Values[localI] = (TArray)(object)newArr;
				});
			}
		}

		private void DrawElement(Rect rect, IPropertyValueEntry<TArray> entry, Context context, int x, int y)
		{
			if (x >= context.ColCount || y >= context.RowCount)
			{
				return;
			}
			int row = (context.Attribute.Transpose ? x : y);
			int col = (context.Attribute.Transpose ? y : x);
			bool showMixedValue = false;
			if (entry.Values.Count != 1)
			{
				for (int i = 1; i < entry.Values.Count; i++)
				{
					TElement a = (entry.Values[i] as TElement[,])[col, row];
					TElement b = (entry.Values[i - 1] as TElement[,])[col, row];
					if (!CompareElement(a, b))
					{
						showMixedValue = true;
						break;
					}
				}
			}
			EditorGUI.showMixedValue = showMixedValue;
			EditorGUI.BeginChangeCheck();
			TElement prevValue = context.Value[col, row];
			TElement value;
			if (context.DrawElement != null)
			{
				context.DrawElement.Context.NamedValues.Set("rect", rect);
				context.DrawElement.Context.NamedValues.Set("element", prevValue);
				context.DrawElement.Context.NamedValues.Set("value", prevValue);
				context.DrawElement.Context.NamedValues.Set("array", context.Value);
				context.DrawElement.Context.NamedValues.Set("x", x);
				context.DrawElement.Context.NamedValues.Set("y", y);
				value = context.DrawElement.GetValue();
			}
			else
			{
				value = DrawElement(rect, prevValue);
			}
			if (EditorGUI.EndChangeCheck())
			{
				for (int j = 0; j < entry.Values.Count; j++)
				{
					(entry.Values[j] as TElement[,])[col, row] = value;
				}
				entry.Values.ForceMarkDirty();
			}
		}

		/// <summary>
		/// Compares the element.
		/// </summary>
		protected virtual bool CompareElement(TElement a, TElement b)
		{
			return EqualityComparer<TElement>.Default.Equals(a, b);
		}

		/// <summary>
		/// Draws a table cell element.
		/// </summary>
		/// <param name="rect">The rect.</param>
		/// <param name="value">The input value.</param>
		/// <returns>The output value.</returns>
		protected abstract TElement DrawElement(Rect rect, TElement value);
	}
}
