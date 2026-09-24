using System;
using System.Reflection;
using Sirenix.OdinInspector.Editor.Validation.Internal;
using Sirenix.Utilities;

namespace Sirenix.OdinInspector.Editor.Validation
{
	public abstract class AttributeValidator<TAttribute> : Validator, IAttributeValidator where TAttribute : Attribute
	{
		private bool? isValueValidator_backing;

		private int attributeNumber;

		private bool IsValueValidator
		{
			get
			{
				if (!isValueValidator_backing.HasValue)
				{
					isValueValidator_backing = this is IAttributeValueValidator;
				}
				return isValueValidator_backing.Value;
			}
		}

		int IAttributeValidator.AttributeNumber => attributeNumber;

		Type IAttributeValidator.AttributeType => typeof(TAttribute);

		public TAttribute Attribute { get; private set; }

		internal virtual IPropertyValueEntry InternalValueEntry => null;

		public sealed override void RunValidation(ref ValidationResult result)
		{
			InitializeResult(ref result);
			if (IsValueValidator && InternalValueEntry == null)
			{
				result.ResultType = ValidationResultType.Error;
				result.Message = "Property " + base.Property.NiceName + " did not have validator " + GetType().GetNiceName() + "'s expected value entry of type '" + (this as IAttributeValueValidator).GetValueType().GetNiceName() + "' on it, but instead a value entry of type '" + base.Property.ValueEntry.TypeOfValue.GetNiceName() + "'!";
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
				result.Message = "An exception was thrown during validation: " + innerException.ToString();
			}
		}

		protected virtual void Validate(ValidationResult result)
		{
			result.ResultType = ValidationResultType.Warning;
			result.Message = "Validation logic for " + GetType().GetNiceName() + " has not been implemented yet. Override Validate(ValidationResult result) to implement validation logic.";
		}

		void IAttributeValidator.SetAttributeInstanceAndNumber(Attribute attribute, int number)
		{
			Attribute = (TAttribute)attribute;
			attributeNumber = number;
		}
	}
	public abstract class AttributeValidator<TAttribute, TValue> : AttributeValidator<TAttribute>, IAttributeValueValidator where TAttribute : Attribute
	{
		private IPropertyValueEntry<TValue> valueEntry;

		internal override IPropertyValueEntry InternalValueEntry => ValueEntry;

		public IPropertyValueEntry<TValue> ValueEntry
		{
			get
			{
				if (valueEntry == null)
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

		Type IAttributeValueValidator.GetValueType()
		{
			return typeof(TValue);
		}

		protected override void Validate(ValidationResult result)
		{
			result.ResultType = ValidationResultType.Warning;
			result.Message = "Validation logic for " + GetType().GetNiceName() + " has not been implemented yet. Override Validate(ValidationResult result) to implement validation logic.";
		}
	}
}
