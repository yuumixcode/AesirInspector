using System;
using System.Collections.Generic;
using Sirenix.OdinInspector.Editor.TypeSearch;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	internal static class TwoDimensionalEnumArrayDrawerLocator
	{
		static TwoDimensionalEnumArrayDrawerLocator()
		{
			RegisterDrawer();
		}

		private static void RegisterDrawer()
		{
			HashSet<Type> canMatch = new HashSet<Type>
			{
				typeof(TwoDimensionalEnumArrayDrawer<, >),
				typeof(TwoDimensionalUnityObjectArrayDrawer<, >),
				typeof(TwoDimensionalGenericArrayDrawer<, >)
			};
			DrawerUtilities.SearchIndex.MatchRules.Add(new TypeMatchRule("Two Dimensional Array Custom Matcher", delegate(TypeSearchInfo info, Type[] targets)
			{
				if (targets.Length != 1)
				{
					return (Type)null;
				}
				Type type = targets[0];
				if (!type.IsArray || type.GetArrayRank() != 2)
				{
					return (Type)null;
				}
				if (!canMatch.Contains(info.MatchType))
				{
					return (Type)null;
				}
				Type elementType = type.GetElementType();
				if (elementType.IsEnum && info.MatchType == typeof(TwoDimensionalEnumArrayDrawer<, >))
				{
					return typeof(TwoDimensionalEnumArrayDrawer<, >).MakeGenericType(type, elementType);
				}
				if (typeof(UnityEngine.Object).IsAssignableFrom(elementType) && info.MatchType == typeof(TwoDimensionalUnityObjectArrayDrawer<, >))
				{
					return typeof(TwoDimensionalUnityObjectArrayDrawer<, >).MakeGenericType(type, elementType);
				}
				return (info.MatchType == typeof(TwoDimensionalGenericArrayDrawer<, >)) ? typeof(TwoDimensionalGenericArrayDrawer<, >).MakeGenericType(type, elementType) : null;
			}));
		}
	}
}
