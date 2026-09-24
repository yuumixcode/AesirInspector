using Sirenix.OdinInspector.Editor.ValueResolvers;

namespace Sirenix.OdinInspector.Editor.Validation
{
	[NoValidationInInspector]
	public class DetailedInfoBoxValidator : AttributeValidator<DetailedInfoBoxAttribute>
	{
		private ValueResolver<bool> showMessageGetter;

		private ValueResolver<string> messageGetter;

		private ValueResolver<string> detailsGetter;

		protected override void Initialize()
		{
			if (base.Attribute.VisibleIf != null)
			{
				showMessageGetter = ValueResolver.Get(base.Property, base.Attribute.VisibleIf, fallbackValue: true);
				messageGetter = ValueResolver.GetForString(base.Property, base.Attribute.Message);
				detailsGetter = ValueResolver.GetForString(base.Property, base.Attribute.Details);
			}
		}

		protected override void Validate(ValidationResult result)
		{
			if (showMessageGetter != null)
			{
				if (showMessageGetter.HasError || messageGetter.HasError || showMessageGetter.HasError)
				{
					result.Message = ValueResolver.GetCombinedErrors(showMessageGetter, messageGetter, detailsGetter);
					result.ResultType = ValidationResultType.Error;
				}
				else if (showMessageGetter.GetValue())
				{
					result.ResultType = base.Attribute.InfoMessageType.ToValidationResultType();
					result.Message = messageGetter.GetValue() + "\n\nDETAILS:\n\n" + detailsGetter.GetValue();
				}
			}
		}
	}
}
