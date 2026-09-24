using UnityEngine;

namespace Sirenix.Reflection.Editor
{
	internal class ColumnGUILayoutGroup : UnityEngine.GUILayoutGroup
	{
		public LayoutSize LayoutSize;

		public LayoutSize MinColWidth;

		public LayoutSize MaxColWidth;

		public Vector2Int HorizontalPadding;

		public ColumnGUILayoutGroup()
		{
			isVertical = true;
			consideredForMargin = true;
		}

		public ColumnGUILayoutGroup(LayoutSize size, Vector2Int horizontalPadding, LayoutSize? minWidth, LayoutSize? maxWidth)
		{
			LayoutSize = size;
			MinColWidth = minWidth ?? LayoutSize.Pixels(0f);
			MaxColWidth = maxWidth ?? LayoutSize.Percentage(1f);
			HorizontalPadding = horizontalPadding;
			consideredForMargin = true;
		}

		public override void CalcHeight()
		{
			foreach (UnityEngine.GUILayoutEntry item in entries)
			{
				item.consideredForMargin = true;
			}
			base.CalcHeight();
		}

		public override void SetVertical(float y, float height)
		{
			base.SetVertical(y, height);
		}

		public override void SetHorizontal(float x, float width)
		{
			x += (float)HorizontalPadding.x;
			width -= (float)(HorizontalPadding.x + HorizontalPadding.y);
			maxWidth = width;
			minWidth = width;
			rect.x = x;
			rect.width = width;
			foreach (UnityEngine.GUILayoutEntry item in entries)
			{
				item.SetHorizontal(x, width);
			}
		}
	}
}
