using System;

namespace Sirenix.OdinInspector.Editor.Validation
{
	public struct ValidationStateChangeInfo
	{
		public ValidationResult ValidationResult;

		[Obsolete("Get the validator from the result isntead.", false)]
		public IValidator Validator => ValidationResult.Setup.Validator as IValidator;
	}
}
