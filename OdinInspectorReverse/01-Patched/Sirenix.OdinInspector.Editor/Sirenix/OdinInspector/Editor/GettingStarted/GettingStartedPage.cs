using System;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.GettingStarted
{
	public class GettingStartedPage
	{
		private Vector2 scrollPos;

		public string Title;

		public SdfIconType TitleIcon;

		public float FooterSize = 80f;

		[NonSerialized]
		public GettingStartedWindow Window;

		private static GUIStyle padding;

		public float VerticalSlideT => 1f - Window.VerticalSlideT;

		public virtual void DrawFooter(Rect rect)
		{
		}

		public virtual void DrawPage(Rect rect)
		{
		}

		public virtual void GoBack()
		{
			if (Window.Pages.Count > 0)
			{
				Window.Pages.RemoveAt(Window.Pages.Count - 1);
			}
		}

		public virtual void EnterPage()
		{
			Window.Pages.Add(this);
		}

		protected void BeginScrollableLayoutPage(Rect rect, int paddingSize = 20)
		{
			padding = padding ?? new GUIStyle
			{
				padding = new RectOffset
				{
					bottom = 20,
					left = 20,
					top = 20,
					right = 20
				}
			};
			padding.padding.left = paddingSize;
			padding.padding.right = paddingSize;
			padding.padding.top = paddingSize;
			padding.padding.bottom = paddingSize;
			GUILayout.BeginArea(rect);
			scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
			GUILayout.BeginVertical(padding);
		}

		protected void EndScrollableLayoutPage()
		{
			GUILayout.EndVertical();
			EditorGUILayout.EndScrollView();
			GUILayout.EndArea();
		}
	}
}
