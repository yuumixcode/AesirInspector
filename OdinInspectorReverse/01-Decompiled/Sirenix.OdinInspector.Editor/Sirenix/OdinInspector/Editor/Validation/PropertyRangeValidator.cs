using System;
using Sirenix.OdinInspector.Editor.ValueResolvers;

namespace Sirenix.OdinInspector.Editor.Validation
{
	public class PropertyRangeValidator<T> : AttributeValidator<PropertyRangeAttribute, T> where T : struct
	{
		private class RangeFix
		{
			public T NewValue;
		}

		private static readonly bool IsNumber = GenericNumberUtility.IsNumber(typeof(T));

		private static readonly bool IsVector = GenericNumberUtility.IsVector(typeof(T));

		private ValueResolver<double> minValueGetter;

		private ValueResolver<double> maxValueGetter;

		public override bool CanValidateProperty(InspectorProperty property)
		{
			if (!IsNumber)
			{
				return IsVector;
			}
			return true;
		}

		protected override void Initialize()
		{
			minValueGetter = ValueResolver.Get(base.Property, base.Attribute.MinGetter, base.Attribute.Min);
			maxValueGetter = ValueResolver.Get(base.Property, base.Attribute.MaxGetter, base.Attribute.Max);
		}

		protected override void Validate(ValidationResult result)
		{
			if (minValueGetter.HasError || maxValueGetter.HasError)
			{
				result.Message = ValueResolver.GetCombinedErrors(minValueGetter, maxValueGetter);
				result.ResultType = ValidationResultType.Error;
				return;
			}
			double min = minValueGetter.GetValue();
			double max = maxValueGetter.GetValue();
			if (!GenericNumberUtility.NumberIsInRange(base.ValueEntry.SmartValue, Math.Min(min, max), Math.Max(min, max)))
			{
				result.AddError("Number is not in range.").WithFix(Fix.Create(delegate(RangeFix f)
				{
					base.ValueEntry.SmartValue = f.NewValue;
				}, offerInInspector: false));
			}
		}
	}
}
