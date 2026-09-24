using System.Collections.Generic;

namespace Sirenix.OdinValidator.Editor
{
	public interface ISerializedRulesContainer
	{
		void SetProjectRules(List<SerializedRule> rules);

		void SetLocalRules(List<SerializedRule> rules);

		void SaveRules();
	}
}
