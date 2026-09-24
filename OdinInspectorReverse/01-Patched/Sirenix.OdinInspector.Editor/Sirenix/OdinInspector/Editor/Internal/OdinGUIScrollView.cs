using System;
using System.Collections.Generic;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Internal
{
	public class OdinGUIScrollView
	{
		internal struct RectInfo
		{
			public int ReferenceId;

			public float Indentation;

			public float Height;
		}

		internal struct VisibleItem
		{
			public Rect Position;

			public object Reference;

			public float Indentation;

			public VisibleItem(float x, float y, float width, float height, object reference, float indentation)
			{
				Position = new Rect(x, y, width, height);
				Reference = reference;
				Indentation = indentation;
			}
		}

		public readonly struct VisibleItems
		{
			public readonly int Length;

			public readonly int Offset;

			internal readonly VisibleItem[] VisibleItemsBuffer;

			public static VisibleItems None => new VisibleItems(0, 0, Array.Empty<VisibleItem>());

			public Rect this[int index] => GetRect(index);

			internal VisibleItems(int offset, int length, VisibleItem[] visibleItemsBuffer)
			{
				Offset = offset;
				Length = length;
				VisibleItemsBuffer = visibleItemsBuffer;
			}

			public Rect GetRect(int index)
			{
				return VisibleItemsBuffer[index].Position;
			}

			/// <summary>
			/// Checks whether the allocated <see cref="T:UnityEngine.Rect" /> has data associated with it.
			/// </summary>
			/// <param name="index">The index of the <see cref="T:UnityEngine.Rect" /> to check.</param>
			/// <returns><c>true</c> if the <see cref="T:UnityEngine.Rect" /> has data associated with it; otherwise <c>false</c>.</returns>
			public bool HasAssociatedData(int index)
			{
				return VisibleItemsBuffer[index].Reference != null;
			}

			/// <summary>
			/// Gets the data associated with the <see cref="T:UnityEngine.Rect" /> at the given <paramref name="index" />; this is the second parameter assigned in the <see cref="M:Sirenix.OdinInspector.Editor.Internal.OdinGUIScrollView.AllocateRect(System.Single,System.Object)" /> method.
			/// </summary>
			/// <param name="index">The index of the <see cref="T:UnityEngine.Rect" /> to retrieve the associated data from.</param>
			/// <returns>The associated data.</returns>
			public object GetAssociatedData(int index)
			{
				return VisibleItemsBuffer[index].Reference;
			}

			/// <summary>
			/// Gets the data associated with the <see cref="T:UnityEngine.Rect" /> at the given <see cref="!:index" />; this is the second parameter assigned in the <see cref="M:Sirenix.OdinInspector.Editor.Internal.OdinGUIScrollView.AllocateRect(System.Single,System.Object)" /> method.
			/// </summary>
			/// <param name="index">The index of the <see cref="T:UnityEngine.Rect" /> to retrieve the associated data from.</param>
			/// <typeparam name="T">The expected associated data type.</typeparam>
			/// <returns>The associated data.</returns>
			public T GetAssociatedData<T>(int index)
			{
				return (T)VisibleItemsBuffer[index].Reference;
			}

			/// <summary>
			/// Gets the indentation set for the <see cref="T:UnityEngine.Rect" /> at the given <paramref name="index" />.
			/// </summary>
			/// <param name="index">The <paramref name="index" /> of the <see cref="T:UnityEngine.Rect" /> to retrieve the indentation for.</param>
			/// <returns>The indentation for the <see cref="T:UnityEngine.Rect" />.</returns>
			/// <remarks>The indentation is set using <see cref="F:Sirenix.OdinInspector.Editor.Internal.OdinGUIScrollView.Indentation" /> during <see cref="M:Sirenix.OdinInspector.Editor.Internal.OdinGUIScrollView.BeginAllocations" /> and <see cref="M:Sirenix.OdinInspector.Editor.Internal.OdinGUIScrollView.EndAllocations" />.</remarks>
			public float GetIndentation(int index)
			{
				return VisibleItemsBuffer[index].Indentation;
			}

			/// <summary>
			/// Creates a <see cref="T:UnityEngine.Rect" /> representing all the visible <see cref="T:UnityEngine.Rect" />'s combined.
			/// </summary>
			/// <returns>The created <see cref="T:UnityEngine.Rect" />.</returns>
			public Rect GetCombinedRects()
			{
				if (Length < 1)
				{
					return Rect.zero;
				}
				Rect result = this[0];
				for (int i = 1; i < Length; i++)
				{
					result.height += VisibleItemsBuffer[i].Position.height;
				}
				return result;
			}
		}

		public const float SCROLL_BAR_SIZE = 12f;

		internal const int NONE_REFERENCE_ID = -1;

		public Rect Bounds;

		public Rect ViewRect;

		public Rect InteractRect;

		public float Indentation;

		internal int NextReferenceId;

		internal int NextRectInfoIndex;

		internal object[] ReferencedObjects;

		internal RectInfo[] RectInfos;

		internal VisibleItem[] VisibleItemsBuffer = new VisibleItem[64];

		internal Stack<int> FreedReferenceIds = new Stack<int>();

		internal bool AdjustViewForVerticalScrollBar;

		internal Rect VerticalScrollBarRect;

		internal Rect HorizontalScrollBarRect;

		internal bool isDraggingVertical;

		internal bool isDraggingHorizontal;

		internal bool isDraggingMouse;

		internal bool IsScrollWaitingUntilDone;

		internal Vector2 CurrentPosition = Vector2.zero;

		internal Vector2 NextPosition = Vector2.zero;

		internal SirenixAnimationUtility.InterpolatedVector2 AnimatedPosition = Vector2.zero;

		internal float ScrollSpeed;

		internal Easing ScrollEasing;

		public int Length => NextRectInfoIndex;

		public int ReferencedAmount => NextReferenceId;

		public bool IsBeyondVerticalBounds
		{
			get
			{
				if (AdjustViewForVerticalScrollBar)
				{
					return ViewRect.height > Bounds.height;
				}
				return ViewRect.height > InteractRect.height;
			}
		}

		public bool IsBeyondHorizontalBounds
		{
			get
			{
				if (AdjustViewForVerticalScrollBar)
				{
					return ViewRect.width > Bounds.width;
				}
				return ViewRect.width > InteractRect.width;
			}
		}

		public bool IsBeyondBounds
		{
			get
			{
				if (IsBeyondVerticalBounds)
				{
					return IsBeyondHorizontalBounds;
				}
				return false;
			}
		}

		public bool IsBeyondAnyBounds
		{
			get
			{
				if (!IsBeyondVerticalBounds)
				{
					return IsBeyondHorizontalBounds;
				}
				return true;
			}
		}

		public bool IsDraggingMouse
		{
			get
			{
				if (SharedUniqueControlId.IsActive)
				{
					return isDraggingMouse;
				}
				return false;
			}
		}

		public bool IsDraggingVerticalScrollBar
		{
			get
			{
				if (SharedUniqueControlId.IsActive)
				{
					return isDraggingVertical;
				}
				return false;
			}
		}

		public bool IsDraggingHorizontalScrollBar
		{
			get
			{
				if (SharedUniqueControlId.IsActive)
				{
					return isDraggingHorizontal;
				}
				return false;
			}
		}

		public Vector2 Position
		{
			get
			{
				return CurrentPosition;
			}
			set
			{
				CurrentPosition = value;
				NextPosition = value;
				AnimatedPosition = value;
			}
		}

		public float PositionX
		{
			get
			{
				return Position.x;
			}
			set
			{
				CurrentPosition.x = value;
				NextPosition.x = value;
				AnimatedPosition.Start.x = value;
				AnimatedPosition.Destination.x = value;
			}
		}

		public float PositionY
		{
			get
			{
				return Position.y;
			}
			set
			{
				CurrentPosition.y = value;
				NextPosition.y = value;
				AnimatedPosition.Start.y = value;
				AnimatedPosition.Destination.y = value;
			}
		}

		public OdinGUIScrollView(int capacity, int? referenceCapacity = null, bool adjustViewForVerticalScrollBar = true)
		{
			if (capacity < 1)
			{
				throw new ArgumentException("capacity can't be less than 1.");
			}
			if (referenceCapacity.HasValue)
			{
				if (referenceCapacity.Value < 1)
				{
					throw new ArgumentException("referenceCapacity can't be less than 1.");
				}
			}
			else
			{
				referenceCapacity = capacity;
			}
			AnimatedPosition.Time = 1f;
			RectInfos = new RectInfo[capacity];
			ReferencedObjects = new object[referenceCapacity.Value];
			AdjustViewForVerticalScrollBar = adjustViewForVerticalScrollBar;
		}

		public void SetBounds(Rect bounds, float viewWidth = float.NaN)
		{
			Bounds = bounds;
			ViewRect = new Rect(0f, 0f, float.IsNaN(viewWidth) ? bounds.width : viewWidth, 0f);
			InteractRect = bounds;
		}

		public void SetBoundsForCurrentAllocations(Rect bounds, float viewWidth = float.NaN)
		{
			Bounds = bounds;
			ViewRect.width = (float.IsNaN(viewWidth) ? bounds.width : viewWidth);
			InteractRect = bounds;
			VerticalScrollBarRect = Rect.zero;
			HorizontalScrollBarRect = Rect.zero;
			EndAllocations();
		}

		public void BeginAllocations()
		{
			ViewRect.height = 0f;
			NextReferenceId = 0;
			NextRectInfoIndex = 0;
			VerticalScrollBarRect = Rect.zero;
			HorizontalScrollBarRect = Rect.zero;
		}

		public void EndAllocations()
		{
			if (GUIUtility.hotControl == 0)
			{
				isDraggingHorizontal = false;
				isDraggingVertical = false;
			}
			if (IsBeyondHorizontalBounds)
			{
				HorizontalScrollBarRect = Bounds.TakeFromBottom(12f);
			}
			else
			{
				PositionX = 0f;
			}
			if (IsBeyondVerticalBounds)
			{
				VerticalScrollBarRect = Bounds.TakeFromRight(12f);
				if (HorizontalScrollBarRect != Rect.zero)
				{
					VerticalScrollBarRect.height += 12f;
					HorizontalScrollBarRect.width -= VerticalScrollBarRect.width;
				}
				if (AdjustViewForVerticalScrollBar)
				{
					ViewRect.width -= 12f;
				}
			}
			else
			{
				PositionY = 0f;
			}
		}

		public void Space(float amount)
		{
			AllocateRect(amount);
		}

		/// <summary>
		/// Allocates an <see cref="T:UnityEngine.Rect" /> in the view, with the option to associate a given <see cref="T:System.Object" /> with it.
		/// </summary>
		/// <param name="height"></param>
		/// <param name="reference"></param>
		/// <remarks>Ensure <see cref="M:Sirenix.OdinInspector.Editor.Internal.OdinGUIScrollView.BeginAllocations" /> is called before calling this, and ensure <see cref="M:Sirenix.OdinInspector.Editor.Internal.OdinGUIScrollView.EndAllocations" /> is called after you're done with <see cref="M:Sirenix.OdinInspector.Editor.Internal.OdinGUIScrollView.AllocateRect(System.Single,System.Object)" /></remarks>
		public void AllocateRect(float height, object reference = null)
		{
			if (NextRectInfoIndex >= RectInfos.Length)
			{
				Array.Resize(ref RectInfos, RectInfos.Length * 2);
			}
			ref RectInfo rectInfo = ref RectInfos[NextRectInfoIndex++];
			rectInfo.Indentation = Indentation;
			rectInfo.Height = height;
			if (reference != null)
			{
				if (NextReferenceId >= ReferencedObjects.Length)
				{
					Array.Resize(ref ReferencedObjects, ReferencedObjects.Length * 2);
				}
				ReferencedObjects[NextReferenceId] = reference;
				rectInfo.ReferenceId = NextReferenceId;
				NextReferenceId++;
			}
			else
			{
				rectInfo.ReferenceId = -1;
			}
			ViewRect.height += height;
		}

		public void ReallocateRect(int index, float height, object reference = null)
		{
			ref RectInfo currentRectInfo = ref RectInfos[index];
			float difference = currentRectInfo.Height - height;
			currentRectInfo.Height = height;
			ViewRect.height -= difference;
			if (reference != null)
			{
				if (currentRectInfo.ReferenceId != -1)
				{
					ReferencedObjects[currentRectInfo.ReferenceId] = reference;
					return;
				}
				if (FreedReferenceIds.Count > 0)
				{
					int referenceId = (currentRectInfo.ReferenceId = FreedReferenceIds.Pop());
					ReferencedObjects[referenceId] = reference;
					return;
				}
				if (NextReferenceId >= ReferencedObjects.Length)
				{
					Array.Resize(ref ReferencedObjects, ReferencedObjects.Length * 2);
				}
				ReferencedObjects[NextReferenceId] = reference;
				currentRectInfo.ReferenceId = NextReferenceId;
				NextReferenceId++;
			}
			else
			{
				if (currentRectInfo.ReferenceId != -1)
				{
					FreedReferenceIds.Push(currentRectInfo.ReferenceId);
				}
				currentRectInfo.ReferenceId = -1;
			}
		}

		public void BeginScrollView(Vector2? offset = null, Vector2? addViewSize = null, float scrollSpeed = 36f)
		{
			Vector2 offsetValue = offset ?? Vector2.zero;
			Rect clipRect = Bounds;
			clipRect.position += offsetValue;
			clipRect.size -= offsetValue;
			Rect clipViewRect = ViewRect;
			_ = VerticalScrollBarRect != Rect.zero;
			if (addViewSize.HasValue)
			{
				clipViewRect.size += addViewSize.Value;
			}
			if (Event.current.IsMouseOver(InteractRect) && Event.current.type == EventType.ScrollWheel)
			{
				if (UnityShims.Misc.GetEventModifiers(Event.current) == 1)
				{
					if (Event.current.delta.x != 0f)
					{
						NextPosition.x += Event.current.delta.x * scrollSpeed;
					}
					else
					{
						NextPosition.x += Event.current.delta.y * scrollSpeed;
					}
				}
				else
				{
					NextPosition.y += Event.current.delta.y * scrollSpeed;
				}
				if (NextPosition.y < 0f)
				{
					NextPosition.y = 0f;
				}
				ScrollTo(NextPosition, 2.857143f, Easing.OutCubic, waitUntilDone: false);
			}
			HandleSmoothScrolling();
			if (HorizontalScrollBarRect != Rect.zero && clipRect.width >= 0f)
			{
				float scaleFactor = clipRect.width / clipViewRect.width;
				if (Event.current.type == EventType.MouseDrag && IsDraggingHorizontalScrollBar)
				{
					PositionX += Event.current.delta.x / scaleFactor;
				}
				if (CurrentPosition.x + clipRect.width > clipViewRect.width)
				{
					float diff = CurrentPosition.x + clipRect.width - clipViewRect.width;
					PositionX -= diff;
				}
				if (CurrentPosition.x < 0f)
				{
					PositionX = 0f;
				}
				ScrollBackground(HorizontalScrollBarRect, isVertical: false);
				Rect buttonRect = HorizontalScrollBarRect;
				buttonRect.x += CurrentPosition.x * (HorizontalScrollBarRect.width / clipViewRect.width);
				buttonRect.width = HorizontalScrollBarRect.width * scaleFactor;
				if (ScrollButton(buttonRect, IsDraggingHorizontalScrollBar))
				{
					SharedUniqueControlId.SetActive();
					isDraggingHorizontal = true;
				}
				if (IsDraggingHorizontalScrollBar && Event.current.OnMouseUp(0))
				{
					SharedUniqueControlId.SetInactive();
					isDraggingHorizontal = false;
				}
			}
			else
			{
				PositionX = 0f;
			}
			if (VerticalScrollBarRect.width > 0f && clipRect.height >= 0f)
			{
				float scaleFactor2 = clipRect.height / clipViewRect.height;
				if (Event.current.type == EventType.MouseDrag && IsDraggingVerticalScrollBar)
				{
					PositionY += Event.current.delta.y / scaleFactor2;
				}
				if (CurrentPosition.y + clipRect.height > clipViewRect.height)
				{
					float diff2 = CurrentPosition.y + clipRect.height - clipViewRect.height;
					PositionY -= diff2;
				}
				if (CurrentPosition.y < 0f)
				{
					PositionY = 0f;
				}
				ScrollBackground(VerticalScrollBarRect, isVertical: true);
				Rect buttonRect2 = VerticalScrollBarRect;
				buttonRect2.y += CurrentPosition.y * (VerticalScrollBarRect.height / clipViewRect.height);
				buttonRect2.height = VerticalScrollBarRect.height * scaleFactor2;
				if (ScrollButton(buttonRect2, IsDraggingVerticalScrollBar))
				{
					SharedUniqueControlId.SetActive();
					isDraggingVertical = true;
				}
				if (IsDraggingVerticalScrollBar && Event.current.OnMouseUp(0))
				{
					SharedUniqueControlId.SetInactive();
					isDraggingVertical = false;
				}
			}
			else if (!IsBeyondVerticalBounds)
			{
				PositionY = 0f;
			}
			GUI.BeginClip(clipRect, -Position, Vector2.zero, resetOffset: false);
		}

		public void EndScrollView()
		{
			GUI.EndClip();
			if (IsBeyondAnyBounds)
			{
				GUIHelper.RequestRepaint();
			}
		}

		public Rect GetClipRect()
		{
			return Bounds;
		}

		public Rect GetViewClipRect()
		{
			return UnityShims.Rect.Ctor(Bounds.position, ViewRect.size);
		}

		public void BeginClip(Rect? clipRect = null, Vector2? offset = null, bool ignoreScrollX = false, bool ignoreScrollY = false)
		{
			Rect clipRectValue = clipRect ?? GetClipRect();
			Vector2 offsetValue = offset ?? Vector2.zero;
			clipRectValue.position += offsetValue;
			clipRectValue.size -= offsetValue;
			Vector2 scrollPosition = new Vector2(ignoreScrollX ? 0f : Position.x, ignoreScrollY ? 0f : Position.y);
			GUI.BeginClip(clipRectValue, -scrollPosition, Vector2.zero, resetOffset: false);
		}

		public void EndClip()
		{
			GUI.EndClip();
		}

		public void ScrollTo(Vector2 position, float speed, Easing easing = Easing.Linear, bool waitUntilDone = true)
		{
			NextPosition = position;
			ScrollEasing = easing;
			ScrollSpeed = speed;
			IsScrollWaitingUntilDone = waitUntilDone;
			if (waitUntilDone)
			{
				AnimatedPosition.ChangeDestination(NextPosition);
			}
		}

		public void ScrollTo(float speed, float xPosition = float.NaN, float yPosition = float.NaN, Easing easing = Easing.Linear, bool waitUntilDone = true)
		{
			NextPosition = new Vector2(float.IsNaN(xPosition) ? NextPosition.x : xPosition, float.IsNaN(yPosition) ? NextPosition.y : yPosition);
			ScrollEasing = easing;
			ScrollSpeed = speed;
			IsScrollWaitingUntilDone = waitUntilDone;
			if (waitUntilDone)
			{
				AnimatedPosition.ChangeDestination(NextPosition);
			}
		}

		public bool HandleMiddleMouseDrag(bool inverted, bool useEvents = false, float speed = 1f)
		{
			if (GUIUtility.hotControl == 0)
			{
				isDraggingMouse = false;
			}
			if (IsDraggingMouse && Event.current.OnMouseUp(2))
			{
				isDraggingMouse = false;
				SharedUniqueControlId.SetInactive();
			}
			if (Event.current.OnMouseDown(InteractRect, 2))
			{
				SharedUniqueControlId.SetActive();
				isDraggingMouse = true;
			}
			if (!IsDraggingMouse)
			{
				return false;
			}
			if (Event.current.type != EventType.MouseDrag)
			{
				return false;
			}
			if (inverted)
			{
				NextPosition -= Event.current.delta * speed;
			}
			else
			{
				NextPosition += Event.current.delta * speed;
			}
			ScrollTo(NextPosition, 2.857143f, Easing.OutCubic, waitUntilDone: false);
			if (useEvents)
			{
				Event.current.Use();
			}
			return true;
		}

		public object GetReferencedObject(int index)
		{
			return ReferencedObjects[index];
		}

		public void Resize(int capacity, int? referenceCapacity = null)
		{
			if (capacity < 1)
			{
				throw new ArgumentException("capacity can't be less than 1.");
			}
			if (referenceCapacity.HasValue)
			{
				if (referenceCapacity.Value < 1)
				{
					throw new ArgumentException("referenceCapacity can't be less than 1.");
				}
			}
			else
			{
				referenceCapacity = capacity;
			}
			Array.Resize(ref RectInfos, capacity);
			Array.Resize(ref ReferencedObjects, referenceCapacity.Value);
		}

		public void ResizeToFit()
		{
			Array.Resize(ref RectInfos, NextRectInfoIndex);
			Array.Resize(ref ReferencedObjects, NextReferenceId);
		}

		public VisibleItems GetVisibleItems()
		{
			int offset = 0;
			int length = -1;
			float currentVisibleHeight = 0f;
			float yMin = 0f;
			float yMax = 0f;
			for (int i = 0; i < Length; i++)
			{
				yMax += RectInfos[i].Height;
				if (!(Position.y >= yMin) || !(Position.y <= yMax))
				{
					yMin = yMax;
					continue;
				}
				offset = i;
				currentVisibleHeight = yMax - Position.y;
				break;
			}
			for (int j = offset + 1; j < Length; j++)
			{
				if (currentVisibleHeight >= Bounds.height)
				{
					length = j - offset + 1;
					break;
				}
				currentVisibleHeight += RectInfos[j].Height;
			}
			if (length == -1)
			{
				length = Length - offset;
			}
			if (length < 1)
			{
				return VisibleItems.None;
			}
			if (VisibleItemsBuffer.Length < length)
			{
				Array.Resize(ref VisibleItemsBuffer, length + 16);
			}
			for (int k = 0; k < length; k++)
			{
				ref RectInfo rectInfo = ref RectInfos[offset + k];
				if (rectInfo.ReferenceId == -1)
				{
					VisibleItemsBuffer[k] = new VisibleItem(ViewRect.x, yMin, ViewRect.width, rectInfo.Height, null, rectInfo.Indentation);
				}
				else
				{
					VisibleItemsBuffer[k] = new VisibleItem(ViewRect.x, yMin, ViewRect.width, rectInfo.Height, ReferencedObjects[rectInfo.ReferenceId], rectInfo.Indentation);
				}
				yMin += rectInfo.Height;
			}
			return new VisibleItems(offset, length, VisibleItemsBuffer);
		}

		private void HandleSmoothScrolling()
		{
			if (!IsScrollWaitingUntilDone)
			{
				AnimatedPosition.ChangeDestination(NextPosition);
			}
			if (AnimatedPosition.IsDone)
			{
				IsScrollWaitingUntilDone = false;
				return;
			}
			AnimatedPosition.Move(ScrollSpeed, ScrollEasing);
			CurrentPosition = AnimatedPosition;
		}

		public static bool ScrollButton(Rect position, bool isDragging)
		{
			if (position.height < 10f)
			{
				position.height = 10f;
			}
			Rect contentPosition = position.Padding(3f);
			bool isMouseOver = Event.current.IsMouseOver(position);
			if (EditorGUIUtility.isProSkin)
			{
				SirenixEditorGUI.DrawRoundRect(contentPosition, new FancyColor((isMouseOver || isDragging) ? 0.4f : 0.3f), 5f);
			}
			else
			{
				SirenixEditorGUI.DrawRoundRect(contentPosition, new FancyColor((isMouseOver || isDragging) ? 0.48f : 0.54f), 5f);
			}
			return Event.current.OnMouseDown(position, 0);
		}

		public static void ScrollBackground(Rect position, bool isVertical)
		{
			if (EditorGUIUtility.isProSkin)
			{
				EditorGUI.DrawRect(position, new FancyColor(0.1f));
			}
			else
			{
				EditorGUI.DrawRect(position, new FancyColor(0.66f));
			}
			EditorGUI.DrawRect(isVertical ? position.AlignLeft(1f) : position.AlignTop(1f), new FancyColor(0f, 0.4f));
		}
	}
}
