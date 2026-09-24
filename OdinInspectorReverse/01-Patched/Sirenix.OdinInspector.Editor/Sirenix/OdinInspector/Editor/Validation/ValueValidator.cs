using System;
using System.Reflection;
using Sirenix.Utilities;

namespace Sirenix.OdinInspector.Editor.Validation
{
	public abstract class ValueValidator<TValue> : Validator, DefaultValidatorLocator.IValueValidator_InternalTemporaryHack
	{
		private IPropertyValueEntry<TValue> valueEntry;

		Type DefaultValidatorLocator.IValueValidator_InternalTemporaryHack.ValidatedType => typeof(TValue);

		public IPropertyValueEntry<TValue> ValueEntry
		{
			get
			{
				if (valueEntry == null || valueEntry != base.Property.ValueEntry)
				{
					valueEntry = base.Property.TryGetTypedValueEntry<TValue>();
				}
				return valueEntry;
			}
		}

		public TValue Value
		{
			get
			{
				return ValueEntry.SmartValue;
			}
			set
			{
				ValueEntry.SmartValue = value;
			}
		}

		protected virtual void Validate(ValidationResult result)
		{
			result.ResultType = ValidationResultType.Warning;
			result.Message = "Validation logic for " + GetType().GetNiceName() + " has not been implemented yet. Override Validate(ValidationResult result) to implement validation logic.";
		}

		public sealed override void RunValidation(ref ValidationResult result)
		{
			InitializeResult(ref result);
			if (ValueEntry == null)
			{
				result.ResultType = ValidationResultType.Error;
				result.Message = "Property " + base.Property.NiceName + " did not have validator " + GetType().GetNiceName() + "'s expected value entry of type '" + typeof(TValue).GetNiceName() + "' on it, but instead a value entry of type '" + base.Property.ValueEntry.TypeOfValue.GetNiceName() + "'!";
				return;
			}
			try
			{
				Validate(result);
			}
			catch (Exception innerException)
			{
				while (innerException is TargetInvocationException)
				{
					innerException = innerException.InnerException;
				}
				result.ResultType = ValidationResultType.Error;
				result.Message = innerException.ToString();
			}
		}
	}
}
