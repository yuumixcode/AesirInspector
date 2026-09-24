using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	public static class GUITableUtilities
	{
		public static void ResizeColumns<T>(Rect rect, IList<T> columns) where T : IResizableColumn
		{
			int colCount = columns.Count;
			if (colCount == 0)
			{
				return;
			}
			if (Event.current.type != EventType.Layout && rect.width > 1.001f)
			{
				for (int i = 0; i < 2; i++)
				{
					float currWidth = columns.Sum((T x) => x.ColWidth);
					float delta = rect.width - currWidth;
					DistributeDelta(columns, delta, 0, columns.Count, push: false);
				}
			}
			Rect tmpRect = rect;
			for (int i2 = 0; i2 < colCount; i2++)
			{
				T col = columns[i2];
				tmpRect.width = col.ColWidth;
				tmpRect.x += tmpRect.width;
				if (i2 == colCount - 1 || !CanResize(columns, i2))
				{
					continue;
				}
				Rect slideRect = tmpRect;
				slideRect.x -= 4f;
				slideRect.width = 8f;
				GUIHelper.PushGUIEnabled(enabled: true);
				float delta2 = SirenixEditorGUI.SlideRect(slideRect).x;
				GUIHelper.PopGUIEnabled();
				T val;
				if (delta2 < 0f)
				{
					delta2 = Mathf.Abs(delta2);
					T nextCol = columns[i2 + 1];
					ref T reference = ref nextCol;
					ref T reference2 = ref reference;
					val = default(T);
					if (val == null)
					{
						val = reference2;
						reference2 = ref val;
					}
					float colWidth = reference.ColWidth + delta2;
					reference2.ColWidth = colWidth;
					float overflow = DistributeDelta(columns, 0f - delta2, i2, 0, push: true);
					reference = ref nextCol;
					ref T reference3 = ref reference;
					val = default(T);
					if (val == null)
					{
						val = reference3;
						reference3 = ref val;
					}
					float colWidth2 = reference.ColWidth + overflow;
					reference3.ColWidth = colWidth2;
				}
				else if (delta2 > 0f)
				{
					ref T reference = ref col;
					ref T reference4 = ref reference;
					val = default(T);
					if (val == null)
					{
						val = reference4;
						reference4 = ref val;
					}
					float colWidth3 = reference.ColWidth + delta2;
					reference4.ColWidth = colWidth3;
					float overflow2 = DistributeDelta(columns, 0f - delta2, i2 + 1, colCount, push: true);
					reference = ref col;
					ref T reference5 = ref reference;
					val = default(T);
					if (val == null)
					{
						val = reference5;
						reference5 = ref val;
					}
					float colWidth4 = reference.ColWidth + overflow2;
					reference5.ColWidth = colWidth4;
				}
			}
		}

		private static bool CanResize<T>(IList<T> columns, int index) where T : IResizableColumn
		{
			if (index == columns.Count - 1)
			{
				return columns[index].Resizable;
			}
			if (columns[index].Resizable)
			{
				return columns[index + 1].Resizable;
			}
			return false;
		}

		private static float DistributeDelta<T>(IList<T> columns, float delta, int from, int to, bool push) where T : IResizableColumn
		{
			bool ltr = from < to;
			int incriment = (ltr ? 1 : (-1));
			float distribute = 0f;
			float pushBuffer = 0f;
			if (push)
			{
				pushBuffer = delta;
			}
			else
			{
				int adjustableColCount = 0;
				for (int i = from; ltr ? (i < to) : (i >= to); i += incriment)
				{
					if (!columns[i].PreserveWidth)
					{
						adjustableColCount++;
					}
				}
				distribute = delta / (float)adjustableColCount;
			}
			for (int j = from; ltr ? (j < to) : (j >= to); j += incriment)
			{
				T col = columns[j];
				float currWidth = col.ColWidth;
				if (!col.PreserveWidth)
				{
					currWidth += distribute;
				}
				else if (currWidth < col.MinWidth)
				{
					currWidth = col.MinWidth;
				}
				currWidth += pushBuffer;
				if (currWidth < col.MinWidth)
				{
					pushBuffer = currWidth - col.MinWidth;
					currWidth = col.MinWidth;
				}
				else
				{
					pushBuffer = 0f;
				}
				col.ColWidth = currWidth;
			}
			return pushBuffer;
		}

		public static void DrawColumnHeaderSeperators<T>(Rect rect, IList<T> columns, Color color) where T : IResizableColumn
		{
			if (columns == null || Event.current.type != EventType.Repaint || columns.Count == 0)
			{
				return;
			}
			float xMax = rect.xMax;
			rect.x = (int)rect.x;
			rect.width = 1f;
			rect.x -= 1f;
			for (int i = 0; i < columns.Count - 1; i++)
			{
				rect.x += columns[i].ColWidth;
				if (rect.x > xMax)
				{
					break;
				}
				EditorGUI.DrawRect(rect, color);
			}
		}
	}
}
