using System.Collections.Generic;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal class ProjectionPass
	{
		private enum ProjectionKind
		{
			None,
			InsertVertical,
			SplitHorizontal
		}

		private struct ProjectionPlan
		{
			public ProjectionKind Kind;

			public DropDirection DropDirection;

			public int SubtreeStart;

			public int SubtreeEnd;

			public Rect GhostRect;

			public float SubtreeBaseX;

			public float SubtreeTargetX;

			public float SubtreeTargetXScale;

			public List<int> InflatedGroupIndices;

			public int ShiftStart;

			public int ShiftEnd;

			public int PushAfterIndex;
		}

		private sealed class Animator
		{
			private const float DampTime = 0.1f;

			public float CurrentHeight;

			public float SubtreeCurrentX;

			public float SubtreeCurrentScaleX = 1f;

			public float CurrentGhostX;

			public float CurrentGhostY;

			public float CurrentGhostWidth;

			public float CurrentGhostHeight;

			private float heightStart;

			private float heightTarget;

			private float heightT;

			private float subtreeXStart;

			private float subtreeXTarget;

			private float subtreeScaleStart;

			private float subtreeScaleTarget;

			private float subtreeT;

			private float ghostXStart;

			private float ghostYStart;

			private float ghostWStart;

			private float ghostHStart;

			private float ghostXTarget;

			private float ghostYTarget;

			private float ghostWTarget;

			private float ghostHTarget;

			private float ghostT;

			private ProjectionKind currentKind;

			public void Reset()
			{
				currentKind = ProjectionKind.None;
				CurrentHeight = 0f;
				SubtreeCurrentX = 0f;
				SubtreeCurrentScaleX = 1f;
				CurrentGhostX = (CurrentGhostY = (CurrentGhostWidth = (CurrentGhostHeight = 0f)));
				heightStart = (heightTarget = (heightT = 0f));
				subtreeXStart = (subtreeXTarget = 0f);
				subtreeScaleStart = (subtreeScaleTarget = 1f);
				subtreeT = 0f;
				ghostXStart = (ghostYStart = (ghostWStart = (ghostHStart = 0f)));
				ghostXTarget = (ghostYTarget = (ghostWTarget = (ghostHTarget = 0f)));
				ghostT = 0f;
			}

			public void Update(ProjectionPlan plan)
			{
				if (plan.Kind != currentKind)
				{
					Reset();
					switch (plan.Kind)
					{
					case ProjectionKind.InsertVertical:
						CurrentGhostX = plan.GhostRect.x;
						CurrentGhostWidth = plan.GhostRect.width;
						CurrentGhostY = plan.GhostRect.y;
						CurrentGhostHeight = 0f;
						break;
					case ProjectionKind.SplitHorizontal:
						SubtreeCurrentX = plan.SubtreeBaseX;
						SubtreeCurrentScaleX = 1f;
						CurrentGhostY = plan.GhostRect.y;
						CurrentGhostHeight = plan.GhostRect.height;
						if (plan.DropDirection == DropDirection.Left)
						{
							CurrentGhostX = plan.GhostRect.x;
							CurrentGhostWidth = 0f;
						}
						else
						{
							CurrentGhostX = plan.GhostRect.x + plan.GhostRect.width;
							CurrentGhostWidth = 0f;
						}
						break;
					}
					currentKind = plan.Kind;
					heightStart = CurrentHeight;
					heightTarget = CurrentHeight;
					heightT = 1f;
					subtreeXStart = SubtreeCurrentX;
					subtreeXTarget = SubtreeCurrentX;
					subtreeScaleStart = SubtreeCurrentScaleX;
					subtreeScaleTarget = SubtreeCurrentScaleX;
					subtreeT = 1f;
					ghostXStart = CurrentGhostX;
					ghostYStart = CurrentGhostY;
					ghostWStart = CurrentGhostWidth;
					ghostHStart = CurrentGhostHeight;
					ghostXTarget = CurrentGhostX;
					ghostYTarget = CurrentGhostY;
					ghostWTarget = CurrentGhostWidth;
					ghostHTarget = CurrentGhostHeight;
					ghostT = 1f;
				}
				if (plan.Kind == ProjectionKind.InsertVertical)
				{
					float targetHeight = ((plan.DropDirection == DropDirection.Center) ? plan.GhostRect.height : (plan.GhostRect.height + 8f));
					if (!Mathf.Approximately(targetHeight, heightTarget))
					{
						heightStart = CurrentHeight;
						heightTarget = targetHeight;
						heightT = 0f;
					}
					heightT = Mathf.Min(1f, heightT + GUITimeHelper.LayoutDeltaTime / 0.1f);
					CurrentHeight = Mathf.Lerp(heightStart, heightTarget, heightT);
				}
				if (plan.Kind == ProjectionKind.SplitHorizontal)
				{
					if (!Mathf.Approximately(plan.SubtreeTargetX, subtreeXTarget) || !Mathf.Approximately(plan.SubtreeTargetXScale, subtreeScaleTarget))
					{
						subtreeXStart = SubtreeCurrentX;
						subtreeXTarget = plan.SubtreeTargetX;
						subtreeScaleStart = SubtreeCurrentScaleX;
						subtreeScaleTarget = plan.SubtreeTargetXScale;
						subtreeT = 0f;
					}
					SubtreeCurrentX = subtreeXTarget;
					SubtreeCurrentScaleX = subtreeScaleTarget;
				}
				Rect targetGhostRect = plan.GhostRect;
				if (!Mathf.Approximately(targetGhostRect.x, ghostXTarget) || !Mathf.Approximately(targetGhostRect.y, ghostYTarget) || !Mathf.Approximately(targetGhostRect.width, ghostWTarget) || !Mathf.Approximately(targetGhostRect.height, ghostHTarget))
				{
					ghostXStart = CurrentGhostX;
					ghostYStart = CurrentGhostY;
					ghostWStart = CurrentGhostWidth;
					ghostHStart = CurrentGhostHeight;
					ghostXTarget = targetGhostRect.x;
					ghostYTarget = targetGhostRect.y;
					ghostWTarget = targetGhostRect.width;
					ghostHTarget = targetGhostRect.height;
					ghostT = 0f;
				}
				ghostT = Mathf.Min(1f, ghostT + GUITimeHelper.LayoutDeltaTime / 0.1f);
				CurrentGhostX = Mathf.Lerp(ghostXStart, ghostXTarget, ghostT);
				CurrentGhostY = Mathf.Lerp(ghostYStart, ghostYTarget, ghostT);
				CurrentGhostWidth = Mathf.Lerp(ghostWStart, ghostWTarget, ghostT);
				CurrentGhostHeight = Mathf.Lerp(ghostHStart, ghostHTarget, ghostT);
			}
		}

		public const float GhostVerticalHeight = 30f;

		public const float GhostHorizontalWidth = 60f;

		private List<Slot> _allSlots;

		private DragAndDropState _dragState;

		private ProjectionPlan _projectionPlan;

		private Animator _animator = new Animator();

		private bool _hasProjectionPlan;

		public void Begin(List<Slot> allSlots, DragAndDropState drag)
		{
			_allSlots = allSlots;
			_dragState = drag;
			_projectionPlan = BuildPlan(_allSlots, _dragState);
			UpdateAnimator(ref _animator, _projectionPlan);
			_hasProjectionPlan = true;
		}

		public Rect Run(Slot slot, Rect localRect)
		{
			if (!_hasProjectionPlan || _projectionPlan.Kind == ProjectionKind.None)
			{
				return localRect;
			}
			return GetProjectedRect(slot.Index, slot, _projectionPlan, _animator, localRect);
		}

		public Rect GetGhostRect(Rect viewportRect)
		{
			if (!_hasProjectionPlan || _projectionPlan.Kind == ProjectionKind.None)
			{
				return Rect.zero;
			}
			Rect ghostRect = new Rect(_animator.CurrentGhostX, _animator.CurrentGhostY, _animator.CurrentGhostWidth, _animator.CurrentGhostHeight);
			if (ghostRect.width <= 0f || ghostRect.height <= 0f)
			{
				return Rect.zero;
			}
			return new Rect(ghostRect.x - viewportRect.x, ghostRect.y - viewportRect.y, ghostRect.width, ghostRect.height);
		}

		public void End()
		{
			_hasProjectionPlan = false;
			_allSlots = null;
			_dragState = null;
		}

		private static ProjectionPlan BuildPlan(List<Slot> slots, DragAndDropState drag)
		{
			ProjectionPlan plan = new ProjectionPlan
			{
				Kind = ProjectionKind.None,
				InflatedGroupIndices = new List<int>(),
				ShiftStart = -1,
				ShiftEnd = -1,
				PushAfterIndex = -1
			};
			if (drag == null || drag.DropDirection == DropDirection.None || drag.SlotUnderCursor == null || drag.NodeBeingDragged == null)
			{
				return plan;
			}
			Slot targetSlot = drag.SlotUnderCursor;
			if (targetSlot.Node.NodeType == DesignerEditorNodeType.Group && targetSlot.Node.Children.Count == 0 && drag.DropDirection == DropDirection.Center)
			{
				if (targetSlot.Node.IsColumn)
				{
					Rect targetRect = slots[targetSlot.Index].Rect;
					float groupHeaderHeight = 28f;
					int groupPadding = 8;
					float ghostX = targetRect.x;
					float ghostY = targetRect.y + 20f + 8f;
					float ghostW = Mathf.Max(0f, targetRect.width);
					plan.Kind = ProjectionKind.InsertVertical;
					plan.DropDirection = DropDirection.Center;
					plan.SubtreeStart = targetSlot.Index;
					plan.SubtreeEnd = targetSlot.Index;
					plan.GhostRect = new Rect(ghostX, ghostY, ghostW, 30f);
					return plan;
				}
				Rect targetRect2 = slots[targetSlot.Index].Rect;
				float groupHeaderHeight2 = 28f;
				int groupPadding2 = 8;
				float ghostX2 = targetRect2.x + (float)groupPadding2;
				float ghostY2 = targetRect2.y + groupHeaderHeight2 + (float)groupPadding2;
				float ghostW2 = Mathf.Max(0f, targetRect2.width - 2f * (float)groupPadding2);
				plan.Kind = ProjectionKind.InsertVertical;
				plan.DropDirection = DropDirection.Center;
				plan.SubtreeStart = targetSlot.Index;
				plan.SubtreeEnd = targetSlot.Index;
				plan.GhostRect = new Rect(ghostX2, ghostY2, ghostW2, 30f);
				return plan;
			}
			var (subtreeStart, subtreeEnd) = FindSubtreeRange(slots, targetSlot.Index);
			if (drag.DropDirection == DropDirection.Top || drag.DropDirection == DropDirection.Bottom)
			{
				int containerIndex = GetParentIndex(slots, targetSlot.Index);
				List<int> inflatedIndices = new List<int>(8);
				for (int ancestorIndex = containerIndex; ancestorIndex >= 0; ancestorIndex = GetParentIndex(slots, ancestorIndex))
				{
					if (slots[ancestorIndex].Node.NodeType == DesignerEditorNodeType.Group)
					{
						inflatedIndices.Add(ancestorIndex);
					}
				}
				inflatedIndices.Sort();
				int ancestorColumnIndex = FindAncestorColumnIndex(slots, containerIndex);
				if (ancestorColumnIndex >= 0)
				{
					(int start, int end) tuple2 = FindSubtreeRange(slots, ancestorColumnIndex);
					int columnStart = tuple2.start;
					int columnEnd = tuple2.end;
					int ancestorRowIndex = GetParentIndex(slots, ancestorColumnIndex);
					if (ancestorRowIndex >= 0 && slots[ancestorRowIndex].Node.IsRow)
					{
						(int start, int end) tuple3 = FindSubtreeRange(slots, ancestorRowIndex);
						int rowStart = tuple3.start;
						int rowEnd = tuple3.end;
						int columnDepth = slots[ancestorColumnIndex].ZIndex;
						for (int i = rowStart + 1; i <= rowEnd; i++)
						{
							Slot slotAtI = slots[i];
							if (slotAtI.ZIndex == columnDepth && slotAtI.Node.IsColumn && inflatedIndices.BinarySearch(i) < 0)
							{
								inflatedIndices.Add(i);
							}
						}
						inflatedIndices.Sort();
						plan.PushAfterIndex = rowEnd;
					}
					plan.ShiftStart = columnStart;
					plan.ShiftEnd = columnEnd;
				}
				float insertY = ComputeInsertY(slots, targetSlot.Index, subtreeStart, subtreeEnd, drag.DropDirection);
				Rect targetRect3 = slots[targetSlot.Index].Rect;
				plan.Kind = ProjectionKind.InsertVertical;
				plan.DropDirection = drag.DropDirection;
				plan.SubtreeStart = subtreeStart;
				plan.SubtreeEnd = subtreeEnd;
				plan.GhostRect = new Rect(targetRect3.x, insertY, targetRect3.width, 30f);
				plan.InflatedGroupIndices = inflatedIndices;
				return plan;
			}
			Rect targetRect4 = slots[targetSlot.Index].Rect;
			Rect mainRect;
			Rect ghostRect;
			if (drag.DropDirection == DropDirection.Left)
			{
				mainRect = new Rect(targetRect4.x + 60f + 8f, targetRect4.y, targetRect4.width - 60f - 8f, targetRect4.height);
				ghostRect = new Rect(mainRect.xMin - 60f - 8f, targetRect4.y, 60f, targetRect4.height);
			}
			else
			{
				mainRect = new Rect(targetRect4.x, targetRect4.y, targetRect4.width - 60f - 8f, targetRect4.height);
				ghostRect = new Rect(mainRect.xMax + 8f, targetRect4.y, 60f, targetRect4.height);
			}
			float xScale = mainRect.width / targetRect4.width;
			plan.Kind = ProjectionKind.SplitHorizontal;
			plan.DropDirection = drag.DropDirection;
			plan.SubtreeStart = subtreeStart;
			plan.SubtreeEnd = subtreeEnd;
			plan.SubtreeBaseX = targetRect4.x;
			plan.SubtreeTargetX = mainRect.x;
			plan.SubtreeTargetXScale = xScale;
			plan.GhostRect = ghostRect;
			return plan;
		}

		private static float ComputeInsertY(List<Slot> slots, int targetIndex, int subtreeStart, int subtreeEnd, DropDirection direction)
		{
			if (direction == DropDirection.Top)
			{
				int parentIndex = GetParentIndex(slots, targetIndex);
				int containerDepth = ((parentIndex >= 0) ? slots[parentIndex].ZIndex : (-1));
				int previousSiblingIndex = FindPrevSibling(slots, subtreeStart, containerDepth + 1);
				float previousBottom = ((previousSiblingIndex >= 0) ? slots[previousSiblingIndex].Rect.yMax : (slots[subtreeStart].Rect.y - 8f));
				float currentTop = slots[subtreeStart].Rect.y;
				return 0.5f * (previousBottom + currentTop) + 4f;
			}
			int parentIndex2 = GetParentIndex(slots, targetIndex);
			int containerDepth2 = ((parentIndex2 >= 0) ? slots[parentIndex2].ZIndex : (-1));
			int nextSiblingIndex = FindNextSibling(slots, subtreeEnd, containerDepth2 + 1);
			float targetBottom = slots[targetIndex].Rect.yMax;
			float a = targetBottom;
			float b = ((nextSiblingIndex >= 0) ? slots[nextSiblingIndex].Rect.y : (targetBottom + 8f));
			return 0.5f * (a + b) + 4f;
		}

		private static int FindPrevSibling(List<Slot> slots, int fromIndex, int wantedDepth)
		{
			int i = fromIndex - 1;
			while (i >= 0 && slots[i].ZIndex >= wantedDepth)
			{
				if (slots[i].ZIndex == wantedDepth)
				{
					return i;
				}
				i--;
			}
			return -1;
		}

		private static int FindNextSibling(List<Slot> slots, int fromIndex, int wantedDepth)
		{
			for (int i = fromIndex + 1; i < slots.Count && slots[i].ZIndex >= wantedDepth; i++)
			{
				if (slots[i].ZIndex == wantedDepth)
				{
					return i;
				}
			}
			return -1;
		}

		private static int GetParentIndex(List<Slot> slots, int childIndex)
		{
			int childDepth = slots[childIndex].ZIndex;
			for (int i = childIndex - 1; i >= 0; i--)
			{
				if (slots[i].ZIndex < childDepth)
				{
					return i;
				}
			}
			return -1;
		}

		private static (int start, int end) FindSubtreeRange(List<Slot> slots, int targetIndex)
		{
			int end = targetIndex;
			int depth = slots[targetIndex].ZIndex;
			for (int i = targetIndex + 1; i < slots.Count && slots[i].ZIndex > depth; i++)
			{
				end = i;
			}
			return (start: targetIndex, end: end);
		}

		private static int FindAncestorColumnIndex(List<Slot> slots, int fromIndex)
		{
			for (int i = fromIndex; i >= 0; i = GetParentIndex(slots, i))
			{
				if (i >= 0 && slots[i].Node.IsColumn)
				{
					return i;
				}
			}
			return -1;
		}

		private static void UpdateAnimator(ref Animator animator, ProjectionPlan plan)
		{
			animator = animator ?? new Animator();
			animator.Update(plan);
		}

		private static Rect GetProjectedRect(int index, Slot slot, ProjectionPlan plan, Animator animator, Rect current)
		{
			switch (plan.Kind)
			{
			case ProjectionKind.InsertVertical:
			{
				Rect rect = current;
				if ((plan.ShiftStart < 0 || (index >= plan.ShiftStart && index <= plan.ShiftEnd)) && IsAfterBoundary(index, plan))
				{
					rect.y += animator.CurrentHeight;
				}
				if (plan.PushAfterIndex >= 0 && index > plan.PushAfterIndex)
				{
					rect.y += animator.CurrentHeight;
				}
				if (plan.DropDirection != DropDirection.Center)
				{
					List<int> inflateSet = plan.InflatedGroupIndices;
					if (slot.Node.NodeType == DesignerEditorNodeType.Group && inflateSet != null && inflateSet.BinarySearch(index) >= 0)
					{
						rect.height += animator.CurrentHeight;
					}
				}
				return rect;
			}
			case ProjectionKind.SplitHorizontal:
			{
				if (index < plan.SubtreeStart || index > plan.SubtreeEnd)
				{
					return current;
				}
				float newX = animator.SubtreeCurrentX + (current.x - plan.SubtreeBaseX) * animator.SubtreeCurrentScaleX;
				float newW = current.width * animator.SubtreeCurrentScaleX;
				return new Rect(newX, current.y, newW, current.height);
			}
			default:
				return current;
			}
		}

		private static bool IsAfterBoundary(int index, ProjectionPlan plan)
		{
			if (plan.Kind != ProjectionKind.InsertVertical)
			{
				return false;
			}
			if (plan.DropDirection == DropDirection.Center)
			{
				return false;
			}
			if (plan.DropDirection != DropDirection.Top)
			{
				return index > plan.SubtreeEnd;
			}
			return index >= plan.SubtreeStart;
		}
	}
}
