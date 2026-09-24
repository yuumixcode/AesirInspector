using System.Collections.Generic;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal static class PropertyInfoPatchesPool
	{
		public static readonly Stack<MergedPropertyPatch> Available = new Stack<MergedPropertyPatch>(16);

		public static MergedPropertyPatch Rent()
		{
			if (Available.Count > 0)
			{
				return Available.Pop();
			}
			return new MergedPropertyPatch
			{
				DesiredIndex = -1,
				AttributePatches = RefList<BakedAttributePatch>.Empty
			};
		}

		public static void Return(MergedPropertyPatch patch)
		{
			patch.IsMoved = false;
			patch.ParentId = null;
			patch.Visibility = PropertyVisibilityState.Default;
			patch.DesiredIndex = -1;
			patch.DesiredIndexDepth = 0;
			patch.AttributePatches.Clear();
			Available.Push(patch);
		}
	}
}
