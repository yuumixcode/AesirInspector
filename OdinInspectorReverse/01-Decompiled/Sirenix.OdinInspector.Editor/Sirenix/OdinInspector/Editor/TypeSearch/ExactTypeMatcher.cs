using System;

namespace Sirenix.OdinInspector.Editor.TypeSearch
{
	public class ExactTypeMatcher : TypeMatcher
	{
		public class Creator : TypeMatcherCreator
		{
			public override bool TryCreateMatcher(TypeSearchInfo info, out TypeMatcher matcher)
			{
				matcher = null;
				if (info.MatchType.IsGenericTypeDefinition)
				{
					return false;
				}
				matcher = new ExactTypeMatcher
				{
					info = info
				};
				return true;
			}
		}

		private TypeSearchInfo info;

		public override string Name => "Exact Match --> Type : Match[<Target>]";

		public override Type Match(Type[] targets, ref bool stopMatching)
		{
			if (targets.Length != info.Targets.Length)
			{
				return null;
			}
			for (int i = 0; i < targets.Length; i++)
			{
				if (targets[i] != info.Targets[i])
				{
					return null;
				}
			}
			return info.MatchType;
		}
	}
}
