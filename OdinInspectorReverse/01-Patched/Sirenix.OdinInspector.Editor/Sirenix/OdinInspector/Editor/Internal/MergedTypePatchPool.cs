using System.Collections.Generic;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal static class MergedTypePatchPool
	{
		public static readonly Stack<MergedTypePatch> Available = new Stack<MergedTypePatch>(128);

		public static MergedTypePatch Rent()
		{
			if (Available.Count > 0)
			{
				return Available.Pop();
			}
			return new MergedTypePatch();
		}

		public static void Return(MergedTypePatch patch)
		{
			foreach (MergedPropertyPatch propertyPatch in patch.PropertyPatches)
			{
				PropertyInfoPatchesPool.Return(propertyPatch);
			}
			patch.PropertyPatches.Clear();
			Available.Push(patch);
		}
	}
}
