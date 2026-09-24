using Sirenix.Utilities;

namespace Sirenix.OdinValidator.Editor
{
	public class ValidationRunnerConfig
	{
		public static ValidationRunnerConfig Default = new ValidationRunnerConfig
		{
			DeepValidation = GlobalConfig<GlobalValidationConfig>.Instance.DeepValidation
		};

		public bool DeepValidation;
	}
}
