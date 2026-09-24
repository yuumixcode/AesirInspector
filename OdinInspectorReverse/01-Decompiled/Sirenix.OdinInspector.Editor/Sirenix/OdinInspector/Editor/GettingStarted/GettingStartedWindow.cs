using System;
using System.Collections.Generic;
using Sirenix.OdinInspector.Editor.Internal;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Sirenix.OdinInspector.Editor.GettingStarted
{
	public class GettingStartedWindow : OdinEditorWindow
	{
		private static float speed = 2f;

		private static Texture2D odinBg;

		[NonSerialized]
		private GettingStartedPage slideToPage;

		[NonSerialized]
		private GettingStartedPage slideFromPage;

		private int HorizontalSlideDirection;

		[NonSerialized]
		private int prevPageCount;

		[SerializeField]
		private float verticalSlideTRaw;

		[SerializeField]
		private float horizontalSlideTRaw;

		[SerializeField]
		private int selectedProduct;

		[SerializeField]
		public List<GettingStartedPage> Pages = new List<GettingStartedPage>();

		[SerializeField]
		public float VerticalSlideT;

		[SerializeField]
		public float HorizontalSlideT;

		public static bool firstOpen;

		private IMGUIContainer imguiContainer;

		private IMGUIContainer eulaContainer;

		private IMGUIContainer popupContainer;

		private bool closedVisualDesignerPopup;

		private static GUIStyle padding;

		private static GUIStyle titleStyle;

		private static GUIStyle descStyle;

		/// <summary>
		/// Gets a texture of an odin bg symbol.
		/// </summary>
		public static Texture2D OdinBg
		{
			get
			{
				if (odinBg == null)
				{
					byte[] bytes = Convert.FromBase64String(GettingStartedBg.PngString);
					odinBg = TextureUtilities.LoadImage(1024, 1024, bytes);
					CleanupUtility.DestroyObjectOnAssemblyReload(odinBg);
				}
				return odinBg;
			}
		}

		private void CreateGUI()
		{
			imguiContainer = new IMGUIContainer(OnImGUI);
			imguiContainer.name = "Odin ImGUIContainer";
			imguiContainer.style.display = DisplayStyle.Flex;
			eulaContainer = new IMGUIContainer(DrawEULA);
			eulaContainer.name = "Odin EULA ImGUIContainer";
			eulaContainer.style.display = DisplayStyle.Flex;
			popupContainer = new IMGUIContainer(DrawVisualDesignerPopup);
			popupContainer.name = "Popup ImGUIContainer";
			popupContainer.style.display = DisplayStyle.Flex;
			base.rootVisualElement.style.flexGrow = 1f;
			imguiContainer.style.flexGrow = 1f;
			FullOverlay(eulaContainer);
			FullOverlay(popupContainer);
			base.rootVisualElement.Add(imguiContainer);
			imguiContainer.Add(eulaContainer);
			imguiContainer.Add(popupContainer);
			static void FullOverlay(VisualElement ve)
			{
				ve.style.position = Position.Absolute;
				ve.style.left = 0f;
				ve.style.right = 0f;
				ve.style.top = 0f;
				ve.style.bottom = 0f;
				ve.pickingMode = PickingMode.Position;
			}
		}

		protected override void OnEnable()
		{
			base.wantsMouseMove = true;
		}

		protected override void OnImGUI()
		{
			UpdateThings();
			Rect rect = base.position.ResetPosition();
			DrawToolbar(rect.TakeFromTop(EditorStyles.toolbarButton.fixedHeight + 4f));
			DrawFancyOdinSelector(ref rect);
			if ((slideToPage ?? slideFromPage) != null)
			{
				if (slideToPage == null || slideFromPage == null)
				{
					GettingStartedPage page = slideToPage ?? slideFromPage;
					DrawPage(ref rect, page, page.FooterSize);
				}
				else if (slideToPage != slideFromPage)
				{
					Rect left = rect;
					Rect right = rect;
					float t = ((HorizontalSlideDirection == 1) ? HorizontalSlideT : (1f - HorizontalSlideT));
					left.x -= t * rect.width;
					right.x = left.xMax;
					GettingStartedPage from = slideFromPage;
					GettingStartedPage to = slideToPage;
					if (HorizontalSlideDirection < 0)
					{
						to = slideFromPage;
						from = slideToPage;
					}
					float footerSize = from.FooterSize * (1f - t) + to.FooterSize * t;
					Color prevCol = GUI.color;
					GUI.color = prevCol * new Color(1f, 1f, 1f, 1f - t);
					DrawPage(ref left, from, footerSize);
					GUI.color = prevCol * new Color(1f, 1f, 1f, t);
					DrawPage(ref right, to, footerSize);
					GUI.color = prevCol;
					rect.TakeFromTop(Mathf.Max(left.height, right.height));
				}
				else
				{
					DrawPage(ref rect, slideToPage, slideToPage.FooterSize);
				}
			}
			this.RepaintIfRequested();
		}

		[InitializeOnLoadMethod]
		private static void AutoOpen()
		{
			if (EditorPrefs.GetBool("ODIN_INSPECTOR_SHOW_GETTING_STARTED", defaultValue: false))
			{
				EditorPrefs.SetBool("ODIN_INSPECTOR_SHOW_GETTING_STARTED", value: false);
				EditorApplication.delayCall = (EditorApplication.CallbackFunction)Delegate.Combine(EditorApplication.delayCall, (EditorApplication.CallbackFunction)delegate
				{
					ShowWindow(showOdinNotification: true, showValidatorNotification: false);
				});
			}
		}

		private void HideContainer(IMGUIContainer container)
		{
			container.style.display = DisplayStyle.None;
			container.pickingMode = PickingMode.Ignore;
		}

		private void ShowContainer(IMGUIContainer container)
		{
			container.style.display = DisplayStyle.Flex;
			container.pickingMode = PickingMode.Position;
		}

		private void UpdateThings()
		{
			if (prevPageCount != Pages.Count)
			{
				HorizontalSlideDirection = ((prevPageCount <= Pages.Count) ? 1 : (-1));
				prevPageCount = Pages.Count;
			}
			if (Event.current.type != EventType.Layout)
			{
				return;
			}
			slideToPage = ((Pages.Count == 0) ? null : Pages[Pages.Count - 1]);
			bool slideVertically = false;
			bool slideHorizontally = false;
			if (slideFromPage != slideToPage)
			{
				if (slideToPage == null)
				{
					slideVertically = true;
				}
				else if (slideFromPage == null)
				{
					slideVertically = true;
				}
				else
				{
					slideHorizontally = true;
				}
			}
			if (slideVertically)
			{
				float targetT = Mathf.MoveTowards(verticalSlideTRaw, (slideToPage == null) ? 1 : 0, GUITimeHelper.LayoutDeltaTime * speed);
				if (targetT != verticalSlideTRaw)
				{
					verticalSlideTRaw = targetT;
					GUIHelper.RequestRepaint();
				}
				else
				{
					slideFromPage = slideToPage;
				}
				VerticalSlideT = verticalSlideTRaw * verticalSlideTRaw * (3f - 2f * verticalSlideTRaw);
				VerticalSlideT = VerticalSlideT * VerticalSlideT * (3f - 2f * VerticalSlideT);
			}
			if (slideHorizontally)
			{
				float targetT2 = Mathf.MoveTowards(horizontalSlideTRaw, 1f, GUITimeHelper.LayoutDeltaTime * speed);
				if (targetT2 != horizontalSlideTRaw)
				{
					horizontalSlideTRaw = targetT2;
					GUIHelper.RequestRepaint();
				}
				else
				{
					slideFromPage = slideToPage;
					horizontalSlideTRaw = 1f;
				}
				HorizontalSlideT = horizontalSlideTRaw * horizontalSlideTRaw * (3f - 2f * horizontalSlideTRaw);
				HorizontalSlideT = HorizontalSlideT * HorizontalSlideT * (3f - 2f * HorizontalSlideT);
			}
			else
			{
				horizontalSlideTRaw = 0f;
				HorizontalSlideT = 0f;
			}
			if (slideFromPage != null)
			{
				slideFromPage.Window = this;
			}
			if (slideToPage != null)
			{
				slideToPage.Window = this;
			}
		}

		private void DrawPage(ref Rect rect, GettingStartedPage page, float footerSize)
		{
			float vertical_t = page.VerticalSlideT;
			Color prevCol = GUI.color;
			GUI.color *= new Color(1f, 1f, 1f, vertical_t);
			if (rect.height > 1f)
			{
				Rect topRect = rect.TakeFromTop(50f * vertical_t);
				string headerText = page.Title;
				GUIStyle style = SirenixGUIStyles.SectionHeaderCentered;
				float textWidth = style.CalcSize(GUIHelper.TempContent(headerText)).x;
				int iconSize = 25;
				int spacing = 5;
				SdfIconType icon = page.TitleIcon;
				Rect r = topRect.AlignCenterX((float)(iconSize + spacing) + textWidth);
				Color c = GUI.color;
				GUI.color = Color.white;
				EditorGUI.DrawRect(topRect, SirenixGUIStyles.HeaderBoxBackgroundColor);
				GUI.color = c;
				GUI.Label(r.AlignRight(textWidth), headerText, style);
				SdfIcons.DrawIcon(r.AlignLeft(iconSize), icon, style.normal.textColor);
				Rect bottomRect = rect.TakeFromBottom(footerSize * vertical_t);
				Color c2 = GUI.color;
				GUI.color = Color.white;
				EditorGUI.DrawRect(bottomRect, SirenixGUIStyles.DarkEditorBackground);
				EditorGUI.DrawRect(bottomRect.AlignTop(1f), SirenixGUIStyles.BorderColor);
				GUI.color = c2;
				bottomRect = bottomRect.HorizontalPadding(20f);
				bottomRect = bottomRect.AlignCenterY(25f);
				if (Button(ref bottomRect, "Back", SdfIconType.ChevronLeft, Direction.Left, Direction.Left))
				{
					page.GoBack();
				}
				page.DrawFooter(bottomRect);
				Rect bodyRect = rect;
				EditorGUI.DrawRect(bodyRect.AlignTop(1f), SirenixGUIStyles.BorderColor);
				page.DrawPage(bodyRect);
			}
			GUI.color = prevCol;
		}

		private void DrawToolbar(Rect rect)
		{
			EditorGUI.DrawRect(rect.TakeFromBottom(2f), new Color(0f, 0f, 0f, 1f));
			rect = rect.AlignCenterY(EditorGUIUtility.singleLineHeight);
			if (ToolbarButton(ref rect, SdfIconType.InfoCircleFill, "Support", fromLeft: false, null))
			{
				Application.OpenURL("https://odininspector.com/support");
			}
			if (ToolbarButton(ref rect, SdfIconType.Discord, "Discord", fromLeft: false, null))
			{
				Application.OpenURL("https://discord.gg/WTYJEra");
			}
			if (ToolbarButton(ref rect, SdfIconType.BookFill, "Tutorials", fromLeft: false, null))
			{
				Application.OpenURL("https://odininspector.com/tutorials");
			}
			string versionName = "Version " + OdinInspectorVersion.Version + " " + OdinInspectorVersion.BuildName;
			if (ToolbarButton(ref rect, SdfIconType.None, versionName, fromLeft: false, SirenixGUIStyles.CenteredGreyMiniLabel))
			{
				Application.OpenURL("https://odininspector.com/patch-notes");
			}
			if (ToolbarButtonFromLeft(ref rect, "Overview", Pages.Count == 0, EditorStyles.toolbarButton))
			{
				Pages.Clear();
			}
			for (int i = 0; i < Pages.Count; i++)
			{
				GettingStartedPage p = Pages[i];
				if (ToolbarButtonFromLeft(ref rect, p.Title, i == Pages.Count - 1, EditorStyles.toolbarButton))
				{
					Pages.SetLength(i + 1);
					break;
				}
			}
		}

		public static void ShowWindow()
		{
			GettingStartedWindow wnd = EditorWindow.GetWindow<GettingStartedWindow>();
			wnd.position = GUIHelper.GetEditorWindowRect().AlignCenter(900f, 900f);
			wnd.verticalSlideTRaw = 1f;
			wnd.VerticalSlideT = 1f;
			wnd.slideToPage = null;
			wnd.slideFromPage = null;
			wnd.ShowUtility();
		}

		internal static void ShowWindow(bool showOdinNotification, bool showValidatorNotification)
		{
			GettingStartedWindow wnd = EditorWindow.GetWindow<GettingStartedWindow>();
			wnd.position = GUIHelper.GetEditorWindowRect().AlignCenter(900f, 900f);
			wnd.verticalSlideTRaw = 1f;
			wnd.VerticalSlideT = 1f;
			wnd.slideToPage = null;
			wnd.slideFromPage = null;
			wnd.ShowUtility();
			if (showOdinNotification)
			{
				GettingStartedWindowData.Products[0].NotificationT = 1f;
				GettingStartedWindowData.Products[0].NotificationTargetT = 1f;
			}
			if (showValidatorNotification)
			{
				GettingStartedWindowData.Products[1].NotificationT = 1f;
				GettingStartedWindowData.Products[1].NotificationTargetT = 1f;
			}
		}

		private void DrawFancyOdinSelector(ref Rect totalRect)
		{
			Rect selectorRect = totalRect.TakeFromTop(Mathf.Lerp(totalRect.height, 120f, 1f - VerticalSlideT));
			descStyle = descStyle ?? new GUIStyle(SirenixGUIStyles.MultiLineCenteredLabel);
			descStyle.wordWrap = false;
			descStyle.alignment = TextAnchor.UpperLeft;
			titleStyle = titleStyle ?? new GUIStyle(SirenixGUIStyles.BoldTitle);
			titleStyle.alignment = TextAnchor.UpperLeft;
			titleStyle.normal.textColor = Color.white;
			titleStyle.fontSize = 14;
			EditorGUI.DrawRect(selectorRect.TakeFromBottom(2f * (1f - VerticalSlideT)), SirenixGUIStyles.BorderColor);
			EditorGUI.DrawRect(selectorRect.Split(0, 3), SirenixGUIStyles.ListItemColorOdd);
			EditorGUI.DrawRect(selectorRect.Split(2, 3), SirenixGUIStyles.ListItemColorOdd);
			EditorGUI.DrawRect(selectorRect.Split(0, 3).AlignRight(1f), SirenixGUIStyles.BorderColor);
			EditorGUI.DrawRect(selectorRect.Split(1, 3).AlignRight(1f), SirenixGUIStyles.BorderColor);
			for (int i = 0; i < GettingStartedWindowData.Products.Length; i++)
			{
				ref GettingStartedProduct p = ref GettingStartedWindowData.Products[i];
				Rect rect = selectorRect.Split(i, GettingStartedWindowData.Products.Length);
				if ((double)VerticalSlideT > 0.001)
				{
					GUI.color = new Color(1f, 1f, 1f, VerticalSlideT);
					rect.TakeFromBottom(20f * VerticalSlideT);
					Rect btnsRect = rect.TakeFromBottom(40f * VerticalSlideT).Padding(10f, 5f * VerticalSlideT);
					for (int j = 0; j < p.Pages.Length; j++)
					{
						if (GUI.Button(btnsRect.Split(j, p.Pages.Length), p.Pages[j].btnName))
						{
							selectedProduct = i;
							p.Pages[j].page.Window = this;
							p.Pages[j].page.EnterPage();
						}
					}
					rect.TakeFromBottom(20f * VerticalSlideT);
					Rect r = rect.TakeFromBottom(40f * VerticalSlideT);
					EditorGUI.DrawRect(r.AlignBottom(1f), SirenixGUIStyles.BorderColor);
					r = r.Padding(10f, 8f * VerticalSlideT);
					Rect iconRect = r.TakeFromLeft(r.height);
					r.TakeFromLeft(10f);
					SdfIcons.DrawIcon(iconRect, p.StatusIcon, p.StatusIconColor);
					GUI.Label(r, p.Status, SirenixGUIStyles.BoldTitle);
					GUI.color = Color.white;
				}
				float greyScale = (p.Enabled ? 0f : 1f);
				Color color = (p.Enabled ? Color.white : new Color(1f, 1f, 1f, 0.5f));
				color = ((selectedProduct == i) ? Color.Lerp(color, new Color(1f, 1f, 1f, 1f), 1f - VerticalSlideT) : Color.Lerp(color, new Color(1f, 1f, 1f, 0.4f), 1f - VerticalSlideT));
				greyScale = Mathf.Lerp(greyScale, (selectedProduct != i) ? 1 : 0, 1f - VerticalSlideT);
				GUITextureDrawingUtil.DrawTexture(rect.Expand(1f), OdinBg, ScaleMode.ScaleAndCrop, color, p.HueColor, greyScale);
				GUITextureDrawingUtil.DrawTexture(rect.Padding(40f, 0f), p.Logo, ScaleMode.ScaleToFit, color, default(Color), greyScale);
				if (i > 0)
				{
					EditorGUI.DrawRect(rect.AlignLeft(2f).AddXMin(-1f), Color.black);
				}
				if (i >= 2 || !p.Enabled)
				{
					continue;
				}
				float t1 = VerticalSlideT;
				if (!(t1 > 0f))
				{
					continue;
				}
				Color bgColor = new Color(0.25f, 0.25f, 0.25f, t1);
				Rect nRect = rect.AddY(40f * t1);
				float t2 = p.NotificationT * t1;
				t2 = t2 * t2 * (3f - 2f * t2);
				Color starColor = new Color(1f, 0.9f, 0f, t1);
				Rect a = nRect.AlignRight(t2 * (rect.width - 20f - 32f) * t1 + 32f * t1).AlignTop(t2 * 85f + 32f * t1);
				int mouseOver = (a.Contains(Event.current.mousePosition) ? 1 : 0);
				Rect starRect = a.Padding(7f * t1).SetSize(17f * t1);
				Rect buttonsRect = a.Padding(10f).AlignBottom(28f);
				Rect titleRect = starRect.AddX(starRect.width + 10f).SetXMax(rect.xMax - 10f).AddY(1f);
				Rect descRect = starRect.AddX(3f).AddY(30f).SetXMax(rect.xMax - 10f)
					.SetYMax(buttonsRect.y - 10f);
				GUI.DrawTexture(a, Texture2D.whiteTexture, ScaleMode.StretchToFill, alphaBlend: true, 0f, bgColor, default(Vector4), new Vector4(3f, 0f, 0f, 3f));
				SdfIcons.DrawIcon(starRect.Expand((float)(2 * mouseOver) * (1f - t2)), SdfIconType.StarFill, starColor);
				if (t2 <= 0.95f && GUI.Button(a, GUIContent.none, GUIStyle.none))
				{
					p.NotificationTargetT = 1f;
				}
				if ((double)t2 > 0.01)
				{
					Color prev = GUI.color;
					GUI.color = new Color(1f, 1f, 1f, t2 * prev.a);
					GUI.Label(titleRect.AddYMax(6f), "Enjoying " + p.Name + "?", titleStyle);
					GUI.Label(descRect, "If you've been enjoying Odin, please consider \nleaving a review - it makes a big difference!", descStyle);
					if (SirenixEditorGUI.SDFIconButton(titleRect.TakeFromRight(titleRect.height), GUIContent.none, SdfIconType.X, IconAlignment.LeftOfText, SirenixGUIStyles.IconButton))
					{
						p.NotificationTargetT = 0f;
					}
					if (SirenixEditorGUI.SDFIconButton(buttonsRect, "Leave a review on the Asset Store", SdfIconType.Link45deg, IconAlignment.LeftOfText, SirenixGUIStyles.ButtonLeft))
					{
						Application.OpenURL(p.ReviewUrl);
					}
					GUI.color = prev;
				}
				if (Event.current.type == EventType.Repaint)
				{
					p.NotificationT = Mathf.MoveTowards(p.NotificationT, p.NotificationTargetT, 4f * GUITimeHelper.RepaintDeltaTime);
				}
				if (p.NotificationT != p.NotificationTargetT)
				{
					GUIHelper.RequestRepaint();
				}
			}
		}

		public static float Smoothstep(float a, float b, float t)
		{
			t = Mathf.Clamp((t - a) / (b - a), 0f, 1f);
			return t * t * (3f - 2f * t);
		}

		private static bool ToolbarButtonFromLeft(ref Rect rect, string text, bool isOn, GUIStyle style)
		{
			GUIContent content = GUIHelper.TempContent(text);
			float textWidth = style.CalcSize(content).x;
			int btnPadding = 5;
			float btnWidth = textWidth + (float)(btnPadding * 2);
			Rect btnRect = rect.TakeFromLeft(btnWidth);
			GUIHelper.RequestRepaint();
			bool hover = btnRect.Contains(Event.current.mousePosition);
			if (Event.current.type == EventType.Repaint)
			{
				style.Draw(btnRect, content, isHover: false, hover, isOn, hasKeyboardFocus: false);
			}
			return GUI.Button(btnRect, GUIContent.none, GUIStyle.none);
		}

		private static bool ToolbarButton(ref Rect rect, SdfIconType icon, string text, bool fromLeft, GUIStyle textLabelStyle)
		{
			GUIStyle textStyle = textLabelStyle ?? SirenixGUIStyles.Label;
			GUIContent content = GUIHelper.TempContent(text);
			float iconWidth = ((icon == SdfIconType.None) ? 0f : rect.height);
			int iconPadding = ((icon != SdfIconType.None) ? 5 : 0);
			int btnPadding = 5;
			float textWidth = textStyle.CalcSize(content).x;
			float btnWidth = textWidth + (float)iconPadding + (float)(btnPadding * 2) + iconWidth;
			Rect r = (fromLeft ? rect.TakeFromLeft(btnWidth) : rect.TakeFromRight(btnWidth));
			bool clicked = GUI.Button(r, GUIContent.none, EditorStyles.toolbarButton);
			r.TakeFromLeft(btnPadding);
			Rect iconRect = r.TakeFromLeft(iconWidth);
			r.TakeFromLeft(iconPadding);
			Rect textRect = r.TakeFromLeft(textWidth);
			if (icon != SdfIconType.None)
			{
				SdfIcons.DrawIcon(iconRect.AlignCenterY(rect.height - 4f), icon, textStyle.normal.textColor);
			}
			GUI.Label(textRect, content, textStyle);
			return clicked;
		}

		internal static float CalcButtonWidth(Rect rect, string text)
		{
			GUIStyle textStyle = SirenixGUIStyles.WhiteLabel;
			GUIContent content = GUIHelper.TempContent(text);
			float iconWidth = rect.height;
			int iconPadding = 0;
			float textWidth = textStyle.CalcSize(content).x;
			return textWidth + (float)iconPadding + 5f + 20f + iconWidth;
		}

		internal static bool Button(ref Rect rect, string text, SdfIconType icon, Direction takeDirection, Direction iconDirection)
		{
			if (Event.current.type == EventType.Layout)
			{
				return false;
			}
			GUIStyle btnStyle = GUI.skin.button;
			GUIStyle textStyle = SirenixGUIStyles.WhiteLabel;
			GUIContent content = GUIHelper.TempContent(text);
			float iconWidth = rect.height;
			int iconPadding = 0;
			float textWidth = textStyle.CalcSize(content).x;
			float btnWidth = textWidth + (float)iconPadding + 5f + 20f + iconWidth;
			Rect r = rect.TakeFromDir(btnWidth, takeDirection);
			Rect btnRect = r;
			int id = GUIUtility.GetControlID(21345155, FocusType.Passive, rect);
			r.TakeFromDir(5f, iconDirection);
			Rect iconRect = r.TakeFromDir(iconWidth, iconDirection).AlignCenterY(r.height * 0.4f);
			r.TakeFromDir(iconPadding, iconDirection);
			Rect textRect = r.TakeFromDir(textWidth, iconDirection);
			Event current = Event.current;
			bool hover = btnRect.Contains(Event.current.mousePosition);
			switch (current.type)
			{
			case EventType.Repaint:
				btnStyle.Draw(btnRect, GUIContent.none, id, GUIUtility.hotControl == id, hover);
				SdfIcons.DrawIcon(iconRect, icon, textStyle.normal.textColor);
				GUI.Label(textRect, content, textStyle);
				break;
			case EventType.MouseDown:
				if (hover)
				{
					GUIUtility.hotControl = id;
					current.Use();
				}
				break;
			case EventType.KeyDown:
			{
				bool flag = current.alt || current.shift || current.command || current.control;
				if ((current.keyCode == KeyCode.Space || current.keyCode == KeyCode.Return || current.keyCode == KeyCode.KeypadEnter) && !flag && GUIUtility.keyboardControl == id)
				{
					current.Use();
					GUI.changed = true;
					return true;
				}
				break;
			}
			case EventType.MouseUp:
				if (GUIUtility.hotControl == id)
				{
					GUIHelper.RemoveFocusControl();
					current.Use();
					if (hover)
					{
						GUI.changed = true;
						return true;
					}
				}
				break;
			case EventType.MouseDrag:
				if (GUIUtility.hotControl == id)
				{
					current.Use();
				}
				break;
			}
			return false;
		}

		private void DrawEULA()
		{
			if (!AcceptEULAWindow.HasAcceptedEULA)
			{
				ShowContainer(eulaContainer);
				Rect rect = GUILayoutUtility.GetRect(0f, 0f, GUILayoutOptions.ExpandWidth().ExpandHeight());
				EditorGUI.DrawRect(rect, new Color(0f, 0f, 0f, 0.85f));
				if (AcceptEULAWindow.Draw(rect.AlignCenter(600f, 480f), closeWindowAfterAccept: false, this))
				{
					ShowContainer(popupContainer);
				}
				if (Event.current.type == EventType.MouseDown)
				{
					Event.current.Use();
				}
			}
			else
			{
				HideContainer(eulaContainer);
				eulaContainer.style.display = DisplayStyle.None;
			}
		}

		private void DrawVisualDesignerPopup()
		{
			if (!AcceptEULAWindow.HasAcceptedEULA)
			{
				HideContainer(popupContainer);
				return;
			}
			if (closedVisualDesignerPopup)
			{
				HideContainer(popupContainer);
				return;
			}
			ShowContainer(popupContainer);
			Rect rect = GUILayoutUtility.GetRect(0f, 0f, GUILayoutOptions.ExpandWidth().ExpandHeight());
			closedVisualDesignerPopup = VisualDesignerGettingStartedPopup.Draw(rect, this);
			if (Event.current.type == EventType.MouseDown)
			{
				Event.current.Use();
			}
		}
	}
}
