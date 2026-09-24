using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Sirenix.Utilities.Editor
{
	/// <summary>
	/// A tab page created by <see cref="T:Sirenix.Utilities.Editor.GUITabGroup" />.
	/// </summary>
	/// <seealso cref="T:Sirenix.Utilities.Editor.GUITabGroup" />
	public class GUITabPage
	{
		private static GUIStyle innerContainerStyle;

		private GUITabGroup tabGroup;

		private Color prevColor;

		private static int pageIndexIncrementer;

		private bool isSeen;

		private bool isMessured;

		public int Order;

		public readonly string TabName;

		public string Title;

		public SdfIconType Icon;

		public Color? TextColor;

		public Rect Rect;

		public bool IsActive;

		public bool IsVisible;

		public string Tooltip;

		private static GUIStyle InnerContainerStyle
		{
			get
			{
				if (innerContainerStyle == null)
				{
					innerContainerStyle = new GUIStyle
					{
						padding = new RectOffset(3, 3, 3, 3)
					};
				}
				return innerContainerStyle;
			}
		}

		internal GUITabPage(GUITabGroup tabGroup, string title)
		{
			TabName = title;
			Title = title;
			this.tabGroup = tabGroup;
			IsActive = true;
		}

		internal void OnBeginGroup()
		{
			pageIndexIncrementer = 0;
			isSeen = false;
		}

		internal void OnEndGroup()
		{
			if (Event.current.type == EventType.Repaint)
			{
				IsActive = isSeen;
			}
		}

		/// <summary>
		/// Begins the page.
		/// </summary>
		public bool BeginPage()
		{
			if (tabGroup.FixedHeight && !isMessured)
			{
				IsVisible = true;
			}
			isSeen = true;
			if (IsVisible)
			{
				GUILayoutOptions.GUILayoutOptionsInstance options = GUILayoutOptions.Width(tabGroup.InnerContainerWidth + 3f).ExpandHeight(tabGroup.ExpandHeight);
				Rect rect = EditorGUILayout.BeginVertical(InnerContainerStyle, options);
				GUIHelper.PushHierarchyMode(hierarchyMode: false);
				GUIHelper.PushLabelWidth(tabGroup.LabelWidth - 4f);
				if (Event.current.type == EventType.Repaint)
				{
					Rect = rect;
				}
				if (tabGroup.IsAnimating)
				{
					prevColor = GUI.color;
					Color col = prevColor;
					col.a *= ((tabGroup.CurrentPage == this) ? tabGroup.T : (1f - tabGroup.T));
					GUI.color = col;
				}
			}
			return IsVisible;
		}

		/// <summary>
		/// Ends the page.
		/// </summary>
		public void EndPage()
		{
			if (IsVisible)
			{
				GUIHelper.PopLabelWidth();
				GUIHelper.PopHierarchyMode();
				if (tabGroup.IsAnimating)
				{
					GUI.color = prevColor;
				}
				EditorGUILayout.EndVertical();
			}
			if (Event.current.type == EventType.Repaint)
			{
				isMessured = true;
				Order = pageIndexIncrementer++;
			}
		}
	}
}
