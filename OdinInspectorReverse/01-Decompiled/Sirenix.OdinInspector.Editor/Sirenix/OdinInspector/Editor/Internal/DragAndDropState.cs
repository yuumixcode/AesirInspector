using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal class DragAndDropState
	{
		public const int DragStartThreshold = 15;

		public bool DragPending;

		public bool DragActive;

		public DesignerEditorNode NodeBeingDragged;

		public Slot SlotUnderCursor;

		public Vector2 DragStartPosition;

		public DropDirection DropDirection;

		public void Reset()
		{
			DragPending = false;
			DragActive = false;
			NodeBeingDragged = null;
			SlotUnderCursor = null;
			DragStartPosition = Vector2.zero;
			DropDirection = DropDirection.None;
		}
	}
}
