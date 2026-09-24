using System;
using System.Collections.Generic;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal class VirtualizedScrollView
	{
		public struct VisibleSlot
		{
			public int Index;

			public Rect Rect;
		}

		private static readonly int ThumbHash = "SVScrollbarThumb".GetHashCode();

		private const int MinimumCapacity = 16;

		private const float EdgeFadeMaskHeight = 40f;

		private const int TrackWidth = 20;

		private const float MinThumbHeight = 30f;

		private Rect[] rects;

		private int count;

		private Rect viewportRect;

		private Rect lastViewport;

		private Rect unpaddedViewportRect;

		private float yMax;

		private float currentScrollPosition;

		private float targetScrollPosition;

		private float scrollSpeed;

		private float dragOffset;

		private float scrollStep = 300f;

		private Color? fadeColor;

		public VirtualizedScrollView(int initialCapacity = 16, float scrollStep = 300f, Color? fadeColor = null)
		{
			initialCapacity = Mathf.Max(initialCapacity, 16);
			rects = new Rect[initialCapacity];
			this.scrollStep = scrollStep;
			this.fadeColor = fadeColor;
		}

		public void Reset()
		{
			count = 0;
			yMax = 0f;
		}

		public void AllocateRect(Rect rect)
		{
			EnsureCapacity(count + 1);
			rects[count] = rect;
			count++;
			if (rect.yMax > yMax)
			{
				yMax = rect.yMax;
			}
		}

		public List<VisibleSlot> GetVisibleSlots(Rect viewportRect, Rect unpaddedViewportRect)
		{
			this.viewportRect = viewportRect;
			if (lastViewport != viewportRect)
			{
				lastViewport = viewportRect;
				targetScrollPosition = Mathf.Clamp(targetScrollPosition, 0f, MaxScroll());
				currentScrollPosition = targetScrollPosition;
			}
			this.unpaddedViewportRect = unpaddedViewportRect;
			Rect viewInContent = new Rect(this.viewportRect.x, this.viewportRect.y + currentScrollPosition, this.viewportRect.width, this.viewportRect.height);
			List<VisibleSlot> result = new List<VisibleSlot>(count);
			for (int i = 0; i < count; i++)
			{
				Rect nodeRect = rects[i];
				if (nodeRect.Overlaps(viewInContent))
				{
					result.Add(new VisibleSlot
					{
						Index = i,
						Rect = new Rect(nodeRect.x - this.viewportRect.x, nodeRect.y - this.viewportRect.y, nodeRect.width, nodeRect.height)
					});
				}
			}
			return result;
		}

		public void ScrollTo(int index)
		{
			if (index >= 0 && index < rects.Length)
			{
				float maxScroll = MaxScroll();
				targetScrollPosition = Mathf.Clamp(rects[index].y - viewportRect.y - 40f, 0f, maxScroll);
				scrollSpeed = 10f;
			}
		}

		public void ScrollToLastItem()
		{
			ScrollTo(count - 1);
		}

		public void AutoScrollNearEdges()
		{
			Event e = Event.current;
			float topEdge = viewportRect.y;
			float bottomEdge = viewportRect.yMax;
			float delta = (e.shift ? 800f : 200f) * GUITimeHelper.LayoutDeltaTime;
			if (e.mousePosition.y <= topEdge)
			{
				float maxScroll = MaxScroll();
				targetScrollPosition = Mathf.Clamp(targetScrollPosition - delta, 0f, maxScroll);
				scrollSpeed = Mathf.Max(scrollSpeed, 10f);
			}
			else if (e.mousePosition.y >= bottomEdge)
			{
				float maxScroll2 = MaxScroll();
				targetScrollPosition = Mathf.Clamp(targetScrollPosition + delta, 0f, maxScroll2);
				scrollSpeed = Mathf.Max(scrollSpeed, 10f);
			}
		}

		internal void Begin()
		{
			Event e = Event.current;
			float maxScroll = MaxScroll();
			if (ContentHeight() <= viewportRect.height)
			{
				currentScrollPosition = 0f;
				targetScrollPosition = 0f;
			}
			else if (e.type == EventType.ScrollWheel && e.IsMouseOver(unpaddedViewportRect))
			{
				scrollSpeed = 3.5f;
				float delta = (((UnityShims.Misc.GetEventModifiers(e) & 1) != 0) ? e.delta.x : e.delta.y);
				targetScrollPosition += Mathf.Sign(delta) * scrollStep;
				targetScrollPosition = Mathf.Clamp(targetScrollPosition, 0f, maxScroll);
			}
			currentScrollPosition = Mathf.Lerp(currentScrollPosition, targetScrollPosition, scrollSpeed * GUITimeHelper.LayoutDeltaTime);
			HandleScrollbarInput();
			GUI.BeginClip(viewportRect.VerticalPadding(1f), new Vector2(0f, 0f - currentScrollPosition), Vector2.zero, resetOffset: false);
		}

		internal void End()
		{
			GUI.EndClip();
			GUI.BeginClip(viewportRect);
			Color c = fadeColor ?? Colors.Shadows.DesignerEditorScrollView;
			GUI.DrawTexture(new Rect(0f, 0f, viewportRect.width, 40f), DesignerTextures.FadeMaskTop, ScaleMode.StretchToFill, alphaBlend: true, 0f, new Color(c.r, c.g, c.b, Mathf.InverseLerp(0f, 30f, currentScrollPosition)), 0f, 0f);
			float contentHeight = ContentHeight();
			if (contentHeight > viewportRect.height)
			{
				float pos = currentScrollPosition + viewportRect.height;
				GUI.DrawTexture(new Rect(0f, viewportRect.height - 40f, viewportRect.width, 40f), DesignerTextures.FadeMaskBottom, ScaleMode.StretchToFill, alphaBlend: true, 0f, new Color(c.r, c.g, c.b, Mathf.InverseLerp(contentHeight, contentHeight - 30f, pos)), 0f, 0f);
			}
			GUI.EndClip();
			DrawScrollbar();
		}

		private void HandleScrollbarInput()
		{
			if (!TryGetScrollbarGeometry(out var trackRect, out var thumbRect, out var maxScrollForThumb, out var thumbMinY, out var thumbMaxY, out var thumbHeight))
			{
				return;
			}
			Event e = Event.current;
			int controlId = GUIUtility.GetControlID(ThumbHash, FocusType.Passive, trackRect);
			if (e.OnMouseDown(0, useEvent: false))
			{
				if (thumbRect.Contains(e.mousePosition))
				{
					GUIUtility.hotControl = controlId;
					dragOffset = e.mousePosition.y - thumbRect.y;
					e.Use();
				}
				else if (trackRect.Contains(e.mousePosition))
				{
					GUIUtility.hotControl = controlId;
					float clickedY = Mathf.Clamp(e.mousePosition.y, trackRect.y, trackRect.yMax);
					float desiredThumbY = Mathf.Clamp(clickedY - thumbHeight * 0.5f, thumbMinY, thumbMaxY);
					float thumbTravel = trackRect.height - thumbHeight;
					float scrollPercent = ((thumbTravel <= 0f) ? 0f : ((desiredThumbY - thumbMinY) / thumbTravel));
					targetScrollPosition = Mathf.Clamp01(scrollPercent) * maxScrollForThumb;
					dragOffset = e.mousePosition.y - desiredThumbY;
					scrollSpeed = 10f;
					e.Use();
				}
			}
			if (e.type == EventType.MouseDrag && e.button == 0 && GUIUtility.hotControl == controlId)
			{
				float newThumbY = Mathf.Clamp(e.mousePosition.y - dragOffset, thumbMinY, thumbMaxY);
				float thumbTravel2 = trackRect.height - thumbHeight;
				float scrollPercent2 = ((thumbTravel2 <= 0f) ? 0f : ((newThumbY - thumbMinY) / thumbTravel2));
				targetScrollPosition = Mathf.Clamp01(scrollPercent2) * maxScrollForThumb;
				currentScrollPosition = targetScrollPosition;
				e.Use();
			}
			else if (e.type == EventType.MouseUp && e.button == 0 && GUIUtility.hotControl == controlId)
			{
				GUIUtility.hotControl = 0;
				e.Use();
			}
		}

		private void DrawScrollbar()
		{
			if (TryGetScrollbarGeometry(out var _, out var thumbRect, out var _, out var _, out var _, out var _))
			{
				Event e = Event.current;
				float basePadding = (e.IsHovering(thumbRect) ? 7f : 8f);
				thumbRect = InsetClamped(thumbRect, basePadding);
				if (thumbRect.width > 0f && thumbRect.height > 0f)
				{
					SirenixEditorGUI.DrawRoundRect(thumbRect, new Color(0.5f, 0.5f, 0.5f), float.MaxValue);
				}
			}
		}

		private bool TryGetScrollbarGeometry(out Rect trackRect, out Rect thumbRect, out float maxScrollForThumb, out float thumbMinY, out float thumbMaxY, out float thumbHeight)
		{
			trackRect = default(Rect);
			thumbRect = default(Rect);
			maxScrollForThumb = 0f;
			thumbMinY = 0f;
			thumbMaxY = 0f;
			thumbHeight = 0f;
			float contentHeight = ContentHeight();
			if (contentHeight <= unpaddedViewportRect.height)
			{
				return false;
			}
			trackRect = new Rect(unpaddedViewportRect.xMax - 20f, unpaddedViewportRect.y, 20f, unpaddedViewportRect.height);
			maxScrollForThumb = MaxScroll();
			float ratio = Mathf.Clamp01(unpaddedViewportRect.height / Mathf.Max(1f, contentHeight));
			float rawThumbHeight = trackRect.height * ratio;
			thumbHeight = Mathf.Min(trackRect.height, Mathf.Max(30f, rawThumbHeight));
			thumbMinY = trackRect.y;
			thumbMaxY = trackRect.yMax - thumbHeight;
			float scrollRatio = ((maxScrollForThumb <= 0f) ? 0f : (currentScrollPosition / maxScrollForThumb));
			scrollRatio = Mathf.Clamp01(scrollRatio);
			float thumbY = Mathf.Lerp(thumbMinY, thumbMaxY, scrollRatio);
			thumbRect = new Rect(trackRect.x, thumbY, trackRect.width, thumbHeight);
			return true;
		}

		private static Rect InsetClamped(Rect rect, float padding)
		{
			if (padding <= 0f)
			{
				return rect;
			}
			float maxHorizontalPadding = Mathf.Max(0f, (rect.width - 0.0001f) * 0.5f);
			float maxVPad = Mathf.Max(0f, (rect.height - 0.0001f) * 0.5f);
			float horizontalPadding = Mathf.Min(padding, maxHorizontalPadding);
			float verticalPadding = Mathf.Min(padding, maxVPad);
			float width = Mathf.Max(0f, rect.width - 2f * horizontalPadding);
			float height = Mathf.Max(0f, rect.height - 2f * verticalPadding);
			return new Rect(rect.x + horizontalPadding, rect.y + verticalPadding, width, height);
		}

		private void EnsureCapacity(int needed)
		{
			if (needed > rects.Length)
			{
				int newCapacity = Mathf.NextPowerOfTwo(Mathf.Max(needed, 16));
				Array.Resize(ref rects, newCapacity);
			}
		}

		private float ContentHeight()
		{
			return Mathf.Max(0f, yMax - viewportRect.y + 40f);
		}

		private float MaxScroll()
		{
			return Mathf.Max(0f, ContentHeight() - viewportRect.height);
		}
	}
}
