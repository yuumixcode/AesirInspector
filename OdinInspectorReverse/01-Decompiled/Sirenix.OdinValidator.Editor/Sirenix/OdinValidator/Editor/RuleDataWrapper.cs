using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Sirenix.OdinValidator.Editor
{
	public class RuleDataWrapper
	{
		[HideInInspector]
		public ISerializedRulesContainer RulesContainer;

		[HideInInspector]
		public CombinedRuleInstance[] Rules;

		public RuleDataWrapper(ISerializedRulesContainer rulesContainer, List<SerializedRule> projectRules, List<SerializedRule> localRules, List<SerializedRule> defaultRules)
		{
			RulesContainer = rulesContainer;
			Rules = new CombinedRuleInstance[defaultRules.Count];
			for (int i = 0; i < defaultRules.Count; i++)
			{
				SerializedRule rule = defaultRules[i];
				Rules[i] = new CombinedRuleInstance
				{
					Default = rule,
					Project = projectRules?.FirstOrDefault((SerializedRule n) => n.ValidatorType == rule.ValidatorType),
					Local = localRules?.FirstOrDefault((SerializedRule n) => n.ValidatorType == rule.ValidatorType)
				};
			}
			Rules = Rules.OrderBy((CombinedRuleInstance x) => x.Name).ToArray();
		}

		public void SaveChanges()
		{
			RulesContainer.SetLocalRules((from n in Rules
				where n.Local != null && (n.Local.EnabledOverridden || n.Local.DataOverride != null)
				select n.Local into x
				orderby x.ValidatorType.Name descending
				select x).ToList());
			RulesContainer.SetProjectRules((from n in Rules
				where n.Project != null && (n.Project.EnabledOverridden || n.Project.DataOverride != null)
				select n.Project into x
				orderby x.ValidatorType.Name descending
				select x).ToList());
			RulesContainer.SaveRules();
		}
	}
}
