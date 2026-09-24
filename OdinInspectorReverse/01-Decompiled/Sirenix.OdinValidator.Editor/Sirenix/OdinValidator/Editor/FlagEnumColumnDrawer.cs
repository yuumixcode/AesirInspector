using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinValidator.Editor
{
	internal class FlagEnumColumnDrawer<TEnum>
	{
		public enum SortDirection
		{
			Ascending,
			Descending
		}

		private EditorPrefFloat[] columnSizes;

		private ResizableColumn[] resizableColumns;

		private TEnum dynamicWidthColumn;

		private Vector2[] columnPositions;

		public Dictionary<TEnum, int> FixedWidthColumns;

		public (TEnum, SortDirection) SortedColumn;

		public Func<TEnum, (TEnum, SortDirection)> SortColumn;

		private string name;

		public EditorPrefEnum<TEnum> Columns;

		public GUIStyle ColumnLabelStyle;

		public FlagEnumColumnDrawer(string name, TEnum @default, TEnum dynamicWidthColumn, Dictionary<TEnum, int> fixedWidthColumns)
		{
			this.name = name;
			Columns = new EditorPrefEnum<TEnum>("ODIN_VALIDATOR_" + name, @default);
			columnSizes = new EditorPrefFloat[EnumTypeUtilities<TEnum>.AllEnumMemberInfos.Length];
			columnPositions = new Vector2[EnumTypeUtilities<TEnum>.AllEnumMemberInfos.Length];
			resizableColumns = new ResizableColumn[columnSizes.Length];
			this.dynamicWidthColumn = dynamicWidthColumn;
			FixedWidthColumns = fixedWidthColumns ?? new Dictionary<TEnum, int>();
			InitializeColumns();
		}

		private void InitializeColumns()
		{
			string[] enumNames = Enum.GetNames(typeof(TEnum));
			for (int i = 0; i < columnSizes.Length; i++)
			{
				columnSizes[i] = new EditorPrefFloat("ODIN_VALIDATOR_" + name + enumNames[i], 100f);
			}
			TEnum[] enabledColumns = EnumTypeUtilities<TEnum>.DecomposeEnumFlagValues(Columns);
			resizableColumns = new ResizableColumn[enabledColumns.Length];
			for (int j = 0; j < resizableColumns.Length; j++)
			{
				TEnum column = enabledColumns[j];
				int index = EnumTypeUtilities<TEnum>.GetIndexOfEnumValue(column);
				float width = columnSizes[index].Value;
				if (FixedWidthColumns.TryGetValue(column, out var fixedWidth))
				{
					resizableColumns[j] = ResizableColumn.FixedColumn(fixedWidth);
				}
				else if (EqualityComparer<TEnum>.Default.Equals(column, dynamicWidthColumn))
				{
					resizableColumns[j] = ResizableColumn.DynamicColumn(width, 60f);
				}
				else
				{
					resizableColumns[j] = ResizableColumn.FlexibleColumn(width, 30f);
				}
			}
		}

		public void BeginDrawColumns(Rect columnHeaderRect)
		{
			TEnum[] columnFlags = EnumTypeUtilities<TEnum>.DecomposeEnumFlagValues(Columns);
			for (int i = 0; i < columnFlags.Length; i++)
			{
				ResizableColumn column = resizableColumns[i];
				columnSizes[EnumTypeUtilities<TEnum>.GetIndexOfEnumValue(columnFlags[i])].Value = column.ColWidth;
			}
			GUITableUtilities.ResizeColumns(columnHeaderRect, resizableColumns);
			GUIStyle labelStyle = ColumnLabelStyle ?? SirenixGUIStyles.LabelCentered;
			TEnum[] columns = EnumTypeUtilities<TEnum>.DecomposeEnumFlagValues(Columns);
			Rect rect = columnHeaderRect;
			for (int j = 0; j < columns.Length; j++)
			{
				TEnum colEnumVal = columns[j];
				string colName = colEnumVal.ToString();
				ResizableColumn col = resizableColumns[j];
				rect.width = col.ColWidth;
				int j2 = EnumTypeUtilities<TEnum>.GetIndexOfEnumValue(colEnumVal);
				columnPositions[j2].x = rect.x;
				columnPositions[j2].y = rect.width;
				if (GUI.Button(rect, GUIContent.none, GUIStyle.none))
				{
					if (Event.current.button == 0)
					{
						if (SortColumn != null)
						{
							SortedColumn = SortColumn(colEnumVal);
						}
					}
					else
					{
						EnumSelector<TEnum> selector = new EnumSelector<TEnum>();
						selector.SelectionTree.Config.DrawSearchToolbar = false;
						selector.SetSelection(Columns);
						selector.SelectionChanged += delegate(IEnumerable<TEnum> x)
						{
							Columns.Value = x.FirstOrDefault();
							InitializeColumns();
						};
						selector.ShowInPopup();
					}
				}
				if (EqualityComparer<TEnum>.Default.Equals(SortedColumn.Item1, colEnumVal))
				{
					GUI.Label(rect.HorizontalPadding(5f), ((SortedColumn.Item2 == SortDirection.Ascending) ? "▲ " : "▼ ") + colName, labelStyle);
				}
				else
				{
					GUI.Label(rect.HorizontalPadding(5f), colName, labelStyle);
				}
				rect.x += rect.width;
			}
			EditorGUI.DrawRect(columnHeaderRect.AlignBottom(1f), ValidatorGui.BorderColor);
			GUITableUtilities.DrawColumnHeaderSeperators(columnHeaderRect, resizableColumns, ValidatorGui.BorderColor);
		}

		public Rect GetColumnRect(Rect rect, TEnum column)
		{
			int index = EnumTypeUtilities<TEnum>.GetIndexOfEnumValue(column);
			rect.x = columnPositions[index].x;
			rect.width = columnPositions[index].y;
			return rect;
		}

		public void EndDrawColumns(Rect resultRect)
		{
			GUITableUtilities.DrawColumnHeaderSeperators(resultRect, resizableColumns, ValidatorGui.BorderColor);
		}
	}
}
