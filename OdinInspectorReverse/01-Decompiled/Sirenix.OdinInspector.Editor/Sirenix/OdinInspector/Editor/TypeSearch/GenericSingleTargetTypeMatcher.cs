using System;
using Sirenix.Utilities;

namespace Sirenix.OdinInspector.Editor.TypeSearch
{
	public class GenericSingleTargetTypeMatcher : TypeMatcher
	{
		public class Creator : TypeMatcherCreator
		{
			public override bool TryCreateMatcher(TypeSearchInfo info, out TypeMatcher matcher)
			{
				matcher = null;
				if (!info.MatchType.IsGenericTypeDefinition)
				{
					return false;
				}
				if (!info.Targets[0].IsGenericType)
				{
					return false;
				}
				matcher = new GenericSingleTargetTypeMatcher
				{
					info = info,
					matchArgs = info.MatchType.GetGenericArguments(),
					matchTargetArgs = info.Targets[0].GetGenericArguments(),
					targetGenericTypeDefinition = info.Targets[0].GetGenericTypeDefinition()
				};
				return true;
			}
		}

		private TypeSearchInfo info;

		private Type[] matchArgs;

		private Type[] matchTargetArgs;

		private Type targetGenericTypeDefinition;

		public override string Name => "Generic Single Target Match --> Type<T1 [, T2]> : Match<GenericType<T1 [, T2]>> [where T1 [, T2] : constraints]";

		public override Type Match(Type[] targets, ref bool stopMatching)
		{
			if (targets.Length != 1)
			{
				return null;
			}
			if (!targets[0].IsGenericType)
			{
				return null;
			}
			if (targetGenericTypeDefinition != targets[0].GetGenericTypeDefinition())
			{
				return null;
			}
			Type[] targetArgs = targets[0].GetGenericArguments();
			if (matchArgs.Length != matchTargetArgs.Length || matchArgs.Length != targetArgs.Length)
			{
				return null;
			}
			if (!info.MatchType.AreGenericConstraintsSatisfiedBy(targetArgs))
			{
				return null;
			}
			return info.MatchType.MakeGenericType(targetArgs);
		}
	}
}
