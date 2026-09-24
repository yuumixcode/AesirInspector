using System;
using Sirenix.OdinInspector.Editor.Validation;
using Sirenix.Utilities;

namespace Sirenix.OdinValidator.Editor
{
	public static class RuleConfigExtensions
	{
		public static ref ResultItem WithModifyRuleDataContextClick<T>(this ref ResultItem item, string path, Action<T> onClick) where T : class, IValidator
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
