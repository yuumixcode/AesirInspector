using System;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal static class MergedPatchCache
	{
		public static MergedTypePatch LastFetched;

		public static FixedDictionary<Type, MergedTypePatch> Cache = new FixedDictionary<Type, MergedTypePatch>(128);

		public static MergedTypePatch Get(InspectorProperty parentProperty)
		{
			Type typeofValue = parentProperty.Info.TypeOfValue;
			if (LastFetched != null && LastFetched.TargetType == typeofValue)
			{
				return LastFetched;
			}
			if (Cache.TryGetValue(typeofValue, out var result))
			{
				return result;
			}
			TypePatch typePatch = TypePatchCache.Get(typeofValue);
			result = new MergedTypePatch();
			result.GroupPatches.Clear();
			result.PropertyPatches.Clear();
			int depth = 0;
			foreach (TypePatch current in typePatch.TraverseFromRoot())
			{
				result.Merge(current, depth);
				depth++;
			}
			return result;
		}
	}
}
