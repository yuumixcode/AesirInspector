using System;

namespace Sirenix.OdinInspector.Editor.Validation
{
	/// <summary>
	/// Use <see cref="M:Sirenix.OdinInspector.Editor.Validation.Validator.InitializeResult(Sirenix.OdinInspector.Editor.Validation.ValidationResult@)" /> to initialize an empty <see cref="T:Sirenix.OdinInspector.Editor.Validation.ValidationResult" />. 
	/// </summary>
	public struct ValidationSetup
	{
		public object Validator;

		[Obsolete("This field is no longer populated by the validation system, as it was never used and caused a lot of garbage allocation.", false)]
		public object Value;

		public object ParentInstance;

		public object Root;
	}
}
