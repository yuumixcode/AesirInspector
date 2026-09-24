using System.Collections.Generic;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.GettingStarted
{
	public class TutorialPage : GettingStartedPage
	{
		private static GUIStyle multiline;

		public List<Tutorial> Tutorials = new List<Tutorial>();

		private float prevWidth = 200f;

		public override void DrawPage(Rect pageRect)
		{
			multiline = multiline ?? new GUIStyle(SirenixGUIStyles.MultiLineLabel)
			{
				alignment = TextAnchor.UpperLeft,
				clipping = TextClipping.Overflow
			};
			if (Event.current.type == EventType.MouseMove)
			{
				GUIHelper.RequestRepaint();
			}
			BeginScrollableLayoutPage(pageRect);
			if (prevWidth != 0f)
			{
				for (int i = 0; i < Tutorials.Count; i++)
				{
					Tutorial item = Tutorials[i];
					if (item.Visible != null && !item.Visible())
					{
						continue;
					}
					bool enabled = true;
					if (item.Enabled != null && !item.Enabled())
					{
						enabled = false;
					}
					bool prevEnabled = GUI.enabled;
					GUI.enabled = enabled;
					bool hasActionBtns = item.ActionButtons != null;
					int actionBtnsHeight = (hasActionBtns ? 25 : 0);
					int textPadding = 10;
					int iconWidth = 50;
					int difficultyRectWidth = 30;
					float predicted = prevWidth - (float)textPadding - (float)textPadding - (float)iconWidth - (float)difficultyRectWidth;
					float descriptionSize = (string.IsNullOrEmpty(item.Description) ? 0f : multiline.CalcHeight(GUIHelper.TempContent(item.Description), predicted));
					Rect rect = GUILayoutUtility.GetRect(0f, (50f + descriptionSize + (float)actionBtnsHeight) * base.VerticalSlideT);
					Rect bgRect = rect;
					Rect iconRect = rect.TakeFromLeft(iconWidth).AlignCenterY(25f * base.VerticalSlideT);
					Rect difficultyRect = rect.AlignBottom(EditorGUIUtility.singleLineHeight).SubX((float)textPadding * 0.5f).SubY((float)textPadding * 0.5f);
					Rect contentRect = rect.Padding(textPadding);
					Rect titleRect = contentRect.TakeFromTop(20f);
					Rect descriptionRect = contentRect;
					bool isMouseOver = enabled && !hasActionBtns && bgRect.Contains(Event.current.mousePosition);
					if (string.IsNullOrEmpty(item.Description))
					{
						titleRect.y += 5f;
					}
					SdfIcons.DrawIcon(iconRect.AddX(5f), item.Icon, SirenixGUIStyles.Label.normal.textColor);
					EditorGUI.DrawRect(bgRect, SirenixGUIStyles.HeaderBoxBackgroundColor);
					if (isMouseOver)
					{
						EditorGUI.DrawRect(bgRect, SirenixGUIStyles.HeaderBoxBackgroundColor);
					}
					SirenixEditorGUI.DrawBorders(bgRect, 1);
					GUI.Label(titleRect, item.Title, SirenixGUIStyles.BoldLabel);
					if (!string.IsNullOrEmpty(item.Description))
					{
						GUI.Label(descriptionRect, item.Description, multiline);
					}
					if (item.Difficulty != Difficulty.None)
					{
						GUI.Label(difficultyRect, item.Difficulty.ToString(), SirenixGUIStyles.RightAlignedGreyMiniLabel);
					}
					if (hasActionBtns)
					{
						Rect btnsRect = descriptionRect.TakeFromBottom(actionBtnsHeight);
						foreach (var btn in item.ActionButtons)
						{
							float w = GUI.skin.button.CalcSize(GUIHelper.TempContent(btn.btnName)).x + 20f;
							if (GUI.Button(btnsRect.TakeFromLeft(w), btn.btnName))
							{
								btn.action();
							}
							btnsRect.TakeFromLeft(10f);
						}
					}
					else if (GUI.Button(rect, GUIContent.none, GUIStyle.none))
					{
						if (item.ChildPage != null)
						{
							item.ChildPage.Window = Window;
							item.ChildPage.EnterPage();
						}
						item.OnClick?.Invoke();
						GUIHelper.ExitGUI(removeFocusControl: true);
					}
					if (i != Tutorials.Count - 1)
					{
						GUILayoutUtility.GetRect(0f, 10f);
					}
					GUI.enabled = prevEnabled;
				}
			}
			float ww = GUILayoutUtility.GetRect(0f, 1f).width;
			if (Event.current.type == EventType.Repaint)
			{
				prevWidth = ww;
			}
			EndScrollableLayoutPage();
		}
	}
}
