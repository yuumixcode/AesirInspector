using UnityEngine;

namespace Sirenix.Reflection.Editor
{
	internal class SmartRowGUILayoutGroup : UnityEngine.GUILayoutGroup
	{
		public SmartRowGUILayoutGroup()
		{
			isVertical = false;
			stretchWidth = 1;
			consideredForMargin = true;
		}

		public override void SetHorizontal(float x, float width)
		{
			rect.x = x;
			rect.width = width;
			minWidth = width;
			maxWidth = width;
			float widthAfterPixels = width;
			float percentageTaken = 0f;
			int autoCount = 0;
			int lastPercentIndex = -1;
			for (int i = 0; i < entries.Count; i++)
			{
				ColumnGUILayoutGroup column = (ColumnGUILayoutGroup)entries[i];
				switch (column.LayoutSize.Type)
				{
				case SizeMode.Auto:
					autoCount++;
					break;
				case SizeMode.Pixels:
					widthAfterPixels -= column.LayoutSize.Value;
					break;
				case SizeMode.Percentage:
					percentageTaken += column.LayoutSize.Value;
					lastPercentIndex = i;
					break;
				}
			}
			if (widthAfterPixels <= 0f)
			{
				float remainder = width;
				for (int j = 0; j < entries.Count; j++)
				{
					ColumnGUILayoutGroup column2 = (ColumnGUILayoutGroup)entries[j];
					SizeMode type = column2.LayoutSize.Type;
					if (type == SizeMode.Pixels)
					{
						float pixels = column2.LayoutSize.Value;
						if (remainder <= 0f)
						{
							pixels = 0f;
						}
						else if (remainder - pixels <= 0f)
						{
							pixels = remainder;
						}
						column2.SetHorizontal(x, pixels);
						x += pixels;
						remainder -= pixels;
					}
					else
					{
						column2.SetHorizontal(x, 0f);
					}
				}
				return;
			}
			float widthAvailable = widthAfterPixels;
			if (percentageTaken >= 0.999f)
			{
				percentageTaken = 1f;
			}
			float percentageRemainder = widthAfterPixels * percentageTaken;
			widthAvailable -= percentageRemainder;
			float autoWidth = ((widthAvailable > 0f) ? (widthAvailable / (float)autoCount) : 0f);
			for (int k = 0; k < entries.Count; k++)
			{
				ColumnGUILayoutGroup column3 = (ColumnGUILayoutGroup)entries[k];
				float pixels2;
				switch (column3.LayoutSize.Type)
				{
				case SizeMode.Auto:
					pixels2 = autoWidth;
					break;
				case SizeMode.Pixels:
					pixels2 = column3.LayoutSize.Value;
					break;
				case SizeMode.Percentage:
					if (percentageRemainder <= 0f)
					{
						pixels2 = 0f;
					}
					else if (k == lastPercentIndex)
					{
						pixels2 = percentageRemainder;
					}
					else
					{
						pixels2 = widthAfterPixels * column3.LayoutSize.Value;
						if (percentageRemainder - pixels2 < 0f)
						{
							pixels2 = percentageRemainder;
						}
					}
					percentageRemainder -= pixels2;
					break;
				default:
					continue;
				}
				column3.SetHorizontal(x, pixels2);
				x += pixels2;
			}
		}
	}
}
