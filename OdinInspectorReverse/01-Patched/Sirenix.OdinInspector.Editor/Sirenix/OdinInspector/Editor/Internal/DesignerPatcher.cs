using System;
using System.Collections.Generic;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal static class DesignerPatcher
	{
		private static Type PatchTargetType;

		private static InspectorProperty lastFetchedProperty;

		private static MergedTypePatch lastFetchedPatch;

		private static Dictionary<Type, MergedTypePatch> MergedTypePatches = new Dictionary<Type, MergedTypePatch>(8);

		public static MergedTypePatch GetTypePatch(InspectorProperty parentProperty)
		{
			if (lastFetchedProperty == parentProperty)
			{
				return lastFetchedPatch;
			}
			lastFetchedProperty = parentProperty;
			Type type = parentProperty.Info.TypeOfValue;
			if (!DesignerUtils.CanTypeBeDesigned(type))
			{
				return lastFetchedPatch = null;
			}
			Type rootType = parentProperty.Tree.RootProperty.Info.TypeOfValue;
			if (rootType != PatchTargetType)
			{
				Reset(rootType);
			}
			if (!MergedTypePatches.TryGetValue(type, out var patch))
			{
				TypePatch typePatch = TypePatchCache.Get(type);
				if (typePatch == null)
				{
					return lastFetchedPatch = null;
				}
				patch = MergedTypePatchPool.Rent();
				if (patch.PropertyPatches == null)
				{
					patch.PropertyPatches = new List<MergedPropertyPatch>(32);
					patch.PropertyPatchMap = new Dictionary<string, MergedPropertyPatch>(32);
				}
				else
				{
					patch.PropertyPatches.Clear();
					patch.PropertyPatchMap.Clear();
				}
				foreach (TypePatch current in typePatch.TraverseFromRoot())
				{
					patch.Merge(current, 0);
				}
				MergedTypePatches[type] = patch;
			}
			return lastFetchedPatch = patch;
		}

		public static void ApplyAttributes(InspectorProperty parentProperty, List<InspectorPropertyInfo> infos)
		{
			MergedTypePatch patch = GetTypePatch(parentProperty);
			if (patch != null)
			{
				for (int i = 0; i < infos.Count; i++)
				{
					InspectorPropertyInfo current = infos[i];
					patch.ApplyPatches(current);
				}
			}
		}

		public static void Reset(Type targetType)
		{
			lastFetchedProperty = null;
			PatchTargetType = targetType;
			foreach (KeyValuePair<Type, MergedTypePatch> mergedTypePatch in MergedTypePatches)
			{
				MergedTypePatch mergedPatch = mergedTypePatch.Value;
				MergedTypePatchPool.Return(mergedPatch);
			}
			MergedTypePatches.Clear();
		}
	}
}
