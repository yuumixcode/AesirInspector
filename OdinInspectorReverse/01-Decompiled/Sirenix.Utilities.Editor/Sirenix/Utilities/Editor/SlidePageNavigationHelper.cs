using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Sirenix.Utilities.Editor
{
	public class SlidePageNavigationHelper<T>
	{
		public class Page
		{
			public T Value;

			public string Name;

			internal int? TitleWidth;

			internal GUITabPage Tab;

			public bool BeginPage()
			{
				return Tab.BeginPage();
			}

			public void EndPage()
			{
				Tab.EndPage();
			}

			public Page(T @object, GUITabPage tab, string name)
			{
				Value = @object;
				Name = name;
				Tab = tab;
			}
		}

		private List<Page> pages;

		private Page prev;

		public GUITabGroup TabGroup;

		public IEnumerable<Page> EnumeratePages
		{
			get
			{
				bool doPrev = true;
				for (int i = Math.Max(0, pages.Count - 3); i < pages.Count; i++)
				{
					Page p = pages[i];
					if (p == prev)
					{
						doPrev = false;
					}
					yield return p;
				}
				if (prev != null && doPrev)
				{
					yield return prev;
				}
			}
		}

		public bool IsOnFirstPage => pages.Count <= 1;

		public SlidePageNavigationHelper()
		{
			TabGroup = new GUITabGroup();
			TabGroup.AnimationSpeed = 4f;
			TabGroup.ExpandHeight = true;
			pages = new List<Page>();
		}

		public void PushPage(T obj, string name)
		{
			GUITabPage tab = TabGroup.RegisterTab(Guid.NewGuid().ToString());
			Page page = new Page(obj, tab, name);
			pages.Add(page);
			TabGroup.GoToPage(page.Tab);
			prev = null;
		}

		public void NavigateBack()
		{
			if (!IsOnFirstPage)
			{
				prev = pages.Last();
				pages.RemoveAt(pages.Count - 1);
				TabGroup.GoToPage(pages[pages.Count - 1].Tab);
			}
		}

		public void NavigateBack(int index)
		{
			if (!IsOnFirstPage)
			{
				prev = pages.Last();
				pages.SetLength(index);
				TabGroup.GoToPage(pages[pages.Count - 1].Tab);
			}
		}

		public void DrawPageNavigation(Rect rect)
		{
			Rect leftBtnRect = rect.AlignLeft(rect.height * 1.3f);
			GUIHelper.PushGUIEnabled(!IsOnFirstPage);
			if (GUI.Button(leftBtnRect, GUIContent.none, GUIStyle.none))
			{
				NavigateBack();
			}
			EditorIcons.TriangleLeft.Draw(leftBtnRect, 19f);
			GUIHelper.PopGUIEnabled();
			rect.xMin += rect.height;
			int totalLength = 0;
			for (int i = pages.Count - 1; i >= 0; i--)
			{
				Page p = pages[i];
				if (!p.TitleWidth.HasValue)
				{
					p.TitleWidth = (int)SirenixGUIStyles.Label.CalcSize(new GUIContent(p.Name)).x + 7;
				}
				totalLength += p.TitleWidth.Value;
			}
			rect.width -= 8f;
			float cut = rect.xMin;
			if ((float)totalLength > rect.width)
			{
				rect.xMin -= (float)totalLength - rect.width;
			}
			for (int j = 0; j < pages.Count; j++)
			{
				Page p2 = pages[j];
				if (!p2.TitleWidth.HasValue)
				{
					p2.TitleWidth = (int)SirenixGUIStyles.Label.CalcSize(new GUIContent(p2.Name)).x + 7;
				}
				rect.width = p2.TitleWidth.Value;
				Rect btnRect = rect;
				btnRect.width -= 6f;
				btnRect.xMin = Mathf.Max(cut, btnRect.xMin);
				if (GUI.Button(btnRect, p2.Name, SirenixGUIStyles.LabelCentered))
				{
					NavigateBack(j + 1);
				}
				if (j != pages.Count - 1)
				{
					Rect lblRect = btnRect.AlignRight(10f);
					lblRect.x += 8f;
					lblRect.xMin = Mathf.Max(cut, lblRect.xMin);
					GUI.Label(lblRect, "/", SirenixGUIStyles.LabelCentered);
				}
				rect.x += rect.width;
			}
		}

		public void BeginGroup()
		{
			TabGroup.BeginGroup(drawToolbar: false, GUIStyle.none);
		}

		public void EndGroup()
		{
			TabGroup.EndGroup();
			if (Event.current.type == EventType.MouseDown && Event.current.button == 4)
			{
				Event.current.Use();
			}
		}
	}
}
