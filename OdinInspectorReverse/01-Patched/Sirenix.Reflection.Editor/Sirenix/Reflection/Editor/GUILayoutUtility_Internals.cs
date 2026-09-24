using System.Collections.Generic;
using UnityEngine;

namespace Sirenix.Reflection.Editor
{
	public static class GUILayoutUtility_Internals
	{
		public static class TopLevel
		{
			public static int Cursor => GUILayoutUtility.topLevel.m_Cursor;

			public static bool HasTopLevel => GUILayoutUtility.topLevel != null;

			public static Rect Rect => GUILayoutUtility.topLevel.rect;

			public static GUILayoutGroup_Internal TopLevelGroup => new GUILayoutGroup_Internal(GUILayoutUtility.topLevel);

			public static GUIStyle Style => GUILayoutUtility.topLevel.style;

			public static GUILayoutEntry_Internal GetNext()
			{
				return new GUILayoutEntry_Internal(GUILayoutUtility.topLevel.GetNext());
			}

			public static void Add<T>(GUILayoutEntry_Internal<T> e) where T : class
			{
				GUILayoutUtility.topLevel.Add(e.entry);
			}
		}

		public static class Current
		{
			public static IEnumerable<GUILayoutGroup_Internal> LayoutGroups
			{
				get
				{
					foreach (object item in GUILayoutUtility.current.layoutGroups)
					{
						if (item is UnityEngine.GUILayoutGroup group)
						{
							yield return new GUILayoutGroup_Internal(group);
						}
					}
				}
			}
		}

		public static Vector2 EditorScreenPointOffset
		{
			get
			{
				return GUIUtility.s_EditorScreenPointOffset;
			}
			set
			{
				GUIUtility.s_EditorScreenPointOffset = value;
			}
		}

		public static Rect MeasureLayout(int from, int to)
		{
			List<UnityEngine.GUILayoutEntry> entries = GUILayoutUtility.topLevel.entries;
			from = Mathf.Min(from, entries.Count - 1);
			to = Mathf.Min(to, entries.Count);
			if (from >= 0)
			{
				Rect rect = entries[from].rect;
				if (from == to)
				{
					rect.width = 0f;
					rect.height = 0f;
				}
				else
				{
					for (int i = from + 1; i < to; i++)
					{
						Rect tmpRect = entries[i].rect;
						rect.xMin = Mathf.Min(rect.xMin, tmpRect.xMin);
						rect.yMin = Mathf.Min(rect.yMin, tmpRect.yMin);
						rect.xMax = Mathf.Max(rect.xMax, tmpRect.xMax);
						rect.yMax = Mathf.Max(rect.yMax, tmpRect.yMax);
					}
				}
				return rect;
			}
			return new Rect(0f, 0f, 0f, 0f);
		}

		public static GUILayoutGroup_Internal BeginLayoutGroup(GUIStyle style, GUILayoutOption[] options)
		{
			return new GUILayoutGroup_Internal(GUILayoutUtility.BeginLayoutGroup(style, options, typeof(UnityEngine.GUILayoutGroup)));
		}
	}
}
