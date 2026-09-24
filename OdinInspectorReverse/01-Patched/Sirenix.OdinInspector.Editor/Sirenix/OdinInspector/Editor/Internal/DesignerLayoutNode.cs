using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal struct DesignerLayoutNode
	{
		public Rect Rect;

		public Rect InteractRect;

		public DesignerEditorNode EditorNode;

		public int Depth;

		public bool IsVisible;

		public bool IsColumn;

		public bool IsDragZone;

		public bool IsDropZone;

		public static DesignerLayoutNode Invalid => new DesignerLayoutNode
		{
			Rect = new Rect(float.NaN, float.NaN, float.NaN, float.NaN)
		};

		public bool IsValid
		{
			get
			{
				if (!float.IsNaN(Rect.x))
				{
					return !float.IsNaN(Rect.y);
				}
				return false;
			}
		}

		public void DetermineVisibility(Vector2 verticalBounds)
		{
			IsVisible = InteractRect.yMax > verticalBounds.x && InteractRect.yMin < verticalBounds.y;
		}
	}
}
