using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Sirenix.Utilities.Editor
{
	/// <summary>
	/// The GUITabGroup is a utility class to draw animated tab groups.
	/// </summary>
	/// <example>
	/// <code>
	/// var tabGroup = SirenixEditorGUI.CreateAnimatedTabGroup(someKey);
	/// // Register your tabs before starting BeginGroup.
	/// var tab1 = tabGroup.RegisterTab("tab 1");
	/// var tab2 = tabGroup.RegisterTab("tab 2");
	///
	/// tabGroup.BeginGroup(drawToolbar: true);
	/// {
	///     if (tab1.BeginPage())
	///     {
	///         // Draw GUI for the first tab page;
	///     }
	///     tab1.EndPage();
	///
	///     if (tab2.BeginPage())
	///     {
	///         // Draw GUI for the second tab page;
	///     }
	///     tab2.EndPage();
	/// }
	/// tabGroup.EndGroup();
	///
	/// // Control the animation speed.
	/// tabGroup.AnimationSpeed = 0.2f;
	///
	/// // If true, the tab group will have the height equal to the biggest page. Otherwise the tab group will animate in height as well when changing page.
	/// tabGroup.FixedHeight = true;
	///
	/// // You can change page by calling:
	/// tabGroup.GoToNextPage();
	/// tabGroup.GoToPreviousPage();
	/// </code>
	/// </example>
	/// <seealso cref="T:Sirenix.Utilities.Editor.SirenixEditorGUI" />
	public class GUITabGroup
	{
		private class ToolbarBtnWidth
		{
			public GUITabPage page;

			public int w1;

			public int w2;

			public int w3;

			internal int width;

			public ToolbarBtnWidth(GUITabPage page, int w1, int w2, int w3)
			{
				this.page = page;
				this.w1 = w1;
				this.w2 = w2;
				this.w3 = w3;
			}
		}

		private GUILayoutOption[] options = GUILayoutOptions.ExpandWidth().ExpandHeight(expand: false);

		private GUITabPage currentPage;

		private GUITabPage targetPage;

		private Vector2 scrollPosition;

		private float currentHeight;

		private Dictionary<string, GUITabPage> pages = new Dictionary<string, GUITabPage>();

		private float t = 1f;

		private bool isAnimating;

		private GUITabPage nextPage;

		private bool drawToolbar;

		private float toolbarHeight = 26f;

		/// <summary>
		/// The animation speed
		/// </summary>
		public float AnimationSpeed = 4f;

		public bool FixedHeight;

		public bool ExpandHeight;

		public TabLayouting TabLayouting;

		[Obsolete("This no longer does anything.")]
		public bool DrawNonSelectedTabsAsDisabled;

		private MultilineWrapLayoutUtility util = new MultilineWrapLayoutUtility(20);

		private List<GUITabPage> tabs = new List<GUITabPage>();

		private IEnumerable<GUITabPage> OrderedPages => from x in pages
			select x.Value into x
			orderby x.Order
			select x;

		/// <summary>
		/// Gets the outer rect of the entire tab group.
		/// </summary>
		public Rect OuterRect { get; private set; }

		/// <summary>
		/// The inner rect of the current tab page.
		/// </summary>
		public Rect InnerRect { get; private set; }

		public GUITabPage NextPage => nextPage;

		/// <summary>
		/// Gets the current page.
		/// </summary>
		public GUITabPage CurrentPage => targetPage ?? currentPage;

		/// <summary>
		/// Gets the t.
		/// </summary>
		public float T => t;

		internal bool IsAnimating => isAnimating;

		internal float InnerContainerWidth { get; private set; }

		internal float LabelWidth { get; private set; }

		/// <summary>
		/// The height of the tab buttons.
		/// </summary>
		public float ToolbarHeight
		{
			get
			{
				return toolbarHeight;
			}
			set
			{
				toolbarHeight = value;
			}
		}

		/// <summary>
		/// If true, the tab group will have the height equal to the biggest page. Otherwise the tab group will animate in height as well when changing page.
		/// </summary>
		/// <summary>
		/// Sets the current page.
		/// </summary>
		/// <param name="page">The page to switch to.</param>
		public void SetCurrentPage(GUITabPage page)
		{
			if (!pages.ContainsValue(page))
			{
				throw new InvalidOperationException("Page is not part of TabGroup");
			}
			currentPage = page;
			targetPage = null;
		}

		/// <summary>
		/// Registers the tab.
		/// </summary>
		public GUITabPage RegisterTab(string title)
		{
			if (title == null)
			{
				throw new ArgumentNullException("title");
			}
			if (!pages.TryGetValue(title, out var result))
			{
				return pages[title] = new GUITabPage(this, title);
			}
			return result;
		}

		/// <summary>
		/// Begins the group.
		/// </summary>
		/// <param name="drawToolbar">if set to <c>true</c> a tool-bar for changing pages is drawn.</param>
		/// <param name="style">The style.</param>
		public void BeginGroup(bool drawToolbar = true, GUIStyle style = null)
		{
			LabelWidth = GUIHelper.BetterLabelWidth;
			if (Event.current.type == EventType.Layout)
			{
				this.drawToolbar = drawToolbar;
			}
			style = style ?? SirenixGUIStyles.MessageBox;
			InnerContainerWidth = OuterRect.width - (float)(style.padding.left + style.padding.right + style.margin.left + style.margin.right);
			if (currentPage == null && pages.Count > 0)
			{
				currentPage = (from x in pages
					select x.Value into x
					orderby x.Order
					select x).First();
			}
			if (currentPage != null && !pages.ContainsKey(currentPage.TabName))
			{
				if (pages.Count > 0)
				{
					currentPage = OrderedPages.First();
				}
				else
				{
					currentPage = null;
				}
			}
			float maxHeight = 0f;
			foreach (GUITabPage page in pages.GFValueIterator())
			{
				page.OnBeginGroup();
				maxHeight = Mathf.Max(page.Rect.height, maxHeight);
				if (Event.current.type == EventType.Layout && page.IsVisible != (page.IsVisible = page == targetPage || page == currentPage))
				{
					if (targetPage == null)
					{
						scrollPosition.x = 0f;
						currentHeight = currentPage.Rect.height;
					}
					else
					{
						scrollPosition.x = ((targetPage.Order >= currentPage.Order) ? 0f : (scrollPosition.x = OuterRect.width));
						currentHeight = currentPage.Rect.height;
					}
				}
			}
			GUILayout.Space(1f);
			Rect outerRect = EditorGUILayout.BeginVertical(style, GUILayoutOptions.ExpandWidth().ExpandHeight(ExpandHeight));
			if (this.drawToolbar)
			{
				DrawToolbar(style);
			}
			if (InnerRect.width > 0f && !ExpandHeight)
			{
				if (options.Length == 2)
				{
					if (currentPage != null)
					{
						currentHeight = currentPage.Rect.height;
					}
					options = GUILayoutOptions.ExpandWidth().ExpandHeight(ExpandHeight).Height(currentHeight);
				}
				if (FixedHeight)
				{
					options[2] = GUILayout.Height(maxHeight);
				}
				else
				{
					options[2] = GUILayout.Height(currentHeight);
				}
			}
			GUIHelper.PushGUIEnabled(enabled: false);
			GUILayout.BeginScrollView(scrollPosition, alwaysShowHorizontal: false, alwaysShowVertical: false, GUIStyle.none, GUIStyle.none, options);
			GUIHelper.PopGUIEnabled();
			Rect innerRect = EditorGUILayout.BeginHorizontal(GUILayoutOptions.ExpandHeight(ExpandHeight));
			if (Event.current.type == EventType.Repaint)
			{
				OuterRect = outerRect;
				InnerRect = innerRect;
			}
			Animate();
		}

		/// <summary>
		/// Ends the group.
		/// </summary>
		public void EndGroup()
		{
			EditorGUILayout.EndHorizontal();
			GUIHelper.PushGUIEnabled(enabled: false);
			GUILayout.EndScrollView();
			GUIHelper.PopGUIEnabled();
			EditorGUILayout.EndVertical();
			if (targetPage != currentPage && targetPage != null)
			{
				GUIHelper.RequestRepaint();
			}
			foreach (GUITabPage page in pages.GFValueIterator())
			{
				page.OnEndGroup();
			}
			if (!isAnimating && nextPage != null)
			{
				targetPage = nextPage;
				nextPage = null;
			}
		}

		private void Animate()
		{
			if (currentPage == null || Event.current.type != EventType.Layout)
			{
				return;
			}
			if (isAnimating && targetPage != null && targetPage != currentPage)
			{
				t += GUITimeHelper.LayoutDeltaTime * AnimationSpeed;
				scrollPosition.x = Mathf.Lerp(currentPage.Rect.x, targetPage.Rect.x, Mathf.Min(1f, MathUtilities.Hermite01(t)));
				currentHeight = Mathf.Lerp(currentPage.Rect.height, targetPage.Rect.height, Mathf.Min(1f, MathUtilities.Hermite01(t)));
				if (t >= 1f)
				{
					currentPage.IsVisible = false;
					currentPage = targetPage;
					targetPage = null;
					scrollPosition.x = 0f;
					currentHeight = currentPage.Rect.height;
					t = 1f;
				}
			}
			else
			{
				t = 0f;
				isAnimating = false;
				scrollPosition.x = currentPage.Rect.x;
				currentHeight = currentPage.Rect.height;
				if (targetPage != null && targetPage != currentPage && targetPage.IsVisible)
				{
					isAnimating = true;
					scrollPosition.x = ((targetPage.Order > currentPage.Order) ? 0f : (scrollPosition.x = OuterRect.width));
					t = 0f;
				}
			}
		}

		private void DrawToolbar(GUIStyle style)
		{
			if (TabLayouting == TabLayouting.Shrink)
			{
				DrawSingleLineToolbar(style);
				return;
			}
			if (TabLayouting == TabLayouting.MultiRow)
			{
				DrawMultilineToolbar(style);
				return;
			}
			throw new NotImplementedException(TabLayouting.ToString());
		}

		private void DrawMultilineToolbar(GUIStyle style)
		{
			tabs.Clear();
			int selectedIndex = 0;
			GUITabPage selected = null;
			foreach (GUITabPage tab in OrderedPages)
			{
				if (tab.IsActive)
				{
					if (tab == (nextPage ?? CurrentPage))
					{
						selectedIndex = tabs.Count;
						selected = tab;
					}
					tabs.Add(tab);
				}
			}
			if (util.Items.Length != tabs.Count)
			{
				util.Items = new MultilineWrapLayoutUtility.Item[tabs.Count];
				for (int i = 0; i < tabs.Count; i++)
				{
					GUITabPage tab2 = tabs[i];
					bool hasIcon = tab2.Icon != SdfIconType.None;
					SirenixEditorGUI.CalculateMinimumSDFIconButtonWidth(tab2.Title, SirenixGUIStyles.LabelCentered, hasIcon, 20f, out var _, out var _, out var _, out var btnWidth);
					util.Items[i].Index = i;
					util.Items[i].width = btnWidth + 2f;
				}
			}
			util.SelectedIndex = selectedIndex;
			util.LineHeight = 20f;
			util.marginTop = -style.padding.top + 1;
			util.marginLeft = -style.padding.left;
			util.marginRight = -style.padding.right;
			util.ComputeAndAllocateRect();
			int bottomBorder = ((style == GUIStyle.none) ? 1 : 0);
			bool isProSkin = EditorGUIUtility.isProSkin;
			Color borderColor = (isProSkin ? SirenixGUIStyles.BorderColor : new Color(0f, 0f, 0f, 0.12f));
			MultilineWrapLayoutUtility.Item[] items = util.Items;
			for (int j = 0; j < items.Length; j++)
			{
				MultilineWrapLayoutUtility.Item item = items[j];
				GUITabPage p = tabs[item.Index];
				Rect r = item.Rect;
				bool isActive = item.Index == selectedIndex;
				if (!isActive)
				{
					Color bgColor = (isProSkin ? new Color(0.19f, 0.19f, 0.19f, 1f) : new Color(0f, 0f, 0f, 0.1f));
					GUI.DrawTexture(r, Texture2D.whiteTexture, ScaleMode.StretchToFill, alphaBlend: false, 0f, bgColor, Vector4.zero, new Vector4(0f, 0f, 0f, 0f));
				}
				int leftBorderWidth = (item.isFirstInRow ? 1 : 0);
				int bottomBorderWidth = 1;
				int rightBorderWidth = ((!isActive) ? 1 : bottomBorder);
				if (!isProSkin && item.isFirstInRow)
				{
					leftBorderWidth = 0;
				}
				if (!isProSkin && item.isLastInRow)
				{
					if (!isActive)
					{
						GUI.DrawTexture(r.AlignBottom(1f), Texture2D.whiteTexture, ScaleMode.StretchToFill, alphaBlend: true, 0f, borderColor, 0f, 0f);
					}
				}
				else
				{
					GUI.DrawTexture(r, Texture2D.whiteTexture, ScaleMode.StretchToFill, alphaBlend: false, 0f, borderColor, new Vector4(leftBorderWidth, 0f, bottomBorderWidth, rightBorderWidth), new Vector4(0f, 0f, 0f, 0f));
				}
				Color prevTextColor = default(Color);
				if (p.TextColor.HasValue)
				{
					prevTextColor = SirenixGUIStyles.LabelCentered.normal.textColor;
					SirenixGUIStyles.LabelCentered.normal.textColor = p.TextColor.Value;
					SirenixGUIStyles.LabelCentered.hover.textColor = p.TextColor.Value;
				}
				if (SirenixEditorGUI.SDFIconButton(r, GUIHelper.TempContent(p.Title, p.Tooltip), p.Icon, IconAlignment.LeftOfText, SirenixGUIStyles.LabelCentered) && currentPage != p)
				{
					nextPage = p;
				}
				if (p.TextColor.HasValue)
				{
					SirenixGUIStyles.LabelCentered.normal.textColor = prevTextColor;
					SirenixGUIStyles.LabelCentered.hover.textColor = prevTextColor;
				}
			}
		}

		private void DrawSingleLineToolbar(GUIStyle style)
		{
			Rect toolbarRect = GUILayoutUtility.GetRect(0f, toolbarHeight - (float)style.padding.vertical).Expand(style.padding.left, style.padding.right, style.padding.top, 0f);
			if (Event.current.type == EventType.Layout)
			{
				return;
			}
			float totalWidth = toolbarRect.width;
			List<ToolbarBtnWidth> tabBtns = new List<ToolbarBtnWidth>();
			int totalW1 = 0;
			int totalW2 = 0;
			int totalW3 = 0;
			foreach (GUITabPage page in OrderedPages.Where((GUITabPage x) => x.IsActive))
			{
				bool hasIcon = page.Icon != SdfIconType.None;
				SirenixEditorGUI.CalculateMinimumSDFIconButtonWidth(page.Title, SirenixGUIStyles.LabelCentered, hasIcon, toolbarRect.height, out var _, out var iconWidth, out var singlePadding, out var btnWidth);
				btnWidth += 1f;
				iconWidth += 1f;
				int w1 = (hasIcon ? ((int)(iconWidth + singlePadding * 2f)) : 15);
				int w2 = (hasIcon ? ((int)(iconWidth + singlePadding * 2f)) : ((int)btnWidth));
				int w3 = (int)btnWidth;
				if (page == CurrentPage)
				{
					w1 = (w2 = w3);
				}
				totalW1 += w1;
				totalW2 += w2;
				totalW3 += w3;
				tabBtns.Add(new ToolbarBtnWidth(page, w1, w2, w3));
			}
			float remaining;
			if (totalWidth >= (float)totalW3)
			{
				remaining = totalWidth - (float)totalW3;
				foreach (ToolbarBtnWidth t in tabBtns)
				{
					t.width = t.w3;
				}
			}
			else if (totalWidth >= (float)totalW2)
			{
				float f = 1f - ((float)totalW3 - totalWidth) / (float)(totalW3 - totalW2);
				remaining = totalWidth;
				foreach (ToolbarBtnWidth t2 in tabBtns)
				{
					if (t2.w1 == 15)
					{
						t2.width = (int)Mathf.Lerp(t2.w2, t2.w3, f);
					}
					else
					{
						t2.width = t2.w2;
					}
					remaining -= (float)t2.width;
				}
			}
			else
			{
				float f2 = 1f - ((float)totalW2 - totalWidth) / (float)(totalW2 - totalW1);
				remaining = totalWidth - Mathf.Lerp(totalW1, totalW2, f2);
				foreach (ToolbarBtnWidth t3 in tabBtns)
				{
					t3.width = (int)Mathf.Lerp(t3.w1, t3.w2, f2);
				}
			}
			remaining = Math.Max(0f, remaining);
			int split = (int)(remaining / (float)tabBtns.Count);
			Rect rect = toolbarRect;
			rect.yMin++;
			int bottomBorder = ((style == GUIStyle.none) ? 1 : 0);
			bool isProSkin = EditorGUIUtility.isProSkin;
			Color borderColor = (isProSkin ? SirenixGUIStyles.BorderColor : new Color(0f, 0f, 0f, 0.12f));
			for (int i = 0; i < tabBtns.Count; i++)
			{
				ToolbarBtnWidth p = tabBtns[i];
				int width = p.width + split;
				Rect r = ((i == tabBtns.Count - 1) ? rect : rect.TakeFromLeft(width));
				bool isActive = p.page == (nextPage ?? CurrentPage);
				if (!isActive)
				{
					Color bgColor = (isProSkin ? new Color(0.19f, 0.19f, 0.19f, 1f) : new Color(0f, 0f, 0f, 0.1f));
					GUI.DrawTexture(r, Texture2D.whiteTexture, ScaleMode.StretchToFill, alphaBlend: false, 0f, bgColor, Vector4.zero, new Vector4(0f, 0f, 0f, 0f));
				}
				int leftBorderWidth = ((i == 0) ? 1 : 0);
				int bottomBorderWidth = 1;
				int rightBorderWidth = ((!isActive) ? 1 : bottomBorder);
				if (!isProSkin && i == 0)
				{
					leftBorderWidth = 0;
				}
				if (!isProSkin && i == tabBtns.Count - 1)
				{
					if (!isActive)
					{
						GUI.DrawTexture(r.AlignBottom(1f), Texture2D.whiteTexture, ScaleMode.StretchToFill, alphaBlend: true, 0f, borderColor, 0f, 0f);
					}
				}
				else
				{
					GUI.DrawTexture(r, Texture2D.whiteTexture, ScaleMode.StretchToFill, alphaBlend: false, 0f, borderColor, new Vector4(leftBorderWidth, 0f, bottomBorderWidth, rightBorderWidth), new Vector4(0f, 0f, 0f, 0f));
				}
				Color prevTextColor = default(Color);
				if (p.page.TextColor.HasValue)
				{
					prevTextColor = SirenixGUIStyles.LabelCentered.normal.textColor;
					SirenixGUIStyles.LabelCentered.normal.textColor = p.page.TextColor.Value;
					SirenixGUIStyles.LabelCentered.hover.textColor = p.page.TextColor.Value;
				}
				if (SirenixEditorGUI.SDFIconButton(r, GUIHelper.TempContent(p.page.Title, p.page.Tooltip), p.page.Icon, IconAlignment.LeftOfText, SirenixGUIStyles.LabelCentered) && currentPage != p.page)
				{
					nextPage = p.page;
				}
				if (p.page.TextColor.HasValue)
				{
					SirenixGUIStyles.LabelCentered.normal.textColor = prevTextColor;
					SirenixGUIStyles.LabelCentered.hover.textColor = prevTextColor;
				}
			}
		}

		/// <summary>
		/// Goes to page.
		/// </summary>
		public void GoToPage(GUITabPage page)
		{
			nextPage = page;
		}

		public void GoToPage(string pageName)
		{
			if (pages.TryGetValue(pageName, out var page))
			{
				GoToPage(page);
				return;
			}
			throw new InvalidOperationException("No such tab page exists");
		}

		/// <summary>
		/// Goes to next page.
		/// </summary>
		public void GoToNextPage()
		{
			if (currentPage == null)
			{
				return;
			}
			bool takeNext = false;
			List<GUITabPage> ordered = OrderedPages.ToList();
			for (int i = 0; i < ordered.Count; i++)
			{
				if (takeNext && ordered[i].IsActive)
				{
					nextPage = ordered[i];
					break;
				}
				if (ordered[i] == (nextPage ?? CurrentPage))
				{
					takeNext = true;
				}
			}
		}

		/// <summary>
		/// Goes to previous page.
		/// </summary>
		public void GoToPreviousPage()
		{
			if (currentPage == null)
			{
				return;
			}
			List<GUITabPage> ordered = OrderedPages.ToList();
			int prevIdx = -1;
			for (int i = 0; i < ordered.Count; i++)
			{
				if (ordered[i] == (nextPage ?? CurrentPage))
				{
					if (prevIdx >= 0)
					{
						nextPage = ordered[prevIdx];
					}
					break;
				}
				if (ordered[i].IsActive)
				{
					prevIdx = i;
				}
			}
		}
	}
}
