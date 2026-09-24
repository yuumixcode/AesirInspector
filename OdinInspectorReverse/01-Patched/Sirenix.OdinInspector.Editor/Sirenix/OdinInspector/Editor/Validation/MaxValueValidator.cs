using Sirenix.OdinInspector.Editor.ValueResolvers;

namespace Sirenix.OdinInspector.Editor.Validation
{
	public class MaxValueValidator<T> : AttributeValidator<MaxValueAttribute, T> where T : struct
	{
		private static readonly bool IsNumber = GenericNumberUtility.IsNumber(typeof(T));

		private static readonly bool IsVector = GenericNumberUtility.IsVector(typeof(T));

		private ValueResolver<double> maxValueGetter;

		public override bool CanValidateProperty(InspectorProperty property)
		{
			bool hasMinValueAttribute = property.Attributes.HasAttribute<MinValueAttribute>();
			if (IsNumber || IsVector)
			{
				return !hasMinValueAttribute;
			}
			return false;
		}

		protected override void Initialize()
		{
			maxValueGetter = ValueResolver.Get(base.Property, base.Attribute.Expression, base.Attribute.MaxValue);
		}

		protected override void Validate(ValidationResult result)
		{
			if (maxValueGetter.HasError)
			{
				result.Message = maxValueGetter.ErrorMessage;
				result.ResultType = ValidationResultType.Error;
				return;
			}
			double max = maxValueGetter.GetValue();
			T value = base.ValueEntry.SmartValue;
			if (!GenericNumberUtility.NumberIsInRange(value, double.NegativeInfinity, max, out var error))
			{
				result.Message = error;
				result.ResultType = ValidationResultType.Error;
			}
		}
	}
}
