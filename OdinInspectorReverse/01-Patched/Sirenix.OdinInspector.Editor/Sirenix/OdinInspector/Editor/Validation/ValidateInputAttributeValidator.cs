using Sirenix.OdinInspector.Editor.ValueResolvers;

namespace Sirenix.OdinInspector.Editor.Validation
{
	public class ValidateInputAttributeValidator<T> : AttributeValidator<ValidateInputAttribute, T>
	{
		private static readonly NamedValue[] customValidationArgs = new NamedValue[3]
		{
			new NamedValue("value", typeof(T)),
			new NamedValue("message", typeof(string)),
			new NamedValue("messageType", typeof(InfoMessageType?))
		};

		private ValidationResultType defaultResultType;

		private ValueResolver<string> defaultValidationMessageGetter;

		private ValueResolver<bool> validationChecker;

		public override RevalidationCriteria RevalidationCriteria
		{
			get
			{
				if (base.Attribute.ContinuousValidationCheck)
				{
					return RevalidationCriteria.Always;
				}
				if (base.Attribute.IncludeChildren)
				{
					return RevalidationCriteria.OnValueChangeOrChildValueChange;
				}
				return RevalidationCriteria.OnValueChange;
			}
		}

		protected override void Initialize()
		{
			defaultResultType = base.Attribute.MessageType.ToValidationResultType();
			defaultValidationMessageGetter = ValueResolver.Get(base.Property, base.Attribute.DefaultMessage, base.Attribute.DefaultMessage ?? ("Value is invalid for '" + base.Property.NiceName + "'"));
			ValueResolverContext context = ValueResolverContext.CreateDefault<bool>(base.Property, base.Attribute.Condition, customValidationArgs);
			context.SyncRefParametersWithNamedValues = true;
			validationChecker = ValueResolver.GetFromContext<bool>(ref context);
		}

		protected override void Validate(ValidationResult result)
		{
			if (defaultValidationMessageGetter.HasError || validationChecker.HasError)
			{
				result.Message = ValueResolver.GetCombinedErrors(validationChecker, defaultValidationMessageGetter);
				result.ResultType = ValidationResultType.Error;
				return;
			}
			validationChecker.Context.NamedValues.Set("value", base.ValueEntry.SmartValue);
			validationChecker.Context.NamedValues.Set("message", null);
			validationChecker.Context.NamedValues.Set("messageType", null);
			if (!validationChecker.GetValue())
			{
				string messageParam = (string)validationChecker.Context.NamedValues.GetValue("message");
				InfoMessageType? resultTypeParam = (InfoMessageType?)validationChecker.Context.NamedValues.GetValue("messageType");
				result.Message = messageParam ?? defaultValidationMessageGetter.GetValue();
				result.ResultType = (resultTypeParam.HasValue ? resultTypeParam.Value.ToValidationResultType() : defaultResultType);
			}
		}
	}
}
