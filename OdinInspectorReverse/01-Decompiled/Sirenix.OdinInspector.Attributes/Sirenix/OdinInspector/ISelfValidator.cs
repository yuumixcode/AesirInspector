namespace Sirenix.OdinInspector
{
	/// <summary>
	/// Any type implementing this interface will be considered to be validating itself using the implemented logic, as if a custom validator had been written for it.
	/// </summary>
	public interface ISelfValidator
	{
		void Validate(SelfValidationResult result);
	}
}
