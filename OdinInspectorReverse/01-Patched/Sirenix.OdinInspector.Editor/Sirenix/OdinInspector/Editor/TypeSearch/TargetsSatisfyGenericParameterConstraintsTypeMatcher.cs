using System;
using Sirenix.Utilities;

namespace Sirenix.OdinInspector.Editor.TypeSearch
{
	public class TargetsSatisfyGenericParameterConstraintsTypeMatcher : TypeMatcher
	{
		public class Creator : TypeMatcherCreator
		{
			public override bool TryCreateMatcher(TypeSearchInfo info, out TypeMatcher matcher)
			{
				matcher = null;
				if (!info.MatchType.IsGenericType)
				{
					return false;
				}
				for (int i = 0; i < info.Targets.Length; i++)
				{
					if (!info.Targets[i].IsGenericParameter)
					{
						return false;
					}
				}
				matcher = new TargetsSatisfyGenericParameterConstraintsTypeMatcher
				{
					info = info,
					genericArgs = info.MatchType.GetGenericArguments()
				};
				return true;
			}
		}

		private TypeSearchInfo info;

		private Type[] genericArgs;

		public override string Name => "Targets Satisfy Generic Parameter Constraints --> Type<T1 [, T2]> : Match<T1 [, T2]> [where T1 [, T2] : constraints]";

		public override Type Match(Type[] targets, ref bool stopMatching)
		{
			if (TypeExtensions.AreGenericConstraintsSatisfiedBy(genericArgs, targets))
			{
				return info.MatchType.MakeGenericType(targets);
			}
			return null;
		}
	}
}
