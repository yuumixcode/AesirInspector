using UnityEngine;

namespace Sirenix.Reflection.Editor
{
	public class GUILayout_Internal
	{
		public static Rect BeginRow()
		{
			EventType type = Event.current.type;
			RowGUILayoutGroup gUILayoutGroup;
			if (type == EventType.Layout || type == EventType.Used)
			{
				gUILayoutGroup = new RowGUILayoutGroup();
				GUILayoutUtility.current.topLevel.Add(gUILayoutGroup);
			}
			else
			{
				gUILayoutGroup = GUILayoutUtility.current.topLevel.GetNext() as RowGUILayoutGroup;
				if (gUILayoutGroup == null)
				{
					throw new ExitGUIException("GUILayout: Mismatched LayoutGroup." + Event.current.type);
				}
				gUILayoutGroup.ResetCursor();
			}
			GUILayoutUtility.current.layoutGroups.Push(gUILayoutGroup);
			GUILayoutUtility.current.topLevel = gUILayoutGroup;
			return gUILayoutGroup.rect;
		}

		public static Rect BeginSmartRow()
		{
			EventType type = Event.current.type;
			SmartRowGUILayoutGroup layoutGroup;
			if (type == EventType.Layout || type == EventType.Used)
			{
				layoutGroup = new SmartRowGUILayoutGroup();
				GUILayoutUtility.current.topLevel.Add(layoutGroup);
			}
			else
			{
				layoutGroup = GUILayoutUtility.current.topLevel.GetNext() as SmartRowGUILayoutGroup;
				if (layoutGroup == null)
				{
					throw new ExitGUIException("GUILayout: Mismatched LayoutGroup." + Event.current.type);
				}
				layoutGroup.ResetCursor();
			}
			GUILayoutUtility.current.layoutGroups.Push(layoutGroup);
			GUILayoutUtility.current.topLevel = layoutGroup;
			return layoutGroup.rect;
		}

		public static Rect ColumnSpace(LayoutSize size)
		{
			BeginColumn(size);
			return EndColumn();
		}

		public static Rect BeginColumn(LayoutSize size, LayoutSize? minWidth = null, LayoutSize? maxWidth = null)
		{
			return BeginColumn(size, Vector2Int.zero, minWidth, maxWidth);
		}

		public static Rect BeginColumn(LayoutSize size, Vector2Int horizontalPadding, LayoutSize? minWidth = null, LayoutSize? maxWidth = null)
		{
			EventType type = Event.current.type;
			ColumnGUILayoutGroup gUILayoutGroup;
			if (type == EventType.Layout || type == EventType.Used)
			{
				gUILayoutGroup = new ColumnGUILayoutGroup(size, horizontalPadding, minWidth, maxWidth);
				GUILayoutUtility.current.topLevel.Add(gUILayoutGroup);
			}
			else
			{
				gUILayoutGroup = GUILayoutUtility.current.topLevel.GetNext() as ColumnGUILayoutGroup;
				if (gUILayoutGroup == null)
				{
					throw new ExitGUIException("GUILayout: Mismatched LayoutGroup." + Event.current.type);
				}
				gUILayoutGroup.ResetCursor();
			}
			GUILayoutUtility.current.layoutGroups.Push(gUILayoutGroup);
			GUILayoutUtility.current.topLevel = gUILayoutGroup;
			return gUILayoutGroup.rect;
		}

		public static Rect EndColumn()
		{
			return End();
		}

		public static Rect EndRow()
		{
			return End();
		}

		public static Rect EndSmartRow()
		{
			return End();
		}

		private static Rect End()
		{
			if (GUILayoutUtility.current.layoutGroups.Count == 0 || Event.current == null)
			{
				Debug.LogError("EndLayoutGroup: BeginLayoutGroup must be called first.");
				return default(Rect);
			}
			Rect rect = GUILayoutUtility.topLevel.rect;
			GUILayoutUtility.current.layoutGroups.Pop();
			if (0 < GUILayoutUtility.current.layoutGroups.Count)
			{
				GUILayoutUtility.current.topLevel = (UnityEngine.GUILayoutGroup)GUILayoutUtility.current.layoutGroups.Peek();
			}
			else
			{
				GUILayoutUtility.current.topLevel = new UnityEngine.GUILayoutGroup();
			}
			return rect;
		}
	}
}
