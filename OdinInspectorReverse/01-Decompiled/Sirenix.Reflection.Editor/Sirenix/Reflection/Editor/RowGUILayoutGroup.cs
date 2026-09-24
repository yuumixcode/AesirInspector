using UnityEngine;

namespace Sirenix.Reflection.Editor
{
	internal class RowGUILayoutGroup : UnityEngine.GUILayoutGroup
	{
		public RowGUILayoutGroup()
		{
			isVertical = false;
			stretchWidth = 1;
			consideredForMargin = true;
		}

		public override void SetVertical(float y, float height)
		{
			base.SetVertical(y, height);
		}

		public override void SetHorizontal(float x, float width)
		{
			rect.x = x;
			rect.width = width;
			maxWidth = width;
			minWidth = width;
			float remaining = width;
			float autoCount = 0f;
			for (int i = 0; i < entries.Count; i++)
			{
				ColumnGUILayoutGroup col = (ColumnGUILayoutGroup)entries[i];
				if (col.LayoutSize.Type == SizeMode.Pixels)
				{
					remaining -= Mathf.Clamp(col.LayoutSize.Value, col.MinColWidth.GetSizeInPixels(width), col.MaxColWidth.GetSizeInPixels(width));
				}
				else if (col.LayoutSize.Type == SizeMode.Percentage)
				{
					remaining -= Mathf.Clamp(col.LayoutSize.Value * width, col.MinColWidth.GetSizeInPixels(width), col.MaxColWidth.GetSizeInPixels(width));
				}
				else
				{
					autoCount += 1f;
				}
			}
			float autoWidth = 0f;
			if (remaining > 0f && autoCount > 0f)
			{
				autoWidth = remaining / autoCount;
			}
			for (int j = 0; j < entries.Count; j++)
			{
				ColumnGUILayoutGroup col2 = (ColumnGUILayoutGroup)entries[j];
				float w = ((col2.LayoutSize.Type != SizeMode.Pixels) ? ((col2.LayoutSize.Type != SizeMode.Percentage) ? autoWidth : Mathf.Clamp(col2.LayoutSize.Value * width, col2.MinColWidth.GetSizeInPixels(width), col2.MaxColWidth.GetSizeInPixels(width))) : Mathf.Clamp(col2.LayoutSize.Value, col2.MinColWidth.GetSizeInPixels(width), col2.MaxColWidth.GetSizeInPixels(width)));
				col2.SetHorizontal(x, w);
				x += w;
			}
		}
	}
}
