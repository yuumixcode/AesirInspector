using Sirenix.OdinInspector.Editor.ValueResolvers;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Validation
{
	public class RequiredValidator<T> : AttributeValidator<RequiredAttribute, T> where T : class
	{
		private ValueResolver<string> errorMessageGetter;

		protected override void Initialize()
		{
			if (base.Attribute.ErrorMessage != null)
			{
				errorMessageGetter = ValueResolver.GetForString(base.Property, base.Attribute.ErrorMessage);
			}
		}

		protected override void Validate(ValidationResult result)
		{
			if (!IsValid(base.ValueEntry.SmartValue))
			{
				ValidatorSeverity severity = base.Attribute.MessageType.ToValidatorSeverity();
				string message = ((errorMessageGetter != null) ? errorMessageGetter.GetValue() : (base.Property.NiceName + " is required"));
				result.Add(severity, message).WithFix(delegate(FixArgs<T> arg)
				{
					base.Property.ValueEntry.WeakSmartValue = arg.NewValue;
				}, offerInInspector: false);
			}
		}

		private bool IsValid(T memberValue)
		{
			if (memberValue == null)
			{
				return false;
			}
			if (memberValue is string && string.IsNullOrEmpty(memberValue as string))
			{
				return false;
			}
			if (memberValue is Object && memberValue as Object == null)
			{
				return false;
			}
			return true;
		}
	}
}
