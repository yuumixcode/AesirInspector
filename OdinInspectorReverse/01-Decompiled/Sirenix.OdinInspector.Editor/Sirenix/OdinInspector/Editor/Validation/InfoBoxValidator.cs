using Sirenix.OdinInspector.Editor.ValueResolvers;

namespace Sirenix.OdinInspector.Editor.Validation
{
	[NoValidationInInspector]
	public class InfoBoxValidator : AttributeValidator<InfoBoxAttribute>
	{
		private ValueResolver<bool> showMessageGetter;

		private ValueResolver<string> messageGetter;

		protected override void Initialize()
		{
			showMessageGetter = ValueResolver.Get(base.Property, base.Attribute.VisibleIf, fallbackValue: true);
			messageGetter = ValueResolver.GetForString(base.Property, base.Attribute.Message);
		}

		protected override void Validate(ValidationResult result)
		{
			if (showMessageGetter != null)
			{
				if (showMessageGetter.HasError || messageGetter.HasError)
				{
					result.Message = ValueResolver.GetCombinedErrors(showMessageGetter, messageGetter);
					result.ResultType = ValidationResultType.Error;
				}
				else if (showMessageGetter.GetValue())
				{
					result.ResultType = base.Attribute.InfoMessageType.ToValidationResultType();
					result.Message = messageGetter.GetValue();
				}
			}
		}
	}
}
