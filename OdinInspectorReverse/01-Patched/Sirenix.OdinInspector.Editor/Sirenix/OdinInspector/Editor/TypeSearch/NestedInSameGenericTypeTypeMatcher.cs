using System;
using Sirenix.Utilities;

namespace Sirenix.OdinInspector.Editor.TypeSearch
{
	public class NestedInSameGenericTypeTypeMatcher : TypeMatcher
	{
		public class Creator : TypeMatcherCreator
		{
			public override bool TryCreateMatcher(TypeSearchInfo info, out TypeMatcher matcher)
			{
				matcher = null;
				if (info.Targets.Length == 0 || !info.MatchType.IsNested || !info.Targets[0].IsNested)
				{
					return false;
				}
				if (!info.MatchType.DeclaringType.IsGenericType || !info.Targets[0].DeclaringType.IsGenericType)
				{
					return false;
				}
				Type matchTypeGenericDefinition = info.MatchType.DeclaringType.GetGenericTypeDefinition();
				if (matchTypeGenericDefinition != info.Targets[0].DeclaringType.GetGenericTypeDefinition())
				{
					return false;
				}
				matcher = new NestedInSameGenericTypeTypeMatcher
				{
					info = info,
					matchTypeGenericDefinition = matchTypeGenericDefinition,
					infoTargetGenericDefinition = info.Targets[0].GetGenericTypeDefinition()
				};
				return true;
			}
		}

		private TypeSearchInfo info;

		private Type matchTypeGenericDefinition;

		private Type infoTargetGenericDefinition;

		public override string Name => "Nested In Same Generic Type ---> Type<T1, [, T2]>.NestedType : Type<T1, [, T2]>.Match<Target>";

		public override Type Match(Type[] targets, ref bool stopMatching)
		{
			if (targets.Length != 1)
			{
				return null;
			}
			Type target = targets[0];
			if (!target.IsNested)
			{
				return null;
			}
			if (!target.DeclaringType.IsGenericType)
			{
				return null;
			}
			if (matchTypeGenericDefinition != target.DeclaringType.GetGenericTypeDefinition())
			{
				return null;
			}
			if (infoTargetGenericDefinition != target.GetGenericTypeDefinition())
			{
				return null;
			}
			Type[] args = target.GetGenericArguments();
			if (info.MatchType.AreGenericConstraintsSatisfiedBy(args))
			{
				return info.MatchType.MakeGenericType(args);
			}
			return null;
		}
	}
}
