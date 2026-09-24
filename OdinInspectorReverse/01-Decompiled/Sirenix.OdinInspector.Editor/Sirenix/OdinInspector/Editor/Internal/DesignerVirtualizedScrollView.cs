using System;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal sealed class DesignerVirtualizedScrollView
	{
		public struct Enumerator : IDisposable
		{
			private readonly DesignerVirtualizedScrollView owner;

			private readonly int heightCount;

			private readonly float[] itemHeights;

			private readonly Rect frame;

			private readonly float scrollOffset;

			private int i;

			private float currentY;

			private int relativeIndex;

			private bool endReached;

			private bool disposed;

			public VisibleItem Current { get; private set; }

			public Enumerator(DesignerVirtualizedScrollView owner, Rect view)
			{
				this.owner = owner;
				this.owner.Begin(view);
				itemHeights = owner.itemHeights;
				heightCount = owner.heightCount;
				frame = owner.frame;
				scrollOffset = owner.scrollOffset;
				i = 0;
				currentY = 0f;
				relativeIndex = 0;
				endReached = false;
				disposed = false;
				Current = default(VisibleItem);
				while (i < heightCount)
				{
					float h = itemHeights[i];
					if (currentY + h < scrollOffset)
					{
						currentY += h;
						i++;
						continue;
					}
					break;
				}
			}

			public Enumerator GetEnumerator()
			{
				return this;
			}

			public bool MoveNext()
			{
				if (endReached || i >= heightCount)
				{
					return false;
				}
				float h = itemHeights[i];
				Current = new VisibleItem
				{
					Rect = new Rect(0f, currentY, frame.width, h),
					RelativeIndex = relativeIndex++,
					AbsoluteIndex = i
				};
				currentY += h;
				i++;
				if (currentY >= scrollOffset + frame.height)
				{
					endReached = true;
				}
				return true;
			}

			public void Dispose()
			{
				if (!disposed)
				{
					disposed = true;
					owner.End();
				}
			}
		}

		private float[] itemHeights;

		private int heightCount;

		private float totalHeight;

		private Rect frame;

		private float scrollOffset;

		private float targetScrollOffset;

		public bool UpdateScrollView;

		public float ScrollPosition
		{
			get
			{
				return scrollOffset;
			}
			set
			{
				scrollOffset = value;
				targetScrollOffset = value;
			}
		}

		public DesignerVirtualizedScrollView(int itemCount)
		{
			itemHeights = new float[itemCount];
		}

		public void BeginRectAllocations()
		{
			totalHeight = 0f;
			heightCount = 0;
			itemHeights = new float[16];
		}

		public void EndRectAllocations()
		{
		}

		public void GetRect(float height)
		{
			if (heightCount == itemHeights.Length)
			{
				Array.Resize(ref itemHeights, itemHeights.Length * 2);
			}
			itemHeights[heightCount] = height;
			totalHeight += height;
			heightCount++;
		}

		public Enumerator VisibleItems(Rect view)
		{
			return new Enumerator(this, view);
		}

		private void Begin(Rect view)
		{
			frame = view;
			Event e = Event.current;
			if (totalHeight <= frame.height)
			{
				scrollOffset = 0f;
				targetScrollOffset = 0f;
			}
			else if (e.IsHovering(frame) && e.type == EventType.ScrollWheel)
			{
				float delta = ((UnityShims.Misc.GetEventModifiers(e) == 1) ? e.delta.x : e.delta.y);
				targetScrollOffset += Mathf.Sign(delta) * 24f * 4f;
				targetScrollOffset = Mathf.Clamp(targetScrollOffset, 0f, totalHeight - frame.height);
				UpdateScrollView = false;
			}
			if (UpdateScrollView)
			{
				UpdateScrollView = false;
				targetScrollOffset = Mathf.Clamp(targetScrollOffset, 0f, totalHeight - frame.height);
			}
			scrollOffset = Mathf.Lerp(scrollOffset, targetScrollOffset, 10f * GUITimeHelper.LayoutDeltaTime);
			GUI.BeginClip(frame, new Vector2(0f, 0f - scrollOffset), Vector2.zero, resetOffset: false);
		}

		private void End()
		{
			GUI.EndClip();
			GUI.BeginClip(frame);
			Color c1 = Colors.Shadow;
			c1.a = Mathf.InverseLerp(0f, 30f, ScrollPosition) * 0.6f * GUI.color.a;
			GUI.DrawTexture(new Rect(0f, 0f, frame.width, 10f), DesignerTextures.TopToBottomFade, ScaleMode.StretchToFill, alphaBlend: true, 1f, c1, 0f, 0f);
			if (totalHeight > frame.height)
			{
				float pos = ScrollPosition + frame.height;
				Color c2 = Colors.Shadow;
				c2.a = Mathf.InverseLerp(totalHeight, totalHeight - 30f, pos) * 0.6f * GUI.color.a;
				GUI.DrawTexture(new Rect(0f, frame.height - 10f, frame.width, 10f), DesignerTextures.BottomToTopFade, ScaleMode.StretchToFill, alphaBlend: true, 1f, c2, 0f, 0f);
			}
			GUI.EndClip();
		}
	}
}
