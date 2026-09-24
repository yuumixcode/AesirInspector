using System;

namespace Sirenix.OdinInspector.Editor.TypeSearch
{
	public class DerivedTypeMatcher : TypeMatcher
	{
		public class Creator : TypeMatcherCreator
		{
			public override bool TryCreateMatcher(TypeSearchInfo info, out TypeMatcher matcher)
			{
				matcher = null;
				if (info.MatchType.IsGenericTypeDefinition || info.MatchType.IsDefined(typeof(DisableNonGenericPolymorphicTypeMatchingAttribute), inherit: false))
				{
					return false;
				}
				matcher = new DerivedTypeMatcher
				{
					info = info
				};
				return true;
			}
		}

		private TypeSearchInfo info;

		public override string Name => "Derived Type Match --> Type : Match[<Target>]";

		public override Type Match(Type[] targets, ref bool stopMatching)
		{
			if (targets.Length != info.Targets.Length)
			{
				return null;
			}
			for (int i = 0; i < targets.Length; i++)
			{
				if (typeof(Attribute).IsAssignableFrom(targets[i]) || !info.Targets[i].IsAssignableFrom(targets[i]))
				{
					return null;
				}
			}
			return info.MatchType;
		}
	}
}
