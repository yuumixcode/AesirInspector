using System;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal class FancyWindow : EditorWindow
	{
		[Flags]
		public enum ResizeEdge
		{
			None = 0,
			Left = 1,
			Right = 2,
			Top = 4,
			Bottom = 8
		}

		public string Title;

		public Rect ContentRect;

		public bool IsDragging;

		public Vector2 DragStartPosition;

		public bool IsResizing;

		public ResizeEdge ActiveResizeEdge;

		public Vector2 ResizeStartMouseScreen;

		public Rect ResizeStartWindowRect;

		private const int TitleBarHeight = 26;

		private static Texture2D gradientHover;

		public static Color ButtonHaloBg
		{
			get
			{
				if (!EditorGUIUtility.isProSkin)
				{
					return new Color(1f, 1f, 1f);
				}
				return new Color(0.3f, 0.3f, 0.3f);
			}
		}

		public static Color CloseHalo
		{
			get
			{
				if (!EditorGUIUtility.isProSkin)
				{
					return new Color(0.47f, 0f, 0f);
				}
				return new Color(1f, 0.27f, 0.26f);
			}
		}

		public static Texture2D GradientHover
		{
			get
			{
				if (gradientHover != null)
				{
					return gradientHover;
				}
				gradientHover = TMP(new Color(1f, 1f, 1f, 0.25f), new Color(1f, 1f, 1f, 0f), 256, 0.1);
				gradientHover.hideFlags = HideFlags.HideAndDontSave;
				CleanupUtility.DestroyObjectOnAssemblyReload(gradientHover);
				return gradientHover;
			}
		}

		private void OnGUI()
		{
			Repaint();
			Rect windowRect = base.position.SetPosition(Vector2.zero);
			Rect titleBarRect = windowRect.TakeFromTop(26f);
			ContentRect = windowRect;
			DrawTitleBar(titleBarRect, Title);
			int viewId = GUIUtility.GetControlID(FocusType.Passive);
			(IsResizing, ActiveResizeEdge, ResizeStartMouseScreen, ResizeStartWindowRect) = HandleWindowResizing(IsResizing, ActiveResizeEdge, ResizeStartMouseScreen, ResizeStartWindowRect, this, viewId, base.position.SetPosition(Vector2.zero));
			if (!IsResizing && titleBarRect.Contains(Event.current.mousePosition))
			{
				(IsDragging, DragStartPosition) = HandleWindowMovement(IsDragging, DragStartPosition, this, viewId);
			}
			else
			{
				IsDragging = false;
			}
			if (IsDragging)
			{
				EditorGUIUtility.AddCursorRect(windowRect, MouseCursor.Pan);
			}
			OnDraw();
			SirenixEditorGUI.DrawBorders(base.position.SetPosition(Vector2.zero), 1, Colors.AttributePopup.Border);
		}

		protected virtual void OnDraw()
		{
		}

		public static (bool isResizing, ResizeEdge activeEdge, Vector2 startMouseScreen, Rect startWindowRect) HandleWindowResizing(bool isResizing, ResizeEdge activeEdge, Vector2 startMouseScreen, Rect startWindowRect, EditorWindow editorWindow, int controlID, Rect windowRectLocal, float edgeThickness = 6f, float minWidth = 240f, float minHeight = 120f)
		{
			Event e = Event.current;
			Vector2 mp = e.mousePosition;
			bool overLeft = mp.x <= edgeThickness;
			bool overRight = mp.x >= windowRectLocal.width - edgeThickness;
			bool overTop = mp.y <= edgeThickness;
			bool overBottom = mp.y >= windowRectLocal.height - edgeThickness;
			ResizeEdge hoveredEdge = ResizeEdge.None;
			if (overLeft)
			{
				hoveredEdge |= ResizeEdge.Left;
			}
			if (overRight)
			{
				hoveredEdge |= ResizeEdge.Right;
			}
			if (overTop)
			{
				hoveredEdge |= ResizeEdge.Top;
			}
			if (overBottom)
			{
				hoveredEdge |= ResizeEdge.Bottom;
			}
			if (!isResizing && hoveredEdge != ResizeEdge.None)
			{
				MouseCursor cursor = MouseCursor.Arrow;
				bool left = (hoveredEdge & ResizeEdge.Left) != 0;
				bool right = (hoveredEdge & ResizeEdge.Right) != 0;
				bool top = (hoveredEdge & ResizeEdge.Top) != 0;
				bool bot = (hoveredEdge & ResizeEdge.Bottom) != 0;
				if ((left && top) || (right && bot))
				{
					cursor = MouseCursor.ResizeUpLeft;
				}
				else if ((right && top) || (left && bot))
				{
					cursor = MouseCursor.ResizeUpRight;
				}
				else if (left || right)
				{
					cursor = MouseCursor.ResizeHorizontal;
				}
				else if (top || bot)
				{
					cursor = MouseCursor.ResizeVertical;
				}
				EditorGUIUtility.AddCursorRect(windowRectLocal, cursor);
			}
			switch (e.type)
			{
			case EventType.MouseDown:
				if (e.button == 0 && hoveredEdge != ResizeEdge.None && GUIUtility.hotControl == 0)
				{
					isResizing = true;
					activeEdge = hoveredEdge;
					startMouseScreen = GUIUtility.GUIToScreenPoint(e.mousePosition);
					startWindowRect = editorWindow.position;
					GUIUtility.hotControl = controlID;
					e.Use();
				}
				break;
			case EventType.MouseDrag:
			{
				if (!isResizing || GUIUtility.hotControl != controlID)
				{
					break;
				}
				Vector2 mouseScreen = GUIUtility.GUIToScreenPoint(e.mousePosition);
				Vector2 delta = mouseScreen - startMouseScreen;
				Rect r = startWindowRect;
				bool left2 = (activeEdge & ResizeEdge.Left) != 0;
				bool right2 = (activeEdge & ResizeEdge.Right) != 0;
				bool top2 = (activeEdge & ResizeEdge.Top) != 0;
				bool bot2 = (activeEdge & ResizeEdge.Bottom) != 0;
				if (left2)
				{
					r.xMin += delta.x;
				}
				if (right2)
				{
					r.xMax += delta.x;
				}
				if (top2)
				{
					r.yMin += delta.y;
				}
				if (bot2)
				{
					r.yMax += delta.y;
				}
				if (r.width < minWidth)
				{
					if (left2 && !right2)
					{
						r.xMin = r.xMax - minWidth;
					}
					else
					{
						r.xMax = r.xMin + minWidth;
					}
				}
				if (r.height < minHeight)
				{
					if (top2 && !bot2)
					{
						r.yMin = r.yMax - minHeight;
					}
					else
					{
						r.yMax = r.yMin + minHeight;
					}
				}
				editorWindow.position = r;
				e.Use();
				break;
			}
			case EventType.MouseUp:
				if (isResizing && GUIUtility.hotControl == controlID)
				{
					isResizing = false;
					activeEdge = ResizeEdge.None;
					GUIUtility.hotControl = 0;
					e.Use();
				}
				break;
			}
			return (isResizing: isResizing, activeEdge: activeEdge, startMouseScreen: startMouseScreen, startWindowRect: startWindowRect);
		}

		public static (bool, Vector2) HandleWindowMovement(bool isDragging, Vector2 dragStartPosition, EditorWindow editorWindow, int controlID)
		{
			Event e = Event.current;
			switch (e.type)
			{
			case EventType.MouseDown:
				if (e.button == 0 && GUIUtility.hotControl == 0)
				{
					isDragging = true;
					dragStartPosition = GUIUtility.GUIToScreenPoint(e.mousePosition - editorWindow.position.position);
					GUIUtility.hotControl = controlID;
					e.Use();
				}
				break;
			case EventType.MouseDrag:
				if (isDragging && GUIUtility.hotControl == controlID)
				{
					Vector2 screenSpaceMousePos = GUIUtility.GUIToScreenPoint(e.mousePosition);
					UnityShims.Rect.Ctor(out var newRect, screenSpaceMousePos - dragStartPosition, editorWindow.position.size);
					editorWindow.position = newRect;
					e.Use();
				}
				break;
			case EventType.MouseUp:
				if (isDragging && GUIUtility.hotControl == controlID)
				{
					isDragging = false;
					GUIUtility.hotControl = 0;
					e.Use();
				}
				break;
			}
			return (isDragging, dragStartPosition);
		}

		private void DrawTitleBar(Rect rect, string title)
		{
			Event e = Event.current;
			Rect headerRect = GUILayoutUtility.GetRect(0f, 26f, GUILayoutOptions.ExpandWidth().ExpandHeight(expand: false));
			EditorGUI.DrawRect(headerRect, Colors.AttributePopup.TitleBarBg);
			EditorGUI.DrawRect(headerRect.AlignBottom(1f).AddY(1f), SirenixGUIStyles.BorderColor);
			headerRect = headerRect.HorizontalPadding(6f, 0f);
			GUI.Label(headerRect, title, SirenixGUIStyles.BoldLabel);
			Rect closeRect = headerRect.TakeFromRight(headerRect.height);
			if (GUI.Button(closeRect, new GUIContent("", ""), GUIStyle.none))
			{
				Close();
			}
			Color iconColor = (closeRect.Contains(e.mousePosition) ? Color.white : EditorStyles.label.normal.textColor);
			SdfIcons.DrawIcon(closeRect.Padding(7f), SdfIconType.X, iconColor);
			bool hovering = e.IsHovering(rect);
			ref SirenixAnimationUtility.InterpolatedFloat tfHover = ref SirenixAnimationUtility.GetTemporaryFloat("title_bar_hover", 0f);
			if (e.type == EventType.Repaint)
			{
				tfHover.ChangeDestination(hovering ? 1f : 0f);
			}
			tfHover.Move(2.5f);
			if ((float)tfHover > 0f)
			{
				GUI.BeginClip(rect);
				float mouseX = e.mousePosition.x;
				float blendFactor = 0f;
				Color haloColor = ButtonHaloBg;
				float blendStart = rect.width - rect.width * 0.15f;
				if (mouseX > blendStart)
				{
					blendFactor = Mathf.InverseLerp(blendStart, rect.width, mouseX);
					haloColor = CloseHalo;
				}
				float cursorSize = 200f + blendFactor * 256f;
				Rect cursorRect = new Rect(0f, 0f, cursorSize, cursorSize);
				cursorRect.x = Event.current.mousePosition.x - cursorRect.height * 0.5f;
				cursorRect.y = Event.current.mousePosition.y - cursorRect.width * 0.5f;
				Color cursorColor = Color.Lerp(ButtonHaloBg, haloColor, blendFactor);
				cursorColor.a = tfHover;
				GUI.DrawTexture(cursorRect.Expand(50f), GradientHover, ScaleMode.ScaleToFit, alphaBlend: true, 1f, cursorColor, Vector4.zero, Vector4.zero);
				GUI.EndClip();
			}
		}

		public static Texture2D TMP(Color centerColor, Color edgeColor, int size, double radiusInner = 0.2, double radiusOuter = 1.25, double noiseIntensity = 0.005)
		{
			Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, mipChain: false);
			texture.filterMode = FilterMode.Bilinear;
			texture.wrapMode = TextureWrapMode.Clamp;
			Vector2 center = new Vector2((float)size / 2f, (float)size / 2f);
			double maxDistance = (double)size / 2.0;
			System.Random rand = new System.Random();
			for (int y = 0; y < size; y++)
			{
				for (int x = 0; x < size; x++)
				{
					double dx = (float)x - center.x;
					double dy = (float)y - center.y;
					double dist = Math.Sqrt(dx * dx + dy * dy) / maxDistance;
					dist = Math.Min(1.0, Math.Max(0.0, dist));
					double t;
					if (dist <= radiusInner)
					{
						t = 0.0;
					}
					else if (dist >= radiusOuter)
					{
						t = 1.0;
					}
					else
					{
						double normDist = (dist - radiusInner) / (radiusOuter - radiusInner);
						t = ((normDist < 0.5) ? (4.0 * normDist * normDist * normDist) : (1.0 - Math.Pow(-2.0 * normDist + 2.0, 3.0) / 2.0));
					}
					double r = (double)centerColor.r + t * (double)(edgeColor.r - centerColor.r);
					double g = (double)centerColor.g + t * (double)(edgeColor.g - centerColor.g);
					double b = (double)centerColor.b + t * (double)(edgeColor.b - centerColor.b);
					double a = (double)centerColor.a + t * (double)(edgeColor.a - centerColor.a);
					int hash = (x * 73856093) ^ (y * 19349663);
					rand = new System.Random(hash);
					double noise = (rand.NextDouble() * 2.0 - 1.0) * noiseIntensity;
					r = Math.Min(1.0, Math.Max(0.0, r + noise));
					g = Math.Min(1.0, Math.Max(0.0, g + noise));
					b = Math.Min(1.0, Math.Max(0.0, b + noise));
					a = Math.Min(1.0, Math.Max(0.0, a + noise * 0.5));
					texture.SetPixel(x, y, new Color((float)r, (float)g, (float)b, (float)a));
				}
			}
			texture.Apply();
			return texture;
		}
	}
}
