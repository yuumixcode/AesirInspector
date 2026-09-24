using Sirenix.OdinInspector.Editor.ValueResolvers;

namespace Sirenix.OdinInspector.Editor.Validation
{
	public class MinMaxValueCompositeValidator<T> : ValueValidator<T> where T : struct
	{
		private static readonly bool IsNumber = GenericNumberUtility.IsNumber(typeof(T));

		private static readonly bool IsVector = GenericNumberUtility.IsVector(typeof(T));

		private ValueResolver<double> minValueGetter;

		private ValueResolver<double> maxValueGetter;

		public override bool CanValidateProperty(InspectorProperty property)
		{
			bool hasMinValueAttribute = property.Attributes.HasAttribute<MinValueAttribute>();
			bool hasMaxValueAttribute = property.Attributes.HasAttribute<MaxValueAttribute>();
			return (IsNumber || IsVector) && hasMinValueAttribute && hasMaxValueAttribute;
		}

		protected override void Initialize()
		{
			MinValueAttribute minValueAttribute = base.Property.Attributes.GetAttribute<MinValueAttribute>();
			MaxValueAttribute maxValueAttribute = base.Property.Attributes.GetAttribute<MaxValueAttribute>();
			minValueGetter = ValueResolver.Get(base.Property, minValueAttribute.Expression, minValueAttribute.MinValue);
			maxValueGetter = ValueResolver.Get(base.Property, maxValueAttribute.Expression, maxValueAttribute.MaxValue);
		}

		protected override void Validate(ValidationResult result)
		{
			if (minValueGetter.HasError)
			{
				result.Message = minValueGetter.ErrorMessage;
				result.ResultType = ValidationResultType.Error;
				return;
			}
			if (maxValueGetter.HasError)
			{
				result.Message = maxValueGetter.ErrorMessage;
				result.ResultType = ValidationResultType.Error;
				return;
			}
			double min = minValueGetter.GetValue();
			double max = maxValueGetter.GetValue();
			T value = base.ValueEntry.SmartValue;
			if (!GenericNumberUtility.NumberIsInRange(value, min, max, out var error))
			{
				result.Message = error;
				result.ResultType = ValidationResultType.Error;
			}
		}
	}
}
