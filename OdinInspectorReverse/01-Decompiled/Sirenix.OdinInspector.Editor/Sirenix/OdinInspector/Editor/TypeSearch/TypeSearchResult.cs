using System;

namespace Sirenix.OdinInspector.Editor.TypeSearch
{
	public struct TypeSearchResult
	{
		public TypeSearchInfo MatchedInfo;

		public Type MatchedType;

		public Type[] MatchedTargets;

		public TypeMatcher MatchedMatcher;

		public TypeMatchRule MatchedRule;

		public TypeSearchIndex MatchedIndex;
	}
}
