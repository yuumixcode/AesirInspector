using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinValidator.Editor
{
	internal struct LargeGuiCollectionHelper
	{
		public int ElementHeight;

		public Rect TotalRect;

		public Rect VisisbleRect;

		public int StartIndex;

		public int EndIndex;

		public Rect GetRect(int index)
		{
			return new Rect(TotalRect.x, TotalRect.y + (float)(index * ElementHeight), TotalRect.width, ElementHeight);
		}

		internal void AllocateLayout(int elementHeight, int elementCount)
		{
			ElementHeight = elementHeight;
			int totalHeight = elementHeight * elementCount;
			Rect r = GUILayoutUtility.GetRect(0f, totalHeight);
			if (Event.current.type == EventType.Repaint)
			{
				TotalRect = r;
				VisisbleRect = GUIClipInfo.VisibleRect;
			}
			float yOffset = VisisbleRect.y - TotalRect.y;
			StartIndex = (int)(yOffset / (float)ElementHeight) - 2;
			EndIndex = StartIndex + (int)((VisisbleRect.height - TotalRect.y) / (float)ElementHeight) + 6;
			StartIndex = Mathf.Clamp(StartIndex, 0, elementCount);
			EndIndex = Mathf.Clamp(EndIndex, 0, elementCount);
		}

		internal void ScrollTo(int selectedIndex, ref Vector2 scrollPos, float padding = 0f)
		{
			Rect rect = GetRect(selectedIndex);
			Rect visibleRect = VisisbleRect;
			if (visibleRect.y > rect.y - padding)
			{
				scrollPos.y = rect.y - padding;
			}
			if (visibleRect.yMax < rect.yMax + padding)
			{
				scrollPos.y = rect.yMax + padding - visibleRect.height;
			}
		}
	}
}
