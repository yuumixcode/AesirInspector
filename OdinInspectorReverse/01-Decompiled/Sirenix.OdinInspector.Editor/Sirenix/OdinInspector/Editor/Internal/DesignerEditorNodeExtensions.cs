using System;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal static class DesignerEditorNodeExtensions
	{
		public static bool IsAdjacentTo(this DesignerEditorNode node, DesignerEditorNode other, bool visualOrder = true)
		{
			if (node == null || other == null)
			{
				return false;
			}
			if (node == other)
			{
				return false;
			}
			if (node.Parent != other.Parent || node.Parent == null)
			{
				return false;
			}
			if (!visualOrder)
			{
				int i = node.GetIndex();
				int j = other.GetIndex();
				if (i >= 0 && j >= 0)
				{
					return Math.Abs(i - j) == 1;
				}
				return false;
			}
			int i2 = node.GetVisualIndex();
			int j2 = other.GetVisualIndex();
			if (i2 >= 0 && j2 >= 0)
			{
				return Math.Abs(i2 - j2) == 1;
			}
			return false;
		}
	}
}
