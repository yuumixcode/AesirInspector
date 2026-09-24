using System;
using Sirenix.Reflection.Editor;
using UnityEngine;

namespace Sirenix.Utilities.Editor
{
	public static class GUILayoutUtilityCalcHeightBasedOnWidthLayoutEntry
	{
		public static Rect GetRect(Func<float, float> calcHeight)
		{
			EventType e = Event.current.type;
			if (e == EventType.Layout)
			{
				GUILayoutUtility_Internals.TopLevel.Add(Create(calcHeight));
				return default(Rect);
			}
			_ = 12;
			return GUILayoutUtility_Internals.TopLevel.GetNext().rect;
		}

		private static GUILayoutEntry_Internal<Func<float, float>> Create(Func<float, float> element)
		{
			return GUILayoutEntry_Internal<Func<float, float>>.CreateCustom(element, SetVertical, SetHorizontal, CalcWidth, CalcHeight);
		}

		private static void CalcWidth(ref GUILayoutEntry_Internal<Func<float, float>> entry)
		{
		}

		private static void CalcHeight(ref GUILayoutEntry_Internal<Func<float, float>> entry)
		{
		}

		private static void SetVertical(ref GUILayoutEntry_Internal<Func<float, float>> entry, float y, float height)
		{
			entry.rect.y = y;
			Update(ref entry);
		}

		private static void SetHorizontal(ref GUILayoutEntry_Internal<Func<float, float>> entry, float x, float width)
		{
			entry.rect.x = x;
			entry.minWidth = width;
			entry.maxWidth = width;
			Update(ref entry);
		}

		private static void Update(ref GUILayoutEntry_Internal<Func<float, float>> entry)
		{
			float size = entry.Value(entry.minWidth);
			entry.maxHeight = size;
			entry.minHeight = size;
			entry.rect.width = entry.minWidth;
			entry.rect.height = entry.minHeight;
		}
	}
}
