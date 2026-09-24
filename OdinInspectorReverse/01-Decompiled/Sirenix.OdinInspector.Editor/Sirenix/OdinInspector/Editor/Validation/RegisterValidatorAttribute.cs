using System;

namespace Sirenix.OdinInspector.Editor.Validation
{
	/// <summary>
	/// Apply this to an assembly to register validators for the validation system.
	/// This enables locating of all relevant validator types very quickly.
	/// </summary>
	/// <seealso cref="!:RegisterValidationRuleAttribute" />
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	public class RegisterValidatorAttribute : Attribute
	{
		public readonly Type ValidatorType;

		public readonly Type SpecialInstantiator;

		public int Priority;

		public RegisterValidatorAttribute(Type validatorType)
		{
			ValidatorType = validatorType;
		}

		protected RegisterValidatorAttribute(Type validatorType, Type specialInstantiator)
		{
			ValidatorType = validatorType;
			SpecialInstantiator = specialInstantiator;
		}
	}
}
