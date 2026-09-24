using System;
using Sirenix.OdinInspector.Editor.TypeSearch;
using Sirenix.Utilities;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	internal static class RenderingLayerMaskDrawerMatcher
	{
		static RenderingLayerMaskDrawerMatcher()
		{
			if (!UnityVersion.IsVersionOrGreater(6000, 0))
			{
				return;
			}
			Type type = RenderingLayerMaskReflection.RenderingLayerMaskType;
			if (type == null || RenderingLayerMaskReflection.RenderingLayerMaskFieldInfo == null)
			{
				return;
			}
			Type drawerType = typeof(RenderingLayerMaskDrawer<>).MakeGenericType(type);
			Type targetMatchType = typeof(UnityObjectDrawer<>);
			DrawerUtilities.SearchIndex.MatchRules.Add(new TypeMatchRule("Unity RenderingLayerMask Matcher", delegate(TypeSearchInfo info, Type[] targets)
			{
				if (targets.Length != 1 || targets[0] != type)
				{
					return (Type)null;
				}
				return (info.MatchType != targetMatchType) ? null : drawerType;
			}));
		}
	}
}
