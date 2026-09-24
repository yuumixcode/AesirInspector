using System.Collections;
using System.Collections.Generic;

namespace Sirenix.OdinInspector.Editor.Validation
{
	public abstract class GlobalValidator : IValidator
	{
		public RevalidationCriteria RevalidationCriteria => RevalidationCriteria.Always;

		internal IEnumerable<ValidationResult> RunValidation()
		{
			ValidationResult result = new ValidationResult
			{
				Setup = new ValidationSetup
				{
					Validator = this
				}
			};
			IEnumerable enumerator = RunValidation(result);
			if (enumerator != null)
			{
				foreach (object item in enumerator)
				{
					_ = item;
					yield return null;
				}
			}
			yield return result;
		}

		public abstract IEnumerable RunValidation(ValidationResult result);

		void IValidator.RunValidation(ref ValidationResult result)
		{
			IEnumerator enumerator = RunValidation(result).GetEnumerator();
			while (enumerator.MoveNext())
			{
			}
		}
	}
}
