using Sirenix.OdinInspector.Editor.ValueResolvers;

namespace Sirenix.OdinInspector.Editor.Validation
{
	public class MinValueValidator<T> : AttributeValidator<MinValueAttribute, T> where T : struct
	{
		private static readonly bool IsNumber = GenericNumberUtility.IsNumber(typeof(T));

		private static readonly bool IsVector = GenericNumberUtility.IsVector(typeof(T));

		private ValueResolver<double> minValueGetter;

		public override bool CanValidateProperty(InspectorProperty property)
		{
			bool hasMaxValueAttribute = property.Attributes.HasAttribute<MaxValueAttribute>();
			if (IsNumber || IsVector)
			{
				return !hasMaxValueAttribute;
			}
			return false;
		}

		protected override void Initialize()
		{
			minValueGetter = ValueResolver.Get(base.Property, base.Attribute.Expression, base.Attribute.MinValue);
		}

		protected override void Validate(ValidationResult result)
		{
			if (minValueGetter.HasError)
			{
				result.Message = minValueGetter.ErrorMessage;
				result.ResultType = ValidationResultType.Error;
				return;
			}
			double min = minValueGetter.GetValue();
			T value = base.ValueEntry.SmartValue;
			if (!GenericNumberUtility.NumberIsInRange(value, min, double.PositiveInfinity, out var error))
			{
				result.Message = error;
				result.ResultType = ValidationResultType.Error;
			}
		}
	}
}
