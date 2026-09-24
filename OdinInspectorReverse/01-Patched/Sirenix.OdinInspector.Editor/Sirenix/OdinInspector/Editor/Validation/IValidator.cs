namespace Sirenix.OdinInspector.Editor.Validation
{
	public interface IValidator
	{
		RevalidationCriteria RevalidationCriteria { get; }

		void RunValidation(ref ValidationResult result);
	}
}
