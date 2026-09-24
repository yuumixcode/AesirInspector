using System;

namespace Sirenix.OdinInspector.Editor.TypeSearch
{
	public class TypeMatchRule
	{
		public delegate Type TypeMatchRuleDelegate1(TypeSearchInfo info, Type[] targets);

		public delegate Type TypeMatchRuleDelegate2(TypeSearchInfo info, Type[] targets, ref bool stopMatchingForInfo);

		public readonly string Name;

		private TypeMatchRuleDelegate1 rule1;

		private TypeMatchRuleDelegate2 rule2;

		public TypeMatchRule(string name, TypeMatchRuleDelegate1 rule)
		{
			Name = name;
			rule1 = rule;
		}

		public TypeMatchRule(string name, TypeMatchRuleDelegate2 rule)
		{
			Name = name;
			rule2 = rule;
		}

		public Type Match(TypeSearchInfo matchInfo, Type[] targets, ref bool stopMatchingForInfo)
		{
			if (rule1 != null)
			{
				return rule1(matchInfo, targets);
			}
			return rule2(matchInfo, targets, ref stopMatchingForInfo);
		}

		public override string ToString()
		{
			return "TypeMatchRule: " + Name;
		}
	}
}
