using Sirenix.OdinInspector.Editor.Validation;

namespace Sirenix.OdinInspector.Editor
{
	public sealed class ValidationComponentProvider : ComponentProvider
	{
		public IValidatorLocator ValidatorLocator;

		public ValidationComponentProvider()
		{
			ValidatorLocator = new DefaultValidatorLocator();
		}

		public ValidationComponentProvider(IValidatorLocator validatorLocator)
		{
			ValidatorLocator = validatorLocator;
		}

		public override PropertyComponent CreateComponent(InspectorProperty property)
		{
			return new ValidationComponent(property, ValidatorLocator);
		}
	}
}
