using System;
using Sirenix.OdinInspector.Editor.Validation;
using Sirenix.Utilities;

namespace Sirenix.OdinValidator.Editor.Validators
{
	public static class WithRuleConfig
	{
		public static ref ResultItem WithModifyRuleDataContextClick<T>(this ref ResultItem item, string path, Action<T> onClick) where T : Validator
		{
			item.WithContextClick(path + "/For me only", delegate
			{
				T ruleData = GlobalConfig<RuleConfig>.Instance.GetRuleData<T>(ConfigSourceType.Local);
				onClick(ruleData);
				GlobalConfig<RuleConfig>.Instance.SetAndSaveRuleData(ruleData, ConfigSourceType.Local);
			});
			item.WithContextClick(path + "/For everyone", delegate
			{
				T ruleData = GlobalConfig<RuleConfig>.Instance.GetRuleData<T>(ConfigSourceType.Project);
				onClick(ruleData);
				GlobalConfig<RuleConfig>.Instance.SetAndSaveRuleData(ruleData, ConfigSourceType.Project);
			});
			return ref item;
		}
	}
}
