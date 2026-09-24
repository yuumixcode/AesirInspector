using System;

namespace Sirenix.OdinInspector.Editor.TypeSearch
{
	public struct TypeSearchInfo
	{
		public Type MatchType;

		public Type[] Targets;

		public TargetMatchCategory[] TargetCategories;

		public double Priority;

		public object CustomData;
	}
}
