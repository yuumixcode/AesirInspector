using System;
using System.IO;
using Sirenix.Serialization;
using UnityEditor;
using UnityEngine;

namespace Sirenix.Utilities.Editor
{
	/// <summary>
	/// This class is due to undergo refactoring.
	/// </summary>
	public class DragHandle
	{
		internal EditorWindow SourceWindow;

		private DropEvents dropEvent;

		private Vector2 mouseDownPostionOffset;

		private bool isMouseDown;

		private bool isDragging;

		private EventType lastSeenEvent;

		private static DragHandle currentHoveringDragHandle;

		public bool Enabled = true;

		public bool IsCrossWindowDrag { get; internal set; }

		public Rect Rect { get; set; }

		public Rect DraggingScreenRect
		{
			get
			{
				Rect r = Rect;
				Vector2 mp = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
				r.x = mp.x - mouseDownPostionOffset.x;
				r.y = mp.y - mouseDownPostionOffset.y;
				return r;
			}
		}

		public object Object { get; internal set; }

		public DragAndDropMethods DragAndDropMethod { get; internal set; }

		public bool OnDragStarted { get; private set; }

		public OnDragFinnished OnDragFinnished { get; set; }

		public bool IsDragging => DragAndDropManager.CurrentDraggingHandle == this;

		public bool IsHovering { get; private set; }

		public DragAndDropMethods CurrentMethod { get; private set; }

		public Vector2 MouseDownPostionOffset => mouseDownPostionOffset;

		public Rect? DragHandleRect { get; set; }

		internal Rect TempRect { get; set; }

		internal int LayoutDepth { get; set; }

		internal bool WillDrop { get; private set; }

		internal bool IsReadyToBeClaimed { get; private set; }

		internal bool IsBeingClaimed { get; private set; }

		internal void Update()
		{
			lastSeenEvent = Event.current.type;
			SetCurrentDragAndDropMethod();
			if (lastSeenEvent == EventType.Repaint)
			{
				FinalizeDropObject();
			}
			if (Event.current.isMouse || Event.current.type == EventType.DragUpdated)
			{
				Rect screenSpaceRect = ((!DragHandleRect.HasValue) ? Rect : DragHandleRect.Value);
				Vector2 screenPos = GUIUtility.GUIToScreenPoint(new Vector2(screenSpaceRect.x, screenSpaceRect.y));
				screenSpaceRect.x = screenPos.x;
				screenSpaceRect.y = screenPos.y;
				IsHovering = screenSpaceRect.Contains(GUIUtility.GUIToScreenPoint(Event.current.mousePosition));
			}
			OnDragStarted = false;
			if (!DragAndDropManager.IsDragInProgress)
			{
				if (Event.current.isMouse && IsHovering)
				{
					if (Enabled && Event.current.type == EventType.MouseDown && Event.current.button == 0)
					{
						isMouseDown = true;
						mouseDownPostionOffset = Event.current.mousePosition - new Vector2(Rect.x, Rect.y);
						GUIHelper.RemoveFocusControl();
						Event.current.Use();
						DragAndDrop.PrepareStartDrag();
					}
					if (isMouseDown && Event.current.type == EventType.MouseDrag)
					{
						isDragging = true;
						OnDragStarted = true;
						DragAndDrop.objectReferences = new UnityEngine.Object[0];
						DragAndDrop.paths = null;
						if (Object != null)
						{
							DragAndDrop.SetGenericData(Object.GetType().Name, Object);
						}
						DragAndDrop.StartDrag("Odin Drag Operation");
					}
				}
			}
			else
			{
				GUIHelper.RequestRepaint();
			}
			if (isDragging)
			{
				GUIHelper.RequestRepaint();
				DragAndDropManager.CurrentDraggingHandle = this;
				if (EditorWindow.mouseOverWindow != null)
				{
					EditorWindow.mouseOverWindow.Focus();
				}
				if (DragAndDropManager.WasDragPerformed)
				{
					IsReadyToBeClaimed = true;
					if (!DragAndDropManager.IsHoveringDropZone)
					{
						DropObject(DropEvents.Canceled);
					}
				}
				return;
			}
			if (IsHovering)
			{
				if (currentHoveringDragHandle == null || LayoutDepth >= currentHoveringDragHandle.LayoutDepth)
				{
					currentHoveringDragHandle = this;
				}
			}
			else if (currentHoveringDragHandle == this)
			{
				currentHoveringDragHandle = null;
			}
			IsHovering = currentHoveringDragHandle == this;
		}

		private void SetCurrentDragAndDropMethod()
		{
			if (Event.current.type == EventType.Repaint)
			{
				CurrentMethod = DragAndDropMethod;
				bool ctrl = (UnityShims.Misc.GetEventModifiers(Event.current) & 2) == 2;
				bool shift = (UnityShims.Misc.GetEventModifiers(Event.current) & 1) == 1;
				if (ctrl && (CurrentMethod == DragAndDropMethods.Reference || CurrentMethod == DragAndDropMethods.Move))
				{
					CurrentMethod = DragAndDropMethods.Copy;
				}
				else if (shift && CurrentMethod == DragAndDropMethods.Copy)
				{
					CurrentMethod = DragAndDropMethods.Reference;
				}
			}
		}

		private void FinalizeDropObject()
		{
			if (WillDrop)
			{
				WillDrop = false;
				IsBeingClaimed = false;
				IsReadyToBeClaimed = false;
				isDragging = false;
				isMouseDown = false;
				IsHovering = false;
				currentHoveringDragHandle = null;
				DragAndDropManager.WasDragPerformed = false;
				DragAndDropManager.CurrentDraggingHandle = null;
				DragAndDropManager.CurrentHoveringDropZone = null;
				GUIHelper.RequestRepaint();
				if (OnDragFinnished != null)
				{
					OnDragFinnished(dropEvent);
				}
			}
		}

		internal object DropObject()
		{
			DropEvents e;
			if (CurrentMethod == DragAndDropMethods.Move)
			{
				e = DropEvents.Moved;
			}
			else if (CurrentMethod == DragAndDropMethods.Reference)
			{
				e = DropEvents.Referenced;
			}
			else
			{
				if (CurrentMethod != DragAndDropMethods.Copy)
				{
					throw new NotImplementedException();
				}
				e = DropEvents.Copied;
			}
			return DropObject(e);
		}

		internal object DropObject(DropEvents dropEvent)
		{
			this.dropEvent = dropEvent;
			WillDrop = true;
			IsBeingClaimed = dropEvent != DropEvents.None && dropEvent != DropEvents.Canceled;
			if (lastSeenEvent == EventType.Repaint)
			{
				FinalizeDropObject();
			}
			if (dropEvent == DropEvents.Copied && Object != null)
			{
				if (Object.GetType().InheritsFrom(typeof(UnityEngine.Object)))
				{
					return Object;
				}
				using MemoryStream stream = new MemoryStream();
				Sirenix.Serialization.SerializationUtility.SerializeValue(Object, stream, DataFormat.Binary, out var unityReferences);
				stream.Position = 0L;
				return Sirenix.Serialization.SerializationUtility.DeserializeValue<object>(stream, DataFormat.Binary, unityReferences);
			}
			return Object;
		}
	}
}
